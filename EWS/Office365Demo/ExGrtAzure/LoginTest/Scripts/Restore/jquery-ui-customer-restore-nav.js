/* public class Item
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public object OtherInformation { get; set; }
        public List<Item> Container { get; set; }
        public List<Item> Leaf { get; set; }

        /// <summary>
        /// Container.Count + leaf.Count
        /// </summary>
        public int ChildCount { get; set; }
        public string ItemType { get; set; }
    }
*/

$.widget("custom.restorenav",
           {
               options: {
                   pagecount: 10,
                   onSelect: null,
                   cacheKey: Restore.Item.KeyForRestore
               },
               _properties: {
                   css: "restorenav"
               },
               isPage: true,
               isShowCount: false,
               pageElement: null,
               navList: null,
               pageIndex: 0,
               navDatasId2Item: null,
               checkAllElement: null,
               _selectedId: null,
               _parentId: null,
               _create: function () {
                   var self = this;
                   if ($(this.element).is("." + this._properties.css)) {
                       this._destroy();
                   }

                   this.element.addClass(this._properties.css);
                   this.element.addClass("container");

                   var layoutRow = $('<div class="row"></div>').appendTo(this.element);
                   var topCol = $('<div class="col-md-12"></div>').appendTo(layoutRow);
                   //var bottomCol = $('<div class="col-md-12"></div>').appendTo(layoutRow);

                   var checkAllDiv = $('<div class="checkall-div"></div>').appendTo(topCol);
                   var checkAllA = $("<a href='javascript:void(0);' ></a>").appendTo(checkAllDiv);

                   this._createSelectAllCheckbox(checkAllA);

                   var navListDiv = $('<div class="col-md-12 restore-nav-list"></div>').appendTo(layoutRow);
                   this.navList = $('<ul class="nav nav-pills nav-stacked restore-nav-content"></ul>').appendTo(navListDiv);

                   if (this.isPage) {
                       this.pageElement = $("<div class='list-pagation'></div>").appendTo(navListDiv);
                       this.pageElement = $("<ul class='pagination userfolders-pagination'></ul>").appendTo(this.pageElement);
                   }

                   //this.checkAllElement = $("#selectall");

                   self.pageIndex = 0;

                   self._listen();

                   //if (typeof(self.options.getdataargs) !== "undefined") {
                   //    self.update(self.options.getdataargs);
                   //}
               },


               _createSelectAllCheckbox: function (parentDom) {

               },

               // @updateData: {url: "ajaxUrl", data:{}}
               update: function (updateData) {
                   var self = this;
                   if (updateData && updateData != null)
                       self._update(updateData);
                   else {
                       self._clear();
                   }
               },

               _update: function (ajaxInfo) {
                   var self = this;
                   this._ajaxInfo = ajaxInfo;
                   self._setOtherInformation();

                   Arcserve.DataProtect.Util.Post(ajaxInfo.data, ajaxInfo.url, function (data) {
                       if (data && data != null) {
                           self.catalogTime = data.CatalogTime;
                           self.navDatas = data.Details;

                           //self.navDatasId2Item = [];
                           //self._createDic(self.navDatas, self.navDatasId2Item);

                           if (self.isPage)
                               self._updatePage();

                           if (self.navDatas.length) {
                               self._updateNavItem();

                               self._addToTree(self._parentId, self.navDatas.length, self.navDatas);

                               $("li:first", self.navList).click();
                           }
                       }
                       else {
                           self._clear();
                       }
                   })
               },

               _clear: function () {
                   this.catalogTime = null;
                   this.navDatas = [];
                   this.navDatasId2Item = [];
                   this.pageIndex = 0;
                   if (this.isPage) {
                       this._updatePage();
                   }

                   this.navList.html("");
                   this._clearOtherInfo();
               },

               _clearOtherInfo: function () {

               },

               _setOtherInformation: function () {

               },

               _addToTree: function (parentId, parentChildCount, childContainers) {
                   var self = this;
                   var restoreItem = Restore.Item.GetItem(parentId, self.options.cacheKey);
                   if (parentId == "0" && restoreItem == null) {
                       restoreItem = Restore.Item.CreateItem(parentId, {}, parentChildCount, "Organization", "Root", 1, self.options.cacheKey, function (event, data) {
                           self._modifyCheckBoxStatus(data);
                       });
                   }

                   $.each(childContainers, function (i, item) {

                       var childItem = Restore.Item.CreateItem(item.Id, item, item.ChildCount, item.ItemType, item.DisplayName, item.CanSelect, self.options.cacheKey, function (event, data) {
                           self._modifyCheckBoxStatus(data)
                       });
                       restoreItem.AddChild(childItem);

                       if (item.Container && item.Container.length) {
                           self._addToTree(item.Id, item.ChildCount, item.Container);
                       }
                   });
               },

               _modifyCheckBoxStatus: function (data) {
                   Restore.Item.ModifyHtmlForStatusChange(data.Id, this.element, data);
               },

               _createDic: function (navData, dic) {
                   var self = this;
                   $.each(navData, function (i, item) {
                       dic[item.Id] = item;
                       if (item.Container) {
                           self._createDic(item.Container, dic);
                       }
                   });
               },

               _destroy: function () {
                   var self = this;
                   this._removeListen();
                   if (self.isPage)
                       this.pageElement.bootstrapPaginator("destroy");
                   this.element
                       .removeClass(this._properties.css)
                       .text("");
               },

               _createCheckboxItem: function (id, itemId, text, canSelect, appendDom) {
                   var disabledAttribute = canSelect == 1 ? "" : "disabled"
                   var dom = $('<input type="checkbox" class="restore-check restore-unselected '+disabledAttribute+'" itemid="' + itemId + '" id="' + id + '" ' + disabledAttribute + ' />').appendTo(appendDom);
                   var spanDom = $('<span class="nav-checkbox-span '+disabledAttribute+'">' + text + '</span>').appendTo(appendDom)
                   dom.tristate();
               },

               _listen: function () {
                   var self = this;
                   self.checkAllElement.bind("click.ui.customer.restorenav.selectall", function (e) {
                       self._onSelectAllItem(e);
                   });
                   self.navList.bind("click.ui.customer.restorenav.select", function (e) {
                       self._onClickItem(e);
                   });
               },

               _removeListen: function () {
                   var self = this;
                   self.checkAllElement.unbind("click.ui.customer.restorenav.selectall");
                   self.navList.unbind("click.ui.customer.restorenav.select");
               },

               _onSelectAllItem: function (e) {

               },
               _onSelectItem: function (e) {
                   var $target = $(e.target);
                   var id = $target.attr("itemid");
                   this._onSelectByItemId(id, $target);
               },

               _onSelectByItemId: function (itemId, dom) {
                   var self = this;
                   var restoreItem = Restore.Item.GetItem(itemId, self.options.cacheKey);

                   var oldStatus = Restore.Item.GetStatus(dom);
                   var newStatus = Restore.Item.GetNewStatus(oldStatus);
                   restoreItem.Select(oldStatus, newStatus);
               },

               _onClickItem: function (e) {
                   var self = this;
                   var $target = $(e.target);

                   var li = $target.is("li") ? $target : $target.parents("li");
                   if ($target.is(".disabled"))
                       return;
                   if (li.length > 1)
                       li = $(li[0]);

                   if (li.length) {
                       var id = li.attr("itemid");
                       var item = Restore.Item.GetItem(id, self.options.cacheKey); // self.navDatasId2Item[id];

                       if (id != self._selectedId) {
                           self._selectedId = id;
                           self.element.trigger("onSelect", item);
                       }

                       //if (typeof (self.options.onSelect) === "function") {
                       //    if (id != self._selectedId) {
                       //        self._selectedId = id;
                       //        self.options.onSelect(item);
                       //    }
                       //}

                       $("li.active", this.element).toggleClass("active");
                       li.toggleClass("active");

                       if ($target.is("span.open-children")) {
                           if (item.ItemData.Container && item.ItemData.Container.length) {
                               $target.toggleClass("glyphicon-chevron-right").toggleClass("glyphicon-chevron-down");
                               $("ul.nav-children:first", $target.parent().parent()).toggleClass("hidden");
                           }
                           else
                               self.element.trigger("onOpenAndGetChildData", { item: item, target: $target });

                           self.element.trigger("onExpandChild", { item: item, target: $target });
                       }
                       else if ($target.is(".restore-check[itemid]")) {
                           self._onSelectItem(e);
                       }
                   }
               },

               _updateNavItem: function () {
                   var self = this;
                   self.navList.html("");

                   self._updateNavChildren(self.navDatas, self.navList, true, 0);
               },

               _updateNavChildren: function (childItems, navList, isRoot, levelIndex) {
                   var self = this;
                   var startIndex = 0;
                   var count = self.options.pagecount;
                   if (isRoot) {
                       if (self.isPage)
                           startIndex = self.pageIndex * self.options.pagecount;
                       else
                           count = childItems.length;
                   }
                   else {
                       startIndex = 0;
                       count = childItems.length;
                   }

                   for (var i = startIndex, j = 0; j < count && i < childItems.length; j++, i++) {
                       var item = childItems[i];

                       var ids = self._getNavItemId(item.Id);
                       var attribute = "";
                       if (item.CanSelect == "0") {
                           attribute = "disabled";
                       }
                       var eachItemElement = $("<li class='restorenav-item "+attribute+"' itemid='" + item.Id + "' " + attribute + "></li>").appendTo(navList);

                       var anchorElement = $("<a href='javascript:void(0);' " + attribute + "></a>").appendTo(eachItemElement);
                       this._createCheckboxItem(ids.checkbox, item.Id, item.DisplayName, item.CanSelect, anchorElement);


                       var hasChildren = item.Container && item.Container.length;
                       var containerLength = 0;
                       if (hasChildren) {
                           $('<span class="glyphicon glyphicon-chevron-right navbar-right open-children" ' + attribute + '></span>').appendTo(anchorElement);
                           containerLength = item.Container.length;
                       }

                       if (this.isShowCount)
                           $('<span ' + attribute + ' class="badge navbar-right ' + (hasChildren ? ' ' : ' restore-nav-placehold ') + '">' + (item.ChildCount - containerLength) + '</span>').appendTo(anchorElement);

                       if (hasChildren) {
                           var childList = $('<ul class="nav nav-pills nav-stacked hidden nav-children" ' + attribute + '></ul>').appendTo(eachItemElement);
                           self._updateNavChildren(item.Container, childList, false, levelIndex + 1);
                       }
                   }

               },

               _getNavItemId: function (id) {
                   return {
                       "checkbox": id + "-checkbox",
                       "a": id + "-a",
                       "li": id + "-li"
                   };
               },
               _updatePage: function () {
                   var self = this;
                   var totalPages = Math.ceil(self.navDatas.length / self.options.pagecount);

                   if (!self.navDatas || self.navDatas.length == 0) {
                       totalPages = 1;
                       self.pageIndex = 0;
                   }

                   var pageSizeOptions = {
                       size: "small",
                       bootstrapMajorVersion: 3,
                       currentPage: self.pageIndex + 1,
                       numberOfPages: self.options.pagecount,
                       totalPages: totalPages,
                       onPageChanged: function (e, lastPage, currentPage) {
                           self._onPageChanged(lastPage, currentPage);
                       }
                   };

                   self.pageElement.bootstrapPaginator(pageSizeOptions);
               },

               _onPageChanged: function (lastPage, currentPage) {
                   this.pageIndex = currentPage - 1;
                   this._updateNavItem();
               },

               _setOption: function (key, value) {
                   if (key == "pagecount") {
                       if (this.isPage)
                           this.pageElement.bootstrapPaginator({ "numberOfPages": value });
                   }

                   this._super(key, value);
               },
               _setOptions: function (options) {
                   this._super(options);
               }
           });

