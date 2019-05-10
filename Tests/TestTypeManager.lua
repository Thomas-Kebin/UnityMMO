local ECS = require "ECS"
TestTypeManager = ECS.BaseClass(require("TestBaseClass"))
	
function TestTypeManager:TestEntityTypeInfo(  )
	local type_index = ECS.TypeManager.GetTypeIndexByName(ECS.Entity.Name)
	lu.assertNotNil(type_index)
	local type_info1 = ECS.TypeManager.GetTypeInfoByName(ECS.Entity.Name)
	lu.assertNotNil(type_info1)
	local type_info2 = ECS.TypeManager.GetTypeInfoByIndex(type_index)
	lu.assertNotNil(type_info2)
	lu.assertEquals(type_info1, type_info2)
end

function TestTypeManager:TestTypeInfo(  )
	local test_comp_name = "TestComponentForTestTypeInfo"
	local type_info = ECS.TypeManager.RegisterType(test_comp_name, {n="number", i="integer", b="boolean"})
	lu.assertNotNil(type_info)
	local type_index = ECS.TypeManager.GetTypeIndexByName(test_comp_name)
	lu.assertNotNil(type_index)
	lu.assertEquals(type_info.TypeIndex, type_index)
	local type_info1 = ECS.TypeManager.GetTypeInfoByName(test_comp_name)
	local type_info2 = ECS.TypeManager.GetTypeInfoByIndex(type_index)
	lu.assertNotNil(type_info1)
	lu.assertNotNil(type_info2)
	lu.assertEquals(type_info1, type_info2)

	lu.assertNotNil(type_info.FieldInfoList)
	lu.assertEquals(#type_info.FieldInfoList, 3)
	lu.assertEquals(type_info.FieldInfoList[1].FieldName, "b")
	lu.assertEquals(type_info.FieldInfoList[1].FieldSize, ECS.CoreHelper.GetBooleanSize())
	lu.assertEquals(type_info.FieldInfoList[1].Offset, 0)
	lu.assertEquals(type_info.FieldInfoList[2].FieldName, "i")
	lu.assertEquals(type_info.FieldInfoList[2].FieldSize, ECS.CoreHelper.GetIntegerSize())
	lu.assertEquals(type_info.FieldInfoList[2].Offset, ECS.CoreHelper.GetBooleanSize())
	lu.assertEquals(type_info.FieldInfoList[3].FieldName, "n")
	lu.assertEquals(type_info.FieldInfoList[3].FieldSize, ECS.CoreHelper.GetNumberSize())
	lu.assertEquals(type_info.FieldInfoList[3].Offset, ECS.CoreHelper.GetBooleanSize()+ECS.CoreHelper.GetIntegerSize())
end