define(["exports", "Pricer.Core/BlackScholesPricer", "./SimpleMath", "Pricer.Core/OptionsModel", "fable-core"], function (exports, _BlackScholesPricer, _SimpleMath, _OptionsModel, _fableCore) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.SimplePricer = undefined;

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

    var SimplePricer = exports.SimplePricer = function () {
        function SimplePricer() {
            _classCallCheck(this, SimplePricer);

            this.bsPricer = new _BlackScholesPricer.BlackScholesPricer(new _SimpleMath.SimpleMathProvider());
        }

        _createClass(SimplePricer, [{
            key: "priceOption",
            value: function priceOption(stock, option) {
                var _this = this;

                return function () {
                    return function () {
                        var objectArg;
                        return objectArg = _this.bsPricer, function (arg00) {
                            return function (arg10) {
                                return objectArg.blackScholes(arg00, arg10);
                            };
                        };
                    }();
                }()(stock)(option);
            }
        }, {
            key: "priceCash",
            value: function priceCash(cash) {
                var Premium;
                return Premium = cash.Price, new _OptionsModel.Pricing(1, Premium);
            }
        }, {
            key: "priceConvert",
            value: function priceConvert(stock, option) {
                throw "implement CB pricing";
            }
        }]);

        return SimplePricer;
    }();

    _fableCore.Util.setInterfaces(SimplePricer.prototype, ["Pricer.Core.IPricer"]);
});
//# sourceMappingURL=SimplePricer.js.map