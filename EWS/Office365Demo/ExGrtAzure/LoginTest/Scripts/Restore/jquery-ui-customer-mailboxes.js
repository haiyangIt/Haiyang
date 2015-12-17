$.widget("custom.usermailboxes",
           {
               options: {
                   pagecount: 10,
                   getdataurl: "",
                   getdataargs: {},
                   onSelectItemChanged: null
               },

               _destroy: function () {
                   this._removeListen();
                   this.pageElement.bootstrapPaginator("destroy");
                   this.element
                       .removeClass("usermailboxes")
                       .text("");
               },
               _create: function () {
                   var self = this;
                   this.element.addClass("usermailboxes");
                   this.table = $("<table class='table table-striped table-hover table-condensed usermailboxes-table' id='mailboxtable'></table>").appendTo(this.element);
                   this.tHeader = $("<thead><tr><th></th><th>User Name</th></tr></thead>").appendTo(this.table);
                   this.checkAllElement = $("<input id='selectall' type='checkbox' class='usermailboxes-selectall' value='false' name='selectall'/>").appendTo($("th:first", this.tHeader));
                   this.tBody = $("<tbody></tbody>").appendTo(this.table);

                   //this.checkAllElement = $("<input id='selectall' type='checkbox' class='usermailboxes-selectall' value='false' name='selectall'/>").appendTo(this.element);
                   //this.mailboxListElement = $("<ul id='mailboxlist' class='nav nav-pills nav-stacked usermailboxes-mailboxlist'></ul>").appendTo(this.element);
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
                           self._clickFirstElement();
                       }
                   })
               },

               _clickFirstElement: function () {
                   $("tr:first", this.tBody).click();
               },

               _listen: function () {
                   var self = this;
                   self.checkAllElement.bind("click.ui.customer.usermailboxes.selectallmailbox", function (e) {
                       self._onSelectAllItem(e);
                   });
                   self.table.bind("click.ui.customer.usermailboxes.selectmailbox", function (e) {
                       self._onClickItem(e)
                   });
               },

               _removeListen: function () {
                   var self = this;
                   self.checkAllElement.unbind("click.ui.customer.usermailboxes.selectallmailbox");
                   self.table.unbind("click.ui.customer.usermailboxes.selectmailbox");
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

                   var tr = $target.is("tr") ? $target : $target.parents("tr");
                   if (tr.length) {
                       if ($target.is("input.usermailboxes-mailboxitem-checkbox")) {
                           self._onSelectItem(e);
                       }

                       //todo click item;
                       if (typeof (self.options.onSelectItemChanged) === "function") {
                           var id = tr.attr("itemid");
                           var item = self.mailboxesId2Item[id];
                           self.options.onSelectItemChanged(item);
                       }

                       $("tr.warning", this.tbody).toggleClass("warning");
                       tr.toggleClass("warning");
                   }

               },

               _updateMailbox: function () {
                   var self = this;
                   self.tBody.html("");
                   var startIndex = self.pageIndex * self.options.pagecount;
                   for (var i = startIndex, j = 0; j < self.options.pagecount && i < self.mailboxes.length; j++, i++) {
                       var item = self.mailboxes[i];

                       var ids = self._getMailboxItemId(item.RootFolderId);
                       var eachRow = $("<tr itemid='" + item.RootFolderId + "'></tr>").appendTo(self.tBody);
                       var firstTd = $("<td></td>").appendTo(eachRow);
                       var eachItemCheckboxElement =
                           $("<input id='" + ids.checkbox + "' type='checkbox' class='usermailboxes-mailboxitem-checkbox' value='false' />").appendTo(firstTd);

                       var secondTd = $("<td></td>").appendTo(eachRow);
                       var anchorElement = $("<a href='#' style='text-decoration: none;' id='" + ids.a + "' ></a>").appendTo(secondTd);
                       anchorElement.html(item.DisplayName);
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