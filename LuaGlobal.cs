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
using MoonSharp.RemoteDebugger;
using Microsoft.Xna.Framework.Audio;
using MoonSharp.Interpreter.Loaders;
//using Neo.IronLua;


namespace MonoUndertale
{
    public static class UndertaleScript
    {
        private static RemoteDebuggerService remoteDebugger = null;
        public static Script L;
        public static DynValue global;
        public const string ScriptPath = @"..\..\..\..\scripts\";

        static void Lua_snd_stop(object o) { Global.StopSound(o); }
        static object Lua_snd_play(object o) { return Global.PlaySound(o); }
        static GameObject lua_create_instance(float x, float y, DynValue value)
        {
            GameObject obj;
            if (value.Type == DataType.Number) obj = Global.CurrentRoom.CreateInstance(x, y, (int) value.Number);
            else if (value.Type == DataType.String) obj = Global.CurrentRoom.CreateInstance(x, y,value.String);
            else throw new Exception("Bad value");

            return obj;
        }

        // teting moonsharp stuff
       // [MoonSharpUserData]
        class TestObject : MoonSharp.Interpreter.Interop.IUserDataType
        {
            Table self = null;
            float _x = 0, _y = 0;
            public float x {  get { return _x; } set { _x = value; } }
            public float y { get { return _y; } set { _y = value; } }
            public DynValue Get(string s)
            {
                return self.Get(s);
            }
            public void Set(string s, float v)
            {
                self[s] = v;
            }
            public void Set(string s, string v)
            {
                self[s] = v;
            }
            public TestObject(Script script) { self = new Table(script); }
            public DynValue Index(Script script, DynValue index, bool isDirectIndexing)
            {
                if(index.Type == DataType.String)
                {
                    if (isDirectIndexing)
                    {
                        switch (index.String)
                        {
                            case "x": return DynValue.NewNumber(_x);
                            case "y": return DynValue.NewNumber(_y);
                        }
                    }
                    if (self == null) self = new Table(script);
                    return self.Get(index);
                }
                throw new NotImplementedException();
            }

            public DynValue MetaIndex(Script script, string metaname)
            {
                Debug.WriteLine("MetaBad: " + metaname);
                return null;
                //throw new NotImplementedException();
            }

