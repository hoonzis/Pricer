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

  var signOf = exports.signOf = function (x) {
    return x < 0 ? 1 : -1;
  };

  var cdf = exports.cdf = function (x) {
    var a1, a2, a3, a4, a5, p, absX, t, y, result;
    return a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741, a4 = -1.453152027, a5 = 1.061405429, p = 0.3275911, absX = Math.abs(x), t = 1 / (1 + p * absX), y = 1 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.exp(-absX * absX), result = signOf(x) * y, result;
  };
});