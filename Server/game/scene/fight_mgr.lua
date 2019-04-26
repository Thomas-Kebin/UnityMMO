local skill_cfg = require "Config.config_skill"
local time = require "game.scene.time"

local fight_mgr = {}

function fight_mgr:init( scene )
	self.scene_mgr = scene
	self.entity_mgr = scene.entity_mgr
	self.aoi = scene.aoi
	self.damage_events = {}
end

function fight_mgr:cast_skill( user_info, req_data )
	--检查施法者状态（技能CD,是否麻痹、中毒、加班等）
	local role_info = self.scene_mgr.role_list[user_info.cur_role_id]
	local is_can_cast = true
	local fight_event = nil
	if is_can_cast then
		fight_event = {
			attacker_uid = role_info.scene_uid,
			skill_id = req_data.skill_id,
			skill_lv = 1,
			attacker_pos_x = req_data.cur_pos_x,--Cat_Todo : 有空记得做校验
			attacker_pos_y = req_data.cur_pos_y,
			attacker_pos_z = req_data.cur_pos_z,
			target_pos_x = req_data.target_pos_x,
			target_pos_y = req_data.target_pos_y,
			target_pos_z = req_data.target_pos_z,
			direction = req_data.direction,
			time = time:get_cur_time(),
			defenders = nil,
		}
		fight_event.defenders = self:cal_defender_list(fight_event, role_info)

		self:add_damage_event_for_defenders(fight_event)

		self.scene_mgr.fight_events[role_info.scene_uid] = self.scene_mgr.fight_events[role_info.scene_uid] or {}
		table.insert(self.scene_mgr.fight_events[role_info.scene_uid], fight_event)
	end
	return is_can_cast and 0 or 1, fight_event
end

--计算受击者列表
function fight_mgr:cal_defender_list( fight_info, role_info )
	local cfg = skill_cfg[fight_info.skill_id]
	if not cfg or not role_info then return end
	
	local skill_bomb = self.aoi:add()
	self.aoi:set_pos(skill_bomb, fight_info.target_pos_x, fight_info.target_pos_y, fight_info.target_pos_z)

	local area = cfg.detail[fight_info.skill_lv].area
	local around = self.aoi:get_around_offset(skill_bomb, area, area)
	local defenders
	if around then
		defenders = {}
		for aoi_handle,v in pairs(around) do
			if aoi_handle ~= role_info.aoi_handle then
				local uid = self.scene_mgr.aoi_handle_uid_map[aoi_handle]
				local entity = self.scene_mgr.uid_entity_map[uid]
				if entity then
					local hp = self.entity_mgr:GetComponentData(entity, "umo.hp")
					local damage_value = self:cal_damage(fight_info, entity)
					table.insert(defenders, {uid=uid, cur_hp=hp.cur, damage=damage_value, flag=math.random(0, 2)})
				end
			end
		end
	end
	self.aoi:remove(skill_bomb)
	return defenders
end

function fight_mgr:add_damage_event_for_defenders( fight_event )
	if not fight_event.defenders then return end
	for k,v in pairs(fight_event.defenders) do
		self.damage_events[v.uid] = self.damage_events[v.uid] or {}
		local damage_event = {
			-- instigator_uid = fight_event.attacker_uid,
			damage = v.damage,
			-- damage_time = --技能不一定中了就马上扣血的
		}
		table.insert(self.damage_events[v.uid], damage_event)
	end
end

function fight_mgr:get_damage_events( scene_uid )
	if not scene_uid then return end
	return self.damage_events[scene_uid]
end

function fight_mgr:clear_damage_events( scene_uid )
	if not scene_uid or not self.damage_events[scene_uid] then return end

	self.damage_events[scene_uid] = nil
end

function fight_mgr:cal_damage( fight_info, entity )
	return math.random(50, 1234)
end

return fight_mgr