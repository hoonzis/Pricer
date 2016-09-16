(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "./OptionsModel", "fable-core", "./StocksModel"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("./OptionsModel"), require("fable-core"), require("./StocksModel"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.OptionsModel, global.fableCore, global.StocksModel);
        global.StrategiesExamples = mod.exports;
    }
})(this, function (exports, _OptionsModel, _fableCore, _StocksModel) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.exampleStrategies = exports.exampleStock = exports.expiry = undefined;
    exports.testStrikes = testStrikes;
    exports.buildOptionLeg = buildOptionLeg;
    exports.strangle = strangle;
    exports.straddle = straddle;
    exports.butterfly = butterfly;
    exports.riskReversal = riskReversal;
    exports.collar = collar;
    exports.coveredCall = coveredCall;
    exports.cashPayOff = cashPayOff;
    exports.condor = condor;
    exports.boxOption = boxOption;
    exports.longCall = longCall;
    exports.shortCall = shortCall;
    exports.callSpread = callSpread;
    exports.putSpread = putSpread;
    exports.strategiesForStock = strategiesForStock;
    exports.getStrategy = getStrategy;

    function testStrikes(stock) {
        return [Math.floor(stock.CurrentPrice * 1.1), Math.floor(stock.CurrentPrice * 1.4)];
    }

    function buildOptionLeg(direction, strike, expiry, kind) {
        return new _OptionsModel.Leg(new _OptionsModel.LegInfo("Option", [new _OptionsModel.OptionLeg(direction, strike, expiry, kind, new _OptionsModel.OptionStyle("European", []), _fableCore.Date.now())]));
    }

    var expiry = exports.expiry = function () {
        var copyOfStruct = _fableCore.Date.now();

        return _fableCore.Date.addDays(copyOfStruct, 60);
    }();

    function strangle(stock) {
        var patternInput = testStrikes(stock);
        var Name = "Long Strangle";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(1, patternInput[0], expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, patternInput[1], expiry, new _OptionsModel.OptionKind("Put", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function straddle(stock) {
        var patternInput = testStrikes(stock);
        var Name = "Straddle";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(1, patternInput[0], expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, patternInput[0], expiry, new _OptionsModel.OptionKind("Put", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function butterfly(stock) {
        var Name = "Butterfly";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(1, stock.CurrentPrice * 1.05, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(-1, stock.CurrentPrice * 1.1, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(-1, stock.CurrentPrice * 1.1, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, stock.CurrentPrice * 1.15, expiry, new _OptionsModel.OptionKind("Call", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function riskReversal(stock) {
        var Name = "Risk Reversal";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(1, stock.CurrentPrice * 1.1, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(-1, stock.CurrentPrice * 0.9, expiry, new _OptionsModel.OptionKind("Put", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function collar(stock) {
        var Name = "Collar";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(-1, stock.CurrentPrice * 1.2, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, stock.CurrentPrice * 0.8, expiry, new _OptionsModel.OptionKind("Put", [])), new _OptionsModel.Leg(new _OptionsModel.LegInfo("Cash", [new _OptionsModel.CashLeg(1, stock.CurrentPrice)]))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function coveredCall(stock) {
        var Name = "Covered Call";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(-1, stock.CurrentPrice * 1.2, expiry, new _OptionsModel.OptionKind("Call", [])), new _OptionsModel.Leg(new _OptionsModel.LegInfo("Cash", [new _OptionsModel.CashLeg(1, stock.CurrentPrice)]))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function cashPayOff(strike, ref) {
        return ref - strike;
    }

    function condor(stock) {
        var strike1 = Math.floor(stock.CurrentPrice * 0.6);
        var strike2 = Math.floor(stock.CurrentPrice * 0.9);
        var strike3 = Math.floor(stock.CurrentPrice) * 1.1;
        var strike4 = Math.floor(stock.CurrentPrice) * 1.4;
        var Name = "Condor";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(-1, strike2, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, strike1, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(-1, strike3, expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, strike4, expiry, new _OptionsModel.OptionKind("Call", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function boxOption(stock) {
        var patternInput = testStrikes(stock);
        var Name = "Box Option";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(1, patternInput[0], expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(-1, patternInput[1], expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, patternInput[1], expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(-1, patternInput[0], expiry, new _OptionsModel.OptionKind("Call", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function longCall(stock) {
        var Name = "Long Call - Out Of Money";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(1, stock.CurrentPrice * 1.2, expiry, new _OptionsModel.OptionKind("Call", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function shortCall(stock) {
        var Name = "Short Call - Out Of Money";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(-1, stock.CurrentPrice * 1.2, expiry, new _OptionsModel.OptionKind("Call", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function callSpread(stock) {
        var patternInput = testStrikes(stock);
        var Name = "Bull Call Spread";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(-1, patternInput[1], expiry, new _OptionsModel.OptionKind("Call", [])), buildOptionLeg(1, patternInput[0], expiry, new _OptionsModel.OptionKind("Call", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function putSpread(stock) {
        var patternInput = testStrikes(stock);
        var Name = "Bull Put Spread";

        var Legs = _fableCore.List.ofArray([buildOptionLeg(-1, patternInput[1], expiry, new _OptionsModel.OptionKind("Put", [])), buildOptionLeg(1, patternInput[0], expiry, new _OptionsModel.OptionKind("Put", []))]);

        return new _OptionsModel.Strategy(stock, Name, Legs);
    }

    function strategiesForStock(stock) {
        return _fableCore.List.ofArray([longCall(stock), shortCall(stock), callSpread(stock), putSpread(stock), straddle(stock), strangle(stock), butterfly(stock), riskReversal(stock), collar(stock), condor(stock), boxOption(stock), coveredCall(stock)]);
    }

    var exampleStock = exports.exampleStock = function () {
        var CurrentPrice = 100;
        var Volatility = 0.05;
        return new _StocksModel.StockInfo(0.03, Volatility, CurrentPrice);
    }();

    var exampleStrategies = exports.exampleStrategies = strategiesForStock(exampleStock);

    function getStrategy(name, stock) {
        return _fableCore.Seq.find(function (s) {
            return s.Name === name;
        }, strategiesForStock(stock));
    }
});
//# sourceMappingURL=StrategiesExamples.js.map