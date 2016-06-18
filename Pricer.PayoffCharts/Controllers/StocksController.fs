
namespace Pricer.PayoffCharts.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Pricer.Core
open Pricer.MarketData

[<RoutePrefix("api/stocks")>]
type StocksController() =
    inherit ApiController()
   
    [<Route("{id}")>]
    member x.Get(id:string) : IHttpActionResult =
        let stocks= MarketProviders.stockLookup id [LSE;EURONEXT]
        x.Ok(stocks) :> _

    member x.Post(stock:StockInfo) : IHttpActionResult = 
        let examples = StrategiesExamples.exampleStrategies |> Seq.map (fun s->s.Name)
        x.Ok(examples) :> _

    member x.Put(stock:StockRefData) : IHttpActionResult =
        let stockInfo = MarketProviders.stock stock.Exchange stock.Ticker None None
        x.Ok(stockInfo) :> _