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
    public class obj_mainchara
    {
        public Vector2 Position = Vector2.Zero;
        public float X {  get { return Position.X; } set { Position.X = value; } }
        public float Y { get { return Position.Y; } set { Position.Y = value; } }
        int lastfacing = 0;
        int nnn = 0;
        int cutscene = 0;
        float oldx = 0;
        float oldy = 0;
        int image_speed = 0;
        int facing = 0;
        int moving = 0;
        int movement = 1;

        int inwater = 0;
        int h_skip = 0;
        int uncan = 0;
        int m_override = 0;
        Sprite[] spriteDirections;
        Sprite sprite_index;
        // D R U L
        Color image_blend;

        public obj_mainchara()
        {
            if ((Global.Flag[7] == 1) && (Global.Flag[287] <= Global.Flag[286])) Global.Flag[287] = Global.Flag[286] + 1;
            if ((Global.Flag[6] == 1) && (Global.CharName.ToLower() != "frisk")) Global.Flag[6] = 0;
            Global.Flag[462] = 0;
            if ((X % 3) == 2) X = X + 1;
            if ((X % 3) == 1) X = X - 1;
            if ((Y % 3) == 2) Y = Y + 1;
            if ((Y % 3) == 1) Y = Y - 1;
            this.lastfacing = 0;
            this.nnn = 0;
            this.cutscene = 0;
            this.oldx = X;
            this.oldy = Y;
            this.image_speed = 0;
            Global.Phasing = 0;
            this.facing = Global.Facing;
            this.moving = 0;
            this.movement = 1;
            //G.currentroom = self.room;
            if ((Global.Interact == 3) && (Global.Entrance > 0))
            {
                Global.Interact = 0;
                if (Global.Entrance == 1)
                {
                 //   X = obj_markerA.x;
                //    Y = obj_markerA.y;
                }
                if (Global.Entrance == 2)
                {
                 //   X = obj_markerB.x;
                 //   Y = obj_markerB.y;
                }
                if (Global.Entrance == 4)
                {
                  //  X = obj_markerC.x;
                  //  Y = obj_markerC.y;
                }
                if (Global.Entrance == 5)
                {
                 //   X = obj_markerD.x;
                  //  Y = obj_markerD.y;
                }
                if (Global.Entrance == 18)
                {
                  //  X = obj_markerr.x;
                 //   Y = obj_markerr.y;
                }
                if (Global.Entrance == 19)
                {
                  //  X = obj_markers.x;
                  //  Y = obj_markers.y;
                }
                if (Global.Entrance == 20)
                {
                  //  X = obj_markert.x;
                  //  Y = obj_markert.y;
                }
                if (Global.Entrance == 21)
                {
                   // X = obj_markeru.x;
                   // Y = obj_markeru.y;
                }
                if (Global.Entrance == 22)
                {
                   // X = obj_markerv.x;
                   // Y = obj_markerv.y;
                }
                if (Global.Entrance == 23)
                {
                  //  X = obj_markerw.x;
                   // Y = obj_markerw.y;
                }
                if (Global.Entrance == 24)
                {
                  //  X = obj_markerX.x;
                  //  Y = obj_markerX.y;
                }
            }
            spriteDirections = new Sprite[4];
            
            if (Global.Flag[85] == 1)
            {
                spriteDirections[0] = Sprite.LoadSprite(1015);
                spriteDirections[1] = Sprite.LoadSprite(1017);
                spriteDirections[2] = Sprite.LoadSprite(1016);
                spriteDirections[3] = Sprite.LoadSprite(1018);
            } else
            {
                spriteDirections[0] = Sprite.LoadSprite(1042);
                spriteDirections[1] = Sprite.LoadSprite(1044);
                spriteDirections[2] = Sprite.LoadSprite(1043);
                spriteDirections[3] = Sprite.LoadSprite(1045);
            }
            sprite_index = spriteDirections[Global.Facing];

            if (Global.Flag[480] == 1)
                image_blend = new Color(0x80, 0x80, 0x80); // 0.3 blend? new Color(FF, FF, FF)
            else
                image_blend = Color.White; // new Color(0x80, 0x80, 0x80); // 0.3 blend? new Color(FF, FF, FF)

        }
    }
}
