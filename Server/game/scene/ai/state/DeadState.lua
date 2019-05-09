local BP = require("Blueprint")
local time = require "game.scene.time"

local FightState = BP.BaseClass(BP.FSM.FSMState)

local SubState = {
	Catch = 1,--追捕
	Fighting = 2,--攻击
}
function FightState:OnInit(  )
	print('Cat:FightState.lua[OnInit]')
	self.aoi = self.blackboard:GetVariable("aoi")
	self.aoi_handle = self.blackboard:GetVariable("aoi_handle")
	self.aoi_area = self.blackboard:GetVariable("aoi_area")
	self.entity = self.blackboard:GetVariable("entity")
	self.entityMgr = self.blackboard:GetVariable("entityMgr")
	self.monsterMgr = self.blackboard:GetVariable("monsterMgr")
	self.cfg = self.blackboard:GetVariable("cfg")
end

function FightState:OnEnter(  )
	print('Cat:FightState.lua[OnEnter]')
	self.targetEnemyEntity = self.blackboard:GetVariable("targetEnemyEntity")
	print('Cat:FightState.lua[23] self.targetEnemyEntity', self.targetEnemyEntity)
end

function FightState:OnUpdate( deltaTime )
	print('Cat:FightState.lua[27] self.targetEnemyEntity', self.targetEnemyEntity)
	if self.targetEnemyEntity then
		--判断是否进入攻击范围，是则发起攻击，否则追上去
		local myPos = self.entityMgr:GetComponentData(self.entity, "umo.position")
		local enemyPos = self.entityMgr:GetComponentData(self.targetEnemyEntity, "umo.position")
		local distanceFromTargetSqrt = Vector3.Distance(myPos, enemyPos)
		local isMaxOk = distanceFromTargetSqrt <= self.cfg.ai.attack_area.max_distance
		local isMinOk = distanceFromTargetSqrt >= self.cfg.ai.attack_area.min_distance
		print('Cat:FightState.lua[34] distanceFromTargetSqrt', distanceFromTargetSqrt, isMaxOk)
		if isMaxOk and isMinOk then
			--离敌人距离刚好，发动攻击
		else
			--离敌人太远或太近了，走位

		end
	else
		--未有目标的话先获取最近的敌人并设置为目标
		local nearestEnemy = self:GetNearestEnemy()

	end

	self.fsm:TriggerState("Patrol")

end

function FightState:GetNearestEnemy(  )
	
end

function FightState:SetSubState( sub_state )
	self.sub_state = sub_state
	self.sub_elapsed_time = Time.time
	if sub_state == SubState.Idle then
	end
end

function FightState:OnExit(  )
end

function FightState:OnPause(  )
end

return FightState