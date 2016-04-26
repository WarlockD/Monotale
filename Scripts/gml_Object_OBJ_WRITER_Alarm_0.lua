function gml_Object_OBJ_WRITER_Alarm_0(self)
	if self.stringpos < string_length(self.originalstring) then 
		self.stringpos = self.stringpos + 1
		if global.typer == 111 then 
			self.stringpos = self.stringpos + 1
		end
		self.alarm[0] = self.textspeed
		print("Update Alarm at: " .. self.alarm[0])
		if (string_char_at(self.originalstring, self.stringpos) == "^") and (string_char_at(self.originalstring, self.stringpos + 1) ~= "0") then 
			self.n = real(string_char_at(self.originalstring, self.stringpos + 1))
			self.alarm[0] = self.n * 10
			print("Long Alarm at: " .. self.alarm[0])
		else
			if (self.txtsound ~= 56) or (self.txtsound == 65) or (self.txtsound == 71) then 
				if self.txtsound == 56 then 
					if (string_char_at(self.originalstring, self.stringpos) ~= "") and (string_char_at(self.originalstring, self.stringpos) ~= "^") and (string_char_at(self.originalstring, self.stringpos) ~= "/") and (string_char_at(self.originalstring, self.stringpos) ~= "%") then 
						snd_stop("snd_mtt1")
						snd_stop("snd_mtt2")
						snd_stop("snd_mtt3")
						snd_stop("snd_mtt4")
						snd_stop("snd_mtt5")
						snd_stop("snd_mtt6")
						snd_stop("snd_mtt7")
						snd_stop("snd_mtt8")
						snd_stop("snd_mtt9")
						self.rnsound = floor(random(9))
						if self.rnsound == 0 then 
							repeat
								snd_play("snd_mtt1")
							until true
						elseif self.rnsound == 1 then 
							repeat
								snd_play("snd_mtt2")
							until true
						elseif self.rnsound == 2 then 
							repeat
								snd_play("snd_mtt3")
							until true
						elseif self.rnsound == 3 then 
							repeat
								snd_play("snd_mtt4")
							until true
						elseif self.rnsound == 4 then 
							repeat
								snd_play("snd_mtt5")
							until true
						elseif self.rnsound == 5 then 
							repeat
								snd_play("snd_mtt6")
							until true
						elseif self.rnsound == 6 then 
							repeat
								snd_play("snd_mtt7")
							until true
						elseif self.rnsound == 7 then 
							repeat
								snd_play("snd_mtt8")
							until true
						elseif self.rnsound == 8 then 
							repeat
								snd_play("snd_mtt9")
							until true
						end
					end
					self.stringpos = self.stringpos + 2
				end
				if (self.txtsound == 71) and (string_char_at(self.originalstring, self.stringpos) ~= "") and (string_char_at(self.originalstring, self.stringpos) ~= "^") and (string_char_at(self.originalstring, self.stringpos) ~= "/") and (string_char_at(self.originalstring, self.stringpos) ~= "%") then 
					snd_stop("snd_wngdng1")
					snd_stop("snd_wngdng2")
					snd_stop("snd_wngdng3")
					snd_stop("snd_wngdng4")
					snd_stop("snd_wngdng5")
					snd_stop("snd_wngdng6")
					snd_stop("snd_wngdng7")
					self.rnsound = floor(random(7))
					if self.rnsound == 0 then 
						repeat
							snd_play("snd_wngdng1")
						until true
					elseif self.rnsound == 1 then 
						repeat
							snd_play("snd_wngdng2")
						until true
					elseif self.rnsound == 2 then 
						repeat
							snd_play("snd_wngdng3")
						until true
					elseif self.rnsound == 3 then 
						repeat
							snd_play("snd_wngdng4")
						until true
					elseif self.rnsound == 4 then 
						repeat
							snd_play("snd_wngdng5")
						until true
					elseif self.rnsound == 5 then 
						repeat
							snd_play("snd_wngdng6")
						until true
					elseif self.rnsound == 6 then 
						repeat
							snd_play("snd_wngdng7")
						until true
					end
				end
				if self.txtsound == 65 then 
					if (string_char_at(self.originalstring, self.stringpos) ~= "") and (string_char_at(self.originalstring, self.stringpos) ~= "^") and (string_char_at(self.originalstring, self.stringpos) ~= "/") and (string_char_at(self.originalstring, self.stringpos) ~= "%") then 
						snd_stop("snd_tem")
						snd_stop("snd_tem2")
						snd_stop("snd_tem3")
						snd_stop("snd_tem4")
						snd_stop("snd_tem5")
						snd_stop("snd_tem6")
						self.rnsound = floor(random(6))
						if self.rnsound == 0 then 
							repeat
								snd_play("snd_tem")
							until true
						elseif self.rnsound == 1 then 
							repeat
								snd_play("snd_tem2")
							until true
						elseif self.rnsound == 2 then 
							repeat
								snd_play("snd_tem3")
							until true
						elseif self.rnsound == 3 then 
							repeat
								snd_play("snd_tem4")
							until true
						elseif self.rnsound == 4 then 
							repeat
								snd_play("snd_tem5")
							until true
						elseif self.rnsound == 5 then 
							repeat
								snd_play("snd_tem6")
							until true
						end
					end
					self.stringpos = self.stringpos + 1
				end
			else
				if (string_char_at(self.originalstring, self.stringpos) ~= "") and (string_char_at(self.originalstring, self.stringpos) ~= " ") and (string_char_at(self.originalstring, self.stringpos) ~= "&") and (string_char_at(self.originalstring, self.stringpos) ~= "^") and (string_char_at(self.originalstring, self.stringpos - 1) ~= "^") and (string_char_at(self.originalstring, self.stringpos) ~= "\\") and (string_char_at(self.originalstring, self.stringpos - 1) ~= "\\") and (string_char_at(self.originalstring, self.stringpos) ~= "/") and (string_char_at(self.originalstring, self.stringpos) ~= "%") then 
					snd_stop(self.txtsound)
					snd_play(self.txtsound)
				end
			end
		end
		if string_char_at(self.originalstring, self.stringpos) == "&" then 
			self.stringpos = self.stringpos + 1
		end
		if string_char_at(self.originalstring, self.stringpos) == "\\" then 
			self.stringpos = self.stringpos + 2
		end
	end
end
