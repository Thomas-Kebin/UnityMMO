local TestSystem = ECS.BaseClass(ECS.ComponentSystem)
ECS.TypeManager.RegisterScriptMgr(TestSystem, "TestSystem")
TestSystem.UpdateAfter = {"LastSystem"}
TestSystem.UpdateBefore = {"FirstSystem"}
TestSystem.UpdateInGroup = {"GroupSystem"}
TestSystem.AlwaysUpdateSystem = true
function TestSystem:Constructor(  )
	local data = {
		position = "ComponentDataArray:ECS.Position:ReadOnly",
		rotation = "Subtractive:ECS.Rotation",
		othersys = "ScriptMgr:ECS.TestSystem2",
		entities = "EntityArray",
		length   = "Length",
	}
	self.group = {}
	self:Inject(self.group, data)
end

function TestSystem:OnCreateManager( )
	ECS.ComponentSystem.OnCreateManager(self)
    self.group_with_filter = self:GetComponentGroup({"ECS.Position:ReadOnly", "ECS.CodeLOD"})
end

function TestSystem:OnUpdate(  )
	for i=1,self.group.length do
		local pos = self.group.position:get(i)
		self.group.position:get(i, "x")
		self.group.position:set(i, "x", pos.x+10)
		pos.x = 10
		self.group.position:set(i, pos)
	end
	
	self.group_with_filter:SetFilter(ECS.CodeLOD.New(1))
	local positions = self.group_with_filter:GetComponentDataArray("ECS.Position")
	for i=1,#positions do
		positions[i].x = positions[i].x+1
	end
end

local use_case = function (  )
	local entity_mgr = ECS.World.Active:GetOrCreateManager(ECS.EntityManager)

	local entity = entity_mgr:CreateEntity()
	entity_mgr:AddComponentData(entity, ECS.Position, {x=0, y=1, z=2})
	local pos = entity_mgr:GetComponentData(entity, ECS.Position)
	pos.x = 3
	print('Cat:TestSystem.lua[46] pos.x, pos.y, pos.z', pos.x, pos.y, pos.z)
	entity_mgr:SetComponentData(entity, ECS.Position, pos)
end