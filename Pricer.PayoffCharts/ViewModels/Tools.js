function Tools() {
    var self = this;
    self.stock = {
        name: "VODAFONE",
        ticker: "VOD",
        volatility: 0.5,
        currentPrice: 235,
        rate: 0.03
    };

    self.valuesToChartData = function(values) {
        return values.map(function (dp) {
            var d = new Date(dp.item1);
            if (isNaN(d.getTime()) || koExtensions.tools.isValidNumber(dp.item1)) {
                return {
                    x: dp.item1,
                    y: dp.item2
                };
            } else {
                return {
                    x: d,
                    y: dp.item2
                };
            }
        });
    }
    self.linesToChartData = function (lines) {
        var chartData = lines.map(function (l) {
            return {
                linename: l.linename,
                values: self.valuesToChartData(l.values)
            };
        });
        return chartData;
    }
}

var tools = new Tools();