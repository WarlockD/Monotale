using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoUndertale
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Room test;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }
        int room_index = 30;
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            System.IO.StreamReader sr = new System.IO.StreamReader(@"D:\Old Undertale\files\data.win");

            UndertaleResources.UndertaleResrouce.LoadResrouces(sr.BaseStream);
            Sprite.LoadTextures(GraphicsDevice, sr.BaseStream);

            sr.Close(); // Shouldn't need anymore
            test = new Room(room_index); // "room_ruins3");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        HashSet<Keys> keysDown = new HashSet<Keys>();
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            foreach(var k in state.GetPressedKeys())
            {
                if (keysDown.Contains(k)) continue;
                switch (k)
                {
                    case Keys.NumPad1:
                        test = new Room(--room_index);
                        keysDown.Add(k);
                        break;
                    case Keys.NumPad3:
                        test = new Room(++room_index);
                        keysDown.Add(k);
                        break;
                }
            }
            keysDown.RemoveWhere(x => state.IsKeyUp(x));

   
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            test.Draw(spriteBatch);
           // spriteBatch.Draw(background, new Rectangle(0, 0, 800, 480), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
