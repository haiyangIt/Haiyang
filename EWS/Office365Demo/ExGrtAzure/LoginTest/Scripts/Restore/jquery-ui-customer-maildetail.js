$.widget("custom.maildetail",
           {
               options: {
                   //getdataurl: null,
                   pageCount: 10,
                   //folderId: null,
                   onSelectItem: null,
                   cacheKey: Restore.Item.KeyForRestore
               },
               _pageIndex: 0,
               _pagation: null,
               _css: "maildetail",
               _tableId: "maildetail-table",
               _selectAllCheckbox: null,
               _mailTable: null,
               _mailId2Mail: [],
               _page2Items: [],
               _folderId: null,
               _totalCount: 0,
               _pageCount: 10,
               _catalogJob: null,
               _ajaxUrl: "",
               _isInit: false,
               _create: function () {
                   var self = this;
                   this.element.addClass(self._css);
                   self._initVals();
               },

               _initVals: function () {
                   this._pageIndex = 0;
                   this._pagation = null;
                   this._selectAllCheckbox = null;
                   this._mailTable = null;
                   this._mailId2Mail = [];
                   this._page2Items = [];
                   this._folderId = null;
                   this._totalCount = 0;
               },

               update: function (updateData) {
                   var self = this;
                   if (updateData && updateData != null) {
                       var folderId = updateData.data.folderId;
                       if (self._folderId == null || self._folderId != folderId) {
                           self._removeAllElement();
                           self._destoryVals();
                           self._initVals();

                           updateData.data.pageCount = updateData.data.pageCount || this._pageCount;
                           updateData.data.pageIndex = updateData.data.pageIndex || this._pageIndex;

                           self._initElement(updateData);
                       }
                   }
                   else {
                       self._removeAllElement();
                       self._destoryVals();
                       self._initVals();
                   }
               },

               _initElement: function (updateData) {
                   var self = this;
                   if (typeof (updateData) !== "undefined") {
                       this._isInit = true;
                       this.element.addClass(self._css);
                       this._mailTable = $("<div class='restore-table'></div").appendTo(this.element);
                       this._pagation = $("<div class='restore-maildetail-pagination' ></div>").appendTo(this.element);
                       this._pagation = $("<ul class='pagination maildetail-pagination'></ul>").appendTo(this._pagation);

                       self._folderId = updateData.data.folderId;
                       self._pageCount = updateData.data.pageCount || self._pageCount;
                       self._catalogJob = updateData.data.catalogJob;
                       self._ajaxUrl = updateData.url;
                       updateData.data.catalogJob.StartTime = Restore.GetDateTime(updateData.data.catalogJob.StartTime);

                       Arcserve.DataProtect.Util.Post(updateData.data,
                           updateData.url,
                           function (data) {
                               if (data && data != null) {
                                   self._totalCount = data.TotalCount;

                                   self._renderTable(data.Mails);
                                   self._renderPagation(data.TotalCount);
                               }
                               else {
                                   self._removeAllElement();
                               }
                           });
                       
                   }
                   else {
                       this._renderPagation(0);
                   }
                   this._listen();
               },

               _convertDateTime: function (mails) {
                   if (mails && mails.length > 0) {
                       for (var i = 0 ; i < mails.length; i++) {
                           mails[i].OtherInformation.StartTime = Restore.GetDateTime(mails[i].OtherInformation.StartTime);
                           mails[i].OtherInformation.CreateTime = Restore.GetDateTime(mails[i].OtherInformation.CreateTime);
                       }
                   }
               },

               _removeAllElement: function () {
                   var self = this;
                   if (this._isInit) {
                       this._removeListen();
                       this._pagation.bootstrapPaginator("destroy");
                       this.element
                           .removeClass(self._css)
                           .text("");
                       this._isInit = false;
                   }
               },

               _destoryVals: function () {
                   if (this._page2Items)
                       delete this._page2Items;
                   if (this._mailId2Mail)
                       delete this._mailId2Mail;
               },

               _destroy: function () {
                   this._removeAllElement();
               },

               _renderTable: function (datas) {
                   var self = this;
                   self._convertDateTime(datas);
                   var html = Mustache.to_html($("#maildetailTemplate").html(), { mails: datas });
                   this._mailTable.empty().append(html);
                   $("input[type='checkbox']", this._mailTable).tristate();

                   this._page2Items[self._pageIndex] = datas;
                   $.each(datas, function (i, item) {
                       self._mailId2Mail[item.Id] = item;
                   });

                   this._addItemToTree(this._folderId, this._totalCount, datas);
               },

               _addItemToTree: function (parentId, parentChildCount, childContainers) {
                   var self = this;
                   var restoreItem = Restore.Item.GetItem(parentId, self.options.cacheKey);

                   $.each(childContainers, function (i, item) {
                       var childItem = Restore.Item.CreateItem(item.Id, item, 0, item.ItemType, item.DisplayName, item.CanSelect, self.options.cacheKey, function (event, data) {
                           self._modifyHtmlForCheckStatusChange(data);
                       });
                       restoreItem.AddChild(childItem);
                   });
               },

               _modifyHtmlForCheckStatusChange: function (data) {
                   Restore.Item.ModifyHtmlForStatusChange(data.Id, this.element, data);
               },

               _renderPagation: function (totalCount) {
                   var self = this;
                   var currentPage = self._pageIndex + 1;

                   var totalPages = Math.ceil(totalCount / self.options.pageCount);
                   if (totalCount == 0) {
                       currentPage = 1;
                       totalPages = 1;
                   }
                   var pageSizeOptions = {
                       size: "small",
                       bootstrapMajorVersion: 3,
                       currentPage: currentPage,
                       numberOfPages: self.options.pageCount,
                       totalPages: totalPages,
                       onPageChanged: function (e, lastPage, currentPage) {
                           self._onPageChanged(lastPage, currentPage);
                       }
                   };

                   self._pagation.bootstrapPaginator(pageSizeOptions);
               },

               _listen: function () {
                   var self = this;
                   self._mailTable.bind("click.ui.customer.maildetail", function (e) {
                       self._clickItem(e);
                   });
               },

               _removeListen: function () {
                   var self = this;
                   self._mailTable.unbind("click.ui.customer.maildetail");
               },

               _clickItem: function (e) {
                   var self = this;
                   $target = $(e.target);

                   var tr = $target.is("tbody>tr") ? $target : $target.parents("tbody>tr");
                   if (tr.length) {
                       if ($target.is(".restore-check[itemid]")) {
                           var id = $target.attr("itemid");
                           var restoreItem = Restore.Item.GetItem(id, self.options.cacheKey);

                           var oldStatus = Restore.Item.GetStatus($target);
                           var newStatus = Restore.Item.GetNewStatus(oldStatus);
                           restoreItem.Select(oldStatus, newStatus);

                           if (typeof (self.options.onSelectItem) === "function") {
                               var item = self._mailId2Mail[id];
                               self.options.onSelectItem(item);
                           }
                       }

                       $("tbody > tr.warning", this._mailTable).toggleClass("warning");
                       tr.toggleClass("warning");
                   }
               },

               _onPageChanged: function (lastPage, currentPage) {
                   var self = this;
                   self._pageIndex = currentPage - 1;
                   var items = this._page2Items[currentPage - 1];

                   if (items) {
                       self._renderTable(items);
                   }
                   else {

                       var postData = { catalogJob: self._catalogJob, folderId: self._folderId, pageIndex: currentPage - 1, pageCount: self._pageCount };
                       postData.catalogJob.StartTime = Restore.GetDateTime(postData.catalogJob.StartTime);

                       Arcserve.DataProtect.Util.Post(postData,
                           self._ajaxUrl,
                           function (data) {
                               self._renderTable(data.Mails);
                           });
                   }
               },

               _setOption: function (key, value) {
                   this._super(key, value);
               },

               _setOptions: function (options) {
                   this._super(options);
               }
           });