local skynet = require "skynet"
local BagConst = require "game.bag.BagConst"
local this = {
	bagLists = {},
	user_info = nil,
	id_service = nil,
	gameDBServer = nil,
}

local function initBagList( pos )
	this.gameDBServer = this.gameDBServer or skynet.localname(".GameDBServer")
	local condition = string.format("roleID=%s and pos=%s", this.user_info.cur_role_id, pos)
	local hasBagList, goodsList = skynet.call(this.gameDBServer, "lua", "select_by_condition", "Bag", condition)
	print('Cat:this.lua[15] hasBagList', hasBagList, pos)
	local bagInfo = {cellNum=200, pos=pos}
	if hasBagList then
		local sort_func = function ( a, b )
			return a.cell < b.cell
		end
		table.sort(goodsList, sort_func)
		bagInfo.goodsList = goodsList
	else
		bagInfo.goodsList = {}
	end
	return bagInfo
end

local findEmptyCell = function ( bagInfo )
	local cell = 1
	if bagInfo and bagInfo.goodsList then
		cell = #bagInfo.goodsList + 1
		for i,v in ipairs(bagInfo.goodsList) do
			if v.cell > i then
				return i
			end
		end
	end
	return cell
end

local findGoodsInList = function ( goodsList, goodsTypeID )
	if not goodsList then return end
	for i,v in ipairs(goodsList) do
		if v.typeID == goodsTypeID then
			return v, i
		end
	end
	return nil
end

local notifyBagChange = function (  )
	if this.cacheChangeList and #this.cacheChangeList > 0 and this.coForGoodsChangeList then
		local co = this.coForGoodsChangeList
		this.coForGoodsChangeList = nil
		skynet.wakeup(co)
	end
end

local addNewGoodsToNotifyCache = function ( goodsInfo, notify )
	print("Cat:BagMgr [start:60] goodsInfo: ", goodsInfo)
	PrintTable(goodsInfo)
	print("Cat:BagMgr [end]")
	this.cacheChangeList = this.cacheChangeList or {}
	table.insert(this.cacheChangeList, goodsInfo)
	if notify then
		notifyBagChange()
	end
end

local changeGoodsNum = function( goodsTypeID, num, pos, notify )
	this.gameDBServer = this.gameDBServer or skynet.localname(".GameDBServer")

	local bagInfo = this.bagLists[pos]
	if bagInfo and bagInfo.goodsList then
		local goodsInfo, goodsIndex = findGoodsInList(bagInfo.goodsList)
		local overlapNum = 10
		local newGoods
		if goodsInfo and goodsInfo.num < overlapNum then
			goodsInfo.num = goodsInfo.num + num
			if goodsInfo.num <= 0 then
				table.remove(bagInfo.goodsList, goodsIndex)
				if goodsInfo.num < 0 then
					skynet.error("bag change goods num less than 0")
				end
				goodsInfo.num = 0
				skynet.call(this.gameDBServer, "lua", "delete", "Bag", "uid", goodsInfo.uid)
			else
				skynet.call(this.gameDBServer, "lua", "update", "Bag", "uid", goodsInfo.uid, goodsInfo)
			end
			newGoods = goodsInfo
		else
			local emptyCell = findEmptyCell(bagInfo)
			print('Cat:BagMgr.lua[81] emptyCell', emptyCell)
			this.id_service = this.id_service or skynet.localname(".id_service")
			local uid = skynet.call(this.id_service, "lua", "gen_uid", "goods")
			local addNum = math.min(overlapNum, num)
			newGoods = {
				uid = uid,
				typeID = goodsTypeID,
				num = addNum,
				pos = pos,
				cell = emptyCell,
				roleID = this.user_info.cur_role_id,
			}
			table.insert(bagInfo.goodsList, emptyCell, newGoods)
			skynet.call(this.gameDBServer, "lua", "insert", "Bag", newGoods)
			if num > overlapNum then
				changeGoodsNum(goodsTypeID, num - overlapNum, pos, notify)
			end
		end
		addNewGoodsToNotifyCache(newGoods, notify)
	else
		--Cat_Todo : uninit?
		skynet.error("bag:add goods uninit bag info")
	end
end

local getGoodsByUID = function ( uid )
	if not this.bagLists then
		return
	end
	for pos,bagList in pairs(this.bagLists) do
		if bagList.goodsList then
			for i,goodsInfo in ipairs(bagList.goodsList) do
				if goodsInfo.uid == uid then
					return goodsInfo, pos, i
				end
			end
		end
	end
	return nil
end

local SprotoHandlers = {}
function SprotoHandlers.Bag_GetInfo( reqData )
	local bagList = this.bagLists[reqData.pos]
	if not bagList then
		bagList = initBagList(reqData.pos)
		this.bagLists[reqData.pos] = bagList
	end
	return bagList
end

function SprotoHandlers.Bag_DropGoods( reqData )
	local goodsInfo, pos, index = getGoodsByUID(reqData.uid)
	print('Cat:BagMgr.lua[146] goodsInfo, pos, index', goodsInfo, pos, index, reqData.uid)
	if goodsInfo then
		goodsInfo.num = 0
		addNewGoodsToNotifyCache(goodsInfo, true)
		table.remove(this.bagLists[pos], index)
		return {result = ErrorCode.Succeed}
	else
		return {result = ErrorCode.CannotFindGoods}
	end
end

function SprotoHandlers.Bag_GetChangeList( reqData )
	print('Cat:BagMgr.lua[131] req get change list')
	if not this.coForGoodsChangeList then
		if not this.cacheChangeList or #this.cacheChangeList <= 0 then
			this.coForGoodsChangeList = coroutine.running()
			print('Cat:BagMgr.lua[134] this.coForGoodsChangeList', this.coForGoodsChangeList)
			skynet.wait(this.coForGoodsChangeList)
		end
		local changeList = this.cacheChangeList
		if changeList then
			this.cacheChangeList = nil
			return {goodsList=changeList}
		else
		end
	else
		--shouldn't be here,the client requested it again before replying
	end
	return {}
end

local PublicFuncs = {}
function PublicFuncs.Init( user_info )
	this.user_info = user_info
	
end
function PublicFuncs.ChangeBagGoods( goodsTypeID, num )
	print('Cat:BagMgr.lua[137] goodsTypeID, num', goodsTypeID, num)
	changeGoodsNum(goodsTypeID, num, BagConst.Pos.Bag, true)
end

SprotoHandlers.PublicClassName = "Bag"
SprotoHandlers.PublicFuncs = PublicFuncs
return SprotoHandlers