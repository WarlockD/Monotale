

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

