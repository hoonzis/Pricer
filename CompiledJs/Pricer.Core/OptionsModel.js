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
        global.OptionsModel = mod.exports;
    }
})(this, function (exports, _fableCore) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.BasicOptions = exports.StrategyData = exports.LegData = exports.Strategy = exports.Leg = exports.Pricing = exports.LegInfo = exports.ConvertibleLeg = exports.CashLeg = exports.OptionLeg = exports.OptionStyle = exports.OptionKind = exports.Transforms = undefined;

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

    var Transforms = exports.Transforms = function ($exports) {
        var directionToString = $exports.directionToString = function directionToString(direction) {
            return direction < 0 ? "Sell" : "Buy";
        };

        var stringToDirection = $exports.stringToDirection = function stringToDirection(direction) {
            return direction === "Sell" ? -1 : 1;
        };

        return $exports;
    }({});

    var OptionKind = exports.OptionKind = function () {
        function OptionKind(caseName, fields) {
            _classCallCheck(this, OptionKind);

            this.Case = caseName;
            this.Fields = fields;
        }

        _createClass(OptionKind, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsUnions(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareUnions(this, other);
            }
        }, {
            key: "ToString",
            value: function ToString() {
                return this.Case === "Call" ? "Call" : "Put";
            }
        }]);

        return OptionKind;
    }();

    _fableCore.Util.setInterfaces(OptionKind.prototype, ["FSharpUnion", "System.IEquatable", "System.IComparable"], "Pricer.Core.OptionKind");

    var OptionStyle = exports.OptionStyle = function () {
        function OptionStyle(caseName, fields) {
            _classCallCheck(this, OptionStyle);

            this.Case = caseName;
            this.Fields = fields;
        }

        _createClass(OptionStyle, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsUnions(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareUnions(this, other);
            }
        }, {
            key: "ToString",
            value: function ToString() {
                return this.Case === "American" ? "American" : "European";
            }
        }]);

        return OptionStyle;
    }();

    _fableCore.Util.setInterfaces(OptionStyle.prototype, ["FSharpUnion", "System.IEquatable", "System.IComparable"], "Pricer.Core.OptionStyle");

    var OptionLeg = exports.OptionLeg = function () {
        function OptionLeg(direction, strike, expiry, kind, style, purchaseDate) {
            _classCallCheck(this, OptionLeg);

            this.Direction = direction;
            this.Strike = strike;
            this.Expiry = expiry;
            this.Kind = kind;
            this.Style = style;
            this.PurchaseDate = purchaseDate;
        }

        _createClass(OptionLeg, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsRecords(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareRecords(this, other);
            }
        }, {
            key: "BuyVsSell",
            get: function get() {
                return Transforms.directionToString(this.Direction);
            }
        }, {
            key: "TimeToExpiry",
            get: function get() {
                var _this = this;

                return function () {
                    var copyOfStruct = _fableCore.Date.op_Subtraction(_this.Expiry, _this.PurchaseDate);

                    return _fableCore.TimeSpan.days(copyOfStruct);
                }() / 365;
            }
        }, {
            key: "Name",
            get: function get() {
                return _fableCore.String.fsFormat("%s %s %.2f")(function (x) {
                    return x;
                })(this.BuyVsSell)(_fableCore.Util.toString(this.Kind))(this.Strike);
            }
        }]);

        return OptionLeg;
    }();

    _fableCore.Util.setInterfaces(OptionLeg.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.OptionLeg");

    var CashLeg = exports.CashLeg = function () {
        function CashLeg(direction, price) {
            _classCallCheck(this, CashLeg);

            this.Direction = direction;
            this.Price = price;
        }

        _createClass(CashLeg, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsRecords(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareRecords(this, other);
            }
        }, {
            key: "BuyVsSell",
            get: function get() {
                return Transforms.directionToString(this.Direction);
            }
        }]);

        return CashLeg;
    }();

    _fableCore.Util.setInterfaces(CashLeg.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.CashLeg");

    var ConvertibleLeg = exports.ConvertibleLeg = function () {
        function ConvertibleLeg(direction, coupon, conversionRatio, maturity, faceValue, referencePrice) {
            _classCallCheck(this, ConvertibleLeg);

            this.Direction = direction;
            this.Coupon = coupon;
            this.ConversionRatio = conversionRatio;
            this.Maturity = maturity;
            this.FaceValue = faceValue;
            this.ReferencePrice = referencePrice;
        }

        _createClass(ConvertibleLeg, [{
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

        return ConvertibleLeg;
    }();

    _fableCore.Util.setInterfaces(ConvertibleLeg.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.ConvertibleLeg");

    var LegInfo = exports.LegInfo = function () {
        function LegInfo(caseName, fields) {
            _classCallCheck(this, LegInfo);

            this.Case = caseName;
            this.Fields = fields;
        }

        _createClass(LegInfo, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsUnions(this, other);
            }
        }, {
            key: "CompareTo",
            value: function CompareTo(other) {
                return _fableCore.Util.compareUnions(this, other);
            }
        }, {
            key: "Name",
            get: function get() {
                return this.Case === "Option" ? this.Fields[0].Name : this.Case === "Convertible" ? _fableCore.String.fsFormat("Convert %f")(function (x) {
                    return x;
                })(this.Fields[0].FaceValue) : "Cash";
            }
        }]);

        return LegInfo;
    }();

    _fableCore.Util.setInterfaces(LegInfo.prototype, ["FSharpUnion", "System.IEquatable", "System.IComparable"], "Pricer.Core.LegInfo");

    var Pricing = exports.Pricing = function () {
        function Pricing(delta, premium) {
            _classCallCheck(this, Pricing);

            this.Delta = delta;
            this.Premium = premium;
        }

        _createClass(Pricing, [{
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

        return Pricing;
    }();

    _fableCore.Util.setInterfaces(Pricing.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.Pricing");

    var Leg = exports.Leg = function () {
        function Leg(definition, pricing) {
            _classCallCheck(this, Leg);

            this.Definition = definition;
            this.Pricing = pricing;
        }

        _createClass(Leg, [{
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

        return Leg;
    }();

    _fableCore.Util.setInterfaces(Leg.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.Leg");

    var Strategy = exports.Strategy = function () {
        function Strategy(stock, name, legs) {
            _classCallCheck(this, Strategy);

            this.Stock = stock;
            this.Name = name;
            this.Legs = legs;
        }

        _createClass(Strategy, [{
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

        return Strategy;
    }();

    _fableCore.Util.setInterfaces(Strategy.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.Strategy");

    var LegData = exports.LegData = function () {
        function LegData(leg, legData) {
            _classCallCheck(this, LegData);

            this.Leg = leg;
            this.LegData = legData;
        }

        _createClass(LegData, [{
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

        return LegData;
    }();

    _fableCore.Util.setInterfaces(LegData.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Core.LegData");

    var StrategyData = exports.StrategyData = function () {
        function StrategyData(caseName, fields) {
            _classCallCheck(this, StrategyData);

            this.Case = caseName;
            this.Fields = fields;
        }

        _createClass(StrategyData, [{
            key: "Equals",
            value: function Equals(other) {
                return _fableCore.Util.equalsUnions(this, other);
            }
        }]);

        return StrategyData;
    }();

    _fableCore.Util.setInterfaces(StrategyData.prototype, ["FSharpUnion", "System.IEquatable"], "Pricer.Core.StrategyData");

    var BasicOptions = exports.BasicOptions = function ($exports) {
        var optionValue = $exports.optionValue = function optionValue(option, stockPrice) {
            return option.Kind.Case === "Put" ? 0 > option.Strike - stockPrice ? 0 : option.Strike - stockPrice : 0 > stockPrice - option.Strike ? 0 : stockPrice - option.Strike;
        };

        var buildLeg = $exports.buildLeg = function buildLeg(kind, strike, direction, style, expiry, buyingDate) {
            var Kind = new OptionKind("Call", []);
            return new OptionLeg(direction, strike, expiry, Kind, new OptionStyle("European", []), buyingDate);
        };

        return $exports;
    }({});
});
//# sourceMappingURL=OptionsModel.js.map