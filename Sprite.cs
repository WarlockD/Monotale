using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace MonoUndertale
{
    public interface IDrawable : IComparable<IDrawable>
    {
        int Depth { get; }
        void Draw(SpriteBatch batch);
    }
    public class Sprite : IEquatable<Sprite>
    {

        public struct Frame // Heap or stack?  Humm
        {
            public Rectangle Origin;
            public Texture2D Texture;
        }
        public string Name { get; private set; }
        public int Index { get; private set; }
        public Frame[] Frames { get; private set; }
        public Point Size { get { return Frames[0].Origin.Size; } }
        Sprite() { Name = null;  Index = -1; Frames = null; }
        public bool Equals(Sprite other)
        {
            return Name == other.Name;
        }
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;
            if (object.ReferenceEquals(obj, this)) return true;
            Sprite test = obj as Sprite;
            return test != null && Equals(test);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        static Dictionary<int, Sprite> IndexOfSprites = new Dictionary<int, Sprite>();
        static Dictionary<string, Sprite> SpriteMap = new Dictionary<string, Sprite>();
        static Texture2D[] textureCache = null;
        public static Texture2D TextureAtIndex(int i)
        {
            return textureCache[i];
        }
        public static void LoadTextures(GraphicsDevice device, System.IO.Stream data_win)
        {
            if (textureCache != null) return;
            List<Texture2D> list = new List<Texture2D>();
            var textures = UndertaleResources.UndertaleResrouce.Textures;
            for (int i = 0; i < textures.Count; i++)
            {
                var s = textures[i].GetTextureStream(data_win);
                list.Add(Texture2D.FromStream(device, s));
            }
            textureCache = list.ToArray();
        }
        public static Frame CreateFrameFromFrame(UndertaleResources.SpriteFrame frame)
        {
            Frame ret = new Frame();
            ret.Texture = textureCache[frame.TextureIndex]; //+frame.OffsetX + frame.OffsetY
            ret.Origin = new Rectangle(frame.X , frame.Y , frame.Width, frame.Height);
            return ret;
        }
        static Sprite CacheSprite(UndertaleResources.Sprite sprite)
        {
            Sprite s = new Sprite();
            s.Name = sprite.Name;
            s.Index = sprite.Index;
            s.Frames = new Frame[sprite.Frames.Length];
            for (int i = 0; i < sprite.Frames.Length; i++)
                s.Frames[i] = CreateFrameFromFrame(sprite.Frames[i]);
            SpriteMap.Add(s.Name, s);
            IndexOfSprites.Add(s.Index, s);
            return s;
        }
        public static Sprite LoadSprite(int index)
        {
            Sprite s;
            if (IndexOfSprites.TryGetValue(index, out s)) return s;
            UndertaleResources.Sprite raw = UndertaleResources.UndertaleResrouce.Sprites[index];
            return CacheSprite(raw);
        }
        public static Sprite LoadSprite(string name)
        {
            Sprite s;
            if (SpriteMap.TryGetValue(name, out s)) return s;
            UndertaleResources.Sprite raw;
            if (!UndertaleResources.UndertaleResrouce.TryGetResource(name, out raw)) throw new Exception("Cannot find sprite");
            return CacheSprite(raw);
        }

    }

}

