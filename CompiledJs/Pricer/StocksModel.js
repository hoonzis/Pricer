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

  var Exchange = exports.Exchange = function Exchange() {
    _classCallCheck(this, Exchange);

    this.Case = arguments[0];
    this.Fields = [];

    for (var i = 1; i < arguments.length; i++) {
      this.Fields[i - 1] = arguments[i];
    }
  };

  var StockRefData = exports.StockRefData = function StockRefData($arg0, $arg1) {
    _classCallCheck(this, StockRefData);

    this.Exchange = $arg0;
    this.Ticker = $arg1;
  };

  var Tick = exports.Tick = function Tick($arg0, $arg1, $arg2) {
    _classCallCheck(this, Tick);

    this.Date = $arg0;
    this.Close = $arg1;
    this.Open = $arg2;
  };

  var VolatilityEstimationMethod = exports.VolatilityEstimationMethod = function VolatilityEstimationMethod() {
    _classCallCheck(this, VolatilityEstimationMethod);

    this.Case = arguments[0];
    this.Fields = [];

    for (var i = 1; i < arguments.length; i++) {
      this.Fields[i - 1] = arguments[i];
    }
  };
});