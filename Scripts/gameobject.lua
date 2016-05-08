require "font_info.lua"
require "sprite_info.lua"

function GetSprite(index, frame)
	local sprite = _sprites[index];
	if sprite then
		local frames = sprite.frames
		return frames[(frame+1) % #frames]
	else
		return nil
	end

end

-- helper functions

-- good enough, nill dosn't match anything
function string_char_at(str,pos)
	return string.sub(str,pos,pos)
end

function string_length(str)
	return string.len(str)
end

function real(o)
	return tonumber(o)
end

