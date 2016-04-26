function gml_Object_OBJ_WRITER_Step_1(self)
	self.myletter = string_char_at(self.originalstring, self.stringpos)
	if self.shake > 38 then 
		self.speed = 2
		self.direction = self.direction + 20
	end
	if self.shake == 42 then 
		self.speed = 4
		self.direction = self.direction - 19
	end
	if (self.halt == 3) or (self.dfy == 1) then
		print("instance_destroy()")
		instance_destroy()
	end
end
