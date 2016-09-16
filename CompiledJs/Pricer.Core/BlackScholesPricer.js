(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "./OptionsModel", "fable-core"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("./OptionsModel"), require("fable-core"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.OptionsModel, global.fableCore);
        global.BlackScholesPricer = mod.exports;
    }
})(this, function (exports, _OptionsModel, _fableCore) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.BlackScholesPricer = undefined;

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

    var BlackScholesPricer = exports.BlackScholesPricer = function () {
        function BlackScholesPricer(math) {
            _classCallCheck(this, BlackScholesPricer);

            this.math = math;
        }

        _createClass(BlackScholesPricer, [{
            key: "blackScholes",
            value: function blackScholes(stock, option) {
                var _this = this;

                var patternInput = option.TimeToExpiry > 0 ? function () {
                    var d1 = (Math.log(stock.CurrentPrice / option.Strike) + (stock.Rate + 0.5 * Math.pow(stock.Volatility, 2)) * option.TimeToExpiry) / (stock.Volatility * Math.sqrt(option.TimeToExpiry));
                    var d2 = d1 - stock.Volatility * Math.sqrt(option.TimeToExpiry);

                    var N1 = _this.math.cdf(d1);

                    var N2 = _this.math.cdf(d2);

                    var discountedStrike = option.Strike * Math.exp(-stock.Rate * option.TimeToExpiry);
                    var call = stock.CurrentPrice * N1 - discountedStrike * N2;

                    if (option.Kind.Case === "Put") {
                        return [call + discountedStrike - stock.CurrentPrice, N1 - 1];
                    } else {
                        return [call, N1];
                    }
                }() : option.Kind.Case === "Put" ? [option.Strike - stock.CurrentPrice > 0 ? option.Strike - stock.CurrentPrice : 0, 1] : [stock.CurrentPrice - option.Strike > 0 ? stock.CurrentPrice - option.Strike : 0, 1];
                return new _OptionsModel.Pricing(patternInput[1], patternInput[0]);
            }
        }]);

        return BlackScholesPricer;
    }();

    _fableCore.Util.setInterfaces(BlackScholesPricer.prototype, [], "Pricer.Core.BlackScholesPricer");
});
//# sourceMappingURL=BlackScholesPricer.js.map