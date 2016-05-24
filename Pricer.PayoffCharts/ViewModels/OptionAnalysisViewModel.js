function OptionAnalysisViewModel() {
    var self = this;
    self.calls = ko.observableArray([]);
    self.puts = ko.observableArray([]);
    self.americanVsEuropean = ko.observableArray([]);
    self.stock = new StockInfoViewModel(self, null);
    self.stock.stockInfo.subscribe(function(stockInfo) {

    });
    self.busy = ko.observable(false);
    self.message = ko.observable(null);
    self.options = ko.observable(null);

    self.updateAnalysis = function () {
        var url = "/api/analyzer/";
        var dto = {
            stockInfo: self.stock.toDto(),
            type: "OptionAnalysis"
        };
    
        self.busy(true);
        $.ajax(url, {
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(dto),
            error: function (err) {
                self.busy(false);
                self.message(err);
            },
            success: function (result) {
                self.busy(false);
                self.calls(tools.linesToChartData(result.calls));
                self.puts(tools.linesToChartData(result.puts));
                self.americanVsEuropean(tools.linesToChartData(result.americanVsEuropean));
                self.options(result.options.map(function(d) {
                    return {
                        premium: d.pricing.premium,
                        expiry: new Date(d.definition.expiry),
                        strike: d.definition.strike,
                        type: d.definition.kind
                    };
                }));
            }
        });
    }
    
    self.chartOptions = {
        showDataPoints: false,
        width: 600,
        marginConf: 1.005
    }

    self.optionsChartOptions = {
        bubbleSize: function (d) { return d.premium; },
        bubbleVertical: function (d) { return d.strike; },
        bubbleHorizontal: function (d) { return d.expiry; },
        bubbleColor: function (d) { return d.type; },
        verticalLabel: 'Strike',
        horizontalLabel: 'Expiry',
        sizeLabel: 'price',
        width: 600,
        height:400,
        legend: false,
        maxBubbleSize: 30,
        yMin: 130,
        yAxisLabel: "Option Strike",
        xAxisLabel: "Option Expiry"
    }

    self.stock.currentPrice(230);
    self.stock.volatility(0.35);
    self.stock.rate(0.03);
    self.updateAnalysis();
}