$.widget("custom.mailboxnav", $.custom.restorenav, {
    isPage: true,
    isShowCount: false,
    _setOtherInformation: function () {
        this._parentId = "0";
    },
    _createSelectAllCheckbox: function (parentDom) {
        this._createCheckboxItem("selectallInMailbox", "0", "Select All", 1, parentDom);
        this.checkAllElement = $("#selectallInMailbox");
    },
    _onSelectAllItem: function (e) {
        this._onSelectByItemId("0", this.checkAllElement);
    },
    _clearOtherInfo: function () {
        Restore.Item.Clear(this.options.cacheKey);
    }
});

$.widget("custom.foldernav", $.custom.restorenav, {
    isPage: false,
    isShowCount: true,
    _rootFolderId: null,
    _setOtherInformation: function () {

    },
    _createSelectAllCheckbox: function (parentDom) {
        this._createCheckboxItem("selectallInFolder", "", "Select All", 1, parentDom);
        this.checkAllElement = $("#selectallInFolder");
    },
    _onSelectAllItem: function (e) {
        this._onSelectByItemId(this._rootFolderId, this.checkAllElement);
    },
    _clearOtherInfo: function () {
        this._rootFolderId = null;
        this._parentId = null;
    },
    setRootFolderId: function (rootFolderId) {
        this._rootFolderId = rootFolderId;
        this._parentId = rootFolderId;
        //this.checkAllElement.attr["itemid"] = this._rootFolderId;
        this.checkAllElement.attr("itemid", this._rootFolderId);
        var selectAllItem = Restore.Item.GetItem(this._rootFolderId, this.options.cacheKey);
        selectAllItem._checkDomStatus(selectAllItem.Status);
    }
});


