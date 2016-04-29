
function gml_Object_OBJ_WRITER_Step_0(self)
	if keyboard_multicheck_pressed(0)  then 
		print("Key Press")
		if self.halt == 1 then
			print("Halt 1") 
			self.myletter = " "
			self.stringpos = 1
			self.stringno = self.stringno + 1
			self.originalstring = self.mystring[self.stringno]
			self.myx = self.writingx
			self.myy = self.writingy
			self.lineno = 0
			self.halt = 0
			self.alarm[0] = self.textspeed
		end
		if self.halt == 2 then 
			print("Halt 2") 
			instance_destroy(self)
		end
		if self.halt == 4 then 
			print("Halt 4") 
			global.myfight = 0
			global.mnfight = 1
			keyboard_clear('\x0D')
			instance_destroy(self)
		end
	end
end