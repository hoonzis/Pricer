(function (global, factory) {
  if (typeof define === "function" && define.amd) {
    define(["exports", "./Options", "fable-core", "./StocksModel"], factory);
  } else if (typeof exports !== "undefined") {
    factory(exports, require("./Options"), require("fable-core"), require("./StocksModel"));
  } else {
    var mod = {
      exports: {}
    };
    factory(mod.exports, global.Options, global.fableCore, global.StocksModel);
    global.unknown = mod.exports;
  }
})(this, function (exports, _Options, _fableCore, _StocksModel) {
  "use strict";

  Object.defineProperty(exports, "__esModule", {
    value: true
  });
  exports.getStrategy = exports.exampleStrategies = exports.exampleStock = exports.strategiesForStock = exports.putSpread = exports.callSpread = exports.shortCall = exports.longCall = exports.boxOption = exports.condor = exports.cashPayOff = exports.coveredCall = exports.collar = exports.riskReversal = exports.butterfly = exports.straddle = exports.strangle = exports.expiry = exports.buildOptionLeg = exports.testStrikes = undefined;
  var copyOfStruct, CurrentPrice, Volatility;

  var testStrikes = exports.testStrikes = function (stock) {
    return [Math.floor(stock.CurrentPrice * 1.1), Math.floor(stock.CurrentPrice * 1.4)];
  };

  var buildOptionLeg = exports.buildOptionLeg = function (direction, strike, expiry, kind) {
    return new _Options.Leg(new _Options.LegInfo("Option", new _Options.OptionLeg(direction, strike, expiry, kind, new _Options.OptionStyle("European"), _fableCore.Date.now())));
  };

  var expiry = exports.expiry = (copyOfStruct = _fableCore.Date.now(), _fableCore.Date.addDays(copyOfStruct, 60));

  var strangle = exports.strangle = function (stock) {
    var patternInput, strike2, strike1, Name, Legs;
    return patternInput = testStrikes(stock), strike2 = patternInput[1], strike1 = patternInput[0], Name = "Long Strangle", Legs = _fableCore.List.ofArray([buildOptionLeg(1, strike1, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, strike2, expiry, new _Options.OptionKind("Put"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var straddle = exports.straddle = function (stock) {
    var patternInput, strike, Name, Legs;
    return patternInput = testStrikes(stock), strike = patternInput[0], Name = "Straddle", Legs = _fableCore.List.ofArray([buildOptionLeg(1, strike, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, strike, expiry, new _Options.OptionKind("Put"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var butterfly = exports.butterfly = function (stock) {
    var Name, Legs;
    return Name = "Butterfly", Legs = _fableCore.List.ofArray([buildOptionLeg(1, stock.CurrentPrice * 1.05, expiry, new _Options.OptionKind("Call")), buildOptionLeg(-1, stock.CurrentPrice * 1.1, expiry, new _Options.OptionKind("Call")), buildOptionLeg(-1, stock.CurrentPrice * 1.1, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, stock.CurrentPrice * 1.15, expiry, new _Options.OptionKind("Call"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var riskReversal = exports.riskReversal = function (stock) {
    var Name, Legs;
    return Name = "Risk Reversal", Legs = _fableCore.List.ofArray([buildOptionLeg(1, stock.CurrentPrice * 1.1, expiry, new _Options.OptionKind("Call")), buildOptionLeg(-1, stock.CurrentPrice * 0.9, expiry, new _Options.OptionKind("Put"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var collar = exports.collar = function (stock) {
    var Name, Legs;
    return Name = "Collar", Legs = _fableCore.List.ofArray([buildOptionLeg(-1, stock.CurrentPrice * 1.2, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, stock.CurrentPrice * 0.8, expiry, new _Options.OptionKind("Put")), new _Options.Leg(new _Options.LegInfo("Cash", new _Options.CashLeg(1, stock.CurrentPrice)))]), new _Options.Strategy(stock, Name, Legs);
  };

  var coveredCall = exports.coveredCall = function (stock) {
    var Name, Legs;
    return Name = "Covered Call", Legs = _fableCore.List.ofArray([buildOptionLeg(-1, stock.CurrentPrice * 1.2, expiry, new _Options.OptionKind("Call")), new _Options.Leg(new _Options.LegInfo("Cash", new _Options.CashLeg(1, stock.CurrentPrice)))]), new _Options.Strategy(stock, Name, Legs);
  };

  var cashPayOff = exports.cashPayOff = function (strike, ref) {
    return ref - strike;
  };

  var condor = exports.condor = function (stock) {
    var strike1, strike2, strike3, strike4, Name, Legs;
    return strike1 = Math.floor(stock.CurrentPrice * 0.6), strike2 = Math.floor(stock.CurrentPrice * 0.9), strike3 = Math.floor(stock.CurrentPrice) * 1.1, strike4 = Math.floor(stock.CurrentPrice) * 1.4, Name = "Condor", Legs = _fableCore.List.ofArray([buildOptionLeg(-1, strike2, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, strike1, expiry, new _Options.OptionKind("Call")), buildOptionLeg(-1, strike3, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, strike4, expiry, new _Options.OptionKind("Call"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var boxOption = exports.boxOption = function (stock) {
    var patternInput, strike2, strike1, Name, Legs;
    return patternInput = testStrikes(stock), strike2 = patternInput[1], strike1 = patternInput[0], Name = "Box Option", Legs = _fableCore.List.ofArray([buildOptionLeg(1, strike1, expiry, new _Options.OptionKind("Call")), buildOptionLeg(-1, strike2, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, strike2, expiry, new _Options.OptionKind("Call")), buildOptionLeg(-1, strike1, expiry, new _Options.OptionKind("Call"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var longCall = exports.longCall = function (stock) {
    var Name, Legs;
    return Name = "Long Call - Out Of Money", Legs = _fableCore.List.ofArray([buildOptionLeg(1, stock.CurrentPrice * 1.2, expiry, new _Options.OptionKind("Call"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var shortCall = exports.shortCall = function (stock) {
    var Name, Legs;
    return Name = "Short Call - Out Of Money", Legs = _fableCore.List.ofArray([buildOptionLeg(-1, stock.CurrentPrice * 1.2, expiry, new _Options.OptionKind("Call"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var callSpread = exports.callSpread = function (stock) {
    var patternInput, strike2, strike1, Name, Legs;
    return patternInput = testStrikes(stock), strike2 = patternInput[1], strike1 = patternInput[0], Name = "Bull Call Spread", Legs = _fableCore.List.ofArray([buildOptionLeg(-1, strike2, expiry, new _Options.OptionKind("Call")), buildOptionLeg(1, strike1, expiry, new _Options.OptionKind("Call"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var putSpread = exports.putSpread = function (stock) {
    var patternInput, strike2, strike1, Name, Legs;
    return patternInput = testStrikes(stock), strike2 = patternInput[1], strike1 = patternInput[0], Name = "Bull Put Spread", Legs = _fableCore.List.ofArray([buildOptionLeg(-1, strike2, expiry, new _Options.OptionKind("Put")), buildOptionLeg(1, strike1, expiry, new _Options.OptionKind("Put"))]), new _Options.Strategy(stock, Name, Legs);
  };

  var strategiesForStock = exports.strategiesForStock = function (stock) {
    return _fableCore.List.ofArray([longCall(stock), shortCall(stock), callSpread(stock), putSpread(stock), straddle(stock), strangle(stock), butterfly(stock), riskReversal(stock), collar(stock), condor(stock), boxOption(stock), coveredCall(stock)]);
  };

  var exampleStock = exports.exampleStock = (CurrentPrice = 100, Volatility = 0.05, new _StocksModel.StockInfo(0.03, Volatility, CurrentPrice));
  var exampleStrategies = exports.exampleStrategies = strategiesForStock(exampleStock);

  var getStrategy = exports.getStrategy = function (name, stock) {
    return _fableCore.Seq.find(function (s) {
      return s.Name === name;
    }, strategiesForStock(stock));
  };
});