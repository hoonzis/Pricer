define(["exports", "./SimplePricer", "Pricer.Core/PayoffsGenerator", "Pricer.Core/StocksModel", "fable-core", "./Tools", "Pricer.Core/OptionsModel", "./Charting", "Pricer.Core/StrategiesExamples"], function (exports, _SimplePricer, _PayoffsGenerator, _StocksModel, _fableCore, _Tools, _OptionsModel, _Charting, _StrategiesExamples) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.Main = exports.VueHelper = undefined;

    function _defineProperty(obj, key, value) {
        if (key in obj) {
            Object.defineProperty(obj, key, {
                value: value,
                enumerable: true,
                configurable: true,
                writable: true
            });
        } else {
            obj[key] = value;
        }

        return obj;
    }

    function _classCallCheck(instance, Constructor) {
        if (!(instance instanceof Constructor)) {
            throw new TypeError("Cannot call a class as a function");
        }
    }

    var _createClass = function () {
        function defineProperties(target, props) {
            for (var i = 0; i < props.length; i++) {
                var descriptor = props[i];
                descriptor.enumerable = descriptor.enumerable || false;
                descriptor.configurable = true;
                if ("value" in descriptor) descriptor.writable = true;
                Object.defineProperty(target, descriptor.key, descriptor);
            }
        }

        return function (Constructor, protoProps, staticProps) {
            if (protoProps) defineProperties(Constructor.prototype, protoProps);
            if (staticProps) defineProperties(Constructor, staticProps);
            return Constructor;
        };
    }();

    var VueHelper = exports.VueHelper = function ($exports) {
        var createFromObj = $exports.createFromObj = function createFromObj(data, extraOpts) {
            var methods = {};
            var computed = {};
            var proto = Object.getPrototypeOf(data);
            {
                var inputSequence = Object.getOwnPropertyNames(proto);
                var _iteratorNormalCompletion = true;
                var _didIteratorError = false;
                var _iteratorError = undefined;

                try {
                    for (var _iterator = inputSequence[Symbol.iterator](), _step; !(_iteratorNormalCompletion = (_step = _iterator.next()).done); _iteratorNormalCompletion = true) {
                        var k = _step.value;
                        var prop = Object.getOwnPropertyDescriptor(proto, k);
                        var matchValue = prop.value;

                        if (matchValue == null) {
                            computed[k] = {
                                get: prop.get,
                                set: prop.set
                            };
                        } else {
                            methods[k] = matchValue;
                        }
                    }
                } catch (err) {
                    _didIteratorError = true;
                    _iteratorError = err;
                } finally {
                    try {
                        if (!_iteratorNormalCompletion && _iterator.return) {
                            _iterator.return();
                        }
                    } finally {
                        if (_didIteratorError) {
                            throw _iteratorError;
                        }
                    }
                }
            }
            extraOpts.data = data;
            extraOpts.computed = computed;
            extraOpts.methods = methods;
            return new Vue(extraOpts);
        };

        return $exports;
    }({});

    var Main = exports.Main = function ($exports) {
        var _directives;

        var pricer = $exports.pricer = new _SimplePricer.SimplePricer();
        var payoffsGenerator = $exports.payoffsGenerator = new _PayoffsGenerator.PayoffsGenerator(pricer);

        var StockViewModel = $exports.StockViewModel = function () {
            function StockViewModel(s) {
                _classCallCheck(this, StockViewModel);

                this.rate = s.Rate;
                this.volatility = s.Volatility;
                this.currentPrice = s.CurrentPrice;
            }

            _createClass(StockViewModel, [{
                key: "buildStock",
                get: function get() {
                    return new _StocksModel.StockInfo(this.rate, this.volatility, this.currentPrice);
                }
            }]);

            return StockViewModel;
        }();

        _fableCore.Util.setInterfaces(StockViewModel.prototype, [], "Pricer.Fabled.Main.StockViewModel");

        var LegViewModel = $exports.LegViewModel = function () {
            function LegViewModel(l) {
                _classCallCheck(this, LegViewModel);

                this.leg = l;
                this.strike = "0.0";
                this.expiry = "test";
                this.kind = "Option";
                this.direction = "Buy";

                if (l.Definition.Case === "Option") {
                    {
                        var copyOfStruct = l.Definition.Fields[0].Strike;
                        this.strike = String(copyOfStruct);
                    }
                    this.expiry = (0, _Tools.toDate)(l.Definition.Fields[0].Expiry);
                    this.direction = l.Definition.Fields[0].BuyVsSell;
                    this.kind = _fableCore.Util.toString(l.Definition.Fields[0].Kind);
                } else {
                    if (l.Definition.Case === "Cash") {
                        this.kind = "Cash";
                        this.direction = l.Definition.Fields[0].BuyVsSell;
                    }
                }
            }

            _createClass(LegViewModel, [{
                key: "getLeg",
                get: function get() {
                    var _this = this;

                    return this.kind === "Cash" ? new _OptionsModel.Leg(new _OptionsModel.LegInfo("Cash", [new _OptionsModel.CashLeg(_OptionsModel.Transforms.stringToDirection(this.direction), Number.parseFloat(this.strike))])) : new _OptionsModel.Leg(new _OptionsModel.LegInfo("Option", [function () {
                        var Direction = _OptionsModel.Transforms.stringToDirection(_this.direction);

                        var Expiry = (0, _Tools.parseDate)(_this.expiry);
                        var Strike = Number.parseFloat(_this.strike);

                        var PurchaseDate = _fableCore.Date.now();

                        return new _OptionsModel.OptionLeg(Direction, Strike, Expiry, _this.kind === "Put" ? new _OptionsModel.OptionKind("Put", []) : new _OptionsModel.OptionKind("Call", []), new _OptionsModel.OptionStyle("European", []), PurchaseDate);
                    }()]));
                }
            }]);

            return LegViewModel;
        }();

        _fableCore.Util.setInterfaces(LegViewModel.prototype, [], "Pricer.Fabled.Main.LegViewModel");

        var StrategyViewModel = $exports.StrategyViewModel = function () {
            function StrategyViewModel(strategy) {
                _classCallCheck(this, StrategyViewModel);

                this.legs = Array.from(_fableCore.List.map(function (l) {
                    return new LegViewModel(l);
                }, strategy.Legs));
                this.name = strategy.Name;
                this.stock = new StockViewModel(strategy.Stock);
            }

            _createClass(StrategyViewModel, [{
                key: "addLeg",
                value: function addLeg(event) {
                    var newLeg = new _OptionsModel.Leg(new _OptionsModel.LegInfo("Option", [new _OptionsModel.OptionLeg(1, 100, _fableCore.Date.now(), new _OptionsModel.OptionKind("Call", []), new _OptionsModel.OptionStyle("European", []), _fableCore.Date.now())]));
                    this.legs = [new LegViewModel(newLeg)].concat(this.legs);
                }
            }, {
                key: "removeLeg",
                value: function removeLeg(leg) {
                    this.legs = this.legs.filter(function (l) {
                        return !l.getLeg.Equals(leg.getLeg);
                    });
                }
            }, {
                key: "generatePayoff",
                value: function generatePayoff() {
                    var _this2 = this;

                    var newStrategy = function () {
                        var Name = _this2.name;

                        var Legs = _fableCore.Seq.toList(_fableCore.Seq.map(function (l) {
                            return l.getLeg;
                        }, _this2.legs));

                        return new _OptionsModel.Strategy(_this2.stock.buildStock, Name, Legs);
                    }();

                    var data = payoffsGenerator.getStrategyData(newStrategy);

                    _Charting.Charting.drawPayoff(data);
                }
            }]);

            return StrategyViewModel;
        }();

        _fableCore.Util.setInterfaces(StrategyViewModel.prototype, [], "Pricer.Fabled.Main.StrategyViewModel");

        var StrategyListViewModel = $exports.StrategyListViewModel = function () {
            function StrategyListViewModel(examples) {
                _classCallCheck(this, StrategyListViewModel);

                this.strategies = Array.from(_fableCore.List.map(function (s) {
                    return new StrategyViewModel(s);
                }, examples));
                this.selectedStrategy = null;
            }

            _createClass(StrategyListViewModel, [{
                key: "select",
                value: function select(strat) {
                    this.selectedStrategy = strat;
                    this.selectedStrategy.generatePayoff();
                }
            }, {
                key: "allStrategies",
                get: function get() {
                    return this.strategies;
                }
            }, {
                key: "strategy",
                get: function get() {
                    return this.selectedStrategy;
                }
            }]);

            return StrategyListViewModel;
        }();

        _fableCore.Util.setInterfaces(StrategyListViewModel.prototype, [], "Pricer.Fabled.Main.StrategyListViewModel");

        var extraOpts = $exports.extraOpts = {
            el: ".payoffapp",
            directives: (_directives = {}, _defineProperty(_directives, _fableCore.Symbol.interfaces, ["Pricer.Fabled.Main.Directives"]), _defineProperty(_directives, "todo-focus", function todoFocus(x) {
                var _this3 = this;

                if (x != null) {
                    (function () {
                        var el = _this3.el;
                        Vue.nextTick(function (unitVar0) {
                            el.focus();
                        });
                    })();
                }
            }), _directives)
        };
        var vm = $exports.vm = new StrategyListViewModel(_StrategiesExamples.exampleStrategies);
        vm.select(vm.allStrategies[4]);
        vm.strategy.generatePayoff();
        var app = $exports.app = VueHelper.createFromObj(vm, extraOpts);
        return $exports;
    }({});
});
//# sourceMappingURL=App.js.map