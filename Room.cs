using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UndertaleResources;

namespace MonoUndertale
{
   
    class Room
    {
        public class Tile : IDrawable
        {
            public Vector2 Position = Vector2.Zero;
            public Sprite.Frame Frame;
            public int Depth { get; set;  }
            public Vector2 Speed = new Vector2(0.0f, 0.0f);
            public Vector2 Scale = new Vector2(1.0f, 1.0f);
            public Color Blend = Color.White;
            public Color Color = Color.White;
            public Rectangle Box {  get { return new Rectangle(Position.ToPoint(), Frame.Origin.Size); } }
            public bool Visiable = true;
            public void Draw(SpriteBatch batch)
            {
                if (Visiable)
                batch.Draw(Frame.Texture, Position, null,Frame.Origin,null, 0.0f, Scale, Color,  SpriteEffects.None);
            }

            public int CompareTo(IDrawable other)
            {
                return -Depth.CompareTo(other.Depth);
            }
        }
        float _next_frame = -1;
        public float UpdatesPerSecond = 30.0f;
        bool CheckFrame(GameTime theGameTime)
        {
            float current = (float)theGameTime.ElapsedGameTime.TotalSeconds;
            if (current > _next_frame)
            {
                _next_frame = current + (1.0f / UpdatesPerSecond); 
                return true;
            }
            return false;
        }
        public string Name { get; private set; }
        public int RoomIndex { get; private set; }
        public Rectangle BondingBox;
        Tile[] _tiles;
        Tile[] _backgrounds;
        Tile[] _foregrounds;
        List<GameObject> _objects;
        List<IDrawable> _drawList;
        public GameObject FindObjectAtPoint(float x, float y)
        {
            return _objects.SingleOrDefault(o => o.BoundingBox.Contains(x, y));
        }
        public void FindObjectAtPoint(float x, float y, List<GameObject> list)
        {
            list.Clear();
            list.AddRange(_objects.Where(o => o.BoundingBox.Contains(x, y)));
        }
        public void FindAllDrawables(float x, float y, List<Tile> list)
        {
            list.Clear();
            list.AddRange(_tiles.Where(o => o.Box.Contains(x, y)));
        }
        Color _backgroundColor;
        void LoadRoom(UndertaleResources.Room rawRoom)
        {
            this.RoomIndex = rawRoom.Index;
            this.Name = rawRoom.Name;
            BondingBox.Location = Point.Zero;
            BondingBox.Size = new Point(rawRoom.Width, rawRoom.Height);
            _backgroundColor = new Color((byte)(rawRoom.Colour>>16), (byte)(rawRoom.Colour>>8),(byte)rawRoom.Colour);
            List<Tile> backgrounds = new List<Tile>();
            List<Tile> foregrounds = new List<Tile>();
            List<Tile> tiles = new List<Tile>();
            foreach (var b in rawRoom.Backgrounds)
            {
                if (b.BackgroundIndex < 0) continue;
                var tile = UndertaleResrouce.Backgrounds[b.BackgroundIndex];
                Tile t = new Tile();
                t.Frame = Sprite.CreateFrameFromFrame(tile.Frame);
                t.Position = new Vector2(b.X, b.Y);
                t.Speed = new Vector2(b.SpeedX, b.SpeedY);
                t.Visiable = b.Visible;
                if (b.Foreground)
                {
                    foregrounds.Add(t);
                    t.Depth = -1000001;
                }
                else {
                    backgrounds.Add(t);
                    t.Depth = 1000001;
                }
            }
            _backgrounds = backgrounds.ToArray();
            _foregrounds = foregrounds.ToArray();
            foreach (var rawTile in rawRoom.Tiles)
            {
                var bgn = UndertaleResrouce.BackgroundAtIndex(rawTile.BackgroundIndex);
                Tile t = new Tile();
              
                t.Frame = Sprite.CreateFrameFromFrame(bgn.Frame);
                Rectangle newOrigin = new Rectangle(
                    t.Frame.Origin.X + rawTile.OffsetX,
                    t.Frame.Origin.Y + rawTile.OffsetY,
                    rawTile.Width, 
                    rawTile.Height
                    );
                t.Frame.Origin = newOrigin;
                t.Position = new Vector2(rawTile.X, rawTile.Y);
               t.Scale = new Vector2(rawTile.ScaleX, rawTile.ScaleY);
                t.Color = Color.White;
                if (rawTile.Ocupancy == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Need Ocupancy");
                }
                if (rawTile.Ocupancy >= 0)
                    t.Color *= (rawTile.Ocupancy / 256);
                else
                    t.Color = Color.White;
                t.Blend.PackedValue = (uint)rawTile.Blend;
                if (rawTile.Blend != 16777215)
                {
                    System.Diagnostics.Debug.WriteLine("Need Blend");
                }
                
                System.Diagnostics.Debug.Assert(rawTile.Depth != 10000000); // wierd?

                t.Depth = rawTile.Depth; // Global.DepthToMonoDepth(  rawTile.Depth);
                tiles.Add(t);
            }
            tiles.Sort();
            foreach (var t in tiles) System.Diagnostics.Debug.WriteLine("Tile Depth: " + t.Depth);
            _tiles = tiles.ToArray();
            _objects = new List<GameObject>();
            foreach(var oo in rawRoom.Objects)
            {
                if (oo.Index > 0)
                {
                    GameObject o = GameObject.CreateInstance(oo.ObjectIndex, oo.X, oo.Y);
                   
                    o.Direction = oo.Rotation;
                    o.ScaleVector = new Vector2(oo.Scale_X, oo.Scale_Y);
                    o.Color.PackedValue = (uint)oo.Colour;
                    if(o.Color != Color.White)
                    {
                        throw new Exception("Humm wierd");
                    }
                    if (o.Width > 2000 || o.Height > 2000) throw new Exception("Uhg");
                    //  oo.Colour hummm
                    _objects.Add(o);
                }
            }
            _objects.Sort();

            using (var s = new System.IO.StreamWriter("debug_room_objects.txt"))
            {
                foreach (var o in _objects)
                {
                    s.WriteLine(o.Name);
                }
            }
        }
        public Room(string name)
        {
            UndertaleResources.Room rawRoom;
          
            if (!UndertaleResrouce.TryGetResource(name, out rawRoom)) throw new Exception("Cannot find room");
            LoadRoom(rawRoom);
        }
        public Room(int index)
        {
            LoadRoom(UndertaleResrouce.RoomAtIndex(index));
        }
        public void Update(GameTime time)
        {
            if (CheckFrame(time))
            {
                foreach (var b in _objects) b.Tick();
            }
            
        }
        public bool DebugObjects = false;
        public bool DebugTile = true;
        public bool DrawTiles = true;
        public bool DrawObjects = true;
        public bool DrawForeground = true;
        public bool DrawBackground = true;
        public static SpriteFont debugFont;
        public void DrawTextCenter(SpriteBatch batch,string text,Vector2 pos, Vector2 size)
        {
            //Vector2 p = new Vector2(size.X / 2 - debugFont.MeasureString(text).Length() / 2, size.Y/2);
            batch.DrawString(debugFont, text, pos, Color.White);
        }

        public void Draw(SpriteBatch batch)
        {
            Global.DrawRectangle(batch, BondingBox, this._backgroundColor);
            if(DrawBackground) foreach(var b in _backgrounds) b.Draw(batch);
            if (_drawList == null) _drawList = new List<IDrawable>(100);
            else _drawList.Clear();
           if(DrawTiles)  _drawList.AddRange(_tiles); // we need to sort them with the tiles
           // foreach (var b in _tiles) b.Draw(batch);
            if (DrawObjects)  _drawList.AddRange(_objects);
            _drawList.Sort();

           foreach (var b in _drawList)
            {
                if (DebugObjects)
                {
                    GameObject o = b as GameObject;
                    if(o != null)
                    {
                        bool save = o.Visible;
                        o.Visible = true;
                        b.Draw(batch);
                        o.Visible = save;
                    }
                    else b.Draw(batch);
                }
                else b.Draw(batch);

            }
            if(DrawForeground) foreach(var b in _foregrounds) b.Draw(batch);
        }
    }
}
