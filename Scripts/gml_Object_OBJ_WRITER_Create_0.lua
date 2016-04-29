--ScriptName: gml_Object_OBJ_WRITER_Create_0
function gml_Object_OBJ_WRITER_Create_0(self)
	SCR_TEXTTYPE(self)
	global.msg[0] = "\\W* Howdy^2!&* I\'m\\Y FLOWEY\\W.^2 &* \\YFLOWEY\\W the \\YFLOWER\\W!/"
		global.msg[1] = "* Hmmm.../"
		global.msg[2] = "* You\'re new to the&  UNDERGROUND^2, aren\'tcha?/"
		global.msg[3] = "* Golly^1, you must be&  so confused./"
		global.msg[4] = "* Someone ought to teach&  you how things work&  around here!/"
		global.msg[5] = "* I guess little old me&  will have to do./"
		global.msg[6] = "* Ready^2?&* Here we go!/%%"
	print("round x")
	self.x = round(self.x)
	self.y = round(self.y)
	print("custom stuff")
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



