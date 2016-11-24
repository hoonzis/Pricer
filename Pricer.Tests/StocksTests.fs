module StocksTests

open Pricer.Core
open Pricer.MarketData
open System
open System.Reflection
open System.IO
open FsUnit
open NUnit.Framework
open Pricer

let stock = {
    Volatility = 0.05
    Rate = 0.03
    CurrentPrice = 230.0
}


let getEmbeddedResourceFile (filename:string) = 
    //just to get the right assembly, use any type from th assembly
    let assembly = typeof<StockRefData>.Assembly
    let allResources = assembly.GetManifestResourceNames();
    let assemblyName = assembly.GetName()
    let file  = allResources |> Array.find (fun x->x = filename)
    let stream = assembly.GetManifestResourceStream(file)
    stream

[<TestFixture>]
type StocksTests() = 

    let getFirstDay data = data |> Seq.map Stocks.tradingDay |> Seq.min
    //first trading day of 2015 was 2
    let startDate = new DateTime(2015,1,2)
    let endDate = new DateTime(2015,3,30)

    [<Test>]
    [<Ignore>]
    member this.``test single lse stock from quandl`` () =
        let ticker,data = MarketProviders.stock LSE "VOD" (Some startDate) (Some endDate)
        let firstDay = data |> getFirstDay
        firstDay.Date |> should equal startDate.Date

    [<Test>]
    member this.``test estimation of vol of lse stock - open values are not available, test only Close Vs Close`` () =
        let ticker,data = MarketProviders.lseData (getEmbeddedResourceFile "lsesinglestock.json")
        let vol = Stocks.estimateVol CloseVsClose data
        vol |> should (equalWithin 0.002) 0.084

    [<Test>]
    member this.``test estimation of vol of euronext stock - open values are available`` () =
        let ticker,data = MarketProviders.euroNextData (getEmbeddedResourceFile "euronextsinglestock.json")
        let vol = Stocks.estimateVol CloseVsClose data
        let vol2 = Stocks.estimateVol CloseVsOpen data

        abs (vol - vol2) |> should be (lessThan 0.08)

    [<Test>]
    [<Ignore>]
    //this invokes directly quandl web service
    member this.``test euronext stock info - `` () =
        let stockInfo = StockAnalysis.stockInfo EURONEXT "ATI" (Some startDate) (Some endDate)
        stockInfo.CurrentPrice |> should equal 6.76
        stockInfo.Rate |> should equal 0.03
        stockInfo.Volatility |> should (equalWithin 0.002) 0.179
        

        