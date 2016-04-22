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
        public class Tile
        {
            public Vector2 Position = new Vector2();
            public Texture2D Texture;
            public Rectangle Rect = new Rectangle();
            public void Draw(SpriteBatch batch)
            {
                batch.Draw(Texture, Position, Rect, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            }
        }
        Tile[] _tiles;
        Sprite[] _backgrounds;
        Sprite[] _foregrounds;


        void LoadRoom(UndertaleResources.Room rawRoom)
        {
            List<Sprite> backgrounds = new List<Sprite>();
            List<Sprite> foregrounds = new List<Sprite>();
            List<Tile> tiles = new List<Tile>();
            foreach (var b in rawRoom.Backgrounds)
            {
                if (b.Index < 0) continue;
                var tile = UndertaleResrouce.BackgroundAtIndex(b.Index);
                Sprite sprite = new Sprite(tile.Frame);
                sprite.Position = new Vector2(b.X, b.Y);
                sprite.Speed = b.SpeedX;
                if (b.Foreground) foregrounds.Add(sprite); else backgrounds.Add(sprite);
            }
            _backgrounds = backgrounds.ToArray();
            _foregrounds = foregrounds.ToArray();
            foreach (var tt in rawRoom.Tiles)
            {
                var bgn = UndertaleResrouce.BackgroundAtIndex(tt.Index);
                Tile t = new Tile();
                t.Texture = Sprite.TextureAtIndex(bgn.Frame.TextureIndex);
                t.Rect = new Rectangle(bgn.Frame.X, bgn.Frame.Y, bgn.Frame.Width, bgn.Frame.Height);
                t.Rect.Location += new Point(tt.OffsetX, tt.OffsetY);
                t.Rect.Size = new Point(tt.Width, tt.Height);
                t.Position = new Vector2(tt.X, tt.Y);
                tiles.Add(t);
            }
            _tiles = tiles.ToArray();
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
        public void Draw(SpriteBatch batch)
        {
            foreach (var b in _backgrounds) b.Draw(batch);
            foreach (var b in _tiles) b.Draw(batch);
            foreach (var b in _foregrounds) b.Draw(batch);
        }
    }
}
