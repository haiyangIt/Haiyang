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
    complete: function(jqXhr, textStatus){},
    loading: false
}
*/
Arcserve.DataProtect.Util.Ajax = function (options) {
    var setting = $.extend({}, Arcserve.DataProtect.Util.Ajax.DefaultOptions, options);
    var data = "";
    var contentType = "application/x-www-form-urlencoded; charset=UTF-8";
    if (options.data instanceof Object) {
        data = JSON.stringify(options.data);
        contentType = setting.contentType;
    }
    else if (typeof(options.data) == "string")
        data = options.data;

    if (typeof (setting.url) === "undefined")
        alert("please check code, missing ajax url.");

    var isLoading = false;
    var timeOut = setTimeout(function () {
        Arcserve.DataProtect.Util.Ajax.loading();
        isLoading = true;
    }, 1000);

    $.ajax({
        url: setting.url,
        type: setting.type,
        method: setting.method,
        contentType: contentType,// "application/json; charset=utf-8",
        data: data,
        error: function (jqXhr, textStatus, errorThrown) {
            setting.error(jqXhr, textStatus, errorThrown);
        },
        success: function (data, textStatus, jqXhr) {
            setting.success(data, textStatus, jqXhr);
        },
        complete: function (jqXhr, textStatus) {
            clearTimeout(timeOut);
            if (isLoading) {
                Arcserve.DataProtect.Util.Ajax.close();
                isLoading = false;
            }
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
        self.loadingDialog = new BootstrapDialog.show({
            type: BootstrapDialog.TYPE_INFO,
            title: "loading",
            message: "<p>Please wait...</p>",
            animate: false,
            closable: false
        });

        self.loadingCounter = 0;
    }

    if (self.loadingCounter == 0) {
        self.loadingDialog.setTitle(title);
        self.loadingDialog.open();
    }
    self.loadingCounter++;

    Arcserve.DataProtect.Util.Ajax.ResetDialogOptions();
};

Arcserve.DataProtect.Util.Ajax.close = function () {
    var self = Arcserve.DataProtect.Util.Ajax;
    if (self.loadingCounter > 0) {
        self.loadingCounter--;
    }
    if (self.loadingCounter == 0) {
        self.loadingDialog.close();
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
    var options = $.extend({}, Arcserve.DataProtect.Util.Alert.DefaultOptions, options);
    BootstrapDialog.show({
        type: options.type,
        title: options.title,
        message: options.message,
        buttons: [{
            label: options.btnYesText,
            action: function (dialogWarning) {
                options.callbackForYes(dialogWarning);
            }
        }]
    });
}

Arcserve.DataProtect.Util.Alert.DefaultOptions = {
    type: BootstrapDialog.TYPE_WARNING,
    title: "Warning",
    message: "warning",
    btnYesText: "Ok",
    callbackForYes: function (dialogWarning) {
        dialogWarning.close();
    }
}