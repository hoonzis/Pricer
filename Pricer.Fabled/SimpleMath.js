define(["exports", "fable-core"], function (exports, _fableCore) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.SimpleMathProvider = exports.SimpleMath = undefined;

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

    var SimpleMath = exports.SimpleMath = function ($exports) {
        var signOf = $exports.signOf = function (x) {
            return x < 0 ? -1 : 1;
        };

        var cdf = $exports.cdf = function (x) {
            var a1, a2, a3, a4, a5, p, absX, t, y, result;
            return a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741, a4 = -1.453152027, a5 = 1.061405429, p = 0.3275911, absX = Math.abs(x), t = 1 / (1 + p * absX), y = 1 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.exp(-absX * absX), result = signOf(x) * y, result;
        };

        return $exports;
    }({});

    var SimpleMathProvider = exports.SimpleMathProvider = function () {
        function SimpleMathProvider() {
            _classCallCheck(this, SimpleMathProvider);
        }

        _createClass(SimpleMathProvider, [{
            key: "cdf",
            value: function cdf(x) {
                return SimpleMath.cdf(x);
            }
        }]);

        return SimpleMathProvider;
    }();

    _fableCore.Util.setInterfaces(SimpleMathProvider.prototype, ["Pricer.Core.IMathProvider"]);
});
//# sourceMappingURL=SimpleMath.js.map