// load the child step by step.
$.widget("custom.foldernavForSteped", $.custom.foldernav, {
    _updateNavChildren: function (childItems, navList, isRoot, levelIndex) {
        var self = this;
        var startIndex = 0;
        var count = self.options.pagecount;
        if (isRoot) {
            if (self.isPage)
                startIndex = self.pageIndex * self.options.pagecount;
            else
                count = childItems.length;
        }
        else {
            startIndex = 0;
            count = childItems.length;
        }

        for (var i = startIndex, j = 0; j < count && i < childItems.length; j++, i++) {
            var item = childItems[i];
            var disabledAttribute = item.CanSelect == 1 ? "" : "disabled";
            var ids = self._getNavItemId(item.Id);
            var eachItemElement = $("<li class='restorenav-item "+disabledAttribute+"' itemid='" + item.Id + "' " + disabledAttribute + "></li>").appendTo(navList);

            var anchorElement = $("<a href='javascript:void(0);' " + disabledAttribute + "></a>").appendTo(eachItemElement);
            this._createCheckboxItem(ids.checkbox, item.Id, item.DisplayName, item.CanSelect, anchorElement);

            var hasChildren = item.Container && item.Container.length;

            var containerLength = 0;
            if (hasChildren) {
                $('<span class="glyphicon glyphicon-chevron-right navbar-right open-children" ' + disabledAttribute + '></span>').appendTo(anchorElement);
                containerLength = item.Container.length;
            }
            else if (item.ChildCount > 0) {
                $('<span class="glyphicon glyphicon-chevron-right navbar-right open-children" ' + disabledAttribute + '></span>').appendTo(anchorElement);
            }


            if (this.isShowCount) {
                if (hasChildren) {
                    $('<span ' + disabledAttribute + ' class="badge navbar-right ' + (hasChildren ? ' ' : ' restore-nav-placehold ') + '">' + (item.ChildCount - containerLength) + '</span>').appendTo(anchorElement);
                }
                else if (item.ChildCount > 0) {
                    $('<span ' + disabledAttribute + ' class="badge navbar-right restore-nav-placehold "></span>').appendTo(anchorElement);
                }
            }


            if (hasChildren) {
                var childList = $('<ul class="nav nav-pills nav-stacked hidden nav-children" ' + disabledAttribute + '></ul>').appendTo(eachItemElement);
                self._updateNavChildren(item.Container, childList, false, levelIndex + 1);
            }
        }

    },
    _addToTree: function (parentId, parentChildCount, childContainers) {
        var self = this;
        var restoreItem = Restore.Item.GetItem(parentId, self.options.cacheKey);
        restoreItem.ChildItemCount = parentChildCount;
        if (parentId == "0" && restoreItem == null) {
            restoreItem = Restore.Item.CreateItem(parentId, {}, parentChildCount, "Organization", "Root", 1, self.options.cacheKey, function (event, data) {
                self._modifyCheckBoxStatus(data);
            });
        }

        $.each(childContainers, function (i, item) {
            var childItem = Restore.Item.CreateItem(item.Id, item, item.ChildCount, item.ItemType, item.DisplayName, item.CanSelect, self.options.cacheKey, function (event, data) {
                self._modifyCheckBoxStatus(data)
            });
            restoreItem.AddChild(childItem);

            if (item.Container && item.Container.length) {
                self._addToTree(item.Id, item.ChildCount, item.Container);
            }
        });
    },

    updateChildContainer: function (parentId, childContainerData) {
        var self = this;
        var parentLi = $("li[itemid='" + parentId + "']", this.element);
        // 1. check the childContainerData has any data, if no data, set the parent Item has no child and remove the right chevron
        if (typeof (childContainerData) === "undefined" || childContainerData == null || childContainerData.length == 0) {
            $("span.open-children", parentLi).remove();
        }
            // 2. if contains child data, update .
        else {
            $("span.badge", parentLi).removeClass("restore-nav-placehold").text(childContainerData.length);

            var $target = $(".glyphicon", parentLi);

            var childList = $('<ul class="nav nav-pills nav-stacked hidden nav-children" ></ul>').appendTo(parentLi);
            this._updateNavChildren(childContainerData, childList, false, 0); // todo
            this._addToTree(parentId, childContainerData.length, childContainerData);

            var parentItem = Restore.Item.GetItem(parentId, self.options.cacheKey); // this.navDatasId2Item[parentId];
            parentItem.Container = childContainerData;
            parentItem.ChildCount = childContainerData.length;

            $target.toggleClass("glyphicon-chevron-right").toggleClass("glyphicon-chevron-down");
            $("ul.nav-children:first", $target.parent().parent()).toggleClass("hidden");
        }
    }
});

; (function ($) {
})(jQuery);