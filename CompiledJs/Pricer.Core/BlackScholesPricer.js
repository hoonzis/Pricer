(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "./OptionsModel"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("./OptionsModel"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.OptionsModel);
        global.unknown = mod.exports;
    }
})(this, function (exports, _OptionsModel) {
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
                var patternInput, price, delta, d1, d2, N1, N2, discountedStrike, call, matchValue;
                return patternInput = option.TimeToExpiry > 0 ? (d1 = (Math.log(stock.CurrentPrice / option.Strike) + (stock.Rate + 0.5 * Math.pow(stock.Volatility, 2)) * option.TimeToExpiry) / (stock.Volatility * Math.sqrt(option.TimeToExpiry)), d2 = d1 - stock.Volatility * Math.sqrt(option.TimeToExpiry), N1 = this.math.cdf(d1), N2 = this.math.cdf(d2), discountedStrike = option.Strike * Math.exp(-stock.Rate * option.TimeToExpiry), call = stock.CurrentPrice * N1 - discountedStrike * N2, matchValue = option.Kind, matchValue.Case === "Put" ? [call + discountedStrike - stock.CurrentPrice, N1 - 1] : [call, N1]) : (matchValue = option.Kind, matchValue.Case === "Put" ? [option.Strike - stock.CurrentPrice > 0 ? option.Strike - stock.CurrentPrice : 0, 1] : [stock.CurrentPrice - option.Strike > 0 ? stock.CurrentPrice - option.Strike : 0, 1]), price = patternInput[0], delta = patternInput[1], new _OptionsModel.Pricing(delta, price);
            }
        }]);

        return BlackScholesPricer;
    }();
});
//# sourceMappingURL=BlackScholesPricer.js.map