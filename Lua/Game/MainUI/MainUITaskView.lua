local MainUITaskView = BaseClass(UINode)

function MainUITaskView:Constructor( parentTrans )
	self.prefabPath = "Assets/AssetBundleRes/ui/mainui/MainUITaskView.prefab"
	self.model = TaskModel:GetInstance()

	self:Load()
end

function MainUITaskView:OnLoad(  )
	local names = {
		"item_scroll/Viewport/item_con","item_scroll",
	}
	UI.GetChildren(self, self.transform, names)
	print('Cat:MainUITaskView.lua[15] on load')
	self:AddEvents()
	self:OnUpdate()
end

function MainUITaskView:AddEvents(  )
	local on_click = function ( click_btn )
		
	end
	-- UIHelper.BindClickEvent(self.return_btn, on_click)

	self:BindEvent(TaskConst.Events.AckTaskList, function()
		self:OnUpdate()
	end, self.model)
end

function MainUITaskView:OnUpdate(  )
	print('Cat:MainUITaskView.lua[34] self.isLoaded', self.isLoaded)
	local taskInfo = self.model:GetTaskInfo()
	if not taskInfo or not taskInfo.taskList or not self.isLoaded then return end
	print("Cat:MainUITaskView [start:47] taskInfo.taskList:", taskInfo.taskList)
	PrintTable(taskInfo.taskList)
	print("Cat:MainUITaskView [end]")

	self.item_list_com = self.item_list_com or self:AddUIComponent(UI.ItemListCreator)
	local info = {
		data_list = taskInfo.taskList, 
		item_con = self.item_con, 
		scroll_view = self.item_scroll,
		prefab_path = "Assets/AssetBundleRes/ui/mainui/MainUITaskItem.prefab",
		item_height = 54,
		space_y = 0,
		child_names = {
			"desc:txt","name:txt","status:txt","click:obj",
		},
		on_update_item = function(item, i, v)
			self:OnUpdateItem(item, i, v)
		end,
	}
	self.item_list_com:UpdateItems(info)
end

function MainUITaskView:OnUpdateItem( item, i, v )
	local on_click = function ( click_obj )
		print('Cat:MainUITaskView.lua[59] click_obj', click_obj)
		if item.click_obj == click_obj then
			print('Cat:MainUITaskView.lua[61]')
			TaskController.GetInstance():DoTask({type = TaskConst.SubType.Talk, npcID = 3001})	
		end
	end
	UI.BindClickEvent(item.click_obj, on_click)
	
	item.desc_txt.text = "主线任务1"
end

return MainUITaskView