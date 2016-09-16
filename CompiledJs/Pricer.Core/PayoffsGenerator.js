(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "fable-core", "./OptionsModel"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("fable-core"), require("./OptionsModel"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.fableCore, global.OptionsModel);
        global.PayoffsGenerator = mod.exports;
    }
})(this, function (exports, _fableCore, _OptionsModel) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.PayoffsGenerator = undefined;

    var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) {
        return typeof obj;
    } : function (obj) {
        return obj && typeof Symbol === "function" && obj.constructor === Symbol ? "symbol" : typeof obj;
    };

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

    var PayoffsGenerator = exports.PayoffsGenerator = function () {
        function PayoffsGenerator(pricer) {
            _classCallCheck(this, PayoffsGenerator);

            this.pricer = pricer;
        }

        _createClass(PayoffsGenerator, [{
            key: "legPricing",
            value: function legPricing(stock, leg) {
                var _this = this;

                return leg.Definition.Case === "Option" ? function (arg00) {
                    return function (arg10) {
                        return _this.pricer.priceOption(arg00, arg10);
                    };
                }(stock)(leg.Definition.Fields[0]) : leg.Definition.Case === "Convertible" ? function (arg00) {
                    return function (arg10) {
                        return _this.pricer.priceConvert(arg00, arg10);
                    };
                }(stock)(leg.Definition.Fields[0]) : this.pricer.priceCash(leg.Definition.Fields[0]);
            }
        }, {
            key: "getInterestingPoints",
            value: function getInterestingPoints(strategy) {
                return _fableCore.Seq.isEmpty(strategy.Legs) ? _fableCore.Seq.empty() : function () {
                    var strikes = _fableCore.List.map(function (leg) {
                        return leg.Definition.Case === "Option" ? leg.Definition.Fields[0].Strike : leg.Definition.Case === "Convertible" ? leg.Definition.Fields[0].ReferencePrice : leg.Definition.Fields[0].Price;
                    }, strategy.Legs);

                    var min = 0.5 * _fableCore.Seq.reduce(function (x, y) {
                        return Math.min(x, y);
                    }, strikes);

                    var max = 1.5 * _fableCore.Seq.reduce(function (x, y) {
                        return Math.max(x, y);
                    }, strikes);

                    return _fableCore.Seq.delay(function (unitVar) {
                        return _fableCore.Seq.append(_fableCore.Seq.singleton(min), _fableCore.Seq.delay(function (unitVar_1) {
                            return _fableCore.Seq.append(_fableCore.Seq.sortWith(function (x, y) {
                                return _fableCore.Util.compare(x, y);
                            }, strikes), _fableCore.Seq.delay(function (unitVar_2) {
                                return _fableCore.Seq.singleton(max);
                            }));
                        }));
                    });
                }();
            }
        }, {
            key: "legPayoff",
            value: function legPayoff(leg, pricing, year, stockPrice) {
                return leg.Case === "Option" ? leg.Fields[0].Direction * (_OptionsModel.BasicOptions.optionValue(leg.Fields[0], stockPrice) - pricing.Premium) : leg.Case === "Convertible" ? leg.Fields[0].Direction * (year * leg.Fields[0].Coupon * leg.Fields[0].FaceValue - pricing.Premium) : leg.Fields[0].Direction * (stockPrice - leg.Fields[0].Price);
            }
        }, {
            key: "getStrategyData",
            value: function getStrategyData(strategy) {
                var _this2 = this;

                var getLegPricing = function getLegPricing(leg) {
                    return leg.Pricing == null ? function (arg00) {
                        return function (arg10) {
                            return _this2.legPricing(arg00, arg10);
                        };
                    }(strategy.Stock)(leg) : leg.Pricing;
                };

                var payOffsPerLeg = _fableCore.Seq.map(function (leg) {
                    var pricing = getLegPricing(leg);

                    var pricedLeg = function () {
                        var Pricing = pricing;
                        return new _OptionsModel.Leg(leg.Definition, Pricing);
                    }();

                    var payoffCalculator = function (arg00) {
                        return function (arg10) {
                            return function (arg20) {
                                return function (arg30) {
                                    return _this2.legPayoff(arg00, arg10, arg20, arg30);
                                };
                            };
                        };
                    }(leg.Definition)(pricing);

                    return [pricedLeg, payoffCalculator];
                }, strategy.Legs);

                var interestingPoints = this.getInterestingPoints(strategy);

                var hasConverts = _fableCore.Seq.exists(function (leg) {
                    return leg.Definition.Case === "Convertible" ? true : false;
                }, strategy.Legs);

                if (hasConverts) {
                    var _ret = function () {
                        var years = _fableCore.List.ofArray([1, 2, 3]);

                        var legsData = _fableCore.Seq.map(function (tupledArg) {
                            return [tupledArg[0], _fableCore.Seq.map(function (year) {
                                return _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                                    return _fableCore.Seq.map(function (stockPrice) {
                                        return [stockPrice, tupledArg[1](year)(stockPrice)];
                                    }, interestingPoints);
                                }));
                            }, years)];
                        }, payOffsPerLeg);

                        var strategyData = _fableCore.Seq.map(function (year) {
                            return _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                                return _fableCore.Seq.map(function (stockPrice) {
                                    return [stockPrice, _fableCore.Seq.sumBy(function (tupledArg) {
                                        return tupledArg[1](year)(stockPrice);
                                    }, payOffsPerLeg)];
                                }, interestingPoints);
                            }));
                        }, years);

                        return {
                            v: new _OptionsModel.StrategyData("MultiYear", [strategyData])
                        };
                    }();

                    if ((typeof _ret === "undefined" ? "undefined" : _typeof(_ret)) === "object") return _ret.v;
                } else {
                    var _legsData = _fableCore.Seq.map(function (tupledArg) {
                        return [tupledArg[0], _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                            return _fableCore.Seq.map(function (stockPrice) {
                                return [stockPrice, tupledArg[1](1)(stockPrice)];
                            }, interestingPoints);
                        }))];
                    }, payOffsPerLeg);

                    var _strategyData = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                        return _fableCore.Seq.map(function (stockPrice) {
                            return [stockPrice, _fableCore.Seq.sumBy(function (tupledArg) {
                                return tupledArg[1](1)(stockPrice);
                            }, payOffsPerLeg)];
                        }, interestingPoints);
                    }));

                    return new _OptionsModel.StrategyData("SingleYear", [_strategyData, _legsData]);
                }
            }
        }]);

        return PayoffsGenerator;
    }();

    _fableCore.Util.setInterfaces(PayoffsGenerator.prototype, [], "Pricer.Core.PayoffsGenerator");
});
//# sourceMappingURL=PayoffsGenerator.js.map