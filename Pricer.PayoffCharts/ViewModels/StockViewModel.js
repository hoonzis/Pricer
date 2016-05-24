function StockViewModel() {
    var self = this;
    self.fullTicker = ko.observable();
    
    self.name = ko.pureComputed(function() {
        var v = self.fullTicker();
        if (v)
            return v.name;
        return null;
    });
    self.exchange = ko.pureComputed(function () {
        var v = self.fullTicker();
        if (v)
            return v.exchange;
        return null;
    });
    self.ticker = ko.pureComputed(function () {
        var v = self.fullTicker();
        if (v)
            return v.ticker;
        return null;
    });
   
    
    self.getTickers = function (options) {
        var url = "/api/stocks/" + options.text;

        var resultPromise = $.ajax(url, {
            type: "GET",
            contentType: "application/json",
        });

        var otherPromise = resultPromise.then(function (result) {
            var tickers = result.map(function (tickerName) {
                return {
                    exchange: tickerName.item1,
                    ticker: tickerName.item2,
                    name: tickerName.item3
                };
            });
            return {
                data: tickers,
                total: 10
            };
        });

        return otherPromise;
    }

    self.comboViewModel = new ComboViewModel(null, self, self.fullTicker);
}