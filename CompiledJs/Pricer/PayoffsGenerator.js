(function (global, factory) {
  if (typeof define === "function" && define.amd) {
    define(["exports", "fable-core", "./OptionsModel"], factory);
  } else if (typeof exports !== "undefined") {
    factory(exports, require("fable-core"), require("./OptionsModel"));
  } else {
    var mod = {
      exports: {}
    };
    factory(mod.exports, global.fableCore, global.OptionsModel);
    global.unknown = mod.exports;
  }
})(this, function (exports, _fableCore, _OptionsModel) {
  "use strict";

  Object.defineProperty(exports, "__esModule", {
    value: true
  });
  exports.PayoffsGenerator = undefined;

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

  var PayoffsGenerator = exports.PayoffsGenerator = function () {
    function PayoffsGenerator(pricer) {
      _classCallCheck(this, PayoffsGenerator);

      this.pricer = pricer;
    }

    _createClass(PayoffsGenerator, [{
      key: "legPricing",
      value: function legPricing(stock, leg) {
        var _this = this;

        return function () {
          var matchValue, cashLeg;
          return matchValue = leg.Definition, matchValue.Case === "Option" ? function () {
            var optionLeg;
            return optionLeg = matchValue.Fields[0], function () {
              return function () {
                var objectArg;
                return objectArg = _this.pricer, function (arg00) {
                  return function (arg10) {
                    return objectArg.priceOption(arg00, arg10);
                  };
                };
              }();
            }()(stock)(optionLeg);
          }() : matchValue.Case === "Convertible" ? function () {
            var convertible;
            return convertible = matchValue.Fields[0], function () {
              return function () {
                var objectArg;
                return objectArg = _this.pricer, function (arg00) {
                  return function (arg10) {
                    return objectArg.priceConvert(arg00, arg10);
                  };
                };
              }();
            }()(stock)(convertible);
          }() : (cashLeg = matchValue.Fields[0], _this.pricer.priceCash(cashLeg));
        }();
      }
    }, {
      key: "getInterestingPoints",
      value: function getInterestingPoints(strategy) {
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
      }
    }, {
      key: "legPayoff",
      value: function legPayoff(leg, pricing, year, stockPrice) {
        var optionLeg, convertible, cashLeg;
        return leg.Case === "Option" ? (optionLeg = leg.Fields[0], optionLeg.Direction * (_OptionsModel.BasicOptions.optionValue(optionLeg, stockPrice) - pricing.Premium)) : leg.Case === "Convertible" ? (convertible = leg.Fields[0], convertible.Direction * (year * convertible.Coupon * convertible.FaceValue - pricing.Premium)) : (cashLeg = leg.Fields[0], cashLeg.Direction * (stockPrice - cashLeg.Price));
      }
    }, {
      key: "getStrategyData",
      value: function getStrategyData(strategy) {
        var getLegPricing,
            payOffsPerLeg,
            interestingPoints,
            hasConverts,
            _this2 = this,
            years,
            legsData,
            strategyData;

        return getLegPricing = function (leg) {
          return function () {
            var matchValue, pricing;
            return matchValue = leg.Pricing, matchValue == null ? function (arg00) {
              return function (arg10) {
                return _this2.legPricing(arg00, arg10);
              };
            }(strategy.Stock)(leg) : (pricing = matchValue, pricing);
          }();
        }, payOffsPerLeg = _fableCore.Seq.map(function (leg) {
          var pricing, pricedLeg, payoffCalculator, Pricing;
          return pricing = getLegPricing(leg), pricedLeg = (Pricing = pricing, new _OptionsModel.Leg(leg.Definition, Pricing)), payoffCalculator = function (arg00) {
            return function (arg10) {
              return function (arg20) {
                return function (arg30) {
                  return _this2.legPayoff(arg00, arg10, arg20, arg30);
                };
              };
            };
          }(leg.Definition)(pricing), [pricedLeg, payoffCalculator];
        }, strategy.Legs), interestingPoints = this.getInterestingPoints(strategy), hasConverts = _fableCore.Seq.exists(function (leg) {
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
        }, years), new _OptionsModel.StrategyData("MultiYear", strategyData)) : (legsData = _fableCore.Seq.map(function (tupledArg) {
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
        })), new _OptionsModel.StrategyData("SingleYear", strategyData, legsData));
      }
    }]);

    return PayoffsGenerator;
  }();
});