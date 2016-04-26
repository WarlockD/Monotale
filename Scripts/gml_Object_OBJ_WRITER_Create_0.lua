--ScriptName: gml_Object_OBJ_WRITER_Create_0
function gml_Object_OBJ_WRITER_Create_0(self)
	SCR_TEXTTYPE(self)
	global.msg[0] = "\\XLa, la.^3 &Time to wake&up and\\R smell\\X &the^4 pain./"
	global.msg[1] = "* Though^2.^4.^6.^8.&It\'s still a&little shaky./"
	global.msg[2] = "fhuehfuehfuehfuheufhe/%"
	global.msg[3] = "%%%"
	self.x = round(self.x)
	self.y = round(self.y)
	self.doak = 0
	self.stringno = 0
	self.stringpos = 1
	self.lineno = 0
	self.halt = 0
	self.writingx = round(self.writingx)
	self.writingy = round(self.writingy)
	self.myx = self.writingx
	self.myy = self.writingy
	--script_execute("attention_hackerz_no_2", global.msc)
	self.n = 0
	self.mystring = {}
	while global.msg[self.n] ~= "%%%" do
		self.mystring[self.n] = global.msg[self.n]
		self.n = self.n + 1
	end
	self.originalstring = self.mystring[0]
	self.dfy = 0
	self.alarm[0] = self.textspeed
	print("Staring Alarm at: " .. self.alarm[0])
end



