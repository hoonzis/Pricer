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
    global.unknown = mod.exports;
  }
})(this, function (exports, _fableCore) {
  "use strict";

  Object.defineProperty(exports, "__esModule", {
    value: true
  });
  exports.BasicOptions = exports.StrategyData = exports.LegData = exports.Strategy = exports.Leg = exports.Pricing = exports.LegInfo = exports.ConvertibleLeg = exports.CashLeg = exports.OptionLeg = exports.OptionStyle = exports.OptionKind = undefined;

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

  var OptionKind = exports.OptionKind = function () {
    function OptionKind() {
      _classCallCheck(this, OptionKind);

      this.Case = arguments[0];
      this.Fields = [];

      for (var i = 1; i < arguments.length; i++) {
        this.Fields[i - 1] = arguments[i];
      }
    }

    _createClass(OptionKind, [{
      key: "Name",
      get: function () {
        return this.Case === "Put" ? "Put" : "Call";
      }
    }]);

    return OptionKind;
  }();

  var OptionStyle = exports.OptionStyle = function OptionStyle() {
    _classCallCheck(this, OptionStyle);

    this.Case = arguments[0];
    this.Fields = [];

    for (var i = 1; i < arguments.length; i++) {
      this.Fields[i - 1] = arguments[i];
    }
  };

  var OptionLeg = exports.OptionLeg = function () {
    function OptionLeg($arg0, $arg1, $arg2, $arg3, $arg4, $arg5) {
      _classCallCheck(this, OptionLeg);

      this.Direction = $arg0;
      this.Strike = $arg1;
      this.Expiry = $arg2;
      this.Kind = $arg3;
      this.Style = $arg4;
      this.PurchaseDate = $arg5;
    }

    _createClass(OptionLeg, [{
      key: "BuyVsSell",
      get: function () {
        return this.Direction < 0 ? "Sell" : "Buy";
      }
    }, {
      key: "TimeToExpiry",
      get: function () {
        var copyOfStruct;
        return (copyOfStruct = _fableCore.Date.op_Subtraction(this.Expiry, this.PurchaseDate), _fableCore.TimeSpan.days(copyOfStruct)) / 365;
      }
    }, {
      key: "Name",
      get: function () {
        return function () {
          return function () {
            var clo1;
            return clo1 = _fableCore.String.fsFormat("%s %s %.2f")(function (x) {
              return x;
            }), function (arg10) {
              return function () {
                var clo2;
                return clo2 = clo1(arg10), function (arg20) {
                  return function () {
                    var clo3;
                    return clo3 = clo2(arg20), function (arg30) {
                      return clo3(arg30);
                    };
                  }();
                };
              }();
            };
          }();
        }()(this.BuyVsSell)(this.Kind.Name)(this.Strike);
      }
    }]);

    return OptionLeg;
  }();

  var CashLeg = exports.CashLeg = function CashLeg($arg0, $arg1) {
    _classCallCheck(this, CashLeg);

    this.Direction = $arg0;
    this.Price = $arg1;
  };

  var ConvertibleLeg = exports.ConvertibleLeg = function ConvertibleLeg($arg0, $arg1, $arg2, $arg3, $arg4, $arg5) {
    _classCallCheck(this, ConvertibleLeg);

    this.Direction = $arg0;
    this.Coupon = $arg1;
    this.ConversionRatio = $arg2;
    this.Maturity = $arg3;
    this.FaceValue = $arg4;
    this.ReferencePrice = $arg5;
  };

  var LegInfo = exports.LegInfo = function () {
    function LegInfo() {
      _classCallCheck(this, LegInfo);

      this.Case = arguments[0];
      this.Fields = [];

      for (var i = 1; i < arguments.length; i++) {
        this.Fields[i - 1] = arguments[i];
      }
    }

    _createClass(LegInfo, [{
      key: "Name",
      get: function () {
        var ol, convert, cl;
        return this.Case === "Option" ? (ol = this.Fields[0], ol.Name) : this.Case === "Convertible" ? (convert = this.Fields[0], function () {
          return function () {
            var clo1;
            return clo1 = _fableCore.String.fsFormat("Convert %f")(function (x) {
              return x;
            }), function (arg10) {
              return clo1(arg10);
            };
          }();
        }()(convert.FaceValue)) : (cl = this.Fields[0], "Cash");
      }
    }]);

    return LegInfo;
  }();

  var Pricing = exports.Pricing = function Pricing($arg0, $arg1) {
    _classCallCheck(this, Pricing);

    this.Delta = $arg0;
    this.Premium = $arg1;
  };

  var Leg = exports.Leg = function Leg($arg0, $arg1) {
    _classCallCheck(this, Leg);

    this.Definition = $arg0;
    this.Pricing = $arg1;
  };

  var Strategy = exports.Strategy = function Strategy($arg0, $arg1, $arg2) {
    _classCallCheck(this, Strategy);

    this.Stock = $arg0;
    this.Name = $arg1;
    this.Legs = $arg2;
  };

  var LegData = exports.LegData = function LegData($arg0, $arg1) {
    _classCallCheck(this, LegData);

    this.Leg = $arg0;
    this.LegData = $arg1;
  };

  var StrategyData = exports.StrategyData = function StrategyData() {
    _classCallCheck(this, StrategyData);

    this.Case = arguments[0];
    this.Fields = [];

    for (var i = 1; i < arguments.length; i++) {
      this.Fields[i - 1] = arguments[i];
    }
  };

  var BasicOptions = exports.BasicOptions = function ($exports) {
    var optionValue = $exports.optionValue = function (option, stockPrice) {
      var matchValue;
      return matchValue = option.Kind, matchValue.Case === "Put" ? 0 > option.Strike - stockPrice ? 0 : option.Strike - stockPrice : 0 > stockPrice - option.Strike ? 0 : stockPrice - option.Strike;
    };

    var buildLeg = $exports.buildLeg = function (kind, strike, direction, style, expiry, buyingDate) {
      var Kind;
      return Kind = new OptionKind("Call"), new OptionLeg(direction, strike, expiry, Kind, new OptionStyle("European"), buyingDate);
    };

    return $exports;
  }({});
});