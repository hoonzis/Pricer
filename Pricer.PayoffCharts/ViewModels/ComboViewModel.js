function ComboViewModel(options, viewModel, selectedObservable) {
	var self = this;
	self.options = options;

	var defaultOptions = {
	    valueMember: "name",
	    keyPressSearchTimeout: 200,
	    minimumSearchTextLength: 1
	};
    if (self.options == null) {
        self.options = defaultOptions;
    }

	self.onKeyPress = function (context, e) {
	    switch (e.keyCode) {
	        case 27:
	            self.hideDropdown();
	            return false;
	        case 13:
	            if (self.dropdownVisible()) {
	                self.selected(self.getCurrentActiveItem());
	            } else {
	                self.forceShow();
	            }
	            return false;
	        case 38:
	            self.navigate(-1);
	            return false;
	        case 40:
	            self.navigate(1);
	            return false;
	        default:
	            return true;
	    }
	};
    self.onSearch = function (value) {
	    if (self.explicitSet || value.length < self.options.minimumSearchTextLength) {
	        return;
	    }

        self.loading(true);
	    self.resetDropdown();
	    clearTimeout(self.searchTimeout);
	    self.searchTimeout = setTimeout(self.getData.bind(self), self.options.keyPressSearchTimeout);
	},
    self.getData = function () {
        var text = self.searchText();
        var callback = function (result) {
            if (self.searchText() == text) {
                self.getDataCallback(result);
            }
        }.bind(self);
        var options = {
            text: text,
            callback: callback
        };
        var result = self.functionDataSource.call(self.viewModel, options);
        if (result) {
            delete options.callback;
            //result is a promise of data
            result.then(callback);
        }
    };
	self.getDataCallback = function (result) {
	    var arr = ko.utils.arrayMap(result.data, function (item) {
	        return new ItemViewModel(item);
	    });
	    self.loading(false);
	    self.dropdownItems(arr);
	    self.dropdownVisible(true);
	    self.navigate(0);
	};
	self.forceFocus = function () {
	    self.searchHasFocus(true);
	};
	self.resetDropdown = function () {
	    self.currentActiveIndex = 0;
	};
	self.selected = function (item) {
	    self.forceFocus();
	    self.selectedObservable(item.item);
	    self.hideDropdown();
	};
	self.setSelectedText = function (item) {
	    self.explicitSet = true;
	    self.searchText(self.getLabel(item));
	    self.explicitSet = false;
	};
	self.hideDropdown = function () {
	    self.dropdownVisible(false);
	};
	self.showDropdown = function () {
	    self.dropdownVisible(true);
	};
	self.forceShow = function () {
	    self.forceFocus();
	    self.showDropdown();
	};
	self.navigate = function (direction) {
	    if (self.dropdownItems().length > 0 && self.dropdownVisible()) {
	        if (direction !== 0) {
	            var currentItem = self.getCurrentActiveItem();
                if (currentItem != null) {
                    self.unnavigated(currentItem);
                }
	            
	            self.currentActiveIndex += direction;
	        }
	        self.currentActiveIndex = self.currentActiveIndex < 0 ? 0 : self.currentActiveIndex;
	        self.navigated(self.getCurrentActiveItem());
	    }
	};
	self.getCurrentActiveItem = function () {
	    return self.dropdownItems()[self.currentActiveIndex];
	};
	self.navigated = function (item) {
	    item.navigated(true);
	};
	self.unnavigated = function (item) {
	    item.navigated(false);
	};
	self.active = function (item) {
	    item.active(true);
	};
	self.inactive = function (item) {
	    item.active(false);
	};
	self.getLabel = function (item) {
	    return ko.utils.unwrapObservable(item ? item[self.options.valueMember] : null);
	};

    self.loading = ko.observable(false);
    self.searchText = ko.observable();
    self.searchText.subscribe(self.onSearch, self);
    self.viewModel = viewModel;
    self.dataSource = viewModel.getTickers;
    self.functionDataSource = self.dataSource;
    self.selectedObservable = selectedObservable;
    self.selectedObservable.subscribe(self.setSelectedText, self);

    if (selectedObservable() != null) {
        self.setSelectedText(selectedObservable());
    }

    self.dropdownVisible = ko.observable(false);
    self.dropdownItems = ko.observableArray([]);

    self.searchHasFocus = ko.observable();

    self.currentActiveIndex = 0;
  
};

function ItemViewModel(item) {
    this.item = item;
    this.navigated = ko.observable();
    this.active = ko.observable();
};