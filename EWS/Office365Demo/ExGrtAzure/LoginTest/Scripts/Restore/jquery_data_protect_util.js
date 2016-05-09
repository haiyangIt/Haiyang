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
    var ajaxObj = new Arcserve.DataProtect.Util.AjaxClass(setting);
    ajaxObj.Ajax();
};

Arcserve.DataProtect.Util.AjaxClass = function (setting) {
    this.setting = setting;
    this.timeOut = null;
    this.isLoading = false;
}

Arcserve.DataProtect.Util.AjaxClass.prototype.Ajax = function () {
    var self = this;
    var data = "";
    var contentType = "application/x-www-form-urlencoded; charset=UTF-8";
    if (this.setting.data instanceof Object) {
        data = JSON.stringify(this.setting.data);
        contentType = this.setting.contentType;
    }
    else if (typeof (this.setting.data) == "string")
        data = this.setting.data;

    if (typeof (this.setting.url) === "undefined")
        alert("please check code, missing ajax url.");

    this.timeOut = setTimeout(function () {
        self.isLoading = true;
        Arcserve.DataProtect.Util.Ajax.loading(null, null);
    }, 1000);

    $.ajax({
        url: this.setting.url,
        type: this.setting.type,
        method: this.setting.method,
        contentType: contentType,// "application/json; charset=utf-8",
        data: data,
        error: function (jqXhr, textStatus, errorThrown) {
            self.FailureCallback(jqXhr, textStatus, errorThrown);
        },
        success: function (returnData, textStatus, jqXhr) {
            self.SuccessCallback(returnData, textStatus, jqXhr);
        },
        complete: function (jqXhr, textStatus) {
            self.CompleteCallback(jqXhr, textStatus);
        }
    });
}

Arcserve.DataProtect.Util.AjaxClass.prototype.SuccessCallback = function (data, textStatus, jqXhr) {

    this.ResetLoading();
    this.setting.success(data, textStatus, jqXhr);
}

Arcserve.DataProtect.Util.AjaxClass.prototype.FailureCallback = function (jqXhr, textStatus, errorThrown) {

    try {
        var data = $.parseJSON(jqXhr.responseText);
        Arcserve.DataProtect.Util.Alert({
            type: BootstrapDialog.TYPE_DANGER,
            title: "Error",
            message: data.Exception,
            btnYesText: "Ok",
            callbackForYes: function (dialogWarning) {
                dialogWarning.close();
            }
        });
    }
    catch (e) {
        Arcserve.DataProtect.Util.Alert({
            type: BootstrapDialog.TYPE_DANGER,
            title: "Error",
            message: "jqXhr.responseText:" + jqXhr.responseText + " \r\n textStatus:" + textStatus + " \r\n errorThrow:" + errorThrown,
            btnYesText: "Ok",
            callbackForYes: function (dialogWarning) {
                dialogWarning.close();
            }
        });
    }

    this.ResetLoading();
    this.setting.error(jqXhr, textStatus, errorThrown);
}

Arcserve.DataProtect.Util.AjaxClass.prototype.CompleteCallback = function (jqXhr, textStatus) {

    this.ResetLoading();
    this.setting.complete(jqXhr, textStatus);
}

Arcserve.DataProtect.Util.AjaxClass.prototype.ResetLoading = function () {
    var self = this;
    if (this.timeOut != null) {
        clearTimeout(this.timeOut);
        this.timeOut = null;
    }
    if (self.isLoading) {
        self.isLoading = false;
        Arcserve.DataProtect.Util.Ajax.close();
    }
}

Arcserve.DataProtect.Util.Post = function (data, url, success, complete, error) {
    Arcserve.DataProtect.Util.Ajax({ data: data, url: url, success: success, complete: complete, error: error });
};

Arcserve.DataProtect.Util.Ajax.DefaultOptions = {
    type: "POST",
    method: "POST",
    contentType: "application/json; charset=utf-8",
    error: function (jqXhr, textStatus, errorThrown) {

    },
    success: function (data, textStatus, jqXhr) {

    },
    complete: function (jqXhr, textStatus) {

    }
};


Arcserve.DataProtect.Util.Ajax.LoadingDialog = function (title, message) {
    this.SetOptions(title, message);
    this.loadingDialog = new BootstrapDialog({
        type: BootstrapDialog.TYPE_INFO,
        title: self.title,
        message: self.message,
        animate: false,
        closable: false
    });
    this.loadingCount = 0;
}

Arcserve.DataProtect.Util.Ajax.LoadingDialog.prototype.Open = function (title, message) {
    var self = this;
    self.SetOptions(title, message);
    self.loadingDialog.setTitle(self.title);
    self.loadingDialog.setMessage(self.message);
    if (self.loadingCount == 0) {
        self.loadingDialog.open();
        self.loadingDialog.updateZIndexEx(3000);
    }
    else {
        self.loadingDialog.updateZIndexEx(3000);
    }
    self.loadingCount++;
}

Arcserve.DataProtect.Util.Ajax.LoadingDialog.prototype.SetOptions = function (title, message) {
    if (title === null) {
        this.title = this.title || "loading";
    }
    else
        this.title = title || "loading";

    if (message == null) {
        this.message = this.message || "<p>Please wait...</p>";
    }
    else
        this.message = message || "<p>Please wait...</p>";
}

Arcserve.DataProtect.Util.Ajax.LoadingDialog.prototype.Close = function () {
    var self = this;
    if (this.loadingCount > 0) {
        this.loadingCount--;
        if (this.loadingCount == 0) {
            this.loadingDialog.close();
            this.title = "loading";
            this.message = "<p>Please wait...</p>";
        }
    }
}
Arcserve.DataProtect.Util.Ajax.LoadingObj = new Arcserve.DataProtect.Util.Ajax.LoadingDialog();
Arcserve.DataProtect.Util.Ajax.loading = function (title, message) {
    Arcserve.DataProtect.Util.Ajax.LoadingObj.Open(title, message);
};

Arcserve.DataProtect.Util.Ajax.close = function () {
    Arcserve.DataProtect.Util.Ajax.LoadingObj.Close();
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