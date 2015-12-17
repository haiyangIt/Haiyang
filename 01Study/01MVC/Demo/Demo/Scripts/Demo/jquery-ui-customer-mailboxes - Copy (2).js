$.widget("custom.usermailboxes",
           {
               options: {
                   pagecount: 10,
                   getdataurl: "",
                   getdataargs: {},
                   onSelectItemChanged: null
               },
               _create: function () {
                   var self = this;
                   this.element.addClass("usermailboxes");

                   
                   this.checkAllNav = $("<ul class='nav nav-pills nav-stacked usermailboxes-mailboxlist'></ul>").appendTo(this.element);
                   this.checkAllItem = $("<li></li>").appendTo(this.checkAllNav);
                   this.checkAllElement = $("<input id='selectall' type='checkbox' class='usermailboxes-selectall' value='false' name='selectall'/>").appendTo(this.checkAllItem);
                   $("<span>&nbsp;Select All</span>").appendTo(this.checkAllItem);

                   this.mailboxListElement = $("<ul id='mailboxlist' class='nav nav-pills nav-stacked usermailboxes-mailboxlist'></ul>").appendTo(this.element);
                   this.pageElement = $("<ul class='pagination usermailboxes-pagination'></ul>").appendTo(this.element);

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

                           self._updateMailbox();
                           self._updatePage();
                       }
                   })
               },

               _listen: function () {
                   var self = this;
                   self.checkAllElement.bind("click.ui.customer.usermailboxes.selectallmailbox", self._onSelectAllItem);
                   self.mailboxListElement.bind("click.ui.customer.usermailboxes.selectmailbox", self._onClickItem);
               },

               _removeListen: function () {
                   var self = this;
                   self.checkAllElement.unbind("click.ui.customer.usermailboxes.selectallmailbox", self._onSelectAllItem);
                   self.mailboxListElement.unbind("click.ui.customer.usermailboxes.selectmailbox", self._onClickItem);
               },

               _onSelectAllItem: function (e) {

               },
               _onSelectItem: function (e) {

               },

               _onClickItem: function (e) {
                   var self = this;
                   // todo
                   var $target = $(e.target);
                   var isClickItem = false;
                   if ($target.is("input.usermailboxes-mailboxitem-checkbox")) {
                       self._onSelectItem(e);
                   }
                   else if ($target.is("li.usermailboxes-mailboxitem")) {
                       isClickItem = true;
                   }

                   if (isClickItem) {
                       //todo click item;
                       if (self.options.onSelectItemChanged) {
                           var id = $target.attr["itemid"];
                           var item = self.mailboxesId2Item[id];
                           self.options.onSelectItemChanged(item);
                       }
                       return false;
                   }
               },

               _updateMailbox: function () {
                   var self = this;
                   self.mailboxListElement.html("");
                   var startIndex = self.pageIndex * self.options.pagecount;
                   for (var i = startIndex, j = 0; j < self.options.pagecount && i < self.mailboxes.length; j++, i++) {
                       var item = self.mailboxes[i];

                       var ids = self._getMailboxItemId(item.RootFolderId);
                       var eachItemElement = $("<li class='usermailboxes-mailboxitem' id='" + ids.li + "' itemid='" + item.RootFolderId + "'></li>").appendTo(self.mailboxListElement);

                       var anchorElement = $("<a href='#' id='" + ids.a + "' ></a>").appendTo(eachItemElement);
                       var eachItemCheckboxElement =
                           $("<input id='" + ids.checkbox + "' type='checkbox' class='usermailboxes-mailboxitem-checkbox' value='false' />").appendTo(anchorElement);
                       $("<span>&nbsp;" + item.DisplayName + "</span>").appendTo(anchorElement);
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
                       onPageChanged: function (pages) {
                           self._onPageChanged(pages[0], pages[1]);
                       }
                   };

                   self.pageElement.bootstrapPaginator(pageSizeOptions);
               },

               _onPageChanged: function (lastPage, currentPage) {
                   self._updateMailbox();
               },

               _setOption: function (key, value) {
                   if (key == "pagecount") {
                       this.pageElement.bootstrapPaginator({ "numberOfPages": value });
                   }

                   this._super(key, value);
               },
               _setOptions: function (options) {
                   this._super(options);
               },

               _destroy: function () {
                   this._removeListen();
                   this.pageElement.bootstrapPaginator("destroy");
                   this.element
                       .removeClass("progressbar")
                       .text("");
               }
           });

; (function ($) {



})(jQuery);