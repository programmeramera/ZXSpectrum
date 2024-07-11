using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Runtime.InteropServices;

namespace ZXBox.Monogame;

public class ZXEmulator : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Texture2D texture;
    private int width = 256 + 20 * 2;
    private int height = 192 + 20 * 2  ;
    private ZXBox.ZXSpectrum speccy;
    byte[] backBuffer;
    int flashcounter = 16;
    bool flash=false;
    
    public ZXEmulator()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        graphics.PreferredBackBufferHeight = height;
        graphics.PreferredBackBufferWidth = width;
        graphics.ApplyChanges();
        
        speccy = new ZXSpectrum(true, true, 20, 20, 20);
        speccy.Reset();

        texture = new Texture2D(GraphicsDevice, width, height);
        backBuffer = new byte[width * height * sizeof(uint)];   
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
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

        var screen = speccy.GetScreenInBytes(flash);
        var MAX = width * height * sizeof(uint);
        var elementsToWrite = screen.Length > MAX ? MAX : screen.Length;
        System.Buffer.BlockCopy(screen, 0, backBuffer, 0, screen.Length);
        texture.SetData<byte>(backBuffer, 0, backBuffer.Length );
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color. CornflowerBlue);
        spriteBatch.Begin();
        spriteBatch.Draw(texture, Vector2.Zero, Color.White);   
        spriteBatch.End();
        base.Draw(gameTime);
    }
}
