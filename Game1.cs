using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Neo.IronLua;
using MoonSharp.Interpreter;
using System.Diagnostics;

namespace MonoUndertale
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameObject obj_writer;
        Camera2D camera;
        Room test;
        Sprite chara;
        ButtonState leftButtonUp = ButtonState.Released;
        Vector2 mouseDown;
        Vector2 mousePos;
        List<GameObject> mouseOverObjects = new List<GameObject>();
        List<Room.Tile> drawables = new List<Room.Tile>();

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
        /// 
        SpriteFont currentFont=null;
        Color currentFontColor = Color.White;
        Random random = new Random();
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            Global.WhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            Global.WhitePixel.SetData(new[] { Color.White });
            mousePos = new Vector2(graphics.GraphicsDevice.Viewport.
                       Width / 2,
                                    graphics.GraphicsDevice.Viewport.
                                    Height / 2);

            Global.StartUpLua();
            Global.DoFile("scr_gamestart.lua");
            Global.DoFile("scr_text.lua");
            //  var chunk = L.CompileChunk(ProgramSource, "test.lua", new LuaCompileOptions() { DebugEngine = LuaStackTraceDebugger.Default }); // compile the script with debug informations, that is needed for a complete stack trace
       
            //  DG.dochunk(test);
            // G.DoChunk(test);
            Global.L.Globals["draw_set_font"] = new Action<DynValue>((DynValue o) =>
            {
                if(o.Type == DataType.Number)
                {

                }
                currentFont = Content.Load<SpriteFont>("fnt_main");
            });
            Global.L.Globals["random"] = new Func<int,int>((int i) =>
            {
                return random.Next(i);
            });
           Global.L.Globals["draw_text_color"] = new Action<DynValue>((DynValue o) =>
            {
                currentFontColor.PackedValue = (uint)o.Number;
            });
            Global.L.Globals["draw_set_color"] = new Action<DynValue>((DynValue o) =>
            {
                currentFontColor.PackedValue = (uint)o.Number;
            });
            Global.L.Globals["draw_text"] = new Action<float,float,string>((float x, float y, string s) =>
            {
                if (currentFont == null) return;
                if (s == null) return;
                spriteBatch.DrawString(currentFont, s, new Vector2(x, y), currentFontColor);
               // draw_set_font(self.myfont)

              //  draw_set_color(self.mycolor)
            });
            Global.L.Globals["draw_text_ext"] = new Action<object>((object o) =>
            {

            });
            

            camera = new Camera2D();
            camera.Initialize(graphics.GraphicsDevice);
            base.Initialize();
        }
        //int room_index = 34; //306 battle room
        int room_index = 306;
        const string data_win_filename = @"C:\Undertale\UndertaleOld\data.win";
        //  const string data_win_filename = @"D:\Old Undertale\files\data.win";
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Room.debugFont = Content.Load<SpriteFont>("DebugFont");
        
        // TODO: use this.Content to load your game content here


        System.IO.StreamReader sr = new System.IO.StreamReader(data_win_filename);

            UndertaleResources.UndertaleResrouce.LoadResrouces(sr.BaseStream);
            Sprite.LoadTextures(GraphicsDevice, sr.BaseStream);

            sr.Close(); // Shouldn't need anymore
           test = new Room(room_index); // "room_ruins3");
                                        //  test = new Room("room_ruins3"); // "room_ruins3");
                                        // test = new Room(306);  //   306 battle room
                                        //  chara = Sprite.LoadUndertaleSprite(1042);
                                        //  camera.Focus = chara;
                                        //   chara.Position = new Vector2( 50,100);
           
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
        GameObject selected = null;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
            if (test != null) test.Update(gameTime);


            MouseState mstate = Mouse.GetState();
            if(leftButtonUp != mstate.LeftButton)
            {
                if(mstate.LeftButton == ButtonState.Pressed)
                {
                    
                    test.FindObjectAtPoint(mstate.X, mstate.Y, mouseOverObjects);
                }
                leftButtonUp = mstate.LeftButton;
            }
            mousePos.X = mstate.X;
            mousePos.Y = mstate.Y;
            test.FindObjectAtPoint(mstate.X, mstate.Y, mouseOverObjects);
            test.FindAllDrawables(mstate.X, mstate.Y, drawables);
            
            // Update our sprites position to the current cursor 


            KeyboardState state = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            foreach(var k in state.GetPressedKeys())
            {
                if (keysDown.Contains(k)) continue;
                switch (k)
                {
                    case Keys.D1:
                        test = new Room(--room_index);
                        keysDown.Add(k);
                        break;
                    case Keys.D2:
                        test = new Room(++room_index);
                        keysDown.Add(k);
                        break;
                    case Keys.D3:
                        test.DebugObjects = !test.DebugObjects;
                        keysDown.Add(k);
                        break;
                    case Keys.D4:
                        test.DrawObjects = !test.DrawObjects;
                        keysDown.Add(k);
                        break;
                    case Keys.D5:
                        test.DrawTiles = !test.DrawTiles;
                        keysDown.Add(k);
                        break;
                    case Keys.D6:
                        test.DrawBackground = !test.DrawBackground;
                        keysDown.Add(k);
                        break;
                    case Keys.D7:
                        test.DrawForeground = !test.DrawForeground;
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
            int line = 0;
            // TODO: Add your drawing code here
            //      spriteBatch.Begin(SpriteSortMode.BackToFront);
            spriteBatch.Begin(SpriteSortMode.Deferred);
           
            
            if (test != null) test.Draw(spriteBatch);
            spriteBatch.DrawString(Room.debugFont, test.Name + "(" + test.RoomIndex + ")", new Vector2(0, (Room.debugFont.LineSpacing * line++)), Color.White);
            spriteBatch.DrawString(Room.debugFont, "Mouse(" + mousePos + ")", new Vector2(0, (Room.debugFont.LineSpacing * line++)), Color.White);
            if (mouseOverObjects.Count > 0)
            {
                for(int i=0;i < mouseOverObjects.Count; i++)
                {
                    var o = mouseOverObjects[i];
                    spriteBatch.DrawString(Room.debugFont, o.Name + "(" + o.Index + ")", new Vector2(0, (Room.debugFont.LineSpacing * line++)), Color.White);

                }

            }
            if (drawables.Count > 0)
            {
                for (int i = 0; i < drawables.Count; i++)
                {
                    var o = drawables[i];
                    spriteBatch.DrawString(Room.debugFont,"Tile(" + i + ") " + "Rect: " + o.Box +  " Depth: " + o.Depth + " ==", new Vector2(0, (Room.debugFont.LineSpacing * line++)), Color.White);

                }

            }
            Global.DrawRectangle(spriteBatch, new Rectangle((int)mousePos.X, (int)mousePos.Y, 10, 10), leftButtonUp == ButtonState.Released ?   Color.White: Color.Yellow);

            //chara.Draw(spriteBatch);
            // 

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
