(function (global, factory) {
  if (typeof define === "function" && define.amd) {
    define(["exports", "fable-core", "./SimpleMath", "./StocksModel"], factory);
  } else if (typeof exports !== "undefined") {
    factory(exports, require("fable-core"), require("./SimpleMath"), require("./StocksModel"));
  } else {
    var mod = {
      exports: {}
    };
    factory(mod.exports, global.fableCore, global.SimpleMath, global.StocksModel);
    global.unknown = mod.exports;
  }
})(this, function (exports, _fableCore, _SimpleMath, _StocksModel) {
  "use strict";

  Object.defineProperty(exports, "__esModule", {
    value: true
  });
  exports.Options = exports.StrategyData = exports.LegData = exports.Strategy = exports.Leg = exports.Pricing = exports.LegInfo = exports.ConvertibleLeg = exports.CashLeg = exports.OptionLeg = exports.OptionStyle = exports.OptionKind = undefined;

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

  var Options = exports.Options = function ($exports) {
    var buildLeg = $exports.buildLeg = function (kind, strike, direction, style, expiry, buyingDate) {
      var Kind;
      return Kind = new OptionKind("Call"), new OptionLeg(direction, strike, expiry, Kind, new OptionStyle("European"), buyingDate);
    };

    var optionValue = $exports.optionValue = function (option, stockPrice) {
      var matchValue;
      return matchValue = option.Kind, matchValue.Case === "Put" ? 0 > option.Strike - stockPrice ? 0 : option.Strike - stockPrice : 0 > stockPrice - option.Strike ? 0 : stockPrice - option.Strike;
    };

    var legPayoff = $exports.legPayoff = function (leg, pricing, year, stockPrice) {
      var optionLeg, convertible, cashLeg;
      return leg.Case === "Option" ? (optionLeg = leg.Fields[0], optionLeg.Direction * (optionValue(optionLeg, stockPrice) - pricing.Premium)) : leg.Case === "Convertible" ? (convertible = leg.Fields[0], convertible.Direction * (year * convertible.Coupon * convertible.FaceValue - pricing.Premium)) : (cashLeg = leg.Fields[0], cashLeg.Direction * (stockPrice - cashLeg.Price));
    };

    var cashPricing = $exports.cashPricing = function (leg) {
      var Premium;
      return Premium = leg.Price, new Pricing(1, Premium);
    };

    var convertiblePricing = $exports.convertiblePricing = function (stock, leg) {
      var Premium;
      return Premium = leg.ReferencePrice, new Pricing(0.45, Premium);
    };

    var blackScholes = $exports.blackScholes = function (stock, option) {
      var patternInput, price, delta, d1, d2, N1, N2, discountedStrike, call, matchValue;
      return patternInput = option.TimeToExpiry > 0 ? (d1 = (Math.log(stock.CurrentPrice / option.Strike) + (stock.Rate + 0.5 * Math.pow(stock.Volatility, 2)) * option.TimeToExpiry) / (stock.Volatility * Math.sqrt(option.TimeToExpiry)), d2 = d1 - stock.Volatility * Math.sqrt(option.TimeToExpiry), N1 = (0, _SimpleMath.cdf)(d1), N2 = (0, _SimpleMath.cdf)(d2), discountedStrike = option.Strike * Math.exp(-stock.Rate * option.TimeToExpiry), call = stock.CurrentPrice * N1 - discountedStrike * N2, matchValue = option.Kind, matchValue.Case === "Put" ? [call + discountedStrike - stock.CurrentPrice, N1 - 1] : [call, N1]) : (matchValue = option.Kind, matchValue.Case === "Put" ? [option.Strike - stock.CurrentPrice > 0 ? option.Strike - stock.CurrentPrice : 0, 1] : [stock.CurrentPrice - option.Strike > 0 ? stock.CurrentPrice - option.Strike : 0, 1]), price = patternInput[0], delta = patternInput[1], new Pricing(delta, price);
    };

    var europeanBSPrice = $exports.europeanBSPrice = function (rate, direction, ref, vol, strike, expiry, legType, buyingDate) {
      var leg, stockInfo;
      return leg = buildLeg(legType, strike, direction, new OptionStyle("European"), expiry, buyingDate), stockInfo = new _StocksModel.StockInfo(rate, vol, ref), blackScholes(stockInfo, leg);
    };

    var legPricing = $exports.legPricing = function (stock, leg) {
      var matchValue, optionLeg, convertible, cashLeg;
      return matchValue = leg.Definition, matchValue.Case === "Option" ? (optionLeg = matchValue.Fields[0], blackScholes(stock, optionLeg)) : matchValue.Case === "Convertible" ? (convertible = matchValue.Fields[0], convertiblePricing(stock, convertible)) : (cashLeg = matchValue.Fields[0], cashPricing(cashLeg));
    };

    var getInterestingPoints = $exports.getInterestingPoints = function (strategy) {
      var strikes, min, max;
      return _fableCore.Seq.isEmpty(strategy.Legs) ? _fableCore.Seq.empty() : (strikes = _fableCore.List.map(function (leg) {
        var matchValue, option, convertible, cash;
        return matchValue = leg.Definition, matchValue.Case === "Option" ? (option = matchValue.Fields[0], option.Strike) : matchValue.Case === "Convertible" ? (convertible = matchValue.Fields[0], convertible.ReferencePrice) : (cash = matchValue.Fields[0], cash.Price);
      }, strategy.Legs), min = 0.5 * _fableCore.Seq.min(strikes), max = 1.5 * _fableCore.Seq.max(strikes), _fableCore.Seq.delay(function (unitVar) {
        return _fableCore.Seq.append(_fableCore.Seq.singleton(min), _fableCore.Seq.delay(function (unitVar_1) {
          return _fableCore.Seq.append(_fableCore.Seq.sort(strikes), _fableCore.Seq.delay(function (unitVar_2) {
            return _fableCore.Seq.singleton(max);
          }));
        }));
      }));
    };

    var getStrategyData = $exports.getStrategyData = function (strategy) {
      var getLegPricing, payOffsPerLeg, interestingPoints, hasConverts, years, legsData, strategyData;
      return getLegPricing = function (leg) {
        var matchValue, pricing;
        return matchValue = leg.Pricing, matchValue == null ? legPricing(strategy.Stock, leg) : (pricing = matchValue, pricing);
      }, payOffsPerLeg = _fableCore.Seq.map(function (leg) {
        var pricing, pricedLeg, payoffCalculator, Pricing_1;
        return pricing = getLegPricing(leg), pricedLeg = (Pricing_1 = pricing, new Leg(leg.Definition, Pricing_1)), payoffCalculator = function () {
          var leg_1;
          return leg_1 = leg.Definition, function (year) {
            return function (stockPrice) {
              return legPayoff(leg_1, pricing, year, stockPrice);
            };
          };
        }(), [pricedLeg, payoffCalculator];
      }, strategy.Legs), interestingPoints = getInterestingPoints(strategy), hasConverts = _fableCore.Seq.exists(function (leg) {
        var matchValue;
        return matchValue = leg.Definition, matchValue.Case === "Convertible" ? true : false;
      }, strategy.Legs), hasConverts ? (years = _fableCore.List.ofArray([1, 2, 3]), legsData = _fableCore.Seq.map(function (tupledArg) {
        var leg, payOff;
        return leg = tupledArg[0], payOff = tupledArg[1], [leg, _fableCore.Seq.map(function (year) {
          return _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
            return _fableCore.Seq.map(function (stockPrice) {
              return [stockPrice, payOff(year)(stockPrice)];
            }, interestingPoints);
          }));
        }, years)];
      }, payOffsPerLeg), strategyData = _fableCore.Seq.map(function (year) {
        return _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
          return _fableCore.Seq.map(function (stockPrice) {
            return [stockPrice, _fableCore.Seq.sumBy(function (tupledArg) {
              return function () {
                var leg, payOff;
                return leg = tupledArg[0], payOff = tupledArg[1], payOff(year)(stockPrice);
              }();
            }, payOffsPerLeg)];
          }, interestingPoints);
        }));
      }, years), new StrategyData("MultiYear", strategyData)) : (legsData = _fableCore.Seq.map(function (tupledArg) {
        var leg, payOff;
        return leg = tupledArg[0], payOff = tupledArg[1], [leg, _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
          return _fableCore.Seq.map(function (stockPrice) {
            return [stockPrice, payOff(1)(stockPrice)];
          }, interestingPoints);
        }))];
      }, payOffsPerLeg), strategyData = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
        return _fableCore.Seq.map(function (stockPrice) {
          return [stockPrice, _fableCore.Seq.sumBy(function (tupledArg) {
            return function () {
              var leg, payOff;
              return leg = tupledArg[0], payOff = tupledArg[1], payOff(1)(stockPrice);
            }();
          }, payOffsPerLeg)];
        }, interestingPoints);
      })), new StrategyData("SingleYear", strategyData, legsData));
    };

    return $exports;
  }({});
});