            public bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing)
            {
                if (index.Type == DataType.String)
                {
                    if (isDirectIndexing)
                    {
                        switch (index.String)
                        {
                            case "x": _x = (float)value.Number; return true;
                            case "y": _y = (float) value.Number; return true;
                        }
                    }
                    if (self == null) self = new Table(script);
                    self.Set(index,value);
                    return true;
                }
                return false;
            }
            public override string ToString()
            {
                return "{" + _x + " , " + _y + "}";
            }
        }
        static void TestObjecthandling()
        {
            string code =
@"
extensibleObjectMeta = {
    __index = function(t,k)
        local obj = rawget(t,""_gameobject"");
        if(obj) then return obj[k] end
    end,
    __newindex  = function(t,k,v)
        local obj = rawget(t,""_gameobject"");
        if(obj) then obj[k] = v end
    end
}
-- create a new wrapped object called myobj, wrapping the object o
function create_instance(x,y, o)
    local self = { _gameobject = o }
    setmetatable(self, extensibleObjectMeta);
    self.x = 5
    self.y = 3
    self.test = 5
    return self
end
function test_object(self)
    local o = """"
    self.y = 39493
    self.test = 343
    o = o .. self.FUCK
    o = o .. ""    "" .. self.x  .. ""     ""  .. self.y
    return o 
end
";
            try
            {
               

                Script script = new Script();
                UserData.RegisterType<TestObject>();
                StartDebugger(script, false);
                script.DoString(code);
                TestObject test = new TestObject(script);
                script.Globals["test"] = test;
                test.x = 1;
                test.y = 2;
                test.Set("FUCK", "happy fuck");
                // var create_instance = script.Globals.Get("create_instance").Function.GetDelegate();
                var test_object = script.Globals.Get("test_object").Function.GetDelegate();
                
                string msg = test_object(test) as string;
                Debug.WriteLine("Humm  " + test.Get("test").ToString());
                Debug.WriteLine("Humm  " + msg);
                Debug.WriteLine("Humm");
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
        }

        static void lua_destory_instance(DynValue value)
        {
            switch (value.Type)
            {
                case DataType.String:
                    Global.CurrentRoom.DestoryInstance(value.String);
                    break;
                case DataType.Number:
                    Global.CurrentRoom.DestoryInstance((int)value.Number);
                    break;
                case DataType.UserData:
                    GameObject gobj = value.UserData.Object as GameObject;
                    Debug.Assert(gobj != null);
                    Global.CurrentRoom.DestoryInstance(gobj);
                    break;
                default:
                    throw new Exception("bad value");
            }
        }
        public static void StartDebugger(Script script,bool willstart=true)
        {

            if (remoteDebugger == null)
            {
                remoteDebugger = new RemoteDebuggerService();

                // the last boolean is to specify if the script is free to run 
                // after attachment, defaults to false
                remoteDebugger.Attach(script, "Description of the script", willstart);

                // start the web-browser at the correct url. Replace this or just
                // pass the url to the user in some way.
                Process.Start(remoteDebugger.HttpUrlStringLocalHost);
            }
        }
        private class MyCustomScriptLoader : ScriptLoaderBase
        {
            public override object LoadFile(string file, Table globalContext)
            {
                Debug.WriteLine(string.Format("print ([[A request to load '{0}' has been made]])", file));
                if (System.IO.File.Exists(file)) return System.IO.File.OpenRead(file);
                if(System.IO.File.Exists(ScriptPath + file)) return System.IO.File.OpenRead(ScriptPath + file);
                return null;
            }
            public override string ResolveModuleName(string modname, Table globalContext)
            {
                Debug.WriteLine(string.Format("print ([[A request to load module '{0}' has been made]])", modname));
                if (!modname.Contains(".lua")) modname += ".lua";
                if (System.IO.File.Exists(modname)) return modname;
                else return ScriptPath + modname;
            }
            public override bool ScriptFileExists(string name)
            {
                if (System.IO.File.Exists(name) || System.IO.File.Exists(ScriptPath + name)) return true;
                return true;
            }
        }
        public static void StartUpLua()
        {
            UserData.RegisterAssembly();
            UserData.RegisterType<GameObject>();
            UserData.RegisterType<GameObject.AlarmIndexer>();
            UserData.RegisterType<StupidArray>();
            //   TestObjecthandling();
            Script script = new Script();
          //  string[] paths = new string[] { ScriptPath + "/?", ScriptPath + "/?.lua" };
            script.Options.ScriptLoader = new MyCustomScriptLoader();
         //   ((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths= paths;// = new string[] { "MyPath/?", "MyPath/?.lua" };
            
            // ScriptPath
            L = script;
            global = DynValue.NewPrimeTable();
            L.Globals["global"] = global;
            L.Globals["print"] = new Action<string>((string msg) =>
            {
                Debug.WriteLine(msg);
            });
            L.Globals["keyboard_multicheck_pressed"] = new Func<DynValue, bool>((DynValue key) =>
            {
                char c = key.Type == DataType.String ? key.String[0] : (char) ((int) key.Number);
                if (c == 0)
                {
                    var state = Keyboard.GetState();
                    Keys[] keys = state.GetPressedKeys();
                    return keys.Length > 0;
                }
                return false;
            });
            L.Globals["snd_stop"] = new Action<object>(Lua_snd_stop);
            L.Globals["snd_play"] = new Func<object, object>(Lua_snd_play);
            L.Globals["snd_play"] = new Func<object, object>(Lua_snd_play);
            L.Globals["instance_destroy"] = new Action<DynValue>(lua_destory_instance);
            L.Globals["create_instance"] = new Func<float,float,DynValue, GameObject>(lua_create_instance);
        //    StartDebugger(script);

        }
        public static ScriptFunctionDelegate GetDelegate(string function)
        {
            try
            {
                L.DoFile(ScriptPath + function + ".lua");
                var del = L.Globals.Get(function).Function;
                if (del == null)
                {
                    L.DoFile(ScriptPath + function + ".lua");
                    del = L.Globals.Get(function).Function;
                    if (del == null) throw new Exception("Cannot find delegate");
                }
                return del.GetDelegate();
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
        public static object TryDelegate(ScriptFunctionDelegate del, params object[] args)
        {
            try
            {
                return del(args);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
        public static object DebugRunDelegate(string function, params object[] args)
        {

            try
            {
                return GetDelegate(function)(args);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
        public static DynValue DoFile(string filename)
        {

            //   Global.G.DoChunk(ScriptPath + filename, new KeyValuePair<string, object>("self", self));
            try
            {
                return L.DoFile(ScriptPath + filename);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
    }
}
