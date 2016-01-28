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
                   onSelect: null
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

                   var layoutRow = $('<div class="row"></div>').appendTo(this.element);
                   var topCol = $('<div class="col-md-12"></div>').appendTo(layoutRow);
                   //var bottomCol = $('<div class="col-md-12"></div>').appendTo(layoutRow);

                   var checkAllDiv = $('<div class="checkall-div"></div>').appendTo(topCol);
                   this._createCheckboxItem("selectall", this._parentId, "Select All").appendTo(checkAllDiv);

                   var navListDiv = $('<div class="col-md-12 restore-nav-list"></div>').appendTo(layoutRow);
                   this.navList = $('<ul class="nav nav-pills nav-stacked restore-nav-content"></ul>').appendTo(navListDiv);

                   if (this.isPage)
                       this.pageElement = $("<ul class='pagination userfolders-pagination'></ul>").appendTo(navListDiv);

                   this.checkAllElement = $("#selectall");

                   self.pageIndex = 0;

                   self._listen();

                   //if (typeof(self.options.getdataargs) !== "undefined") {
                   //    self.update(self.options.getdataargs);
                   //}
               },

               update: function (updateData) {
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

                           self.navDatasId2Item = [];
                           self._createDic(self.navDatas, self.navDatasId2Item);

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
                   });
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
                   self._clearOtherInfo();
               },

               _clearOtherInfo:function(){

               },

               _setOtherInformation: function () {

               },

               _addToTree: function (parentId, parentChildCount, childContainers) {
                   var self = this;
                   var restoreItem = Restore.Item.GetItem(parentId);
                   if (parentId == "0" && restoreItem == null) {
                       restoreItem = Restore.Item.CreateItem(parentId, {}, parentChildCount, function (event, data) {
                           self._modifyCheckBoxStatus(data);
                       });
                   }

                   $.each(childContainers, function (i, item) {
                       var childItem = Restore.Item.CreateItem(item.Id, item, item.ChildCount, function (event, data) {
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

               _createCheckboxItem: function (id, itemId, text) {
                   return $('<input type="checkbox" class="restore-check restore-unselected" itemid="' + itemId + '" id="' + id + '" /><span class="nav-checkbox-span">' + text + '</span>')
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
                   var restoreItem = Restore.Item.GetItem(id);

                   var oldStatus = Restore.Item.GetStatus($target);
                   var newStatus = Restore.Item.GetNewStatus(oldStatus);
                   restoreItem.Select(oldStatus, newStatus);
               },

               _onClickItem: function (e) {
                   var self = this;
                   var $target = $(e.target);

                   var li = $target.is("li") ? $target : $target.parents("li");
                   if (li.length > 1)
                       li = $(li[0]);

                   if (li.length) {
                       var id = li.attr("itemid");
                       var item = self.navDatasId2Item[id];

                       if (id != self._selectedId) {
                           self._trigger("onSelect", self, item);
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
                           if (item.Container && item.Container.length) {
                               $target.toggleClass("glyphicon-chevron-right").toggleClass("glyphicon-chevron-down");
                               $("ul.nav-children:first", $target.parent().parent()).toggleClass("hidden");
                           }
                       }
                       else if ($target.is(".restore-check[itemid]")) {
                           self._onSelectItem(e);
                       }
                   }
               },

               _updateNavItem: function () {
                   var self = this;
                   self.navList.html("");

                   self._updateNavChildren(self.navDatas, self.navList, true);
               },

               _updateNavChildren: function (childItems, navList, isRoot) {
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
                       var eachItemElement = $("<li class='restorenav-item' itemid='" + item.Id + "'></li>").appendTo(navList);

                       var anchorElement = $("<a href='javascript:void(0);' ></a>").appendTo(eachItemElement);
                       this._createCheckboxItem(ids.checkbox, item.Id, item.DisplayName).appendTo(anchorElement);


                       var hasChildren = item.Container && item.Container.length;
                       var containerLength = 0;
                       if (hasChildren) {
                           $('<span class="glyphicon glyphicon-chevron-right navbar-right open-children" ></span>').appendTo(anchorElement);
                           containerLength = item.Container.length;
                       }

                       if (this.isShowCount)
                           $('<span class="badge navbar-right ' + (hasChildren ? ' ' : ' restore-nav-placehold ') + '">' + (item.ChildCount - containerLength) + '</span>').appendTo(anchorElement);

                       if (hasChildren) {
                           var childList = $('<ul class="nav nav-pills nav-stacked hidden nav-children" ></ul>').appendTo(eachItemElement);
                           self._updateNavChildren(item.Container, childList, false);
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
    _onSelectAllItem: function (e) {

    },
    _clearOtherInfo: function () {
        Restore.Item.Clear();
    }
});

$.widget("custom.foldernav", $.custom.restorenav, {
    isPage: false,
    isShowCount: true,
    _rootFolderId: null,
    _setOtherInformation: function () {
        this._rootFolderId = this._ajaxInfo.data.rootFolderId;
        this._parentId = this._rootFolderId;
    },

    _onSelectAllItem: function (e) {

    },
    _clearOtherInfo: function () {

    }
});

$.widget("custom.catalognav", $.custom.restorenav, {
    isPage: false,
    isShowCount: true,
    _rootFolderId: null,
    _setOtherInformation: function () {
        
    },

    _onSelectAllItem: function (e) {

    },
    _clearOtherInfo: function () {

    }
})

; (function ($) {
})(jQuery);