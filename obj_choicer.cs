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
    public class obj_choicer : GameObject
    {
        bool canchoose = false;
        GameObject creator_0;
        float add;
        object Alarm0(params object[] args) {
          //  creator_0 = function(self)script_execute("scr_msgup")

           // this.canchoose = true; return null;
            this.DestoryInstance();
            return null;
        }
        object Alarm1(params object[] args) { this.canchoose = true; return null; }
        protected override void Create()
        {
            base.Create();
            this.Alarms[1] = 3;
            this.Alarms.AlarmDelegates[1] = Alarm1;



        }
    }
}
