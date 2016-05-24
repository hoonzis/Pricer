function StockInfoViewModel(parent,dto) {
    var self = this;
    self.stock = new StockViewModel();
    self.volatility = ko.observable().extend({ required: true});
    self.currentPrice = ko.observable().extend({ required: true });
    self.rate = ko.observable().extend({ required: true });
    self.busy = ko.observable(false);
    self.message = ko.observable();
    self.parent = parent;

    if (dto != null) {
        self.volatility(dto.volatility);
        self.currentPrice(dto.currentPrice);
        self.rate(dto.rate);
    } else {
        self.volatility(0.245);
        self.currentPrice(201.35);
        self.rate(0.025);
    }

    self.toDto = function () { 
        return  {
            Volatility: self.volatility(),
            CurrentPrice: self.currentPrice(),
            Rate: self.rate()
        }
    }

    self.stockInfo = ko.pureComputed(function () {
        return self.toDto();
    });

    self.getStock = function (ticker) {
        var url = "/api/stocks/";
        self.busy(true);
        $.ajax(url, {
            type: "PUT",
            contentType: "application/json",
            data: JSON.stringify(ticker),
            error: function (err) {
                self.busy(false);
                self.message("No data found for given stock");
                self.volatility(null);
                self.currentPrice(null);
            },
            success: function (result) {
                self.message(null);
                self.busy(false);
                self.volatility(result.volatility);
                self.currentPrice(result.currentPrice);
                if (self.parent.getAnalysis != null)
                    self.parent.getAnalysis();
                if(result.rate!=null)
                    self.rate(result.rate);
            }
        });
    }
    self.stock.ticker.subscribe(self.getStock);
}