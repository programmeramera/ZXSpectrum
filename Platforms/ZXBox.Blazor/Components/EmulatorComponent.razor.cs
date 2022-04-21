using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using ZXBox.Hardware.Input;
using ZXBox.Hardware.Input.Joystick;
using ZXBox.Hardware.Output;
using ZXBox.Snapshot;

namespace ZXBox.Blazor.Pages
{
    public partial class EmulatorComponentModel : ComponentBase
    {
        private ZXSpectrum speccy;
        public System.Timers.Timer gameLoop;
        int flashcounter = 16;
        bool flash = false;
        JavaScriptKeyboard Keyboard = new JavaScriptKeyboard();
        Kempston kempston;
        Beeper<byte> beeper;

        [Inject]
        Toolbelt.Blazor.Gamepad.GamepadList GamePadList { get; set; }

        [Inject]
        protected HttpClient Http { get; set; }
        [Inject]
        protected IJSInProcessRuntime JSRuntime { get; set; }
        public EmulatorComponentModel()
        {
            gameLoop = new System.Timers.Timer(20);
            gameLoop.Elapsed += GameLoop_Elapsed;
        }

        public ZXSpectrum GetZXSpectrum(RomEnum rom)
        {
            return new ZXSpectrum(true, true, 20, 20, 20, rom);
        }

        public void StartZXSpectrum(RomEnum rom)
        {
            speccy = GetZXSpectrum(rom);
            speccy.InputHardware.Add(Keyboard);

            kempston = new Kempston();
            speccy.InputHardware.Add(kempston);
            //48000 samples per second, 50 frames per second (20ms per frame)
            beeper = new Beeper<byte>(0, 127, 48000 / 50, 1);
            speccy.OutputHardware.Add(beeper);

            speccy.Reset();
            gameLoop.Start();
        }

        public async Task HandleFileSelected(InputFileChangeEventArgs args)
        {
            var file = args.File;

            var ms = new MemoryStream();

            await file.OpenReadStream().CopyToAsync(ms);

            var handler = FileFormatFactory.GetSnapShotHandler(file.Name);
            var bytes = ms.ToArray();
            handler.LoadSnapshot(bytes, speccy);
        }
        [Inject]
        HttpClient httpClient { get; set; }
        public string Instructions = "";
        public async Task LoadGame(string filename, string instructions)
        {
            var ms = new MemoryStream();
            var handler = FileFormatFactory.GetSnapShotHandler(filename);
            var stream = await httpClient.GetStreamAsync("Roms/" + filename + ".json");
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            handler.LoadSnapshot(bytes, speccy);
            Instructions = instructions;
        }

        private async void GameLoop_Elapsed(object sender, ElapsedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Get gamepads
            kempston.Gamepads = await GamePadList.GetGamepadsAsync();

            //Run JavaScriptInterop to find the currently pressed buttons
            Keyboard.KeyBuffer = await JSRuntime.InvokeAsync<List<string>>("getKeyStatus");

            speccy.DoIntructions(69888);

            beeper.GenerateSound();
            await BufferSound();

            Paint();
            sw.Stop();
            if (sw.ElapsedMilliseconds > 20)
            {
                Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            }
        }

        protected async Task BufferSound()
        {
            var soundbytes = beeper.GetSoundBuffer();

            var gch = GCHandle.Alloc(soundbytes, GCHandleType.Pinned);
            var pinned = gch.AddrOfPinnedObject();
            var mono = JSRuntime as WebAssemblyJSRuntime;
            mono.InvokeUnmarshalled<IntPtr, string>("addAudioBuffer", pinned);
            gch.Free();
        }

        protected async override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeAsync<bool>("InitCanvas");
            }
            base.OnAfterRender(firstRender);
        }

        //uint[] screen = new uint[68672]; //Height * width (256+20+20)*(192+20+20)
        public async void Paint()
        {
            if (flashcounter == 0)
            {
                flashcounter = 16;
                flash = !flash;
            }
            else
            {
                flashcounter--;
            }

            var screen = speccy.GetScreenInUint(flash);

            //Allocate memory
            var gch = GCHandle.Alloc(screen, GCHandleType.Pinned);
            var pinned = gch.AddrOfPinnedObject();
            var mono = JSRuntime as WebAssemblyJSRuntime;
            mono.InvokeUnmarshalled<IntPtr, string>("PaintCanvas", pinned);
            gch.Free();
        }
    }
}