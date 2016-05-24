(function () {
    ko.bindingHandlers.combobox = {
        setDefaults: function (options) {
            ko.utils.extend(defaultOptions, options);
        },
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var options = ko.utils.unwrapObservable(valueAccessor());
            for (var index in defaultOptions) {
                if (options[index] === undefined) {
                    options[index] = defaultOptions[index];
                }

            }
            var selectedIsObservable = ko.isObservable(allBindingsAccessor().comboboxValue);
            var selected = ko.computed({
                read: function () {
                    return ko.utils.unwrapObservable(allBindingsAccessor().comboboxValue);
                },
                write: function (value) {
                    writeValueToProperty(allBindingsAccessor().comboboxValue, allBindingsAccessor, "comboboxValue", value);
                    if (!selectedIsObservable) selected.notifySubscribers(value);
                },
                disposeWhenNodeIsRemoved: element
            });

            var model = new ko.bindingHandlers.combobox.ViewModel(options, viewModel, selected);
            renderTemplate(element, options.comboboxTemplate, model, bindingContext);

            return { controlsDescendantBindings: true };
        }
    };
    if (ko.expressionRewriting._twoWayBindings) {
        ko.expressionRewriting._twoWayBindings.comboboxValue = true;
    }

    ko.bindingHandlers.__cb__flexibleTemplate = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var options = ko.utils.unwrapObservable(valueAccessor());
            renderTemplate(element, options.template, options.data, bindingContext);

            return { controlsDescendantBindings: true };
        }
    };

    ko.bindingHandlers.__cb__clickedIn = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var target = valueAccessor();
            var clickedIn = false;
            ko.utils.registerEventHandler(document, "click", function (e) {
                if (!clickedIn) {
                    target(e.target == element);
                }

                clickedIn = false;
            });

            ko.utils.registerEventHandler(element.parentNode || element.parentElement, "click", function (e) {
                clickedIn = true;
            });
        }
    };

    ko.bindingHandlers.combobox.ViewModel = function (options, viewModel, selectedObservable) {
        this.options = options;
        this.searchText = ko.observable("");
        this.searchText.subscribe(this.onSearch, this);
        this.placeholder = options.placeholder;
        this.viewModel = viewModel;
        this.dataSource = this.options.dataSource;
        this.functionDataSource = !ko.isObservable(this.dataSource) && typeof this.dataSource == 'function'
            ? this.dataSource
            : this.searchOnClientSide.bind(this);

        this.selectedObservable = selectedObservable;
        this.selectedObservable.subscribe(this.setSelectedText, this);

        if (selectedObservable() != null) {
            this.setSelectedText(selectedObservable());
        }

        this.dropdownVisible = ko.observable(false);
        this.dropdownItems = options.dropdownItemsArray || ko.observableArray();

        this.searchHasFocus = ko.observable();

        this.paging = new ko.bindingHandlers.combobox.PagingViewModel(options, this.getData.bind(this), this.dropdownItems, this.forceFocus.bind(this));
        this.currentActiveIndex = 0;

        this.rowTemplate = options.rowTemplate.replace("$$valueMember&&", options.valueMember);
    };

    ko.bindingHandlers.combobox.ViewModel.prototype = {
        onKeyPress: function (context, e) {
            switch (e.keyCode) {
                case 27:
                    this.hideDropdown();
                    return false;
                case 13:
                    if (this.dropdownVisible()) {
                        this.selected(this.getCurrentActiveItem());
                    } else {
                        this.forceShow();
                    }
                    return false;
                case 38:
                    this.navigate(-1);
                    return false;
                case 40:
                    this.navigate(1);
                    return false;
                default:
                    return true;
            }
        },
        onSearch: function (value) {
            if (this.explicitSet || value.length < this.options.minimumSearchTextLength) {
                return;
            }

            this.resetDropdown();
            clearTimeout(this.searchTimeout);
            this.searchTimeout = setTimeout(this.getData.bind(this), this.options.keyPressSearchTimeout);
        },
        getData: function(page) {
            var text = this.searchText();
            var callback = function(result) {
                if (this.searchText() == text) {
                    this.getDataCallback(result);
                }
            }.bind(this);
            var options = {
                text: text,
                page: page ? page : 0,
                pageSize: this.options.pageSize,
                total: this.paging.totalCount(),
                callback: callback
            };
            var result = this.functionDataSource.call(this.viewModel, options);
            if (result) {
                delete options.callback;
                if (isThenable(result)) {
                    result.then(callback);
                } else {
                    callback(result);
                }
            }
        },
        getDataCallback: function (result) {
            var arr = ko.utils.arrayMap(result.data, function (item) {
                return new ko.bindingHandlers.combobox.ItemViewModel(item);
            });
            this.dropdownItems(arr);
            this.paging.totalCount(result.total);
            this.dropdownVisible(true);
            this.navigate(0);
        },
        forceFocus: function () {
            this.searchHasFocus(true);
        },
        resetDropdown: function () {
            this.currentActiveIndex = 0;
            this.paging.reset();
        },
        selected: function (item) {
            this.forceFocus();
            this.selectedObservable(item.item);
            this.hideDropdown();
        },
        setSelectedText: function (item) {
            this.explicitSet = true;
            this.searchText(this.getLabel(item));
            this.explicitSet = false;
        },
        hideDropdown: function () {
            this.dropdownVisible(false);
        },
        showDropdown: function () {
            this.dropdownVisible(true);
        },
        forceShow: function () {
            this.forceFocus();
            if (this.paging.itemCount() == 0) {
                this.getData();
            } else {
                this.showDropdown();
            }
        },
        navigate: function (direction) {
            if (this.dropdownItems().length > 0 && this.dropdownVisible()) {
                if (direction !== 0) {
                    this.unnavigated(this.getCurrentActiveItem());
                    this.currentActiveIndex += direction;
                }
                this.currentActiveIndex = this.currentActiveIndex < 0 ? 0 : this.currentActiveIndex;
                this.currentActiveIndex = this.currentActiveIndex >= this.paging.itemCount() ? this.paging.itemCount() - 1 : this.currentActiveIndex;
                this.navigated(this.getCurrentActiveItem());
            }
        },
        getCurrentActiveItem: function () {
            return this.dropdownItems()[this.currentActiveIndex];
        },
        navigated: function (item) {
            item.navigated(true);
        },
        unnavigated: function (item) {
            item.navigated(false);
        },
        active: function (item) {
            item.active(true);
        },
        inactive: function (item) {
            item.active(false);
        },
        getLabel: function (item) {
            return ko.utils.unwrapObservable(item ? item[this.options.valueMember] : null);
        },
        searchOnClientSide: function (options) {
            var lowerCaseText = (options.text || "").toLowerCase();
            var filtered = ko.utils.arrayFilter(ko.utils.unwrapObservable(this.dataSource), function (item) {
                return this.getLabel(item).toLowerCase().slice(0, lowerCaseText.length) === lowerCaseText;
            }.bind(this));
            return {
                total: filtered.length, //be sure of calculate length before splice
                data: filtered.splice(options.pageSize * options.page, options.pageSize)
            };
        }
    };

    ko.bindingHandlers.combobox.ItemViewModel = function (item) {
        this.item = item;
        this.navigated = ko.observable();
        this.active = ko.observable();
    };

    ko.bindingHandlers.combobox.PagingViewModel = function (options, callback, dropdownItems, forceFocus) {
        this.options = options;
        this.currentPage = ko.observable(0);
        this.totalCount = ko.observable();
        this.totalCount.subscribe(this.update, this);

        this.itemCount = ko.computed(function () {
            return dropdownItems().length;
        }, this);

        this.currentFloor = ko.computed(function () {
            return this.itemCount() === 0 ? 0 : (this.currentPage() * options.pageSize) + 1;
        }, this);

        this.currentRoof = ko.computed(function () {
            return (this.currentPage() * options.pageSize) + this.itemCount();
        }, this);

        this.pages = ko.observableArray();

        this.show = ko.computed(function () {
            return this.options.paging && this.totalCount() > this.options.pageSize;
        }, this);

        this.callback = callback;
        this.forceFocus = forceFocus;
    };

    ko.bindingHandlers.combobox.PagingViewModel.prototype = {
        update: function (count) {
            var current = this.currentPage();
            var pages = [];
            var totalPageCount = Math.ceil(count / this.options.pageSize);
            var maxLinks = Math.min(this.options.pagingLinks, totalPageCount);

            var min = current - (maxLinks / 2);
            var max = current + (maxLinks / 2);

            if (min < 0) {
                max += Math.abs(min);
                min = 0;
            }

            if (max > totalPageCount) {
                min = min - (max - totalPageCount);
                max = totalPageCount;
            }

            for (var i = min; i < max; i++) {
                pages.push(this.createPage(i));
            }

            this.pages(pages);
        },
        createPage: function (index) {
            return {
                name: index + 1,
                index: index,
                isCurrent: ko.computed(function () {
                    return index == this.currentPage()
                }, this)
            };
        },
        pageSelected: function (page) {
            this.forceFocus();
            this.currentPage(page.index);
            this.callback(page.index);
            this.update(this.totalCount());
        },
        reset: function () {
            this.currentPage(0);
        }
    };

    var isObject = function (value) {
        return value === Object(value);
    };
    var isThenable = function (object) {
        return isObject(object) && typeof object.then === "function";
    };

    //TODO: remove this function when writeValueToProperty is made public by KO team
    var writeValueToProperty = function (property, allBindingsAccessor, key, value, checkIfDifferent) {
        if (!property || !ko.isObservable(property)) {
            var propWriters = allBindingsAccessor()['_ko_property_writers'];
            if (propWriters && propWriters[key])
                propWriters[key](value);
        } else if (ko.isWriteableObservable(property) && (!checkIfDifferent || property.peek() !== value)) {
            property(value);
        }
    };

    var engines = {};
    var renderTemplate = function (element, template, data, bindingContext) {
        var engine = engines[template];

        var success = false;
        do {
            try {
                ko.renderTemplate(template, bindingContext.createChildContext(data), engine, element, "replaceChildren");
                success = true;
                engines[template] = engine;
            } catch (err) {
                if (engine != null)
                    throw "Template engine not found";

                engine = { templateEngine: stringTemplateEngine };
            }

        } while (!success)
    };

    //string template source engine
    var stringTemplateSource = function (template) {
        this.template = template;
    };

    stringTemplateSource.prototype.text = function () {
        return this.template;
    };

    var stringTemplateEngine = new ko.nativeTemplateEngine();
    stringTemplateEngine.makeTemplateSource = function (template) {
        return new stringTemplateSource(template);
    };

    //Built in templates
    var comboboxTemplate = '<div data-bind="event: { keydown: onKeyPress }">\
        <input data-bind="value: searchText, valueUpdate: \'afterkeydown\', hasfocus: searchHasFocus, attr: { placeholder: placeholder }"></input><button type="button" class="btn btn-arrow" data-bind="click: forceShow, css: { open: dropdownVisible }"><span class="caret"></span></button>\
        <div class="dropdown-menu" data-bind="visible: dropdownVisible, __cb__clickedIn: dropdownVisible">\
            <!-- ko foreach: dropdownItems -->\
                <div data-bind="click: $parent.selected.bind($parent), event: { mouseover: $parent.active.bind($parent), mouseout: $parent.inactive.bind($parent) }, css: { active: navigated, highlighted: active },  __cb__flexibleTemplate: { template: $parent.rowTemplate, data: $data.item }"></div>\
            <!-- /ko -->\
            <div class="nav" data-bind="with: paging">\
                <p class="counter">Showing <span data-bind="text: currentFloor"></span>-<span data-bind="text: currentRoof"></span> of <span data-bind="text: totalCount"></span></p>\
                <div class="pagination"><ul data-bind="visible: show, foreach: pages"><li data-bind="click: $parent.pageSelected.bind($parent), text: name, disable: isCurrent, css: {current: isCurrent}"></li></ul></div>\
            </div>\
        </div>\
    </div>';

    var rowTemplate = '<span data-bind="text: $$valueMember&&"></span>';

    var defaultOptions = {
        comboboxTemplate: comboboxTemplate,
        rowTemplate: rowTemplate,
        valueMember: "name",
        pageSize: 10,
        paging: true,
        pagingLinks: 4,
        keyPressSearchTimeout: 200,
        minimumSearchTextLength: 1
    };
} ());