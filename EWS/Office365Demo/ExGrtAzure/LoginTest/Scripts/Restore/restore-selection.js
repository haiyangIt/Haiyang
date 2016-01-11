/* Thr-checkbox tree.

author: linghaiyang.
version: 1.0


1. public method:
    static method:
    1.1 Restore.Item.CreateItem
    1.2 Restore.Item.GetItem
    1.3 Restore.Item.GetRootItem
    1.4 Restore.Item.GetTotalSelectItems
    1.5 Restore.Item.GetStatus
    1.6 Restore.Item.GetNewStatus
    1.7 Restore.Item.Clear
    1.8 Restore.Item.IsSelectAnyItem

    object method
    1.8 Restore.Item.prototype.AddChild
    1.9 Restore.Item.prototype.Select
    
    Note: other methond is private.

2. Call sequence:
    2.1. Create item (Restore.Item.CreateItem)
    2.2. Add item to parent (Restore.Item.prototype.AddChild)
    2.3. When client to click the control to change the status, you need:
        2.3.1 get old status (Restore.Item.GetStatus)
        2.3.2 get new status which the control will be (Restore.Item.GetNewStatus)
        2.3.3 call select (Restore.Item.prototype.Select)
    2.4 If you want get all selected items, please call GetTotalSelectItems(Restore.Item.GetTotalSelectItems)

3. Design & Data Struct
    Each item contains two part: loaded children (this.ChildrenLoaded) and unloaded children. use this.SelectedLoadedChildren to save selected children and use UnloadChildStatus to save unloaded children status.

    When add children (this.AddChild), parent will listen child item status changing.
    When item status changes:
        a. will trigger an event to notice parent (call parent callback function). it will trigger the change to all parents util root item. And the callback function only change the parent item status, html and SelectedLoadedChildren.
        b. will change self status and html.
        c. will change children status.
        d. will modify this.SelectedLoadedChildren and this.UnloadChildStatus

4. Const data.
    root item id is "0".
    Restore.Item.ItemStatusChanged = "OnItemStatusChanged";
    Restore.Item.SelectedStatus = 1;
    Restore.Item.UnSelectedStatus = 0;
    Restore.Item.IndeterminateStatus = 2;

    Restore.Item.SelectedCss = "restore-selected";
    Restore.Item.UnSelectedCss = "restore-unselected";
    Restore.Item.IndeterminateCss = "restore-indeterminate";
    Restore.Item.Thr_CheckboxCss = "restore-check";

4. Todo list:
    4.1 Now all data is saved in Restore.Item.DataCacheDom().data(cacheKey). so we need change this way. Can create a plugin.
    4.2 Now all Thr-checkbox is input checkbox. we need change it.

*/


; var Restore = Restore || {};

Restore.DateReg = /Date\(\d{12,14}\)/;
Restore.GetDateTime = function (jsonDateTime) {
    if (!(jsonDateTime instanceof Date))
        if (Restore.DateReg.test(jsonDateTime))
            return new Date(parseInt(jsonDateTime.replace("/Date(", "").replace(")/", ""), 10));
    return jsonDateTime;
}

Restore.Item = function (itemId, itemData, childItemCount, itemType, displayName, modifyHtmlForStatusChangeCallback) {
    this.ChildrenLoaded = [];
    this.ItemData = itemData;
    this.Id = itemId;
    this.DisplayName = displayName;
    this.Status = Restore.Item.UnSelectedStatus;
    //this.SelectedLoadedChildren = [];
    this.ChildItemCount = childItemCount;
    this.Event = [];
    this.ParentId = null;
    this.ItemType = itemType,
    this.UnloadChildStatus = Restore.Item.UnSelectedStatus;
    this.ModifyHtmlForStatusChangeCallback = modifyHtmlForStatusChangeCallback;
};

Restore.Item.ItemStatusChanged = "OnItemStatusChanged";
Restore.Item.SelectedStatus = 1;
Restore.Item.UnSelectedStatus = 0;
Restore.Item.IndeterminateStatus = 2;

Restore.Item.SelectedCss = "restore-selected";
Restore.Item.UnSelectedCss = "restore-unselected";
Restore.Item.IndeterminateCss = "restore-indeterminate";
Restore.Item.Thr_CheckboxCss = "restore-check";

Object.size = function (obj) {
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};

Restore.Item.KeyForRestore = "Restore_TreeDic";

Restore.Item.DataCacheDom = function () {
    var dom = $("#three-type-select-tree-dataCache");
    if (dom.length == 0) {
        dom = $("<span id='three-type-select-tree-dataCache' style='display:hidden'></span>").appendTo("body");
    }
    return dom;
}

