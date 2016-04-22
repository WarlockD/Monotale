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
    public class Sprite
    {
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
            for (int i = 0; i < textures.Count; i++) {
                var s = textures[i].GetTextureStream(data_win);
                list.Add(Texture2D.FromStream(device, s));
            }
            textureCache = list.ToArray();
        }
        struct Frame
        {
            public Texture2D texture;
            public Rectangle frame;
        }
        public Vector2 Position = Vector2.Zero;
        private Frame[] Frames;
        Point _size;
        public Vector2 Size 
        {
            get { return new Vector2(_size.X * _scale, _size.Y * _scale); }
        }
        public float Width {  get { return _size.X * _scale; } }
        public float Height { get { return _size.Y * _scale; } }

        int _currentFrame = 0;
        int _speed = 0;
        float _direction = 0;
        int _image_speed = 0;
        float _scale = 1.0f;
        Vector2 _movementVector = Vector2.Zero;
        int _current_image_time = 0;
        float _next_frame = -1;
        public bool Visible = true;
        public Sprite() { Frames = null; }
        public Sprite(int textureIndex, Rectangle frame)
        { // this manualy builds one
            Frames = new Frame[] { new Frame() { texture = textureCache[textureIndex], frame = frame } };
            _size = frame.Size;
        }
        public Sprite(UndertaleResources.SpriteFrame frame)
        { // this manualy builds one
            Frames = new Frame[] { new Frame() { texture = textureCache[frame.TextureIndex], frame = new Rectangle(frame.X, frame.Y, frame.OriginalWidth, frame.OriginalHeight) } };
            _size = new Point(frame.Width, frame.Height);
        }
        public int ImageSpeed
        {
            get { return _image_speed; }
            set
            {
                _image_speed = value;
                _current_image_time = _image_speed;
            }
        }
        public float Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                setMovementVector();
            }
        }
        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                setMovementVector();
            }
        }
        bool CheckFrame(GameTime theGameTime)
        {
            float current = (float)theGameTime.ElapsedGameTime.TotalSeconds;
            if (current > _next_frame)
            {
                _next_frame = current + (1.0f / 30.0f); // Add a 30th of a second
                return true;
            }
            return false;
        }
        void setMovementVector()
        {
            if (_speed == 0) _movementVector = Vector2.Zero;
            else
            {
                _movementVector.X = (float)Math.Cos(Math.PI * _direction / 180.0) * _speed;
                _movementVector.Y = (float)Math.Sin(Math.PI * _direction / 180.0) * _speed;
            }
        }
        public int CurrentFrame
        {
            get { return _currentFrame; }
            set
            {
                if (value > 0 && value < Frames.Length) _currentFrame = value;
            }
        }
        public void NextFrame()
        {
            if (++_currentFrame >= Frames.Length) _currentFrame = 0;
        }
        public void PreviousFrame()
        {
            if (--_currentFrame < 0) _currentFrame = Frames.Length - 1;
        }
        static Sprite LoadUndertaleSprite(UndertaleResources.Sprite rawSprite)
        {
            Sprite sprite = new Sprite();
            sprite.Frames = new Frame[rawSprite.Frames.Length];
            for (int i = 0; i < rawSprite.Frames.Length; i++)
            {
                var frame = rawSprite.Frames[i];
                sprite.Frames[i].frame = new Rectangle(frame.X, frame.Y, frame.OriginalWidth, frame.OriginalHeight);
                sprite.Frames[i].texture = textureCache[frame.TextureIndex];
            }
            var f = sprite.Frames[0];
            sprite._size = new Point(f.frame.Width, f.frame.Height);
            sprite.Position = Vector2.Zero;
            sprite.CurrentFrame = 0; // sets it up
            return sprite;
        }
        public static Sprite LoadUndertaleSprite(string name)
        {
            UndertaleResources.Sprite rawSprite;
            if (!UndertaleResources.UndertaleResrouce.TryGetResource(name, out rawSprite))
            {
                throw new Exception("Coudln't find sprite");
            }
            return LoadUndertaleSprite(rawSprite);
        }

        public static Sprite LoadUndertaleSprite(int index)
        {
            UndertaleResources.Sprite rawSprite = UndertaleResources.UndertaleResrouce.SpriteAtIndex(index);
            return LoadUndertaleSprite(rawSprite);
        }
        public void Update(GameTime theGameTime)
        {
            if (CheckFrame(theGameTime))
            {
                Position += _movementVector;
                if (_image_speed != 0 && _current_image_time == 0)
                {
                    if (_current_image_time == 0)
                    {
                        _current_image_time = Math.Abs(_image_speed); // reset the image speed
                        if (_image_speed > 0) NextFrame();
                        else PreviousFrame();
                    }
                    else _current_image_time--;
                }
            }
        }
        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                Frame f = Frames[_currentFrame];
                batch.Draw(f.texture, Position, f.frame, Color.White, _direction, Vector2.Zero, _scale, SpriteEffects.None, 0.0f);
            }
        }
    }
}
