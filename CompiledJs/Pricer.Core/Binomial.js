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
        global.Binomial = mod.exports;
    }
})(this, function (exports, _fableCore, _OptionsModel) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.Binomial = exports.BinomialPricing = exports.Implementation = undefined;

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

    var Implementation = exports.Implementation = function () {
        function Implementation(caseName, fields) {
            _classCallCheck(this, Implementation);

            this.Case = caseName;
            this.Fields = fields;
        }

        _createClass(Implementation, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsUnions(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareUnions(this, other);
            }
        }]);

        return Implementation;
    }();

    _fableCore.Util.setInterfaces(Implementation.prototype, ["FSharpUnion", "System.IEquatable", "System.IComparable"], "Pricer.Core.Implementation");

    var BinomialPricing = exports.BinomialPricing = function () {
        function BinomialPricing(periods, down, up, pUp, pDown, option, rate, ref) {
            _classCallCheck(this, BinomialPricing);

            this.Periods = periods;
            this.Down = down;
            this.Up = up;
            this.PUp = pUp;
            this.PDown = pDown;
            this.Option = option;
            this.Rate = rate;
            this.Ref = ref;
        }

        _createClass(BinomialPricing, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsRecords(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareRecords(this, other);
            }
        }]);

        return BinomialPricing;
    }();

    _fableCore.Util.setInterfaces(BinomialPricing.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.BinomialPricing");

    var Binomial = exports.Binomial = function ($exports) {
        var binomialPrice = $exports.binomialPrice = function binomialPrice(ref, strike, rate, up) {
            var down = 1 / up;
            var q = (Math.exp(-rate) - down) / (up - down);
            var cu = 0 > up * ref - strike ? 0 : up * ref - strike;
            var cd = 0 > down * ref - strike ? 0 : down * ref - strike;
            var call = Math.exp(-rate) * (q * cu + (1 - q) * cd);
            return call;
        };

        var binomialPricing = $exports.binomialPricing = function binomialPricing(pricing) {
            var prices = new Float64Array(pricing.Periods);

            var optionValue = function () {
                var matchValue = pricing.Option.Kind;

                if (matchValue.Case === "Put") {
                    return function (i) {
                        return 0 > pricing.Option.Strike - prices[i] ? 0 : pricing.Option.Strike - prices[i];
                    };
                } else {
                    return function (i) {
                        return 0 > prices[i] - pricing.Option.Strike ? 0 : prices[i] - pricing.Option.Strike;
                    };
                }
            }();

            prices[0] = pricing.Ref * Math.pow(pricing.Down, pricing.Periods);
            var oValues = new Float64Array(pricing.Periods);
            oValues[0] = optionValue(0);

            for (var i = 1; i <= pricing.Periods - 1; i++) {
                prices[i] = prices[i - 1] * pricing.Up * pricing.Up;
                oValues[i] = optionValue(i);
            }

            var counter = pricing.Periods - 2;

            for (var _step = counter; _step >= 0; _step--) {
                for (var j = 0; j <= _step; j++) {
                    oValues[j] = (pricing.PUp * oValues[j + 1] + pricing.PDown * oValues[j]) * (1 / pricing.Rate);

                    if (pricing.Option.Style.Equals(new _OptionsModel.OptionStyle("American", []))) {
                        prices[j] = pricing.Down * prices[j + 1];

                        if (oValues[j] > optionValue(j)) {
                            oValues[j] = oValues[j];
                        } else {
                            oValues[j] = optionValue(j);
                        }
                    }
                }
            }

            var delta = (oValues[1] - oValues[1]) / (pricing.Ref * pricing.Up - pricing.Ref * pricing.Down);
            var Premium = oValues[0];
            return new _OptionsModel.Pricing(delta, Premium);
        };

        var generateEndNodePrices = $exports.generateEndNodePrices = function generateEndNodePrices(ref, up, periods, optionVal) {
            var down = 1 / up;
            var lowestStock = ref * Math.pow(down, periods);
            var first = [lowestStock, optionVal(lowestStock)];

            var values = _fableCore.Seq.unfold(function (tupledArg) {
                var stock_ = tupledArg[0] * up * up;
                var der_ = optionVal(stock_);
                return [[tupledArg[0], tupledArg[1]], [stock_, der_]];
            }, first);

            return _fableCore.Seq.toList(function (source) {
                return _fableCore.Seq.take(periods, source);
            }(values));
        };

        var step = $exports.step = function step(pricing, optionVal, prices) {
            return _fableCore.Seq.toList(_fableCore.Seq.map(function (tupledArg) {
                var derValue = (pricing.PUp * tupledArg[1][1] + pricing.PDown * tupledArg[0][1]) * (1 / pricing.Rate);
                var stockValue = tupledArg[1][0] * pricing.Down;

                var der_ = function () {
                    var matchValue = pricing.Option.Style;

                    if (matchValue.Case === "European") {
                        return derValue;
                    } else {
                        var prematureExValue = optionVal(stockValue);

                        if (derValue > prematureExValue) {
                            return derValue;
                        } else {
                            return prematureExValue;
                        }
                    }
                }();

                return [stockValue, der_];
            }, _fableCore.Seq.pairwise(prices)));
        };

        var binomialPricingFunc = $exports.binomialPricingFunc = function binomialPricingFunc(pricing) {
            var optionValue = function optionValue(stockPrice) {
                return _OptionsModel.BasicOptions.optionValue(pricing.Option, stockPrice);
            };

            var prices = generateEndNodePrices(pricing.Ref, pricing.Up, pricing.Periods, optionValue);

            var reductionStep = function reductionStep(prices_1) {
                return step(pricing, optionValue, prices_1);
            };

            var reducePrices = function reducePrices(prices_1) {
                var $target1 = function $target1(prs) {
                    return reducePrices(reductionStep(prs));
                };

                if (prices_1.tail != null) {
                    if (prices_1.tail.tail == null) {
                        var der = prices_1.head[1];
                        var stock = prices_1.head[0];
                        return der;
                    } else {
                        return $target1(prices_1);
                    }
                } else {
                    return $target1(prices_1);
                }
            };

            var premium = reducePrices(prices);
            return new _OptionsModel.Pricing(1, premium);
        };

        var generateEndNodePricesFast = $exports.generateEndNodePricesFast = function generateEndNodePricesFast(ref, up, periods, optionVal) {
            var down = 1 / up;
            var lowestStock = ref * Math.pow(down, periods);
            var first = [lowestStock, optionVal(lowestStock)];

            var values = _fableCore.Seq.unfold(function (tupledArg) {
                var stock_ = tupledArg[0] * up * up;
                var der_ = optionVal(stock_);
                return [[tupledArg[0], tupledArg[1]], [stock_, der_]];
            }, first);

            return Array.from(function (source) {
                return _fableCore.Seq.take(periods, source);
            }(values));
        };

        var stepFast = $exports.stepFast = function stepFast(pricing, optionVal, prices) {
            return Array.from(_fableCore.Seq.pairwise(prices)).map(function (tupledArg) {
                var derValue = (pricing.PUp * tupledArg[1][1] + pricing.PDown * tupledArg[0][1]) * (1 / pricing.Rate);
                var stockValue = tupledArg[1][0] * pricing.Down;

                var der_ = function () {
                    var matchValue = pricing.Option.Style;

                    if (matchValue.Case === "European") {
                        return derValue;
                    } else {
                        var prematureExValue = optionVal(stockValue);

                        if (derValue > prematureExValue) {
                            return derValue;
                        } else {
                            return prematureExValue;
                        }
                    }
                }();

                return [stockValue, der_];
            });
        };

        var binomialPricingFuncFast = $exports.binomialPricingFuncFast = function binomialPricingFuncFast(pricing) {
            var optionValue = function optionValue(stockPrice) {
                return _OptionsModel.BasicOptions.optionValue(pricing.Option, stockPrice);
            };

            var prices = generateEndNodePricesFast(pricing.Ref, pricing.Up, pricing.Periods, optionValue);

            var reductionStep = function reductionStep(prices_1) {
                return stepFast(pricing, optionValue, prices_1);
            };

            var reducePrices = function reducePrices(prices_1) {
                return prices_1.length === 1 ? function () {
                    var stock = prices_1[0][0];
                    var der = prices_1[0][1];
                    return der;
                }() : reducePrices(reductionStep(prices_1));
            };

            var premium = reducePrices(prices);
            return new _OptionsModel.Pricing(1, premium);
        };

        var binomial = $exports.binomial = function binomial(stock, option, steps, implementation) {
            var deltaT = option.TimeToExpiry / steps;
            var up = Math.exp(stock.Volatility * Math.sqrt(deltaT));
            var down = 1 / up;
            var R = Math.exp(stock.Rate * deltaT);
            var p_up = (R - down) / (up - down);
            var p_down = 1 - p_up;
            var pricing = new BinomialPricing(steps, down, up, p_up, p_down, option, R, stock.CurrentPrice);

            if (implementation.Case === "Functional") {
                return binomialPricingFunc(pricing);
            } else {
                if (implementation.Case === "FunctionalFast") {
                    return binomialPricingFuncFast(pricing);
                } else {
                    return binomialPricing(pricing);
                }
            }
        };

        return $exports;
    }({});
});
//# sourceMappingURL=Binomial.js.map