Restore.Item.Clear = function (cacheKey) {
    delete Restore.Item.DataCacheDom().data(cacheKey);
    var dic = [];
    Restore.Item.DataCacheDom().data(cacheKey, dic);
};

Restore.Item.GetRootItem = function (cacheKey) {
    return Restore.Item.DataCacheDom().data(cacheKey)["0"];
};

Restore.Item._SaveItem = function (itemId, item, cacheKey) {
    var dic = Restore.Item.DataCacheDom().data(cacheKey)
    if (typeof (dic) === "undefined") {
        dic = [];
        Restore.Item.DataCacheDom().data(cacheKey, dic);
    }

    dic[itemId] = item;
};

Restore.Item.GetItem = function (itemId, cacheKey) {
    var dic = Restore.Item.DataCacheDom().data(cacheKey);
    if (typeof (dic) !== "undefined") {
        var result = dic[itemId];
        if (typeof (result) !== "undefined")
            return result;
        else
            return null;
    }
    return null;
};

Restore.Item.IsSelectAnyItem = function (cacheKey) {
    var rootItem = Restore.Item.GetRootItem(cacheKey);
    if (typeof (rootItem) === "undefined")
        throw "please create item first.";
    if (rootItem.Status == Restore.Item.UnSelectedStatus)
        return false;
    return true;
}

// @return {Id:0, ItemData:{}, Status:0, UnloadedChildrenStatus:0, TotalChildCount:100, LoadedChildren:[], ItemType}. 
Restore.Item.GetTotalSelectItems = function (cacheKey) {
    var rootItem = Restore.Item.GetRootItem(cacheKey);
    if (typeof (rootItem) === "undefined")
        throw "please create item first.";

    var result = Restore.Item._ConvertItemForSelected(rootItem);
    return result;
};

Restore.Item._GetChildItem = function (childData) {
    var result = [];

    var key;

    for (key in childData) {
        if (childData.hasOwnProperty(key)) {
            var item = childData[key];
            var tempResult = Restore.Item._ConvertItemForSelected(item);
            if (tempResult != null) {
                result.push(tempResult);
            }
        }
    }
    return result;
};

Restore.Item._ConvertItemForSelected = function (item) {
    var result = {};
    result.Id = item.Id;
    //result.ItemData = item.ItemData;
    result.Status = item.Status;
    result.UnloadedChildrenStatus = item.UnloadChildStatus;
    result.TotalChildCount = item.ChildItemCount;
    result.ItemType = item.ItemType;
    result.SelectedChildCount = item._GetSelectedChildCount(); //Object.size(item.SelectedLoadedChildren);
    result.LoadedChildrenCount = Object.size(item.ChildrenLoaded);
    result.LoadedChildren = Restore.Item._GetChildItem(item.ChildrenLoaded);
    result.DisplayName = item.DisplayName;
    if (item.ItemData.OtherInformation)
        result.ItemData = JSON.stringify(item.ItemData.OtherInformation);
    return result;
};

Restore.Item.CreateItem = function (itemId, itemData, childItemCount, itemType, displayName, cacheKey, modifyHtmlForStatusChangeCallback) {
    var item = Restore.Item.GetItem(itemId, cacheKey);
    if (item == null) {
        item = new Restore.Item(itemId, itemData, childItemCount, itemType, displayName, modifyHtmlForStatusChangeCallback);
        Restore.Item._SaveItem(itemId, item, cacheKey);
    }
    else {
        item.ModifyHtmlForStatusChangeCallback = modifyHtmlForStatusChangeCallback;
    }

    return item;
};

Restore.Item.GetStatus = function (dom) {
    if (dom.is("." + Restore.Item.UnSelectedCss))
        return Restore.Item.UnSelectedStatus;
    else if (dom.is("." + Restore.Item.SelectedCss))
        return Restore.Item.SelectedStatus;
    else
        return Restore.Item.IndeterminateStatus;
};

Restore.Item.GetNewStatus = function (oldStatus) {
    switch (oldStatus) {
        case Restore.Item.SelectedStatus:
            return Restore.Item.UnSelectedStatus;
        default:
            return Restore.Item.SelectedStatus;
    }
};

