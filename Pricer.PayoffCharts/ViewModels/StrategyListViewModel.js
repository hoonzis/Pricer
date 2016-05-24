function StrategyListViewModel() {
    var self = this;
    self.strategies = ko.observableArray([]);
    self.strategy = ko.observable();
    self.busy = ko.observable(false);
    self.message = ko.observable();

    //this is defined in koextensions
    Date.prototype.addDays = function (days) {
        var dat = new Date(this.valueOf());
        dat.setDate(dat.getDate() + days);
        return dat;
    }

    self.newStrategy = function() {
        var today = new Date();
        var legs = [
            {
                definition:{
                    expiry: today.addDays(20),
                    strike: 250,
                    kind: "Call",
                    style: "European"
                }
            }
        ];

        var strategy = new StrategyViewModel(legs, tools.stock, "new", self);
        self.strategy(strategy);
        self.loadStrategy("Collar");
        self.loadExamples();
    }

    self.loadExamples = function() {
        var url = "/api/stocks/";
        var stock = self.strategy().stock().toDto();
        self.busy(true);
        $.ajax(url, {
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(stock),
            error: function(err) {
                self.busy(false);
                self.message(err);
            },
            success: function(result) {
                self.busy(false);
                self.strategies(result.map(function(s) {
                    return new StrategyExampleViewModel(s,self);
                }));
            }
        });
    };

    self.loadStrategy = function(name) {
        var url = "/api/pricing/";
        var stock = self.strategy().stock().toDto();
        var query = {
            stock: stock,
            name: name
        };
        self.busy(true);
        $.ajax(url, {
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(query),
            error: function (err) {
                self.busy(false);
                self.message(err);
            },
            success: function (result) {
                self.busy(false);
                var strategy = new StrategyViewModel(result.legs, result.stock, result.name, self);
                strategy.getPayoff();
                self.strategy(strategy);
            }
        });
    }
}