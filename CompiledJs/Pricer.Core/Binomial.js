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
        global.unknown = mod.exports;
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

    var Implementation = exports.Implementation = function Implementation() {
        _classCallCheck(this, Implementation);

        this.Case = arguments[0];
        this.Fields = [];

        for (var i = 1; i < arguments.length; i++) {
            this.Fields[i - 1] = arguments[i];
        }
    };

    var BinomialPricing = exports.BinomialPricing = function BinomialPricing($arg0, $arg1, $arg2, $arg3, $arg4, $arg5, $arg6, $arg7) {
        _classCallCheck(this, BinomialPricing);

        this.Periods = $arg0;
        this.Down = $arg1;
        this.Up = $arg2;
        this.PUp = $arg3;
        this.PDown = $arg4;
        this.Option = $arg5;
        this.Rate = $arg6;
        this.Ref = $arg7;
    };

    var Binomial = exports.Binomial = function ($exports) {
        var binomialPrice = $exports.binomialPrice = function (ref, strike, rate, up) {
            var down, q, cu, cd, call;
            return down = 1 / up, q = (Math.exp(-rate) - down) / (up - down), cu = 0 > up * ref - strike ? 0 : up * ref - strike, cd = 0 > down * ref - strike ? 0 : down * ref - strike, call = Math.exp(-rate) * (q * cu + (1 - q) * cd), call;
        };

        var binomialPricing = $exports.binomialPricing = function (pricing) {
            return function () {
                var prices = new Float64Array(pricing.Periods);

                var optionValue = function () {
                    var matchValue;
                    return matchValue = pricing.Option.Kind, matchValue.Case === "Put" ? function (i) {
                        return 0 > pricing.Option.Strike - prices[i] ? 0 : pricing.Option.Strike - prices[i];
                    } : function (i) {
                        return 0 > prices[i] - pricing.Option.Strike ? 0 : prices[i] - pricing.Option.Strike;
                    };
                }();

                prices[0] = pricing.Ref * Math.pow(pricing.Down, pricing.Periods);
                var oValues = new Float64Array(pricing.Periods);
                oValues[0] = optionValue(0);

                for (var i = 1; i <= pricing.Periods - 1; i++) {
                    prices[i] = prices[i - 1] * pricing.Up * pricing.Up;
                    oValues[i] = optionValue(i);
                }

                var counter = pricing.Periods - 2;

                for (var step = counter; step <= 0; step++) {
                    for (var j = 0; j <= step; j++) {
                        oValues[j] = (pricing.PUp * oValues[j + 1] + pricing.PDown * oValues[j]) * (1 / pricing.Rate);
                        _fableCore.Util.compareTo(pricing.Option.Style, new _OptionsModel.OptionStyle("American")) === 0 ? (prices[j] = pricing.Down * prices[j + 1], oValues[j] = oValues[j] > optionValue(j) ? oValues[j] : optionValue(j)) : null;
                    }
                }

                var delta = (oValues[1] - oValues[1]) / (pricing.Ref * pricing.Up - pricing.Ref * pricing.Down);
                var Premium = oValues[0];
                return new _OptionsModel.Pricing(delta, Premium);
            }();
        };

        var generateEndNodePrices = $exports.generateEndNodePrices = function (ref, up, periods, optionVal) {
            var down, lowestStock, first, values;
            return down = 1 / up, lowestStock = ref * Math.pow(down, periods), first = [lowestStock, optionVal(lowestStock)], values = _fableCore.Seq.unfold(function (tupledArg) {
                var stock, der, stock_, der_;
                return stock = tupledArg[0], der = tupledArg[1], stock_ = stock * up * up, der_ = optionVal(stock_), [[stock, der], [stock_, der_]];
            }, first), _fableCore.Seq.toList(function (source) {
                return _fableCore.Seq.take(periods, source);
            }(values));
        };

        var step = $exports.step = function (pricing, optionVal, prices) {
            return _fableCore.Seq.toList(_fableCore.Seq.map(function (tupledArg) {
                var _arg1, _arg2, sDown, dDown, sUp, dUp, derValue, stockValue, der_, matchValue, prematureExValue;

                return _arg1 = tupledArg[0], _arg2 = tupledArg[1], sDown = _arg1[0], dDown = _arg1[1], sUp = _arg2[0], dUp = _arg2[1], derValue = (pricing.PUp * dUp + pricing.PDown * dDown) * (1 / pricing.Rate), stockValue = sUp * pricing.Down, der_ = (matchValue = pricing.Option.Style, matchValue.Case === "European" ? derValue : (prematureExValue = optionVal(stockValue), derValue > prematureExValue ? derValue : prematureExValue)), [stockValue, der_];
            }, _fableCore.Seq.pairwise(prices)));
        };

        var binomialPricingFunc = $exports.binomialPricingFunc = function (pricing) {
            var optionValue, prices, reductionStep, reducePrices, premium;
            return optionValue = function () {
                var option;
                return option = pricing.Option, function (stockPrice) {
                    return _OptionsModel.BasicOptions.optionValue(option, stockPrice);
                };
            }(), prices = generateEndNodePrices(pricing.Ref, pricing.Up, pricing.Periods, optionValue), reductionStep = function (prices_1) {
                return step(pricing, optionValue, prices_1);
            }, reducePrices = function (prices_1) {
                var $target1, der, stock;
                return $target1 = function (prs) {
                    return reducePrices(reductionStep(prs));
                }, prices_1.tail != null ? prices_1.tail.tail == null ? (der = prices_1.head[1], stock = prices_1.head[0], der) : $target1(prices_1) : $target1(prices_1);
            }, premium = reducePrices(prices), new _OptionsModel.Pricing(1, premium);
        };

        var binomial = $exports.binomial = function (stock, option, steps, implementation) {
            var deltaT, up, down, R, p_up, p_down, pricing;
            return deltaT = option.TimeToExpiry / steps, up = Math.exp(stock.Volatility * Math.sqrt(deltaT)), down = 1 / up, R = Math.exp(stock.Rate * deltaT), p_up = (R - down) / (up - down), p_down = 1 - p_up, pricing = new BinomialPricing(steps, down, up, p_up, p_down, option, R, stock.CurrentPrice), implementation.Case === "Functional" ? binomialPricingFunc(pricing) : binomialPricing(pricing);
        };

        return $exports;
    }({});
});
//# sourceMappingURL=Binomial.js.map