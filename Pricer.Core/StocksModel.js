(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports);
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports);
        global.unknown = mod.exports;
    }
})(this, function (exports) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });

    function _classCallCheck(instance, Constructor) {
        if (!(instance instanceof Constructor)) {
            throw new TypeError("Cannot call a class as a function");
        }
    }

    var StockInfo = exports.StockInfo = function StockInfo($arg0, $arg1, $arg2) {
        _classCallCheck(this, StockInfo);

        this.Rate = $arg0;
        this.Volatility = $arg1;
        this.CurrentPrice = $arg2;
    };
});
//# sourceMappingURL=StocksModel.js.map