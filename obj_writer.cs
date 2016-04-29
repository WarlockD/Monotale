using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace MonoUndertale
{
    public class obj_writer : GameObject
    {
        float myx;
        float myy;
        float writingx;
        float writingy;
        string originalstring;
        float vspacing;
        int lineno;
        int stringpos;
        int stringno;
        int halt;
        int nextAlarm = 0;

        void NewLine()
        {
            myx = writingx;
            myy = myy + vspacing;
            lineno++;
        }
        protected override void InternalDraw(SpriteBatch batch) {
            Color textColor = Color.White;
            myx = writingx;
            myy = writingy;
            int n = 0;
            while (n < stringpos)
            {
                char current = originalstring.ElementAtOrDefault(n);
                char next = originalstring.ElementAtOrDefault(n + 1);
                int nskip = 0;
                switch (current)
                {
                    case '&':
                        NewLine();
                        n++;
                        break;
                    case '^':
                        if (next == '0') nskip = 1; else n += 2;
                        break;
                    case '\\':
                        switch (next) // escapes
                        {
                            case 'R': textColor.PackedValue = 255; break;
                            case 'G': textColor.PackedValue = 65280; break;
                            case 'W': textColor.PackedValue = 16777215; break;
                            case 'Y': textColor.PackedValue = 65535; break;
                            case 'X': textColor.PackedValue = 0; break;
                            case 'B': textColor.PackedValue = 16711680; break;
                            case 'O': textColor.PackedValue = 4235519; break;
                            case 'L': textColor.PackedValue = 16629774; break;
                            case 'P': textColor.PackedValue = 16711935; break;
                            case 'p': textColor.PackedValue = 13941759; break;
                            case 'C':
                                if (Global.InBattle == 0)
                                {
                                    /*
                                                                        if instance_exists("obj_choicer") == 0 then
                                                        self.choicer = instance_create(0, 0, "obj_choicer")

                                                    end
                                                    self.choicer.creator = self.id
                                                    self.halt = 5
                                                    */
                                }
                                break;
                            case 'M':
                                Global.Flag[20] = (int) originalstring.ElementAtOrDefault(n + 2);
                                n++;
                                break;
                            case 'E':
                                Global.FaceMotion = (int) originalstring.ElementAtOrDefault(n + 2);
                                n++;
                                break;
                            case 'F':
                                Global.FaceChoice = (int) originalstring.ElementAtOrDefault(n + 2);
                                Global.FaceChange = 1;
                                n++;
                                break;
                            case 'T': // change box
                                Debug.WriteLine("T in string");
                                n++;
                                break;
                            case 'z': // sopme sort of partical effect?
                                n++;
                                Debug.WriteLine("z in string");
                                break;
                        }
                        n += 2;
                        break;
                    case '/':
                        halt = 1;
                        switch (next)
                        {
                            case '%': halt = 2; break;
                            case '^':
                                if (originalstring.ElementAtOrDefault(n + 2) == '0') halt = 4;
                                break;
                            case '*': halt = 6; break;
                        }
                        return;
                    case '%':
                        if (next == '%')
                        {
                            Debug.WriteLine("destroy");
                            this.Visible = false;
                            return;
                        } else
                        {

                            stringpos = 0;

                            stringno++;

                            // originalstring = self.mystring[self.stringno]
                            myx = writingx;
                            myy = writingy;

                    lineno = 0;

                         //   nextAlarm = textspeed;

                        //    myletter = " "
                        }
                        break;


                }

            }
        }
    }
}
#if false
            for(int n=0;n < stringpos;)




        public override void self.myx = self.writingx
    self.myy = self.writingy

    self.n = 1
	while self.n< (self.stringpos + 1) do

	

					self.stringpos = 1
					self.stringno = self.stringno + 1
					self.originalstring = self.mystring[self.stringno]
                    self.myx = self.writingx
                    self.myy = self.writingy

                    self.lineno = 0

                    self.alarm[0] = self.textspeed

                    self.myletter = " "
					break

                end
			else
				if self.myx > self.writingxend then

                    script_execute("SCR_NEWLINE")

                end
                self.myletter = string_char_at(self.originalstring, self.n)
				if global.typer == 18 then 
					if (self.myletter == "l") or(self.myletter == "i") then
                       self.myx = self.myx + 2

                   end
					if self.myletter == "I" then
                       self.myx = self.myx + 2

                   end
					if self.myletter == "!" then
                       self.myx = self.myx + 2

                   end
					if self.myletter == "." then
                       self.myx = self.myx + 2

                   end
					if self.myletter == "S" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "?" then
                       self.myx = self.myx + 2

                   end
					if self.myletter == "D" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "A" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "\'" then
                       self.myx = self.myx + 1

                   end
               end

               draw_set_font(self.myfont)

                draw_set_color(self.mycolor)
				if self.shake > 38 then 
					if self.shake == 39 then
                        self.direction = self.direction + 10

                        draw_text(self.myx + self.hspeed, self.myy + self.vspeed, self.myletter)

                    end
					if self.shake == 40 then
                        draw_text(self.myx + self.hspeed, self.myy + self.vspeed, self.myletter)

                    end
					if self.shake == 41 then
                        self.direction = self.direction + (10 * self.n)

                        draw_text(self.myx + self.hspeed, self.myy + self.vspeed, self.myletter)

                        self.direction = self.direction - (10 * self.n)
					end
					if self.shake == 42 then
                        self.direction = self.direction + (20 * self.n)

                        draw_text(self.myx + self.hspeed, self.myy + self.vspeed, self.myletter)

                        self.direction = self.direction - (20 * self.n)
					end
					if self.shake == 43 then
                        self.direction = self.direction + (30 * self.n)

                        draw_text((self.myx + (self.hspeed* 0.7)) + 10, self.myy + (self.vspeed* 0.7), self.myletter)
						self.direction = self.direction - (30 * self.n)
					end
				else

                    draw_text(self.myx + (random(self.shake) - (self.shake / 2)), self.myy + (random(self.shake) - (self.shake / 2)), self.myletter)
				end
                self.myx = self.myx + self.spacing
				if self.myfont == 8 then
					if self.myletter == "w" then
                        self.myx = self.myx + 2

                    end
					if self.myletter == "m" then
                        self.myx = self.myx + 2

                    end
					if self.myletter == "i" then
                        self.myx = self.myx - 2

                    end
					if self.myletter == "l" then
                        self.myx = self.myx - 2

                    end
					if self.myletter == "s" then
                        self.myx = self.myx - 1

                    end
					if self.myletter == "j" then
                        self.myx = self.myx - 1

                    end
                end
				if self.myfont == 9 then
					if self.myletter == "D" then
                        self.myx = self.myx + 1

                    end
					if self.myletter == "Q" then
                        self.myx = self.myx + 3

                    end
					if self.myletter == "M" then
                        self.myx = self.myx + 1

                    end
					if self.myletter == "L" then
                        self.myx = self.myx - 1

                    end
					if self.myletter == "K" then
                        self.myx = self.myx - 1

                    end
					if self.myletter == "C" then
                        self.myx = self.myx + 1

                    end
					if self.myletter == "." then
                        self.myx = self.myx - 3

                    end
					if self.myletter == "!" then
                        self.myx = self.myx - 3

                    end
					if (self.myletter == "O") or(self.myletter == "W") then
                       self.myx = self.myx + 2

                   end
					if self.myletter == "I" then
                       self.myx = self.myx - 6

                   end
					if self.myletter == "T" then
                       self.myx = self.myx - 1

                   end
					if self.myletter == "P" then
                       self.myx = self.myx - 2

                   end
					if self.myletter == "R" then
                       self.myx = self.myx - 2

                   end
					if self.myletter == "A" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "H" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "B" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "G" then
                       self.myx = self.myx + 1

                   end
					if self.myletter == "F" then
                       self.myx = self.myx - 1

                   end
					if self.myletter == "?" then
                       self.myx = self.myx - 3

                   end
					if self.myletter == "\'" then
                       self.myx = self.myx - 6

                   end
					if self.myletter == "J" then
                       self.myx = self.myx - 1

                   end
               end

               self.n = self.n + self.nskip

               self.n = self.n + 1

           end
       end

   end
end
    }

}
#endif