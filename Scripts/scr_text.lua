function print_r ( t )  
	local print_r_cache={}
	local function sub_print_r(t,indent)
		if (print_r_cache[tostring(t)]) then
			print(indent.."*"..tostring(t))
		else
			print_r_cache[tostring(t)]=true
			if (type(t)=="table") then
				for pos,val in pairs(t) do
					if (type(val)=="table") then
						print(indent.."["..pos.."] => "..tostring(t).." {")
						sub_print_r(val,indent..string.rep(" ",string.len(pos)+8))
						print(indent..string.rep(" ",string.len(pos)+6).."}")
					elseif (type(val)=="string") then
						print(indent.."["..pos..'] => "'..val..'"')
						else
						print(indent.."["..pos.."] => "..tostring(val))
					end
				end
			else
				print(indent..tostring(t))
			end
		end
	end
	if (type(t)=="table") then
		print(tostring(t).." {")
		sub_print_r(t,"  ")
		print("}")
	else
		sub_print_r(t,"  ")
	end
	print()
end
 
function round (num, idp)
	local mult = 10 ^ (idp or 0)
	return math.floor (num * mult + 0.5) / mult
end

function create_instance(x,y, name)
	local self = {}

	self.name = name
	self.x = x
	self.y = y
	self.view_xview = {}
	self.view_yview = {}
	for i=0,7 do
		self.view_xview[i] = 0;
		self.view_yview[i] = 0;
	end
	self.view_current = 0;
	return self;
end
function SCR_NEWLINE(self)
	self.myx = self.writingx
	self.myy = self.myy + self.vspacing
	self.lineno = self.lineno + 1
end

function SCR_TEXTSETUP(self, font,color,writingx,writingy,writingxend,shake,textspeed,textsound,spacing,vspacing) 
	self.myfont = font
	self.mycolor = color
	self.writingx = writingx
	self.writingy = writingy
	self.writingxend = writingxend
	self.shake = shake
	self.textspeed = textspeed
	self.txtsound = textsound
	self.spacing = spacing
	self.vspacing = vspacing
end

