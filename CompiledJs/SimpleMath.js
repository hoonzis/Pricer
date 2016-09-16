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
        var signOf = $exports.signOf = function signOf(x) {
            return x < 0 ? -1 : 1;
        };

        var cdf = $exports.cdf = function cdf(x) {
            var a1 = 0.254829592;
            var a2 = -0.284496736;
            var a3 = 1.421413741;
            var a4 = -1.453152027;
            var a5 = 1.061405429;
            var p = 0.3275911;
            var absX = Math.abs(x);
            var t = 1 / (1 + p * absX);
            var y = 1 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.exp(-absX * absX);
            var result = signOf(x) * y;
            return result;
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

    _fableCore.Util.setInterfaces(SimpleMathProvider.prototype, ["Pricer.Core.IMathProvider"], "Pricer.Fabled.SimpleMathProvider");
});
//# sourceMappingURL=SimpleMath.js.map