Restore.Item.ModifyHtmlForStatusChange = function (itemId, range, data) {
    var domArray = $(".restore-check[itemid='" + itemId + "']");

    $.each(domArray, function (i, item) {
        var dom = $(item);
        if (data.NewStatus == Restore.Item.SelectedStatus) {
            dom.removeClass(Restore.Item.UnSelectedCss);
            dom.removeClass(Restore.Item.IndeterminateCss);
            dom.addClass(Restore.Item.SelectedCss);
            if (dom.is("input[type='checkbox']")) {
                //dom[0].checked = true;
                dom.tristate('state', true);
            }
        }
        else if (data.NewStatus == Restore.Item.UnSelectedStatus) {
            dom.addClass(Restore.Item.UnSelectedCss);
            dom.removeClass(Restore.Item.IndeterminateCss);
            dom.removeClass(Restore.Item.SelectedCss);
            if (dom.is("input[type='checkbox']")) {
                //dom[0].checked = false;
                dom.tristate('state', false);
            }
        }
        else {
            dom.removeClass(Restore.Item.UnSelectedCss);
            dom.addClass(Restore.Item.IndeterminateCss);
            dom.removeClass(Restore.Item.SelectedCss);
            if (dom.is("input[type='checkbox']"))
                //dom[0].checked = false;
                dom.tristate('state', null);
        }
    });
};

Restore.Item.prototype.AddChild = function (item) {
    var self = this;
    var isItemExist = typeof (this.ChildrenLoaded[item.Id]) !== "undefined";
    this.ChildrenLoaded[item.Id] = item;
    item.ParentId = this.Id;
    item._AddEvent(Restore.Item.ItemStatusChanged, this.Id, function (childItem, childItemData) {
        self._OnItemStatusChanged(childItem, childItemData);
    }) // parent listen child status change.

    var itemStatus = Restore.Item.UnSelectedStatus;
    switch (this.Status) {
        case Restore.Item.SelectedStatus:
        case Restore.Item.UnSelectedStatus:
            itemStatus = this.Status;
            break;
        case Restore.Item.IndeterminateStatus:
            if (!isItemExist) {
                if (this.UnloadChildStatus == Restore.Item.IndeterminateStatus)
                    throw "not support this unload child status";
                itemStatus = this.UnloadChildStatus;
            }
            else {
                itemStatus = item.Status;
            }
            break;
        default:
            throw "not support status";
    }

    //switch (itemStatus) {
    //    case Restore.Item.SelectedStatus:
    //        this.SelectedLoadedChildren[item.Id] = 1;
    //        break;
    //    case Restore.Item.UnSelectedStatus:
    //        if (typeof (this.SelectedLoadedChildren[item.Id] !== "undefined"))
    //            delete this.SelectedLoadedChildren[item.Id];
    //        break;
    //    case Restore.Item.IndeterminateStatus:
    //        break;
    //    default:
    //        throw "not support item status";
    //}

    item._checkDomStatus(itemStatus);
    item._ChangeSelectStatus(itemStatus);
    item._ChangeUnloadChildStatus(itemStatus);
    item.Status = itemStatus;
};

// when client click the dom object to change status, we need call this function.
Restore.Item.prototype.Select = function (oldStatus, newStatus) {
    // 1. Change self status; should be event;
    this._ChangeSelectStatus(newStatus);
    this._ChangeUnloadChildStatus(newStatus);
    // 2. Change parent select status; should be event.
    this._TriggerEvent(Restore.Item.ItemStatusChanged, { CurrentStatus: newStatus, OldStatus: oldStatus });
    // 3. Change selectedChild status; 
    this._ChangeChildrenSelectStatus(newStatus);
};


Restore.Item.prototype._AddEvent = function (type, parentId, callback) {
    if (typeof (this.Event[type]) === "undefined")
        this.Event[type] = [];

    this.Event[type][parentId] = callback;
};

Restore.Item.prototype._TriggerEvent = function (type, data) {
    if (typeof (this.Event[type]) !== "undefined" && typeof (this.Event[type][this.ParentId]) !== "undefined") {
        this.Event[type][this.ParentId](this, data);
    }
};

Restore.Item.prototype._GetSelectedChildCount = function () {
    var key;
    var count = 0;
    for (key in this.ChildrenLoaded) {
        if (this.ChildrenLoaded.hasOwnProperty(key)) {
            var item = this.ChildrenLoaded[key];
            if (item.Status == Restore.Item.SelectedStatus) {
                count++;
            }
        }
    }
    return count;
};

Restore.Item.prototype._GetLoadedChildrenStatus = function () {
    var key;
    var isAllSelected = true;
    var isAllUnSelected = true;
    var isIndeterminate = false;
    var isContainSelectedItem = false;
    var hasChild = false;
    for (key in this.ChildrenLoaded) {
        if (this.ChildrenLoaded.hasOwnProperty(key)) {
            hasChild = true;
            var item = this.ChildrenLoaded[key];
            if (item.Status != Restore.Item.SelectedStatus) {
                isAllSelected = false;
            }
            else {
                isContainSelectedItem = true;
            }

            if (item.Status != Restore.Item.UnSelectedStatus) {
                isAllUnSelected = false;
            }
            if(item.Status == Restore.Item.IndeterminateStatus) {
                isIndeterminate = true;
            }
        }
    }

    if (!hasChild)
        return Restore.Item.UnSelectedStatus;

    if (isAllUnSelected)
        return Restore.Item.UnSelectedStatus;

    if (isAllSelected)
        return Restore.Item.SelectedStatus;
    
    if (isIndeterminate || isContainSelectedItem)
        return Restore.Item.IndeterminateStatus;
    throw "Not support";
};

