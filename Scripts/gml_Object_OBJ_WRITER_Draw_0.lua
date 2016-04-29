function gml_Object_OBJ_WRITER_Draw_0(self)
	function script_execute(name,...)
		if name == "SCR_NEWLINE" then
			self.myx = self.writingx
			self.myy = self.myy + self.vspacing
			self.lineno = self.lineno + 1
		end
	end


	self.myx = self.writingx
	self.myy = self.writingy
	self.n = 1
	while self.n < (self.stringpos + 1) do
		self.nskip = 0
		if string_char_at(self.originalstring, self.n) == "&" then 
			SCR_NEWLINE(self)
			self.n = self.n + 1
		end
		if string_char_at(self.originalstring, self.n) == "^" then 
			if string_char_at(self.originalstring, self.n + 1) == "0" then 
				self.nskip = 1
			else
				self.n = self.n + 2
			end
		end
		if string_char_at(self.originalstring, self.n) == "\\" then 
			if string_char_at(self.originalstring, self.n + 1) == "R" then 
				self.mycolor = 255
			end
			if string_char_at(self.originalstring, self.n + 1) == "G" then 
				self.mycolor = 65280
			end
			if string_char_at(self.originalstring, self.n + 1) == "W" then 
				self.mycolor = 16777215
			end
			if string_char_at(self.originalstring, self.n + 1) == "Y" then 
				self.mycolor = 65535
			end
			if string_char_at(self.originalstring, self.n + 1) == "X" then 
				self.mycolor = 0
			end
			if string_char_at(self.originalstring, self.n + 1) == "B" then 
				self.mycolor = 16711680
			end
			if string_char_at(self.originalstring, self.n + 1) == "O" then 
				self.mycolor = 4235519
			end
			if string_char_at(self.originalstring, self.n + 1) == "L" then 
				self.mycolor = 16629774
			end
			if string_char_at(self.originalstring, self.n + 1) == "P" then 
				self.mycolor = 16711935
			end
			if string_char_at(self.originalstring, self.n + 1) == "p" then 
				self.mycolor = 13941759
			end
			if (string_char_at(self.originalstring, self.n + 1) == "C") and (global.inbattle == 0) then 
				if instance_exists("obj_choicer") == 0 then 
					print("Creating choicer")
					self.choicer = instance_create(0, 0, "obj_choicer")
				end
				self.choicer.creator = self.id
				self.halt = 5
			end
			if string_char_at(self.originalstring, self.n + 1) == "M" then 
				global.flag[20] = real(string_char_at(self.originalstring, self.n + 2))
				self.n = self.n + 1
			end
			if string_char_at(self.originalstring, self.n + 1) == "E" then 
				global.faceemotion = real(string_char_at(self.originalstring, self.n + 2))
				self.n = self.n + 1
			end
			if string_char_at(self.originalstring, self.n + 1) == "F" then 
				global.facechoice = real(string_char_at(self.originalstring, self.n + 2))
				global.facechange = 1
				self.n = self.n + 1
			end
			if string_char_at(self.originalstring, self.n + 1) == "T" then 
				self.newtyper = string_char_at(self.originalstring, self.n + 2)
				if self.newtyper == "T" then 
					global.typer = 4
				end
				if self.newtyper == "t" then 
					global.typer = 48
				end
				if self.newtyper == "0" then 
					global.typer = 5
				end
				if self.newtyper == "S" then 
					global.typer = 10
				end
				if self.newtyper == "F" then 
					global.typer = 16
				end
				if self.newtyper == "s" then 
					global.typer = 17
				end
				if self.newtyper == "P" then 
					global.typer = 18
				end
				if self.newtyper == "M" then 
					global.typer = 27
				end
				if self.newtyper == "U" then 
					global.typer = 37
				end
				if self.newtyper == "A" then 
					global.typer = 47
				end
				if self.newtyper == "a" then 
					global.typer = 60
				end
				if self.newtyper == "R" then 
					global.typer = 76
				end
				SCR_TEXTTYPE(self, global.typer)
				global.facechange = 1
				self.n = self.n + 1
			end
			if string_char_at(self.originalstring, self.n + 1) == "z" then 
				self.sym = real(string_char_at(self.originalstring, self.n + 2))
				self.sym_s = 837
				if self.sym == 4 then 
					self.sym_s = 837
				end
				if self.sym == 4 then 
					draw_sprite_ext(self.sym_s, 0, self.myx + (random(self.shake) - (self.shake / 2)), (self.myy + 10) + (random(self.shake) - (self.shake / 2)), 2, 2, 0, 16777215, 1)
				end
				self.n = self.n + 1
			end
			self.n = self.n + 2
		end
		if string_char_at(self.originalstring, self.n) == "/" then 
			self.halt = 1
			if string_char_at(self.originalstring, self.n + 1) == "%" then 
				self.halt = 2
			end
			if (string_char_at(self.originalstring, self.n + 1) == "^") and (string_char_at(self.originalstring, self.n + 2) ~= "0") then 
				self.halt = 4
			end
			if string_char_at(self.originalstring, self.n + 1) == "*" then 
				self.halt = 6
			end
			break
		else
			if string_char_at(self.originalstring, self.n) == "%" then 
				if string_char_at(self.originalstring, self.n + 1) == "%" then 
					instance_destroy(self)
					break
				else
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
					SCR_NEWLINE(self)
					--script_execute("SCR_NEWLINE")
				end
				self.myletter = string_char_at(self.originalstring, self.n)
				if global.typer == 18 then 
					if (self.myletter == "l") or (self.myletter == "i") then 
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
						draw_text((self.myx + (self.hspeed * 0.7)) + 10, self.myy + (self.vspeed * 0.7), self.myletter)
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
					if (self.myletter == "O") or (self.myletter == "W") then 
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