function SCR_TEXTTYPE (self, typer)
	print("SCR_TEXTTYPE start")
	if typer ~= nil then 
		global.typer = typer
	end
	if global.typer == 1 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, self.x + (global.idealborder[1] - 55), 1, 1, 94, 16, 32)
	end
	if global.typer == 2 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 190, 43, 2, 95, 9, 20)
	end
	if global.typer == 3 then 
		SCR_TEXTSETUP(self, 7, 8421376, self.x, self.y, self.x + 100, 39, 3, 95, 10, 10)
	end
	if global.typer == 4 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 101, 8, 18)
	end
	if global.typer == 5 then
		SCR_TEXTSETUP (self, 2, 16777215, self.x + 20, self.y + 20,  self.view_xview[self.view_current] + 290, 0, 1, 95, 8, 18)
	end
	if global.typer == 6 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 1, 97, 9, 20)
	end
	if global.typer == 7 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 2, 2, 98, 9, 20)
	end
	if global.typer == 8 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 1, 101, 9, 20)
	end
	if global.typer == 9 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 97, 8, 18)
	end
	if global.typer == 10 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 96, 8, 18)
	end
	if global.typer == 11 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 2, 94, 9, 18)
	end
	if global.typer == 12 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 1, 3, 99, 10, 20)
	end
	if global.typer == 13 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 2, 4, 99, 11, 20)
	end
	if global.typer == 14 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 3, 5, 99, 14, 20)
	end
	if global.typer == 15 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 10, 99, 18, 20)
	end
	if global.typer == 16 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 1.2, 2, 98, 8, 18)
	end
	if global.typer == 17 then 
		SCR_TEXTSETUP(self, 8, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 88, 8, 18)
	end
	if global.typer == 19 then 
		global.typer = 18
	end
	if global.typer == 18 then 
		SCR_TEXTSETUP(self, 9, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 87, 11, 18)
	end
	if global.typer == 20 then 
		SCR_TEXTSETUP(self, 5, 0, self.x, self.y, self.x + 200, 0, 2, 98, 25, 20)
	end
	if global.typer == 21 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 4, 96, 10, 18)
	end
	if global.typer == 22 then 
		SCR_TEXTSETUP(self, 9, 0, self.x + 10, self.y, self.x + 200, 1, 1, 87, 11, 20)
	end
	if global.typer == 23 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 310, 0, 1, 95, 8, 18)
	end
	if global.typer == 24 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 310, 0, 1, 65, 8, 18)
	end
	if global.typer == 27 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 3, 56, 8, 18)
	end
	if global.typer == 28 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 2, 65, 8, 18)
	end
	if global.typer == 30 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, 9999, 0, 2, 90, 20, 36)
	end
	if global.typer == 31 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, 9999, 0, 2, 90, 12, 18)
	end
	if global.typer == 32 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, 9999, 0, 2, 84, 20, 36)
	end
	if global.typer == 33 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 190, 43, 1, 95, 9, 20)
	end
	if global.typer == 34 then 
		SCR_TEXTSETUP(self, 0, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 3, 71, 16, 18)
	end
	if global.typer == 35 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 2, 84, 10, 18)
	end
	if global.typer == 36 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 8, 85, 10, 18)
	end
	if global.typer == 37 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 78, 8, 18)
	end
	if global.typer == 38 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 6, 78, 8, 18)
	end
	if global.typer == 39 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 0, 1, 78, 9, 20)
	end
	if global.typer == 40 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 1, 2, 78, 9, 20)
	end
	if global.typer == 41 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 0, 1, 78, 9, 20)
	end
	if global.typer == 42 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 2, 4, 78, 9, 20)
	end
	if global.typer == 43 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 2, 4, 80, 9, 20)
	end
	if global.typer == 44 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 2, 5, 81, 9, 20)
	end
	if global.typer == 45 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 2, 7, 82, 9, 20)
	end
	if global.typer == 47 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 83, 8, 18)
	end
	if global.typer == 48 then 
		SCR_TEXTSETUP(self, 8, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 89, 8, 18)
	end
	if global.typer == 49 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 190, 43, 1, 83, 9, 20)
	end
	if global.typer == 50 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 10, 999, 0, 3, 56, 8, 18)
	end
	if global.typer == 51 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 20, self.y + 16, 999, 0, 3, 56, 8, 18)
	end
	if global.typer == 52 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 20, self.y + 20, 999, 0, 1, 83, 8, 18)
	end
	if global.typer == 53 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 20, self.y + 10, 999, 1.5, 4, 56, 8, 18)
	end
	if global.typer == 54 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 20, self.y + 10, 999, 0, 7, 56, 8, 18)
	end
	if global.typer == 55 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 999, 0, 2, 96, 9, 20)
	end
	if global.typer == 60 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 2, 90, 8, 18)
	end
	if global.typer == 61 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, self.x + 99999, 0, 2, 96, 16, 32)
	end
	if global.typer == 62 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 3, 90, 9, 20)
	end
	if global.typer == 63 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 2, 90, 9, 20)
	end
	if global.typer == 64 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 2, 3, 90, 9, 20)
	end
	if global.typer == 66 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 2, 97, 9, 20)
	end
	if global.typer == 67 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, self.x + 999, 2, 5, 98, 16, 32)
	end
	if global.typer == 68 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 500, 0, 1, 97, 9, 20)
	end
	if global.typer == 69 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 500, 2, 2, 98, 9, 20)
	end
	if global.typer == 70 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 500, 1, 3, 97, 9, 20)
	end
	if global.typer == 71 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 500, 2, 5, 98, 9, 20)
	end
	if global.typer == 72 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 500, 1, 2, 97, 9, 20)
	end
	if global.typer == 73 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, self.x + 99999, 0, 5, 96, 16, 32)
	end
	if global.typer == 74 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 490, 0, 1, 83, 9, 20)
	end
	if global.typer == 75 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 490, 2, 1, 83, 9, 20)
	end
	if global.typer == 76 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 84, 8, 18)
	end
	if global.typer == 77 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 4, 98, 9, 20)
	end
	if global.typer == 78 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 2, 3, 98, 9, 20)
	end
	if global.typer == 79 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 2, 85, 8, 18)
	end
	if global.typer == 80 then 
		SCR_TEXTSETUP(self, 8, 0, self.x, self.y, self.x + 200, 0, 1, 88, 10, 20)
	end
	if global.typer == 81 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 190, 0, 1, 78, 9, 20)
	end
	if global.typer == 82 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 490, 2, 3, 83, 9, 20)
	end
	if global.typer == 83 then 
		SCR_TEXTSETUP(self, 9, 0, self.x + 2, self.y, self.x + 200, 1, 3, 87, 11, 20)
	end
	if global.typer == 84 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 1, 2, 99, 10, 20)
	end
	if global.typer == 85 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 2, 84, 9, 20)
	end
	if global.typer == 86 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 10, self.y, self.x + 200, 0, 1, 85, 9, 20)
	end
	if global.typer == 87 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 10, self.y, self.x + 200, 0, 3, 85, 9, 20)
	end
	if global.typer == 88 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 10, self.y, self.x + 200, 2, 3, 85, 9, 20)
	end
	if global.typer == 89 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 84, 8, 18)
	end
	if global.typer == 90 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 3, 84, 8, 18)
	end
	if global.typer == 91 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, 9999, 0, 3, 101, 10, 18)
	end
	if global.typer == 92 then 
		SCR_TEXTSETUP(self, 4, 16777215, self.x, self.y, self.x + 190, 43, 1, 95, 9, 20)
	end
	if global.typer == 93 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 0, 1, 79, 9, 20)
	end
	if global.typer == 94 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 1, 2, 79, 9, 20)
	end
	if global.typer == 95 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 2, 3, 79, 9, 20)
	end
	if global.typer == 96 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, self.x + 190, 3, 4, 79, 9, 20)
	end
	if global.typer == 97 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 16, self.y, 999, 1, 3, 56, 8, 18)
	end
	if global.typer == 98 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 8, self.y, self.x + 200, 0, 1, 97, 9, 20)
	end
	if global.typer == 99 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 8, self.y, self.x + 200, 1, 1, 97, 9, 20)
	end
	if global.typer == 100 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 0, 1, 96, 8, 18)
	end
	if global.typer == 101 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 8, self.y, self.x + 200, 1, 2, 97, 9, 20)
	end
	if global.typer == 102 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 8, self.y, self.x + 200, 2, 3, 97, 9, 20)
	end
	if global.typer == 103 then 
		SCR_TEXTSETUP(self, 4, 0, self.x + 8, self.y, self.x + 200, 2, 5, 84, 9, 20)
	end
	if global.typer == 104 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, 999, 0, 4, 96, 16, 34)
	end
	if global.typer == 105 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, 999, 0, 3, 96, 16, 34)
	end
	if global.typer == 106 then 
		SCR_TEXTSETUP(self, 2, 16777215, self.x + 20, self.y + 20, 999, 0, 3, 96, 8, 18)
	end
	if global.typer == 107 then 
		SCR_TEXTSETUP(self, 8, 0, self.x + 5, self.y, self.x + 200, 0, 2, 88, 10, 20)
	end
	if global.typer == 108 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 200, 0, 4, 96, 9, 20)
	end
	if global.typer == 109 then 
		SCR_TEXTSETUP(self, 8, 0, self.x + 5, self.y, self.x + 200, 0, 1, 88, 10, 20)
	end
	if global.typer == 110 then 
		SCR_TEXTSETUP(self, 1, 16777215, self.x + 20, self.y + 20, 9999, 0, 2, 88, 20, 36)
	end
	if global.typer == 111 then 
		SCR_TEXTSETUP(self, 4, 0, self.x, self.y, self.x + 190, 43, 1, 95, 9, 20)
	end
	if global.typer == 666 then 
		SCR_TEXTSETUP(self, 0, 16777215, self.x + 20, self.y + 20, self.view_xview[self.view_current] + 290, 1, 4, 71, 16, 18)
	end
	print ("SCR_TEXTTYPE finish")
end