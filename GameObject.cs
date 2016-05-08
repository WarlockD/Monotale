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
using MoonSharp.Interpreter.Interop;

namespace MonoUndertale
{
    [MoonSharpUserData]
    public class StupidArray 
    {
        Dictionary<int, int> array = new Dictionary<int, int>();

        public int this[int i]
        {
            get
            {
                int v;
                if (@array.TryGetValue(i, out v)) array[i] = v = 0;
                return v;
            }
            set
            {

                array[i] = value;
            }
        }
    }
    public class GameObject :  IUserDataType, IDrawable
    {

        public class AlarmIndexer : IUserDataType
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
            public void Tick(GameObject self)
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
           
            public DynValue Index(Script script, DynValue index, bool isDirectIndexing)
            {
                if (!isDirectIndexing && index.Type == DataType.Number)
                {
                    return DynValue.NewNumber( alarm[(int) index.Number]);
                }
                throw new NotImplementedException();
            }

            public bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing)
            {
                if (!isDirectIndexing && index.Type == DataType.Number && value.Type == DataType.Number)
                {
                    alarm[(int) index.Number] = (int) value.Number;
                    return true;
                }
                throw new NotImplementedException();
                return false;
            }

            public DynValue MetaIndex(Script script, string metaname)
            {
                return null; // nothing else is supported
            }
        }
        Dictionary<string, DynValue> self = new Dictionary<string, DynValue>();
        static readonly Dictionary<string, Func<GameObject, DynValue>> objectIndexers = new Dictionary<string, Func<GameObject, DynValue>>();
        static readonly Dictionary<string, Action<GameObject, DynValue>> obectAssigners = new Dictionary<string, Action<GameObject, DynValue>>();
        static GameObject()
        {
            objectIndexers["x"] = (GameObject o)=>   DynValue.NewNumber(o.X);
            objectIndexers["y"] = (GameObject o) => DynValue.NewNumber(o.Y);
            objectIndexers["hspeed"] = (GameObject o) => DynValue.NewNumber(o.HSpeed);
            objectIndexers["vspeed"] = (GameObject o) => DynValue.NewNumber(o.VSpeed);
            objectIndexers["direction"] = (GameObject o) => DynValue.NewNumber(o.Direction);
            objectIndexers["speed"] = (GameObject o) => DynValue.NewNumber(o.Direction);
            objectIndexers["depth"] = (GameObject o) => DynValue.NewNumber(o.Depth);
            objectIndexers["alarm"] = (GameObject o) => UserData.Create(o.Alarms);
            objectIndexers["view_current"] = (GameObject o) => DynValue.NewNumber(Global.CurrentRoom.current_view);
        //    objectIndexers["view_xview"] = (GameObject o) => UserData.Create(Global.CurrentRoom.xview);
         //   objectIndexers["view_yview"] = (GameObject o) => UserData.Create(Global.CurrentRoom.yview);

            obectAssigners["x"] = (GameObject o, DynValue v) => o.X = (float)v.Number;
            obectAssigners["y"] = (GameObject o, DynValue v) => o.Y = (float)v.Number;
            obectAssigners["hspeed"] = (GameObject o, DynValue v) => o.HSpeed = (float) v.Number;
            obectAssigners["vspeed"] = (GameObject o, DynValue v) => o.VSpeed = (float) v.Number;
            obectAssigners["direction"] = (GameObject o, DynValue v) => o.Direction = (float) v.Number;
            obectAssigners["speed"] = (GameObject o, DynValue v) => o.Speed = (float) v.Number;
            obectAssigners["depth"] = (GameObject o, DynValue v) => o.VSpeed = (float) v.Number;
            obectAssigners["alarm"] = (GameObject o, DynValue v) => Debug.WriteLine("alarm was being set?");
            obectAssigners["view_current"] = (GameObject o, DynValue v) => Global.CurrentRoom.current_view = (int) v.Number;

            objectIndexers["view_current"] = (GameObject o) => DynValue.NewNumber(Global.CurrentRoom.current_view);
        }
        DynValue IUserDataType.Index(Script script, DynValue index, bool isDirectIndexing)
        {

            if (isDirectIndexing && index.Type == DataType.String)
            {
                Func<GameObject, DynValue> func;
                if (objectIndexers.TryGetValue(index.String, out func)) return (DynValue) func(this);
                DynValue v;
                if (!self.TryGetValue(index.String, out v))
                { // return a table as at this point it might be an array
                    
                    v = DynValue.NewPrimeTable();
                    // hack here
                    for (int i = 0; i < 20; i++) v.Table.Set(DynValue.NewNumber(i), DynValue.NewNumber(0));
                 //   if (index.String == "view_xview")
                        self[index.String] = v;

                }
                return v;
            }
            throw new NotImplementedException();
            return null;
        }

        public bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing)
        {
            Debug.Assert(index.String != "view_xview");
            if (isDirectIndexing && index.Type == DataType.String)
            {
                DynValue v;
                Action<GameObject, DynValue> func;
                if (obectAssigners.TryGetValue(index.String, out func))  func(this,value);
                else
                    self[index.String] = value;
                return true;
            }
            throw new NotImplementedException();
            return false;
        }

        public DynValue MetaIndex(Script script, string metaname)
        { 
            throw new NotImplementedException();
        }
        public static GameObject CreateInstance(UndertaleResources.UObject uobj, float x, float y)
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
            o.Alarms = new AlarmIndexer();
            if (o.Name == "OBJ_WRITER")
            {
                
                o.DrawDelegate= UndertaleScript.GetDelegate("gml_Object_OBJ_WRITER_Draw_0");
                o.UpdateDelegate = UndertaleScript.GetDelegate("gml_Object_OBJ_WRITER_Step_1");
                o.StartUpdateDelegate = UndertaleScript.GetDelegate("gml_Object_OBJ_WRITER_Step_0");
                o.Alarms.AlarmDelegates[0] = UndertaleScript.GetDelegate("gml_Object_OBJ_WRITER_Alarm_0");
       //         UndertaleScript.StartDebugger(UndertaleScript.L,false);
                var value = UndertaleScript.L.DoString("return GetSprite('spr_brattyface',0)");
                UndertaleScript.DebugRunDelegate("gml_Object_OBJ_WRITER_Create_0", UserData.Create(o));
            }
            o.Create();
            Debug.Assert(!o.Persistant);
            // need to handle mask too
            return o;
        }
        protected virtual void Create() { }
        public GameObject CreateInstance(int x, int y, int index)
        {
            return this.Room.CreateInstance(x, y, index);
        }
        public GameObject CreateInstance(int x, int y, string name)
        {
            return this.Room.CreateInstance(x, y, name);
        }
        public bool InstanceExists(string name)
        {
            return this.Room.InstanceExists(name);
        }
        public void DestoryInstance()
        {
            this.Room.DestoryInstance(this);
        }
        protected GameObject() { }
        protected AlarmIndexer Alarms;

        ScriptFunctionDelegate StartUpdateDelegate = null;
        ScriptFunctionDelegate UpdateDelegate = null;
        ScriptFunctionDelegate DrawDelegate = null;
        public string Name = null;
        public int Index = -1;

        public Vector2 Position { get { return _position; } set { _position = value; } }
        public Vector2 Size { get { return _size; } }
        public Room Room = null;
        
        public int Depth { get; set; }
        public bool Visible = true;
        public bool Solid = false;
        public bool Persistant = false;
        public Color Color = Color.White;
        bool _persistant = false;
        int _currentFrame = 0;
        float _speed = 0;
        float _direction = 0;
        int _image_speed = 0;
        float _scale = 1.0f;
        Vector2 _movementVector = Vector2.Zero;
        int _current_image_time = 0;
        Vector2 _position = Vector2.Zero;
        Vector2 _size = Vector2.Zero;
        Vector2 _scaleVector = new Vector2(1.0f, 1.0f);
        Sprite _sprite = null;
        public float Scale { get { return ScaleVector.X; } set { ScaleVector = new Vector2(value, value); } }
        public float X { get { return _position.X; } set { _position.X = value; } }
        public float Y { get { return _position.Y; } set { _position.Y = value; } }
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
                if (_speed == 0.0f) _movementVector = Vector2.Zero;
                else
                {
                    _movementVector.X = (float) Math.Cos(Math.PI * _direction / 180.0) * _speed;
                    _movementVector.Y = (float) Math.Sin(Math.PI * _direction / 180.0) * _speed;
                }
            }
        }
        public float Speed
        {
            get { return _speed; }
            set
            {
                _speed = value; 
                if (_speed == 0.0f) _movementVector = Vector2.Zero;
                else
                {
                    _movementVector.X = (float) Math.Cos(Math.PI * _direction / 180.0) * _speed;
                    _movementVector.Y = (float) Math.Sin(Math.PI * _direction / 180.0) * _speed;
                }
            }
        }
        public float HSpeed
        {
            get { return _movementVector.X; }
            set
            {
                _movementVector.X = value;
                _direction = (float) (Math.Atan2((float) _movementVector.X, (float) _movementVector.Y) / Math.PI * 180);
                _speed = (float) Math.Sqrt(_movementVector.X * _movementVector.X + _movementVector.Y * _movementVector.Y);
            }
        }
        public float VSpeed
        {
            get { return _movementVector.Y; }
            set
            {
                _movementVector.Y = value;
                _direction = (float) (Math.Atan2((float) _movementVector.X, (float) _movementVector.Y) / Math.PI * 180);
                _speed = (float)Math.Sqrt(_movementVector.X * _movementVector.X + _movementVector.Y * _movementVector.Y);
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
        public virtual void BeginStep() { }
        public virtual void Step() { }
        public virtual void EndStep() { }

        public void Tick()
        {
            BeginStep();
            if(StartUpdateDelegate != null) StartUpdateDelegate(this);

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
            Step();
            Alarms.Tick(this);
            if (UpdateDelegate != null) UpdateDelegate(this);
            

            EndStep();
        }
        protected virtual void InternalDraw(SpriteBatch batch) { }
        public void Draw(SpriteBatch batch)
        {
            if (Visible)
            {
                if (DrawDelegate != null) UndertaleScript.TryDelegate(DrawDelegate,self);

                else if (Sprite != null)
                {
                    var f = Sprite.Frames[_currentFrame];
                    batch.Draw(f.Texture, Position, null, f.Origin, null, _direction, ScaleVector, Color, SpriteEffects.None);
                }
                InternalDraw(batch);
            }
        }


        public int CompareTo(IDrawable other)
        {
            return -Depth.CompareTo(other.Depth);
        }

     
    }
}

