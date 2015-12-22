; var Arcserve = Arcserve || {};

Arcserve.DataProtect = {};

Arcserve.DataProtect.Util = {};


/*
defaultOptions:{
    type:"POST",
    method:"POST",
    contentType:"application/json; charset=utf-8",
    error: function(jqXhr, textStatus, errorThrown){ alert(e.);},
    success:function(data, textStatus, jqXhr){alert("operator success.");},
    complete: function(jqXhr, textStatus){}
}
*/
Arcserve.DataProtect.Util.Ajax = function (options) {
    var setting = $.extend({}, Arcserve.DataProtect.Util.Ajax.DefaultOptions, options);
    var data = "";
    if (options.data instanceof Object) {
        data = JSON.stringify(options.data);
    }

    if (typeof (setting.url) === "undefined")
        alert("please check code, missing ajax url.");

    Arcserve.DataProtect.Util.Ajax.loading();
    $.ajax({
        url: setting.url,
        type: setting.type,
        method: setting.method,
        contentType: setting.contentType,// "application/json; charset=utf-8",
        data: data,
        error: function (jqXhr, textStatus, errorThrown) {
            setting.error(jqXhr, textStatus, errorThrown);
        },
        success: function (data, textStatus, jqXhr) {
            setting.success(data, textStatus, jqXhr);
        },
        complete: function (jqXhr, textStatus) {
            Arcserve.DataProtect.Util.Ajax.close();
            setting.complete(jqXhr, textStatus);

        }
    });
};

Arcserve.DataProtect.Util.Post = function (data, url, success, complete, error) {
    Arcserve.DataProtect.Util.Ajax({ data: data, url: url, success: success, complete: complete, error: error });
};

Arcserve.DataProtect.Util.Ajax.DefaultOptions = {
    type: "POST",
    method: "POST",
    contentType: "application/json; charset=utf-8",
    error: function (jqXhr, textStatus, errorThrown) { alert(textStatus + errorThrown); },
    success: function (data, textStatus, jqXhr) { alert("operator success."); },
    complete: function (jqXhr, textStatus) { }
};

Arcserve.DataProtect.Util.Ajax.loading = function () {
    var self = Arcserve.DataProtect.Util.Ajax;
    var title = "loading";
    if (self.dialogOptions.title)
        title = self.dialogOptions.title;
    if (!self.loadingDialog) {
        $("<div id='for_ajax_loading'></div>").appendTo($("body"));
        self.loadingDialog = $("#for_ajax_loading").dialog({
            hide: 'slide',
            show: 'slide',
            autoOpen: false,
            title: title
        });

        self.loadingCounter = 0;
    }

    if (self.loadingCounter == 0) {
        self.loadingDialog.dialog("option", "title", title);
        self.loadingDialog.dialog("open").html("<p>Please wait...</p>");
        self.ResetDialogOptions();
    }
    self.loadingCounter++;
};

Arcserve.DataProtect.Util.Ajax.close = function () {
    var self = Arcserve.DataProtect.Util.Ajax;
    self.loadingCounter--;
    if (self.loadingCounter == 0) {
        self.loadingDialog.dialog("close");
    }
};

Arcserve.DataProtect.Util.Ajax.Dialog = Arcserve.DataProtect.Util.Ajax.Dialog || {};

Arcserve.DataProtect.Util.Ajax.Dialog.DefaultOptions = {
    title: "loading"
};

Arcserve.DataProtect.Util.Ajax.ResetDialogOptions = function () {
    Arcserve.DataProtect.Util.Ajax.dialogOptions = Arcserve.DataProtect.Util.Ajax.Dialog.DefaultOptions;
};

Arcserve.DataProtect.Util.Ajax.dialogOptions = Arcserve.DataProtect.Util.Ajax.Dialog.DefaultOptions;

Arcserve.DataProtect.Util.Ajax.SetLoadingDialog = function (options) {
    var self = Arcserve.DataProtect.Util.Ajax;
    self.dialogOptions = $.extend({}, self.Dialog.DefaultOptions, options);
};



Arcserve.DataProtect.Util.Alert = function (options) {
    var self = Arcserve.DataProtect.Util.Alert;
    var options = $.extend({}, self.DefaultOptions, options);

};

Arcserve.DataProtect.Util.Alert.DefaultOptions = {
    title: "Warning",
    btnYes: "Ok",
    functionYes: function () {

    }
};

Arcserve.DataProtect.Util.Confirm = function (options) {

};

Arcserve.DataProtect.Util.Confirm.DefaultOptions = {
    title: "Prompt",
    btnYes: "Yes",
    btnNo: "No",
    functionYes: function () { },
    functionNo: function () { }
};
