function LegViewModel(dto,parent) {
    var self = this;

    self.parent = parent;
    self.expiry = ko.observable(new Date()).extend({ required: true });
    self.strike = ko.observable().extend({ required: true });
    self.kind = ko.observable().extend({ required: true });
    self.style = ko.observable("European");
    self.premium = ko.observable();
    self.delta = ko.observable();
    self.isSame = function(leg) {
        var kind = self.kind();
        var strike = self.strike();        
        var exp1 = new Date(leg.expiry);
        var exp2 = self.expiry();
        return kind === leg.kind && strike === leg.strike && exp1.getYear() === exp2.getYear() && exp1.getDate() === exp2.getDate() && exp1.getMonth() === exp2.getMonth();
    }

    self.directions = ko.observableArray(['Buy', 'Sell']);
    self.direction = ko.observable().extend({ required: true });


    if (dto != null) {
        self.expiry(dto.definition.expiry);
        self.strike(dto.definition.strike);
        self.kind(dto.definition.kind);
        if (dto.definition.kind == null) {
            self.kind("Cash");
        }
        if (dto.pricing != null) {
            self.premium(dto.pricing.premium);
        }
        
        if (dto.definition.direction > 0) {
            self.direction("Buy");
        } else {
            self.direction("Sell");
        };
        if (dto.definition.style != null) {
            self.style(dto.definition.style);
        }
    }

    self.legTypes = ko.observableArray(["Call", "Put", "Cash"]);

    self.toDto = function () {
        //strike is common to cash legs and option legs
        var dto = {
            pricing: null,
            definition : {
                Strike: self.strike()
            }
        };

        if (self.isOption()) {
            dto.definition.Expiry = self.expiry();
            dto.definition.Style = self.style();
            dto.definition.PurchaseDate = new Date();
            dto.definition.Kind = self.kind();
        }
        
        if (self.direction() === "Buy") {
            dto.definition.Direction = 1;
        } else {
            dto.definition.Direction = -1;
        }
        return dto;
    }

    self.remove = function () {
        parent.removeLeg(self);
    }

    self.isOption = ko.pureComputed(function () {
        var legType = self.kind();
        return legType === "Call" || legType === "Put";
    });
}