Restore.Item.prototype._OnItemStatusChanged = function (childItem, childItemStatusData) {
    //if (childItemStatusData.CurrentStatus == Restore.Item.SelectedStatus) {
    //    this.SelectedLoadedChildren[childItem.Id] = 1;
    //}
    //else {//if (childItemStatusData.CurrentStatus == Restore.Item.UnSelectedStatus) {
    //    if (typeof (this.SelectedLoadedChildren[childItem.Id]) !== "undefined") {
    //        delete this.SelectedLoadedChildren[childItem.Id];
    //    }
    //}
    //else {
    //    throw "not support the status."
    //}

    var newStatus = Restore.Item.UnSelectedStatus;
    var childCount = Object.size(this.ChildrenLoaded);
    //var childSelectCount = Object.size(this.SelectedLoadedChildren);
    var childLoadedStatus = this._GetLoadedChildrenStatus();

    if (childCount == this.ChildItemCount) { // if all children loaded
        newStatus = childLoadedStatus;
        //if (childSelectCount == childCount)
        //    newStatus = Restore.Item.SelectedStatus;
        //else {
        //    if (childSelectCount > 0)
        //        newStatus = Restore.Item.IndeterminateStatus;
        //    else if (childItemStatusData.CurrentStatus == Restore.Item.IndeterminateStatus)
        //        newStatus = Restore.Item.IndeterminateStatus;
        //    else
        //        newStatus = Restore.Item.UnSelectedStatus;
        //}
    }
    else { // if not all children loaded
        if (childLoadedStatus == this.UnloadChildStatus && this.UnloadChildStatus == Restore.Item.SelectedStatus) {
            newStatus = Restore.Item.SelectedStatus;
        }
        else if (childLoadedStatus == this.UnloadChildStatus && this.UnloadChildStatus == Restore.Item.UnSelectedStatus)
            newStatus = Restore.Item.UnSelectedStatus;
        else
            newStatus = Restore.Item.IndeterminateStatus;
    }

    if (this.Status != newStatus) {
        this._ChangeSelectStatus(newStatus);
        this._ChangeUnloadChildStatus(newStatus);
        this._TriggerEvent(Restore.Item.ItemStatusChanged, { CurrentStatus: newStatus, OldStatus: this.Status });
    }
};

// can not trigger statuschange event.
Restore.Item.prototype._ChangeChildrenSelectStatus = function (status) {

    var key;

    for (key in this.ChildrenLoaded) {
        if (this.ChildrenLoaded.hasOwnProperty(key)) {
            var item = this.ChildrenLoaded[key];
            item._ChangeSelectStatus(status);
            item._ChangeUnloadChildStatus(status);
            item._ChangeChildrenSelectStatus(status);
            //if (status == Restore.Item.SelectedStatus) {
            //    this.SelectedLoadedChildren[item.Id] = 1;
            //}
            //else if (status == Restore.Item.UnSelectedStatus) {
            //    if (typeof (this.SelectedLoadedChildren[item.Id]) !== "undefined")
            //        delete this.SelectedLoadedChildren[item.Id];
            //}
        }
    }
};

// can not trigger statuschange event.
// only modify the status and html.
Restore.Item.prototype._ChangeSelectStatus = function (status) {

    if (this.Status != status) {
        //alert("itemId = " + this.DisplayName + " newStatus:" + status + " oldStatus:" + this.Status);
        this.ModifyHtmlForStatusChangeCallback(this, { NewStatus: status, OldStatus: this.Status, Id: this.Id });
        this.Status = status;
    }
};

Restore.Item.prototype._ChangeUnloadChildStatus = function (status) {
    if (status == Restore.Item.SelectedStatus) {
        this.UnloadChildStatus = Restore.Item.SelectedStatus;
    }
    else if (status == Restore.Item.UnSelectedStatus) {
        this.UnloadChildStatus = Restore.Item.UnSelectedStatus;
    }
};

Restore.Item.prototype._checkDomStatus = function (status) {
    this.ModifyHtmlForStatusChangeCallback(this, { NewStatus: status, OldStatus: this.Status, Id: this.Id });
};