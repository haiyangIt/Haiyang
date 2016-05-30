$.widget("custom.usermailboxdetail",
           {
               options: {
                   mailbox: null,
                   onMailboxOpen: null
               },

               _create: function () {
                   var self = this;
                   this.element.addClass("usermailboxdetail");
                   this.contentElement = $('<div class="jumbotron"><h2 id="usermailbox-displayName"></h2><p class="lead" id="usermailbox-address"></p><p><a href="javascript:void(0);" id="usermailbox-open" class="btn btn-primary btn-lg">Open Mailbox</a></p></div>').appendTo(this.element);
                   this.displayNameElement = $("#usermailbox-displayName");
                   this.addressElement = $("#usermailbox-address");
                   this.openBtn = $("#usermailbox-open");
                   this.mailbox = this.options.mailbox;
                   this._listen();
                   this._updateContent();

               },
               _destroy: function () {
                   this._removeListen();
                   this.element
                       .removeClass("usermailboxdetail")
                       .text("");
               },

               update: function (mailbox) {
                   this.mailbox = mailbox;
                   this._updateContent();
               },

               _updateContent: function () {
                   if (this.mailbox && this.mailbox != null) {
                       this.displayNameElement.text(this.mailbox.OtherInformation.DisplayName);
                       this.addressElement.text(this.mailbox.OtherInformation.MailAddress);
                       this.openBtn.attr("itemid", this.mailbox.OtherInformation.RootFolderId);
                   }
                   else {
                       this.clear();
                   }
               },

               clear:function(){
                   this.displayNameElement.text("");
                   this.addressElement.text("");
                   this.openBtn.attr("itemid", "");
               },

               _listen: function () {
                   var self = this;
                   self.openBtn.bind("click.ui.customer.usermailbox.openmailbox", function (e) {
                       self._openMailbox(e);
                   });
               },

               _removeListen: function () {
                   var self = this;
                   self.openBtn.bind("click.ui.customer.usermailbox.openmailbox");
               },

               _openMailbox: function (e) {
                   this.element.trigger("openMailbox", this.mailbox);
               },

               _setOption: function (key, value) {
                   if (key == "mailbox") {
                       this.mailbox = value;
                       this._updateContent();
                   }

                   this._super(key, value);
               },
               _setOptions: function (options) {
                   this._super(options);
               }
           });