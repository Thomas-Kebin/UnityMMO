--[[
skill_id：技能id
shape：攻击范围形状--1圆形，2直线，3扇形
duration：施放时间（毫秒）
detail：每级的具体属性
detail.condition：学习条件--{lv,1}角色等级，{money,100}货币
detail.cd：冷却时间
detail.attack_max_num：攻击最大数量
detail.damage_rate：伤害比率
detail.area：攻击范围--shape为圆形时即半径，直线时即为距离
]]--
local config = {
	--人物技能
	[120000] = {
		skill_id = 120000, shape = 1, duration = 1000, detail = {
			[1] = {
				condition = {{lv, 1}}, cd = 100, attack_max_num = 2, damage_rate = 10000, area = 5000,
			},
		},	
	},
	[120001] = {
		skill_id = 120001, shape = 1, duration = 1000, detail = {
			[1] = {
				condition = {{lv, 1}}, cd = 100, attack_max_num = 2, damage_rate = 10000, area = 5000,
			},
		},	
	},
	[120002] = {
		skill_id = 120002, shape = 1, duration = 1000, detail = {
			[1] = {
				condition = {{lv, 1}}, cd = 100, attack_max_num = 2, damage_rate = 10000, area = 5000,
			},
		},	
	},
	[120003] = {
		skill_id = 120003, shape = 1, duration = 1000, detail = {
			[1] = {
				condition = {{lv, 1}}, cd = 100, attack_max_num = 2, damage_rate = 10000, area = 5000,
			},
		},	
	},
	[120013] = {
		skill_id = 120013, shape = 1, duration = 1000, detail = {
			[1] = {
				condition = {{lv, 1}}, cd = 100, attack_max_num = 2, damage_rate = 10000, area = 5000,
			},
		},	
	},
	--怪物技能
	[200000] = {
		skill_id = 200000, shape = 1, duration = 1000, detail = {
			[1] = {
				condition = {{lv, 1}}, cd = 100, attack_max_num = 2, damage_rate = 10000, area = 5000,
			},
		},	
	},
}

return config