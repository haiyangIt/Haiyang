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
        url : setting.url,
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
    if (!self.loadingDialog) {
        $("<div title='loading' id='for_ajax_loading'></div>").appendTo($("body"));
        self.loadingDialog = $("#for_ajax_loading").dialog({
            hide: 'slide',
            show: 'slide',
            autoOpen: false
        });
        
        self.loadingCounter = 0;
    }

    if (self.loadingCounter == 0) {
        self.loadingDialog.dialog("open").html("<p>Please wait...</p>");
    }
    self.loadingCounter++;
};

Arcserve.DataProtect.Util.Ajax.close = function () {
    var self = Arcserve.DataProtect.Util.Ajax;
    self.loadingCounter--;
    if (self.loadingCounter == 0)
    {
        self.loadingDialog.dialog("close");
    }
};