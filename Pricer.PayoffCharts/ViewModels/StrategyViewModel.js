function StrategyViewModel(legs, stock,name,parent){
    var self = this;
    self.legs = ko.observableArray([]);
    self.stock = ko.observable();
    self.payoff = ko.observable();
    self.isBusy = ko.observable(false);
    self.message = ko.observable();
    self.name = ko.observable();
    self.parent = parent;

    self.updateExamples = function() {
        self.parent.loadExamples();
    }

    if (name != null) {
        self.name(name);
    }
    
    if (legs && stock) {
        self.legs(legs.map(function (l) {
            return new LegViewModel(l,self);
        }));

        self.stock(new StockInfoViewModel(self,stock));
    }

    self.getPayoff = function () {
        var url = "/api/pricing/";
        var dto = self.toDto();
        self.isBusy(true);
        $.ajax(url, {
            data: JSON.stringify(dto),
            type: "PUT",
            contentType: "application/json",
            error: function (err) {
                self.isBusy(false);
                self.message(err);
            },
            success: function (result) {
                self.isBusy(false);               
                var legs = self.legs();

                //update the premiums on the legs in the view
                //careful here the legs was a sequence of LegInfo. LegInfo is parent to CashLeg and OptionLeg and this is serialized az Tuple
                result.legs.forEach(function(l) {
                    var foundLeg = koExtensions.tools.find(legs, function(leg) {
                        return leg.isSame(l.definition);
                    });
                    if (foundLeg != null) {
                        foundLeg.premium(l.pricing.premium);
                        foundLeg.delta(l.pricing.delta);
                    }
                });

                var chartData = result.legPayoffs.map(function (l) {
                    return {
                        linename: l.linename,
                        width: 2,
                        values: tools.valuesToChartData(l.values)
                    }
                });

                var strategyLine = {
                    linename: "Strategy",
                    values: tools.valuesToChartData(result.strategyPayoff.values),
                    width: 4
                }

                chartData.push(strategyLine);

                self.payoff(chartData);
            }
        });
    }

    self.addLeg = function () {
        var leg = new LegViewModel({},self);
        self.legs.push(leg);
    }

    self.removeLeg = function (leg) {
        self.legs.remove(leg);
    }

    self.toDto = function () {
        var dto = {
            Stock: self.stock().toDto(),
            Legs: self.legs().map(function (l) {
                return l.toDto();
            }),
            Rate: 0.03
        }
        return dto;
    }

    self.chartOptions = {
        showDataPoints: false,
        width: 600,
        verticalCursorLine: false
    }
}

function StrategyExampleViewModel(name,parent) {
    var self = this;
    self.parent = parent;
    self.name = ko.observable(name);
    
    self.load = function () {
        self.parent.loadStrategy(self.name());
    }
}