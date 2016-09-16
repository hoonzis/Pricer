(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "fable-core", "./OptionsModel", "./Binomial"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("fable-core"), require("./OptionsModel"), require("./Binomial"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.fableCore, global.OptionsModel, global.Binomial);
        global.OptionsAnalyzer = mod.exports;
    }
})(this, function (exports, _fableCore, _OptionsModel, _Binomial) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.OptionsAnalyzer = exports.Serie = undefined;

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

    var Serie = exports.Serie = function () {
        function Serie(series, values) {
            _classCallCheck(this, Serie);

            this.Series = series;
            this.Values = values;
        }

        _createClass(Serie, [{
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

        return Serie;
    }();

    _fableCore.Util.setInterfaces(Serie.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.Serie");

    var OptionsAnalyzer = exports.OptionsAnalyzer = function () {
        function OptionsAnalyzer(pricer) {
            _classCallCheck(this, OptionsAnalyzer);

            this.pricer = pricer;
        }

        _createClass(OptionsAnalyzer, [{
            key: "combine",
            value: function combine(alist, blist) {
                return _fableCore.Seq.collect(function (x) {
                    return x;
                }, _fableCore.Seq.map(function (el) {
                    return _fableCore.Seq.map(function (el2) {
                        return [el, el2];
                    }, blist);
                }, alist));
            }
        }, {
            key: "buildOption",
            value: function buildOption(strike, style, expiry, kind) {
                var Direction = 1;
                return new _OptionsModel.OptionLeg(Direction, strike, expiry, kind, style, _fableCore.Date.now());
            }
        }, {
            key: "optionPrices",
            value: function optionPrices(stock, kind, style) {
                var _this = this;

                var expiries = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                    return _fableCore.Seq.map(function (day) {
                        var copyOfStruct = _fableCore.Date.now();

                        return _fableCore.Date.addDays(copyOfStruct, day * 3);
                    }, _fableCore.Seq.range(0, 8));
                }));

                var strikes = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                    return _fableCore.Seq.map(function (ref) {
                        return ref;
                    }, _fableCore.Seq.range(0.9 * stock.CurrentPrice, stock.CurrentPrice * 1.1));
                }));

                return _fableCore.List.map(function (expiry) {
                    var data = _fableCore.List.map(function (strike) {
                        var option = _this.buildOption(strike, style, expiry, kind);

                        return [strike, function () {
                            var objectArg = _this.pricer;
                            return function (arg00) {
                                return function (arg10) {
                                    return objectArg.priceOption(arg00, arg10);
                                };
                            };
                        }()(stock)(option).Premium];
                    }, strikes);

                    return new Serie(_fableCore.String.format('{0:' + "d" + '}', expiry), data);
                }, expiries);
            }
        }, {
            key: "americanVsEuropeanPut",
            value: function americanVsEuropeanPut(stock) {
                var _this2 = this;

                var expiries = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                    return _fableCore.Seq.map(function (i) {
                        var copyOfStruct = _fableCore.Date.now();

                        return _fableCore.Date.addDays(copyOfStruct, i * 80);
                    }, _fableCore.Seq.range(1, 10));
                }));

                var styles = _fableCore.List.ofArray([new _OptionsModel.OptionStyle("American", []), new _OptionsModel.OptionStyle("European", [])]);

                return _fableCore.Seq.map(function (style) {
                    var data = _fableCore.List.map(function (expiry) {
                        var option = function () {
                            var strike = stock.CurrentPrice * 1.5;
                            var kind = new _OptionsModel.OptionKind("Put", []);
                            return _this2.buildOption(strike, style, expiry, kind);
                        }();

                        return [expiry, _Binomial.Binomial.binomial(stock, option, 1000, new _Binomial.Implementation("Imperative", [])).Premium];
                    }, expiries);

                    return new Serie(_fableCore.Util.toString(style), data);
                }, styles);
            }
        }, {
            key: "optionPricesTripes",
            value: function optionPricesTripes(stock) {
                var _this3 = this;

                var expiries = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                    return _fableCore.Seq.map(function (i) {
                        var copyOfStruct = _fableCore.Date.now();

                        return _fableCore.Date.addDays(copyOfStruct, i * 80);
                    }, _fableCore.Seq.range(1, 10));
                }));

                var strikes = _fableCore.Seq.toList(_fableCore.Seq.delay(function (unitVar) {
                    return _fableCore.Seq.map(function (i) {
                        return stock.CurrentPrice * 0.6 + stock.CurrentPrice / 10 * i;
                    }, _fableCore.Seq.range(1, 10));
                }));

                var combinations = function () {
                    var clo0 = function clo0(alist) {
                        return function (blist) {
                            return _this3.combine(alist, blist);
                        };
                    };

                    return function (arg00) {
                        var clo1 = clo0(arg00);
                        return function (arg10) {
                            return clo1(arg10);
                        };
                    };
                }()(strikes)(expiries);

                return _fableCore.Seq.map(function (tupledArg) {
                    var option = function () {
                        var style = new _OptionsModel.OptionStyle("European", []);
                        var kind = new _OptionsModel.OptionKind("Call", []);
                        return _this3.buildOption(tupledArg[0], style, tupledArg[1], kind);
                    }();

                    var pricing = function () {
                        var objectArg = _this3.pricer;
                        return function (arg00) {
                            return function (arg10) {
                                return objectArg.priceOption(arg00, arg10);
                            };
                        };
                    }()(stock)(option);

                    return new _OptionsModel.Leg(new _OptionsModel.LegInfo("Option", [option]), pricing);
                }, combinations);
            }
        }]);

        return OptionsAnalyzer;
    }();

    _fableCore.Util.setInterfaces(OptionsAnalyzer.prototype, [], "Pricer.Core.OptionsAnalyzer");
});
//# sourceMappingURL=OptionsAnalyzer.js.map