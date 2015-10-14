# Pricer
Pricing of options and other financial products. This small library can be used to price options and generate payoff charts.

The library contains a model for describing options and stocks parameters which are necessary. Besides the options description one needs the stock's volatility and current price and interest free rate. The library uses free data from Quandl to get it for stocks.
```
let option = {
    Strike = 250.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Call
    Style = American
    PurchaseDate = DateTime.Now
}

//get current price, historical volatility and interest free rate for VOD from London Stock Exchange, taking last 60 days

let stock = Stocks.stockInfo LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
```

Calculating prices:
```
let bsPrice = Options.blackScholes stock option
let binomialPrice = Options.binomial stock option 2000
```

Binomial pricing takes the number of steps used in the simulation, the result is *Pricing* record which contains the *Premium* and *Delta*.
