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
//using Neo.IronLua;

namespace MonoUndertale
{
    // Undertale uses this big global flags for alot of its stuff
    // before I seeprate eveything out I am putting it here just so stuff works

    public static class Global
    {
        public static Texture2D WhitePixel;
        public static Color IntToColor(int c)
        {
            return new Color((byte)(c>>24), (byte)(c >> 16), (byte)(c >> 8), (byte)c);

        }
        public static void DrawRectangle(SpriteBatch batch, Rectangle rect, Color color)
        {
            batch.Draw(WhitePixel, rect, color);
        }
        // Option One (if you have integer size and coordinates)
        // Game maker defines depth to be anything, but we need to make sure its between 0.0 and 1.0f
        const float MaxDepth = 1000000; // seriously? A million?
        public static float DepthToMonoDepth(int depth)
        {
            Debug.Assert(Math.Abs(depth) <= MaxDepth);
            if (depth == 0) return 0.5f;
            else if (depth == MaxDepth) return 0.00001f;// make sure its above the background layer
            else if(depth == -MaxDepth) return 0.99999f; // .. but not over the foreground layer
            else
            {
                float ret = (((float)(-depth) / MaxDepth) / 2); // hacky
                return 0.5f + ret;
            }
        }
        //  public static Lua L;
        //  public static LuaGlobal G;
        //  public static LuaTable global;
        private static RemoteDebuggerService remoteDebugger =null;
        public static Script L;
        public static DynValue global;
        public const string ScriptPath = @"..\..\..\..\scripts\";
        static Dictionary<string, SoundEffect> soundLookup = new Dictionary<string, SoundEffect>();
        static Dictionary<string, SoundEffectInstance> soundInstances = new Dictionary<string, SoundEffectInstance>();
        public static SoundEffect FindUndertaleSound(string name)
        {
            SoundEffect sound;
            if (!soundLookup.TryGetValue(name, out sound)) {
                UndertaleResources.AudioFile audioFile;
                if (!UndertaleResources.UndertaleResrouce.TryGetResource<UndertaleResources.AudioFile>(name, out audioFile)) throw new Exception("Sound file not found");

                sound = SoundEffect.FromStream(new System.IO.MemoryStream(UndertaleResources.UndertaleResrouce.AudioDataAtIndex(audioFile.SoundIndex)));
                soundLookup[name] = sound;
            }
            return sound;
        }
        public static void StartUpLua()
        {
            UserData.RegisterAssembly();
            Script script = new Script();
            L = script;
            global = DynValue.NewPrimeTable();
            L.Globals["global"] = global;
            L.Globals["print"] = new Action<string>((string msg) =>
            {
                Debug.WriteLine(msg);
            });
            L.Globals["string_char_at"] = new Func<object, int, string>((object o, int pos) =>
              {
                  if (o == null) return null;
                  string msg = o as string;
                  if (pos >= 0 && pos < msg.Length)
                  {
                      string ret = msg[pos].ToString();
                      return ret;
                  }
                  return null;
              });
            L.Globals["string_length"] = new Func<string, int>((string s) =>
            {
                if (string.IsNullOrEmpty(s)) return 0;
                else return s.Length;
            });

            L.Globals["real"] = new Func<DynValue, DynValue>((DynValue o) =>
            {
                switch (o.Type)
                {
                    case DataType.String:
                        return DynValue.NewNumber((int)o.String[0]);
                    case DataType.Number:
                        return DynValue.NewNumber(o.Number);
                    default:
                        Debug.Assert(false);
                        return DynValue.NewNumber(0);
                }
            });
            L.Globals["snd_stop"] = new Action<object>((object o) =>
            {
                SoundEffectInstance instance = o as SoundEffectInstance;
                if (instance != null) instance.Stop();
                else if (o is string && soundInstances.TryGetValue(o as string,out instance)) instance.Stop();
            });
            L.Globals["snd_play"] = new Func<object, SoundEffectInstance>((object o) =>
            {
                SoundEffectInstance instance = o as SoundEffectInstance;
                if (instance == null){
                    string name = o as string;
                    if(name != null)
                    {
                        if(!soundInstances.TryGetValue(name, out instance))
                        {
                            SoundEffect effect = FindUndertaleSound(name);
                            instance = effect.CreateInstance();
                            soundInstances[name] = instance;
                        }
                    }
                }
                instance.Play();
                return instance;
            });

            if (remoteDebugger == null)
            {
                remoteDebugger = new RemoteDebuggerService();

                // the last boolean is to specify if the script is free to run 
                // after attachment, defaults to false
                remoteDebugger.Attach(script, "Description of the script", true);
            }

            // start the web-browser at the correct url. Replace this or just
            // pass the url to the user in some way.
          //  Process.Start(remoteDebugger.HttpUrlStringLocalHost);
        }
        public static ScriptFunctionDelegate GetDelegate(string function)
        {
            try
            {
                L.DoFile(ScriptPath + function + ".lua");
                return Global.L.Globals.Get(function).Function.GetDelegate();
            }
            catch (ScriptRuntimeException ex)
            {
                Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
        public static Table CreateNewSelf()
        {
            Table self = new Table(Global.L);
            Global.L.Globals.Get("SCR_SELFSETUP").Function.GetDelegate()(self);
            return self;
        }
        public static object CallFunction(string function, params object[] parm)
        {
            try
            {
                ScriptFunctionDelegate del = Global.L.Globals.Get(function).Function.GetDelegate();
                if (parm.Length == 0) return del();
                else return del(parm);
            }
            catch (ScriptRuntimeException ex)
            {
                Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
        public static void DebugRunDelegate(string function, Table self)
        {

            try
            {
                L.DoFile(ScriptPath + function + ".lua");
                var del =  Global.L.Globals.Get(function).Function.GetDelegate();
                del(self);
            }
            catch (ScriptRuntimeException ex)
            {
                Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
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
                Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            return null;
        }
       
        public static int[] Flag = new int[512];
        public static string CharName = "Frisk";
        public static int CurrentRoom = 0;
        public static int Facing = 0;
        public static int Phasing = 0;
        public static int Interact = 0;
        public static int Entrance = 0;
        public static int Border = 0;
        public static int[] IdealBorder = new int[4];
        public static int BattleGroup = 0;
        public static int Turn;
        public static int[] Monster = new int[4];
        public static int[] MonsterType = new int[4];
        public static GameObject[] MonsterInstance = new GameObject[3];
        public static int Msc = 0;
        public static int BatMusic = 0;
        public static int ActFirst = 0;
        public static int BattleLv = 0;
        public static int ExtraIntro = 0;

        public static void SCR_BATTLEGROUP()
        {
            Global.Monster[0] = 0;
            Global.Monster[1] = 0;
            Global.Monster[2] = 0;
            Global.Monster[3] = 0;
            Global.Turn = 0;
            switch (Global.BattleGroup)
            {
                case 1:
                    Global.MonsterType[0] = 1;
                    Global.MonsterType[1] = 1;
                    Global.MonsterType[2] = 1;
                    // Global.BatMusic = caster_load("music/battle1.ogg");
                    // caster_loop(global.batmusic, 0.5, 1);
                    Global.Msc = 2;
                    Global.BattleLv = 1;
                    Global.ActFirst = 0;
                    Global.ExtraIntro = 0;
                    Global.MonsterInstance[0] = GameObject.CreateInstance("obj_testmonster", 216, 136);
                    Global.MonsterInstance[1] = GameObject.CreateInstance("obj_testmonster", 418, 136);
                    Global.MonsterInstance[2] = GameObject.CreateInstance("obj_testmonster", 14, 136);
                    break;
                default:
                    throw new Exception("Unkonwn battlegroup");
            }
        }
        public static void SCR_BORDERSETUP()
        {
            if (Global.Border == 0)
            {
                Global.IdealBorder[0] = 32;
                Global.IdealBorder[1] = 602;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 1)
            {
                Global.IdealBorder[0] = 217;
                Global.IdealBorder[1] = 417;
                Global.IdealBorder[2] = 180;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 2)
            {
                Global.IdealBorder[0] = 217;
                Global.IdealBorder[1] = 417;
                Global.IdealBorder[2] = 125;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 3)
            {
                Global.IdealBorder[0] = 237;
                Global.IdealBorder[1] = 397;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 4)
            {
                Global.IdealBorder[0] = 267;
                Global.IdealBorder[1] = 367;
                Global.IdealBorder[2] = 295;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 5)
            {
                Global.IdealBorder[0] = 192;
                Global.IdealBorder[1] = 442;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 6)
            {
                Global.IdealBorder[0] = 227;
                Global.IdealBorder[1] = 407;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 7)
            {
                Global.IdealBorder[0] = 227;
                Global.IdealBorder[1] = 407;
                Global.IdealBorder[2] = 200;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 8)
            {
                Global.IdealBorder[0] = 202;
                Global.IdealBorder[1] = 432;
                Global.IdealBorder[2] = 290;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 9)
            {
                Global.IdealBorder[0] = 132;
                Global.IdealBorder[1] = 492;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 10)
            {
                Global.IdealBorder[0] = 147;
                Global.IdealBorder[1] = 487;
                Global.IdealBorder[2] = 200;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 11)
            {
                Global.IdealBorder[0] = 32;
                Global.IdealBorder[1] = 602;
                Global.IdealBorder[2] = 330;
                Global.IdealBorder[3] = 465;
            }
#if false
	if(Global.Border == 12) {
		Global.IdealBorder[0] = (self.room_width / 2) - 40;
		Global.IdealBorder[1] = (self.room_width / 2) + 40;
		Global.IdealBorder[2] = (self.room_height / 2) - 40;
		Global.IdealBorder[3] = (self.room_height / 2) + 40;
	}
	if(Global.Border == 13) {
		Global.IdealBorder[0] = (self.room_width / 2) - 40;
		Global.IdealBorder[1] = (self.room_width / 2) + 40;
		Global.IdealBorder[2] = 250;
		Global.IdealBorder[3] = 385;
	}
	if(Global.Border == 14) {
		Global.IdealBorder[0] = (self.room_width / 2) - 35;
		Global.IdealBorder[1] = (self.room_width / 2) + 35;
		Global.IdealBorder[2] = 300;
		Global.IdealBorder[3] = 385;
	}
	if(Global.Border == 15) {
		Global.IdealBorder[0] = (self.room_width / 2) - 50;
		Global.IdealBorder[1] = (self.room_width / 2) + 50;
		Global.IdealBorder[2] = 250;
		Global.IdealBorder[3] = 385;
	}
	if(Global.Border == 16) {
		Global.IdealBorder[0] = (self.room_width / 2) - 50;
		Global.IdealBorder[1] = (self.room_width / 2) + 50;
		Global.IdealBorder[2] = 50;
		Global.IdealBorder[3] = 385;
	}
#endif
            if (Global.Border == 17)
            {
                Global.IdealBorder[0] = 162;
                Global.IdealBorder[1] = 472;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 18)
            {
                Global.IdealBorder[0] = 162;
                Global.IdealBorder[1] = 472;
                Global.IdealBorder[2] = 220;
                Global.IdealBorder[3] = 385;
            }
#if false
	if(Global.Border == 19) {
		Global.IdealBorder[0] = (self.room_width / 2) - 100;
		Global.IdealBorder[1] = (self.room_width / 2) + 100;
		Global.IdealBorder[2] = 185;
		Global.IdealBorder[3] = 385;
	}
#endif
            if (Global.Border == 20)
            {
                Global.IdealBorder[0] = 257;
                Global.IdealBorder[1] = 547;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 21)
            {
                Global.IdealBorder[0] = 197;
                Global.IdealBorder[1] = 437;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
#if false
	if(Global.Border == 22) {
		self.offpurple = 0;
		if(instance_exists("obj_purpleheart")) {
			self.offpurple = obj_purpleheart.yzero;
			if(self.offpurple > 250) self.offpurple = 250;
		}
		Global.IdealBorder[0] = 197;
		Global.IdealBorder[1] = 437;
		Global.IdealBorder[2] = 250;
		if(self.offpurple != 0) Global.IdealBorder[2] = self.offpurple - 10;
		Global.IdealBorder[3] = 385;
	}

            if (Global.Border == 23)
            {
                self.offpurple = 0;
                if (instance_exists("obj_purpleheart"))
                {
                    self.offpurple = obj_purpleheart.yzero;
                    if (self.offpurple > 250) self.offpurple = 250;
                }
                Global.IdealBorder[0] = 197;
                Global.IdealBorder[1] = 537;
                Global.IdealBorder[2] = 250;
                if (self.offpurple != 0) Global.IdealBorder[2] = self.offpurple - 10;
                Global.IdealBorder[3] = 385;
            }
#endif
            if (Global.Border == 24)
            {
                Global.IdealBorder[0] = 235;
                Global.IdealBorder[1] = 405;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 25)
            {
                Global.IdealBorder[0] = 235;
                Global.IdealBorder[1] = 405;
                Global.IdealBorder[2] = 160;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 26)
            {
                Global.IdealBorder[0] = 295;
                Global.IdealBorder[1] = 345;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 27)
            {
                Global.IdealBorder[0] = 270;
                Global.IdealBorder[1] = 370;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 28)
            {
                Global.IdealBorder[0] = 235;
                Global.IdealBorder[1] = 405;
                Global.IdealBorder[2] = 35;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 29)
            {
                Global.IdealBorder[0] = 207;
                Global.IdealBorder[1] = 427;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 30)
            {
                Global.IdealBorder[0] = 207;
                Global.IdealBorder[1] = 427;
                Global.IdealBorder[2] = 200;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 31)
            {
                Global.IdealBorder[0] = 32;
                Global.IdealBorder[1] = 602;
                Global.IdealBorder[2] = 100;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 35)
            {
                Global.IdealBorder[0] = 132;
                Global.IdealBorder[1] = 502;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 36)
            {
                Global.IdealBorder[0] = 240;
                Global.IdealBorder[1] = 400;
                Global.IdealBorder[2] = 225;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 37)
            {
                Global.IdealBorder[3] = 385;
                Global.IdealBorder[2] = Global.IdealBorder[3] - 200;
                Global.IdealBorder[0] = 120;
                Global.IdealBorder[1] = 520;
            }
            if (Global.Border == 38)
            {
                Global.IdealBorder[0] = 270;
                Global.IdealBorder[1] = 370;
                Global.IdealBorder[2] = 285;
                Global.IdealBorder[3] = 385;
            }
            if (Global.Border == 39)
            {
                Global.IdealBorder[0] = 132;
                Global.IdealBorder[1] = 502;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
                Global.IdealBorder[0] = Global.IdealBorder[0] - 20;
                Global.IdealBorder[1] = Global.IdealBorder[1] + 40;
                Global.IdealBorder[2] = Global.IdealBorder[2] - 20;
            }
            if (Global.Border == 50)
            {
                Global.IdealBorder[0] = 192;
                Global.IdealBorder[1] = 512;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 385;
            }
#if false
            if(Global.Border == 51) {
		Global.IdealBorder[0] = 192;
		Global.IdealBorder[1] = 512;
		Global.IdealBorder[2] = 250;
		if(obj_heart.y< 270) Global.IdealBorder[2] = round((obj_heart.y - 20) / 5) * 5;
		Global.IdealBorder[3] = 385;
	}
#endif
            if (Global.Border == 52)
            {
                Global.IdealBorder[0] = 250;
                Global.IdealBorder[1] = 390;
                Global.IdealBorder[2] = 250;
                Global.IdealBorder[3] = 320;
            }
        }

        static Global()
        {
            for (int i = 0; i < 512; i++) Flag[i] = 0;
        }
}
}
