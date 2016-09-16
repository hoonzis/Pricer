# Pricer
[![Nuget Package](https://img.shields.io/nuget/v/pricer.svg)](https://www.nuget.org/packages/Pricer)
[![Build status](https://ci.appveyor.com/api/projects/status/rqvploew3rhe8b3e?svg=true)](https://ci.appveyor.com/project/hoonzis/pricer)

Small library that can be used to price options (Black Scholes and Binomial pricing), generate payoff charts and maybe analyze stock prices. It was based by Tomas Petricek's [Financial Computing in F# series](http://www.tryfsharp.org/Learn/financial-computing). I have added binomial pricing, different algorithm to estimate the volatility and few other features.

The **Pricer.Core** project does not depend on anything else than BCL, so it can be transpiled into JavaScript using [Fable](https://fable-compiler.github.io/). A sample application **Pricer.Fabled** is part of the solution and compiled is available here: http://www.payoffcharts.com.

#### Options pricing
The library contains a model for describing options and stocks. Besides the options description one needs the stock's volatility, current price and interest free rate.  

```fsharp
let stock = {
  CurrentPrice = 201.0
  Volatility = 0.124
  Rate = 0.03
}

let option = {
    Strike = 250.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Call
    Style = American
    PurchaseDate = DateTime.Now
}

let price = Options.blackScholes stock option
let binomialPrice = Options.binomial stock option 2000
```

Binomial pricing takes also the depth of the binomial tree as parameter, the result is *Pricing* record which contains the *Premium* and *Delta*.

Alternatively you can obtain the historical volatility and current price from small referential data module which uses free [Quandl API](https://www.quandl.com/). The bellow example uses trading data for last 60 days. The library uses free data from Quandl to estimate the volatility for stocks. The rate for now is fixed.
```fsharp
let stock = Stocks.stockInfo LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
```

#### Payoff charts data generation
You can generate payoff charts data for any strategy. Strategy is composed of legs which can be already priced. The library also contains method to generate example strategies that just accept the *Stock* as parameter. Here is how to visualize the result of priced strategy.

```fsharp
let strategy = StrategiesExamples.callSpread stock
let strategyData,legsData = Options.getStrategyData strategy
let strategyLine = Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
let legsLines = legsData |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,leg.Definition.Name))
let allLines = legsLines |> Seq.append [strategyLine]
let chart = Chart.Combine allLines |> Chart.WithLegend(true)
```

#### Stock price and volatility
You can also use the library only to get the stock data and perform some basics analysis, like for instance floating averages:

```fsharp
let ticker,data = MarketData.stock LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)
let tenDaysAvg = Stocks.floatingAvg 10 data
let fiveDaysAvg = Stocks.floatingAvg 5 data
Chart.Combine [
    Chart.Line((data|>Seq.sortBy Stocks.tradingDay |> Seq.map (fun tick -> tick.Date, tick.Close)),Name = "Day")
    Chart.Line(tenDaysAvg,Name="10 days avg")
    Chart.Line(fiveDaysAvg,Name="5 days avg")
] |> Chart.WithLegend true
```
There are several ways of estimating the historical volatility of the stock. The standard and simplest method is Close To Close estimation when log returns based on closing prices are compared. There is also a second mode for pricing volatility which uses opening prices, both described in [this blog](http://blog.quantitations.com/stochastic%20processes/2012/12/30/estimating-stock-volatility/)

```
let ticker,data = MarketData.stock EURONEXT "ATI" (Some (DateTime.Now.AddDays -180.0)) (Some DateTime.Now)
let vol = Stocks.estimateVol CloseVsClose data
let vol2 = Stocks.estimateVol CloseVsOpen data
```

#### Building & Contributing
Pricer uses FAKE as it's build system. Fake is included in the repository and a proxy *fake.cmd* script runs any of fake tasks and passes the parameters.

```
fake Test
```
**Pricer.Fabled** is transpiled into JavaScript, but depends on **Pricer.Core** which has to be transpiled first. A **build.cmd** command is available in the **Pricer.Fabled** folder to chain both fable commands.
