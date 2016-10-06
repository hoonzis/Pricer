#r "bin/Debug/Pricer.Core.dll"
#r "bin/Debug/Pricer.dll"
#r "bin/Debug/Pricer.Fabled.exe"
#load "../packages/FSharp.Charting/FSharp.Charting.fsx"
#r "../packages/MathNet.Numerics/lib/net40/MathNet.Numerics.dll"

open System
open Pricer.Core
open Pricer
open Pricer.Fabled
open FSharp.Charting.ChartTypes
open FSharp.Charting
open System.Windows.Forms.DataVisualization
open System.Drawing
open MathNet.Numerics.Statistics

let option =  {
    Strike = 250.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Call
    Style = European
    PurchaseDate = DateTime.Now
}

//get current price, volatility and interest free rate for VOD from London Stock Exchange
//let stock = Stocks.stockInfo LSE "VOD" (Some (DateTime.Now.AddDays -60.0)) (Some DateTime.Now)

let stock = {
  CurrentPrice = 201.0
  Volatility = 0.124
  Rate = 0.03
}

let convert = { 
    Coupon = 0.4
    ReferencePrice = 200.0
    Direction = 1.0
    ConversionRatio = 1.0
    Maturity = DateTime.Now.AddDays(40.0)
    FaceValue = 10000.0
}

let convertStrategy = {
    Stock = stock
    Legs = [
            {
                Definition = Convertible convert
                Pricing = None
            }
    ]
    Name = "Convertible Example"
}

let chartSingleYear strategy (strategyData: SingleLine, legsData: (Leg*SingleLine) list) =
    let strategyLine = Chart.Line(strategyData,Name = strategy.Name) |> Chart.WithSeries.Style(Color = Color.Red, BorderWidth = 5)
    let legsLines = legsData |> Seq.mapi (fun i (leg,legData) -> Chart.Line(legData,(sprintf "Leg %i %s" i leg.Definition.Name)))
    let allLines = legsLines |> Seq.append [strategyLine]
    let chart = Chart.Combine allLines |> Chart.WithLegend(true)
    chart
    

let simpleProvider = new SimpleMathProvider() :> IMathProvider
let realProvider = new MathNetProvider() :> IMathProvider

let getLine valueProvider name = 
    let data = [-1.0.. 0.01 .. 1.0] |> List.map (fun x-> 
        let y = valueProvider x
        x, y
    )
    Chart.Line(data,name)

let lines = seq {
    yield getLine SimpleMath.erf1 "ERF"
    yield getLine simpleProvider.cdf "ERF - to - CDF"
    yield getLine realProvider.cdf "CDF"
}

let chart= Chart.Combine lines |> Chart.WithLegend(true)