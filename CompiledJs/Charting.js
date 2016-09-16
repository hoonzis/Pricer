define(["exports", "fable-core", "d3"], function (exports, _fableCore, _d) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.Charting = exports.LineData = exports.Value = undefined;

    var d3 = _interopRequireWildcard(_d);

    function _interopRequireWildcard(obj) {
        if (obj && obj.__esModule) {
            return obj;
        } else {
            var newObj = {};

            if (obj != null) {
                for (var key in obj) {
                    if (Object.prototype.hasOwnProperty.call(obj, key)) newObj[key] = obj[key];
                }
            }

            newObj.default = obj;
            return newObj;
        }
    }

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

    var Value = exports.Value = function () {
        function Value(x, y) {
            _classCallCheck(this, Value);

            this.x = x;
            this.y = y;
        }

        _createClass(Value, [{
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

        return Value;
    }();

    _fableCore.Util.setInterfaces(Value.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Fabled.Value");

    var LineData = exports.LineData = function () {
        function LineData(key, values) {
            _classCallCheck(this, LineData);

            this.key = key;
            this.values = values;
        }

        _createClass(LineData, [{
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

        return LineData;
    }();

    _fableCore.Util.setInterfaces(LineData.prototype, ["FSharpRecord", "System.IEquatable", "System.IComparable"], "Pricer.Fabled.LineData");

    var Charting = exports.Charting = function ($exports) {
        var tuplesToPoints = $exports.tuplesToPoints = function tuplesToPoints(data) {
            return Array.from(_fableCore.List.map(function (tupledArg) {
                return new Value((tupledArg[0] + 0x80000000 >>> 0) - 0x80000000, tupledArg[1]);
            }, data));
        };

        var buildLines = $exports.buildLines = function buildLines(data) {
            return _fableCore.Seq.map(function (tupledArg) {
                return new LineData(tupledArg[0].Definition.Name, tuplesToPoints(tupledArg[1]));
            }, data);
        };

        var genrateChart = $exports.genrateChart = function genrateChart(data) {
            var chart = nv.models.lineChart().useInteractiveGuideline(true).showLegend(true).showXAxis(true);
            chart.xAxis.axisLabel("Underlying Price").tickFormat(d3.format(",.1f"));
            chart.yAxis.axisLabel("Profit").tickFormat(d3.format(",.1f"));
            return chart;
        };

        var clearAndGetParentChartDiv = $exports.clearAndGetParentChartDiv = function clearAndGetParentChartDiv(selector) {
            var element = d3.select(selector);
            element.html("");
            return element;
        };

        var clearAndGetChartElement = $exports.clearAndGetChartElement = function clearAndGetChartElement() {
            clearAndGetParentChartDiv("#payoffchart");
        };

        var drawLineChart = $exports.drawLineChart = function drawLineChart(data) {
            var chart = genrateChart(data);
            var parentDiv = clearAndGetParentChartDiv("#payoffchart");
            var chartElement = parentDiv.append("svg");
            var chartElement_1 = d3.select("#payoffchart");
            chartElement_1.style("height", "500px");
            chartElement_1.datum(data).call(chart);
        };

        var drawPayoff = $exports.drawPayoff = function drawPayoff(data) {
            var payoff = data.Case === "SingleYear" ? function () {
                var legLines = buildLines(data.Fields[1]);
                var strategyLine = new LineData("Strategy", tuplesToPoints(data.Fields[0]));
                return Array.from(_fableCore.Seq.delay(function (unitVar) {
                    return _fableCore.Seq.append(legLines, _fableCore.Seq.delay(function (unitVar_1) {
                        return _fableCore.Seq.singleton(strategyLine);
                    }));
                }));
            }() : function () {
                throw "not implemented";
            }();
            drawLineChart(payoff);
        };

        return $exports;
    }({});
});
//# sourceMappingURL=Charting.js.map