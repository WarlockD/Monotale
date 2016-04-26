using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using MoonSharp.Interpreter;

namespace MonoUndertale
{
    public class GameObject : IDrawable
    {
        [MoonSharpUserData]
        class AlarmIndexer
        {
            [MoonSharp.Interpreter.Interop.MoonSharpVisible(false)]
            int[] alarm = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
            [MoonSharp.Interpreter.Interop.MoonSharpVisible(false)]
            public ScriptFunctionDelegate[] AlarmDelegates = new ScriptFunctionDelegate[8];
            
            public int this[int idx]
            {
                get { return alarm[idx]; }
                set { alarm[idx] = value; }
            }
            [MoonSharp.Interpreter.Interop.MoonSharpVisible(false)]
            public void Tick(Table self)
            {
                for (int i = 0; i < alarm.Length; i++)
                {
                    if (alarm[i] >= 0)
                    {
                        if (alarm[i] == 0)
                        {
                            alarm[i] = -1;
                            AlarmDelegates[i](self);
                        }
                        else alarm[i]--;
                    }
                }
            }
        }
        Table self;
        static GameObject CreateInstance(UndertaleResources.UObject uobj, float x, float y)
        {

            GameObject o = new GameObject();
            o.Name = uobj.Name;
            o.Index = uobj.Index;
            o.Position = new Vector2(x, y);
            if (uobj.Sprite != null) o.ImageIndex = uobj.Sprite.Index;
            o.Depth = uobj.Depth; //  Global.DepthToMonoDepth(uobj.Depth);
            o.Solid = uobj.Solid;
            o.Visible = uobj.Visible;
            o.Persistant = uobj.Persistent; // not sure how to handle this
            o.Alarms = null;
            if (o.Name == "OBJ_WRITER")
            {
                o.Alarms = new AlarmIndexer();
                //o.self = Global.CreateNewSelf();
              //  UserData.RegisterType<AlarmIndexer>();
                o.self = Global.CallFunction("create_instance", x, y, "OBJ_WRITER") as Table;
                //  UserData data;

                // o.self.Set("alarm", DynValue.NewTable( o.Alarms)); // ["alarm"] = UserData.Create(o.Alarms);
                o.self["alarm"] = o.Alarms;
                Global.DebugRunDelegate("gml_Object_OBJ_WRITER_Create_0", o.self);
                o.DrawDelegate=  Global.GetDelegate("gml_Object_OBJ_WRITER_Draw_0");
                o.UpdateDelegate = Global.GetDelegate("gml_Object_OBJ_WRITER_Step_1");
                o.Alarms.AlarmDelegates[0] = Global.GetDelegate("gml_Object_OBJ_WRITER_Alarm_0");
            }
            Debug.Assert(!o.Persistant);
            // need to handle mask too
            return o;
        }
        public static GameObject CreateInstance(int index, int x, int y)
        {
            UndertaleResources.UObject uobj = UndertaleResources.UndertaleResrouce.Objects[index];
            return CreateInstance(uobj, x, y);
        }
        public static GameObject CreateInstance(string name, int x, int y)
        {
            UndertaleResources.UObject uobj;
            if (UndertaleResources.UndertaleResrouce.TryGetResource(name, out uobj))
                return CreateInstance(uobj, x, y);
            else
                return null;
        }
        protected GameObject() { }
        AlarmIndexer Alarms; 
      
        
        ScriptFunctionDelegate UpdateDelegate = null;
        ScriptFunctionDelegate DrawDelegate = null;
        public string Name = null;
        public int Index = -1;
        public Vector2 Position = Vector2.Zero;
        public Vector2 Size { get { return _size; } }
        public int Depth { get; set; }
        public bool Visible = true;
        public bool Solid = false;
        public bool Persistant = false;
        public Color Color = Color.White;
        bool _persistant = false;
        int _currentFrame = 0;
        int _speed = 0;
        float _direction = 0;
        int _image_speed = 0;
        float _scale = 1.0f;
        Vector2 _movementVector = Vector2.Zero;
        int _current_image_time = 0;
        Vector2 _size = Vector2.Zero;
        Vector2 _scaleVector = new Vector2(1.0f, 1.0f);
        Sprite _sprite = null;
        public float Scale { get { return ScaleVector.X; } set { ScaleVector = new Vector2(value, value); } }
        public float X { get { return Position.X; } set { Position.X = value; } }
        public float Y { get { return Position.Y; } set { Position.Y = value; } }
        public float Width { get { return Size.X; }  }
        public float Height { get { return Size.Y; } }
        public Rectangle BoundingBox { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)(Size.X * ScaleVector.X), (int)(Size.Y * ScaleVector.Y)); } }
        void UpdateObjectSize()
        {
            if (_sprite != null)
            {
                _size = new Vector2(_sprite.Size.X, _sprite.Size.Y);
                _size *= _scaleVector;
            }
            else _size = Vector2.Zero;
        }
        public Sprite Sprite
        {
            get { return _sprite; }
            set
            {
                _sprite = value;
                UpdateObjectSize();
            }
        }
        public Vector2 ScaleVector {
            get { return _scaleVector; }
            set
            {
                Debug.Assert(value.X >= 0.0f && value.Y >= 0.0f);
                _scaleVector = value;
                UpdateObjectSize();
            }
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
        public int ImageIndex
        {
            get { return Sprite == null ? -1 : Sprite.Index; }
            set
            {
                if (value > 0)
                {
                    Sprite = Sprite.LoadSprite(value);
                    _currentFrame = 0; // reset the frame
                }
                else Sprite = null;
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
                if (Sprite != null)
                {
                    if (value > 0 && value < Sprite.Frames.Length) _currentFrame = value;
                }
                else _currentFrame = 0;
            }
        }
        public void NextFrame()
        {
            if (Sprite != null && ++_currentFrame >= Sprite.Frames.Length) _currentFrame = 0;
        }
        public void PreviousFrame()
        {
            if (Sprite != null && --_currentFrame < 0) _currentFrame = Sprite.Frames.Length - 1;
        }
        public virtual void Tick()
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
            if(self != null)
            {
                if(Alarms != null)
                {
                    Alarms.Tick(self);
                }
                if (UpdateDelegate != null) UpdateDelegate(self);
            }
        }
        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                if (DrawDelegate != null) DrawDelegate(self);
                else if (Sprite != null)
                {
                    var f = Sprite.Frames[_currentFrame];
                    batch.Draw(f.Texture, Position, null, f.Origin, null, _direction, ScaleVector, Color, SpriteEffects.None);
                }
            }
        }


        public int CompareTo(IDrawable other)
        {
            return -Depth.CompareTo(other.Depth);
        }
    }
}

