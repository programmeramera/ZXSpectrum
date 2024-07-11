using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;
using ZXBox.Snapshot;

namespace ZXBox.Monogame;

public class ZXEmulator : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private int width = 256 + 20 * 2;
    private int height = 192 + 20 * 2  ;
    private ZXBox.ZXSpectrum speccy;
    private const int SCALE = 2;
    int flashcounter = 16;
    bool flash=false;
    Hardware.Screen screen;
    
    public ZXEmulator()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        graphics.PreferredBackBufferHeight = height * SCALE;
        graphics.PreferredBackBufferWidth = width * SCALE;
        graphics.ApplyChanges();
        
        var keyboard = new Hardware.Keyboard(this);
        this.Components.Add(keyboard);

        speccy = new ZXSpectrum(true, true, 20, 20, 20);
        speccy.InputHardware.Add(keyboard);
        speccy.Reset();

        screen = new Hardware.Screen(this, width, height, SCALE);
        this.Components.Add(screen);

        base.Initialize();
    }

    protected override async void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        await Task.Delay(5000);
        await LoadGame("ManicMiner.z80");
    }

    private async Task LoadGame(string filename)
    {
        var ms = new MemoryStream();
        var handler = FileFormatFactory.GetSnapShotHandler(filename);
        var stream = new FileStream("Platforms/ZXBox.Monogame/Roms/" + filename + ".json", FileMode.Open);
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        handler.LoadSnapshot(bytes, speccy);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        speccy.DoIntructions(69888);

        if (flashcounter == 0)
        {
            flashcounter = 16;
            flash = !flash;
        }
        else
        {
            flashcounter--; 
        }

        screen.SetPixels(speccy.GetScreenInBytes(flash));

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color. CornflowerBlue);
        base.Draw(gameTime);
    }
}
