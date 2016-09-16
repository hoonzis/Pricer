(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "fable-core"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("fable-core"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.fableCore);
        global.StocksModel = mod.exports;
    }
})(this, function (exports, _fableCore) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.StockInfo = undefined;

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

    var StockInfo = exports.StockInfo = function () {
        function StockInfo(rate, volatility, currentPrice) {
            _classCallCheck(this, StockInfo);

            this.Rate = rate;
            this.Volatility = volatility;
            this.CurrentPrice = currentPrice;
        }

        _createClass(StockInfo, [{
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

        return StockInfo;
    }();

    _fableCore.Util.setInterfaces(StockInfo.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.StockInfo");
});
//# sourceMappingURL=StocksModel.js.map