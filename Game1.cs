using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Serialization;

using MonoGame.Extended.ViewportAdapters;
using MonoGame.Extended.Content;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Input;
using MonoGame.Extended;
using System.Threading.Tasks;
using System;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace asyncMono
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        TiledMapRenderer _tiledMapRenderer;


        private Actor playerActor = new(new Vector2(5, 5)) { visibile = true };
        private Actor fireballActor = new() { visibile = false };
        private OrthographicCamera _camera;
        private Vector2 _cameraPosition = new Vector2(12 * 32, 8 * 32);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            var viewportadapter = new BoxingViewportAdapter(Window, GraphicsDevice, this._graphics.PreferredBackBufferWidth, this._graphics.PreferredBackBufferHeight);
            _camera = new OrthographicCamera(viewportadapter);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var spriteSheet = Content.Load<SpriteSheet>("char.sf", new JsonContentLoader());
            playerActor.Sprite = new AnimatedSprite(spriteSheet);

            var spriteSheetFireball = Content.Load<SpriteSheet>("fireball.sf", new JsonContentLoader());
            fireballActor.Sprite = new AnimatedSprite(spriteSheetFireball);
            // TODO: use this.Content to load your game content here

          var  tiledMap = Content.Load<TiledMap>("orthogonal-outside");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, tiledMap);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _tiledMapRenderer.Update(gameTime);


            var state = KeyboardExtended.GetState();
            if (state.WasKeyJustUp(Keys.A))
            {
                RunScript(ScriptFire);
            }
            if (state.WasKeyJustUp(Keys.S))
            {
                RunScript(ScriptCircle);
            }


            _camera.LookAt(_cameraPosition);
            playerActor.Update(gameTime);
            fireballActor.Update(gameTime);


            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: _camera.GetViewMatrix());

            // TODO: Add your drawing code here
            _tiledMapRenderer.Draw(viewMatrix: _camera.GetViewMatrix());

            playerActor.Draw(_spriteBatch);
            fireballActor.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private Task runningScript = Task.CompletedTask;
        private bool RunScript(Func<Task> script)
        {
            if (!runningScript.IsCompleted)
                return false;
            runningScript = script();
            return true;

        }
        private async Task ScriptFire()
        {
            await playerActor.Up();
            await playerActor.Right();
            await Task.WhenAll(
                fireballActor.SetVisibility(true),
                fireballActor.SetPosition(playerActor.Position),
                fireballActor.Right(6),
                fireballActor.SetVisibility(false),
                playerActor.Down(3));
            await playerActor.Up(3);
            await playerActor.Left();
            await playerActor.Down();
        }

        private async Task ScriptCircle()
        {
            await playerActor.Up(2);
            await playerActor.Right(2);
            await playerActor.Down(2);
            await playerActor.Left(2);
        }
    }
}
