$.widget("custom.usermailboxesnav",
           {
               options: {
                   pagecount: 10,
                   getdataurl: "",
                   getdataargs: {},
                   onSelectItemChanged: null
               },
               _create: function () {
                   var self = this;
                   this.element.addClass("usermailboxesnav");

                   var layoutRow = $('<div class="row"></div>').appendTo(this.element);
                   var topCol = $('<div class="col-md-12"></div>').appendTo(layoutRow);
                   var bottomCol = $('<div class="col-md-12"></div>').appendTo(layoutRow);

                   var checkAllDiv = $('<div class="checkall-div"></div>').appendTo(topCol);
                   this._createCheckboxItem("selectall", "Select All").appendTo(checkAllDiv);

                   var mailboxListDiv = $('<div class="col-md-12"></div>').appendTo(layoutRow);
                   this.mailboxListNav = $('<ul class="nav nav-pills nav-stacked"></ul>').appendTo(mailboxListDiv);

                   this.pageElement = $("<ul class='pagination usermailboxes-pagination'></ul>").appendTo(mailboxListDiv);

                   this.checkAllElement = $("#selectall");

                   self.pageIndex = 0;

                   self._listen();

                   $.ajax({
                       url: this.options.getdataurl,
                       type: "POST",
                       data: this.options.getdataargs,
                       success: function (data) {
                           self.catalogTime = data.CatalogTime;
                           self.mailboxes = data.MailboxInfos;

                           self.mailboxesId2Item = [];
                           $.each(self.mailboxes, function (i, item) {
                               self.mailboxesId2Item[item.RootFolderId] = item;
                           })

                           
                           self._updatePage();

                           if (self.mailboxes.length) {
                               self._updateMailbox();
                               $("li:first", self.mailboxListNav).click();
                           }
                       }
                   })
               },

               _destroy: function () {
                   this._removeListen();
                   this.pageElement.bootstrapPaginator("destroy");
                   this.element
                       .removeClass("usermailboxesnav")
                       .text("");
               },

               _createCheckboxItem: function (id, text) {
                   return $('<input type="checkbox" id="' + id + '"><span class="usermailbox-checkbox-span">' + text + '</span>')
               },

               _listen: function () {
                   var self = this;
                   self.checkAllElement.bind("click.ui.customer.usermailboxes.selectallmailbox", function (e) {
                       self._onSelectAllItem(e);
                   });
                   self.mailboxListNav.bind("click.ui.customer.usermailboxes.selectmailbox", function (e) {
                       self._onClickItem(e);
                   });
               },

               _removeListen: function () {
                   var self = this;
                   self.checkAllElement.unbind("click.ui.customer.usermailboxes.selectallmailbox", self._onSelectAllItem);
                   self.mailboxListNav.unbind("click.ui.customer.usermailboxes.selectmailbox", self._onClickItem);
               },

               _onSelectAllItem: function (e) {

               },
               _onSelectItem: function (e) {

               },

               _onClickItem: function (e) {
                   var self = this;
                   var $target = $(e.target);

                   var li = $target.is("li") ? $target : $target.parents("li");

                   if(li.length){
                       if (typeof(self.options.onSelectItemChanged) === "function") {
                           var id = li.attr("itemid");
                           var item = self.mailboxesId2Item[id];
                           self.options.onSelectItemChanged(item);
                       }

                       $("li.active", this.element).toggleClass("active");
                       li.toggleClass("active");
                   }
               },

               _updateMailbox: function () {
                   var self = this;
                   self.mailboxListNav.html("");
                   var startIndex = self.pageIndex * self.options.pagecount;
                   for (var i = startIndex, j = 0; j < self.options.pagecount && i < self.mailboxes.length; j++, i++) {
                       var item = self.mailboxes[i];

                       var ids = self._getMailboxItemId(item.RootFolderId);
                       var eachItemElement = $("<li class='usermailboxes-mailboxitem' itemid='" + item.RootFolderId + "'></li>").appendTo(self.mailboxListNav);

                       var anchorElement = $("<a href='javascript:void(0);' ></a>").appendTo(eachItemElement);
                       this._createCheckboxItem(ids.checkbox, item.DisplayName).appendTo(anchorElement);
                   }
               },

               _getMailboxItemId: function (mailboxRootFolderId) {
                   return {
                       "checkbox": mailboxRootFolderId + "-checkbox",
                       "a": mailboxRootFolderId + "-a",
                       "li": mailboxRootFolderId + "-li"
                   };
               },
               _updatePage: function () {
                   var self = this;
                   var totalPages = Math.ceil(self.mailboxes.length / self.options.pagecount);
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
                   this._updateMailbox();
               },

               _setOption: function (key, value) {
                   if (key == "pagecount") {
                       this.pageElement.bootstrapPaginator({ "numberOfPages": value });
                   }

                   this._super(key, value);
               },
               _setOptions: function (options) {
                   this._super(options);
               }
           });