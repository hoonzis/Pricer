namespace Pricer.PayoffCharts.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Pricer
open Pricer.PayoffCharts.Model
open Pricer.MarketData

/// Retrieves values.
[<RoutePrefix("api/analyzer")>]
type AnalyzerController() =
    inherit ApiController()
    
    let combine alist blist =
        alist |> Seq.map (fun el -> blist |> Seq.map(fun el2 -> el, el2)) |> Seq.collect id

    let buildOption strike style expiry kind = {
        Direction = 1.0
        Strike = strike
        Kind = kind
        Expiry = expiry
        Style = style
        PurchaseDate = DateTime.Now
    }
    //returns several lines, one per expiry
    let optionPrices stock kind style = 
        let expiries = [for day in 0 .. 8 -> DateTime.Now.AddDays(float day*3.0)]
        let strikes = [for ref in 0.9*stock.CurrentPrice ..stock.CurrentPrice*1.1 -> ref]
   
        expiries |> List.map (fun expiry -> 
            let data = strikes |> List.map (fun strike ->
                let option = buildOption strike style expiry kind
                strike,(Options.blackScholes stock option).Premium
            )
            {
                Linename = expiry.ToString("d")
                Values = data
            }
        )

    //returns 2 lines, one for each style, strike is fixed by Current
    let americanVsEuropeanPut stock = 
        let expiries = [for i in 1 .. 10 -> DateTime.Now.AddDays ((float i)*80.0)]
        let styles = [American;European]

        styles |> Seq.map (fun (style) ->
            let data = expiries |> List.map (fun expiry ->
                let option = buildOption (stock.CurrentPrice*1.5) style expiry Put
                expiry,(Binomial.binomial stock option 1000 Implementation.Imperative).Premium
            )
            {
                Linename = Tools.caseString style
                Values = data
            }
        )
    //returns multiple triples strike - expiry - pricing result
    let optionPricesTripes stock = 
        let expiries = [for i in 1 .. 10 -> DateTime.Now.AddDays ((float i)*80.0)]
        let strikes = [for i in 1 .. 10 -> stock.CurrentPrice * 0.6 +  stock.CurrentPrice/10.0 * float i]

        let combinations = combine strikes expiries

        combinations |> Seq.map (fun (strike, exp) ->
            let option = buildOption strike European exp Call
            let pricing = Options.blackScholes stock option
            {
                Definition = Option option
                Pricing = Some pricing
            }
        )

    [<Route("")>]
    member x.Post([<FromBody>] query:StockAnalysisQuery) : IHttpActionResult = 
        match query.Type with
            | OptionAnalysis ->
                let stock = query.StockInfo
                let analysisData  = {
                    Calls = optionPrices stock Call European
                    Puts = optionPrices stock Put European
                    AmericanVsEuropean = americanVsEuropeanPut stock
                    Options = optionPricesTripes stock
                }
                x.Ok(analysisData) :> _
            | StockAnalysis -> 
                let averages = StockAnalysis.stockFloatingAvg query.Stock.Exchange query.Stock.Ticker
                let data = {
                    Averages = averages |> Seq.map (fun (name,data) -> 
                                {
                                    Linename = name
                                    Values = data |> List.ofSeq
                                }
                            )
                }
                x.Ok(data) :> _