function StockAnalysisViewModel() {
    var self = this;
    self.averages = ko.observableArray([]);
    self.busy = ko.observable(false);
    self.message = ko.observable();
    self.stock = new StockViewModel();
    
    self.getAnalysis = function (ticker) {
        var url = "/api/analyzer/";
        var dto = {
            stock: ticker,
            stockInfo: null,
            type: "StockAnalysis"
        };
        self.busy(true);
        self.averages([]);
        self.message("Loading the data...");
        $.ajax(url, {
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(dto),
            error: function (err) {
                self.busy(false);
                self.message("Error while getting stock data");
            },
            success: function (result) {
                self.busy(false);
                self.message(null);
                self.averages(result.averages.map(function (l) {
                    return {
                        linename: l.linename,
                        values: tools.valuesToChartData(l.values)
                    }
                }));
            }
        });
    }
    
    self.stock.fullTicker.subscribe(self.getAnalysis);

    self.chartOptions = {
        showDataPoints: false,
        width: 600,
        marginConf: 1.005,
        verticalCursorLine: false,
        horizontalSlider: true,
        xAxisTextAngle:45
    }

    self.stock.fullTicker({
        exchange: "LSE",
        name: "Land Securities Group PLC",
        ticker:"LAND"
    });
}