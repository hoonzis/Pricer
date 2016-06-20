define(["exports", "fable-core", "./SimplePricer", "../Pricer.Core/PayoffsGenerator", "../Pricer.Core/OptionsModel", "./Charting", "../Pricer.Core/StrategiesExamples"], function (exports, _fableCore, _SimplePricer, _PayoffsGenerator, _OptionsModel, _Charting, _StrategiesExamples) {
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
        var createFromObj = $exports.createFromObj = function (data, extraOpts) {
            var methods, computed, proto;
            return methods = {}, computed = {}, proto = Object.getPrototypeOf(data), function () {
                var _ret;

                var inputSequence = Object.getOwnPropertyNames(proto);
                var _iteratorNormalCompletion = true;
                var _didIteratorError = false;
                var _iteratorError = undefined;

                try {
                    for (var _iterator = inputSequence[Symbol.iterator](), _step; !(_iteratorNormalCompletion = (_step = _iterator.next()).done); _iteratorNormalCompletion = true) {
                        var f;
                        var k = _step.value;
                        var prop = Object.getOwnPropertyDescriptor(proto, k);
                        var matchValue = prop.value;
                        _ret = matchValue == null ? computed[k] = {
                            get: prop.get,
                            set: prop.set
                        } : (f = matchValue, methods[k] = f);
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

                return _ret;
            }(), extraOpts.data = data, extraOpts.computed = computed, extraOpts.methods = methods, new Vue(extraOpts);
        };

        return $exports;
    }({});

    var Main = exports.Main = function ($exports) {
        var _directives;

        var dateToString = $exports.dateToString = function (date) {
            return function () {
                return function () {
                    var clo1;
                    return clo1 = _fableCore.String.fsFormat("%i-%0i-%0i")(function (x) {
                        return x;
                    }), function (arg10) {
                        return function () {
                            var clo2;
                            return clo2 = clo1(arg10), function (arg20) {
                                return function () {
                                    var clo3;
                                    return clo3 = clo2(arg20), function (arg30) {
                                        return clo3(arg30);
                                    };
                                }();
                            };
                        }();
                    };
                }();
            }()(_fableCore.Date.year(date))(_fableCore.Date.month(date))(_fableCore.Date.day(date));
        };

        var pricer = $exports.pricer = new _SimplePricer.SimplePricer();
        var payoffsGenerator = $exports.payoffsGenerator = new _PayoffsGenerator.PayoffsGenerator(pricer);

        var LegViewModel = $exports.LegViewModel = function () {
            function LegViewModel(l) {
                var opt,
                    _this = this,
                    cash;

                _classCallCheck(this, LegViewModel);

                this.leg = l;
                this.strike = 0;
                this.expiry = "test";
                this.kind = "Option";
                this.direction = "Buy";
                var matchValue = l.Definition;
                matchValue.Case === "Option" ? (opt = matchValue.Fields[0], this.strike = opt.Strike, this.expiry = dateToString(opt.Expiry), this.direction = function (direction) {
                    return _this.getDirection(direction);
                }(opt.Direction), this.kind = function (kind) {
                    return _this.getKind(kind);
                }(opt.Kind)) : matchValue.Case === "Cash" ? (cash = matchValue.Fields[0], this.kind = "Cash", this.direction = function (direction) {
                    return _this.getDirection(direction);
                }(cash.Direction)) : null;
            }

            _createClass(LegViewModel, [{
                key: "getDirection",
                value: function getDirection(direction) {
                    return direction === 1 ? "Buy" : "Sell";
                }
            }, {
                key: "getKind",
                value: function getKind(kind) {
                    return _fableCore.Util.compareTo(kind, new _OptionsModel.OptionKind("Put")) === 0 ? "Put" : "Call";
                }
            }, {
                key: "getLeg",
                get: function () {
                    var Direction, Expiry, Strike, PurchaseDate;
                    return this.kind === "Cash" ? new _OptionsModel.Leg(new _OptionsModel.LegInfo("Cash", new _OptionsModel.CashLeg(this.direction === "Buy" ? 1 : -1, this.strike))) : new _OptionsModel.Leg(new _OptionsModel.LegInfo("Option", (Direction = this.direction === "Buy" ? 1 : -1, Expiry = _fableCore.Date.now(), Strike = this.strike, PurchaseDate = _fableCore.Date.now(), new _OptionsModel.OptionLeg(Direction, Strike, Expiry, this.kind === "Put" ? new _OptionsModel.OptionKind("Put") : new _OptionsModel.OptionKind("Call"), new _OptionsModel.OptionStyle("European"), PurchaseDate))));
                }
            }]);

            return LegViewModel;
        }();

        var StrategyViewModel = $exports.StrategyViewModel = function () {
            function StrategyViewModel(strategy) {
                _classCallCheck(this, StrategyViewModel);

                this.legs = Array.from(_fableCore.List.map(function (l) {
                    return new LegViewModel(l);
                }, strategy.Legs));
                this.name = strategy.Name;
                this.stock = strategy.Stock;
            }

            _createClass(StrategyViewModel, [{
                key: "addLeg",
                value: function addLeg(event) {
                    var newLeg = new _OptionsModel.Leg(new _OptionsModel.LegInfo("Option", new _OptionsModel.OptionLeg(1, 100, _fableCore.Date.now(), new _OptionsModel.OptionKind("Call"), new _OptionsModel.OptionStyle("European"), _fableCore.Date.now())));
                    this.legs = Array.from(_fableCore.Seq.append([new LegViewModel(newLeg)], this.legs));
                }
            }, {
                key: "generatePayoff",
                value: function generatePayoff() {
                    var Name, Legs;
                    var newStrategy = (Name = this.name, Legs = _fableCore.Seq.toList(_fableCore.Seq.map(function (l) {
                        return l.getLeg;
                    }, this.legs)), new _OptionsModel.Strategy(this.stock, Name, Legs));
                    var data = payoffsGenerator.getStrategyData(newStrategy);

                    _Charting.Charting.drawPayoff(data);
                }
            }]);

            return StrategyViewModel;
        }();

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
                }
            }]);

            return StrategyListViewModel;
        }();

        var extraOpts = $exports.extraOpts = {
            el: ".todoapp",
            directives: (_directives = {}, _defineProperty(_directives, _fableCore.Symbol.interfaces, ["Pricer.Fabled.Main.Directives"]), _defineProperty(_directives, "todo-focus", function (x) {
                var el;
                x != null ? (el = this.el, Vue.nextTick(function (unitVar0) {
                    el.focus();
                })) : null;
            }), _directives)
        };
        var vm = $exports.vm = new StrategyListViewModel(_StrategiesExamples.exampleStrategies);
        var app = $exports.app = VueHelper.createFromObj(vm, extraOpts);
        return $exports;
    }({});
});
//# sourceMappingURL=App.js.map