define(["exports", "fable-core", "d3"], function (exports, _fableCore, _d) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.Charting = exports.LineData = exports.Value = undefined;

    var $import2 = _interopRequireWildcard(_d);

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

    var Value = exports.Value = function Value($arg0, $arg1) {
        _classCallCheck(this, Value);

        this.x = $arg0;
        this.y = $arg1;
    };

    var LineData = exports.LineData = function LineData($arg0, $arg1) {
        _classCallCheck(this, LineData);

        this.key = $arg0;
        this.values = $arg1;
    };

    var Charting = exports.Charting = function ($exports) {
        var tuplesToPoints = $exports.tuplesToPoints = function (data) {
            return Array.from(_fableCore.List.map(function (tupledArg) {
                var x, y;
                return x = tupledArg[0], y = tupledArg[1], new Value(Math.floor(x), y);
            }, data));
        };

        var buildLines = $exports.buildLines = function (data) {
            return _fableCore.Seq.map(function (tupledArg) {
                var leg, linedata;
                return leg = tupledArg[0], linedata = tupledArg[1], new LineData(leg.Definition.Name, tuplesToPoints(linedata));
            }, data);
        };

        var genrateChart = $exports.genrateChart = function (data) {
            var chart;
            return chart = nv.models.lineChart().useInteractiveGuideline(true).showLegend(true).showXAxis(true), chart.xAxis.axisLabel("Underlying Price").tickFormat($import2.format(",.1f")), chart.yAxis.axisLabel("Profit").tickFormat($import2.format(",.1f")), chart;
        };

        var clearAndGetParentChartDiv = $exports.clearAndGetParentChartDiv = function (selector) {
            var element;
            return element = $import2.select(selector), element.html(""), element;
        };

        var clearAndGetChartElement = $exports.clearAndGetChartElement = function () {
            clearAndGetParentChartDiv("#payoffchart");
        };

        var drawLineChart = $exports.drawLineChart = function (data) {
            var chart = genrateChart(data);
            var parentDiv = clearAndGetParentChartDiv("#payoffchart");
            var chartElement = parentDiv.append("svg");
            var chartElement_1 = $import2.select("#payoffchart");
            chartElement_1.style("height", "500px");
            chartElement_1.datum(data).call(chart);
        };

        var drawPayoff = $exports.drawPayoff = function (data) {
            var strategyData, legsData, legLines, strategyLine;
            var payoff = data.Case === "SingleYear" ? (strategyData = data.Fields[0], legsData = data.Fields[1], legLines = buildLines(legsData), strategyLine = new LineData("Strategy", tuplesToPoints(strategyData)), Array.from(_fableCore.Seq.delay(function (unitVar) {
                return _fableCore.Seq.append(legLines, _fableCore.Seq.delay(function (unitVar_1) {
                    return _fableCore.Seq.singleton(strategyLine);
                }));
            }))) : function () {
                throw "not implemented";
            }();
            drawLineChart(payoff);
        };

        return $exports;
    }({});
});
//# sourceMappingURL=Charting.js.map