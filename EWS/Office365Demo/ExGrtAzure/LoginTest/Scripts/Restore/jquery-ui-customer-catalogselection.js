/*
 ref : https://silviomoreto.github.io/bootstrap-select/

*/

$.widget("custom.catalogselection", {
    _properties: {
        _css: "catalogselection",
        _currentDay: null,
        _day2catalogs: [],
        _id2catalogs: []
    },
    _create: function () {
        var self = this;

        this.element.addClass(this._properties._css);
        
        this.selectElement = $('<select class="selectpicker show-tick" data-width="100%" data-size="8"></select>').appendTo(this.element);
        this.selectElement.selectpicker();
        this.selectElement.on("change", function () {
            var item = self._properties._id2catalogs[self.selectElement.selectpicker('val')];
            self.element.trigger("onSelect", item);
        })
    },
    _destroy: function () {
        this.element.removeClass(this._properties._css);
        this.element.html("");
    },
    update: function (updateData) {
        var self = this;

        var day = updateData.data.day;
        self._properties._currentDay = day;
        var catalogs = this._properties._day2catalogs[+day];
        if (typeof (catalogs) !== "undefined") {
            self.updateWithData({catalogs: catalogs ,day: day});
        }
        else {
            var data = { day: Restore.GetDateTime(updateData.data.day), organization: updateData.data.organization };

            Arcserve.DataProtect.Util.Post(data, updateData.url,
                function (data) {
                    self.updateWithData({ catalogs: data.CatalogInfos, day: self._properties._currentDay });
                });
        }
    },
    updateWithData: function (data) {
        var self = this;
        this.selectElement.html("");
        if (data && data != null) {

            if (data && typeof (data.catalogs) !== "undefined") {
                for (var index = 0 ; index < data.catalogs.length; index++) {
                    data.catalogs[index].StartTime = Restore.GetDateTime(data.catalogs[index].StartTime)
                }
            }

            self._properties._currentDay = Restore.GetDateTime(data.day);

            self._properties._day2catalogs[+data.day] = data.catalogs;

            $.each(data.catalogs, function (i, item) {
                self._properties._id2catalogs[item.StartTime] = item;
                $('<option value="' + item.StartTime + '">' + item.CatalogJobName + '</option>').appendTo(self.selectElement);
            })

            if (data.catalogs.length > 0) {
                this.selectElement.selectpicker('val', "" + data.catalogs[0].StartTime);
                self.element.trigger("onSelect", data.catalogs[0]);
            }
        }

        this.selectElement.selectpicker('refresh');
    }
})