local LoginConst = require("UI/Login/LoginConst")

local LoginView = {}

function LoginView:Open(  )
	local on_load_succeed = function ( view )
		print('Cat:LoginView.lua[on_load_succeed]')
		self.view = view

		local names = {"login", "account",}
		GetChildren(self, view, names)
		self.login_btn = self.login:GetComponent("Button")
        self.account_txt = self.account:GetComponent("InputField")
		self:AddEvents()
	end
	PanelMgr:CreatePanel('Assets/AssetBundleRes/ui/prefab/login/LoginView.prefab', on_load_succeed)
	
end

function LoginView:Close(  )
	print('Cat:LoginView.lua[22] self.view.gameObject', self.view)
	self.view:SetActive(false)
    GameObject.Destroy(self.view)
end

function LoginView:AddEvents(  )
	local on_click = function (  )
        local account = tonumber(self.account_txt.text)
        if not account then
            account = 123
        end
        local login_info = {
            account = account
        }
        Event.Brocast(LoginConst.Event.StartLogin, login_info)
	end
	UIHelper.BindClickEvent(self.login_btn, on_click)
end

return LoginView