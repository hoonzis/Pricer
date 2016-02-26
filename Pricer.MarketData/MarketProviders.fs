namespace Pricer.MarketData

open System
open System.IO
open System.Collections.Generic
open FSharp.Data
open System.Net
open Pricer

type LseStocks = JsonProvider<"lsedataset.json",EmbeddedResource="OptionsPricing,lsedataset.json">
type SingleLseStock = JsonProvider<"lsesinglestock.json",EmbeddedResource="OptionsPricing,lsesinglestock.json">
type SingleEuroNextStock = JsonProvider<"euronextsinglestock.json",EmbeddedResource="OptionsPricing,euronextsinglestock.json">

module MarketProviders =
    let mutable authKey = ""
    let  mutable allStocks = new List<Exchange*string*string>()

    let getWebStream (url:string) =
          let req = WebRequest.Create(url)
          let resp = req.GetResponse()
          resp.GetResponseStream()


    let fetchUrlAsync exchange url =
        async {
            let! data = LseStocks.AsyncLoad url
            let stocks = data.Datasets |> Seq.map (fun stock -> exchange, stock.DatasetCode, stock.Name)
            return stocks
        }

    let getExchangeStocks exchange =
        let dataSites = [for i in 1 .. 10 do yield (sprintf "https://www.quandl.com/api/v3/datasets.json?database_code=%s&per_page=100&sort_by=id&page=%i%s" (Tools.caseString exchange) i authKey)]
        let results =
            dataSites
                |> Seq.map (fetchUrlAsync exchange)
                |> Async.Parallel
                |> Async.RunSynchronously
        results |> Seq.reduce (fun s1 s2 -> s1 |> Seq.append s2) |> List.ofSeq

    let loadAllStocks (exchangeList:Exchange seq) =
        let allbyExchange = exchangeList |> Seq.map getExchangeStocks
        let test = (allbyExchange |> Seq.collect id)
        allStocks <- new List<Exchange*string*string>(test)


    let stockLookup (nameOrTicker:string) exchangeList =
        let lowered = nameOrTicker.ToLower();
        allStocks|> Seq.filter (fun (exchange,ticker,name) ->
                ticker.ToLower().Contains(lowered) || name.ToLower().Contains(lowered)
        )

    let euroNextData (stream:Stream) =
        let data = SingleEuroNextStock.Load stream
        data.Dataset.Name,data.Dataset.Data |> Seq.map (fun d ->
            let tick = {
                Date = d.DateTime
                Close = float d.Numbers.[3]
                Open = Some (float d.Numbers.[0])
            }
            tick
        )

    let lseData (stream:Stream) =
        let data = SingleLseStock.Load stream
        data.Dataset.Name,data.Dataset.Data |> Seq.map (fun d ->
            let availableNumbers = d.Numbers |> Array.length
            {
                Date = d.DateTime
                Close = float d.Numbers.[0]
                Open = match  availableNumbers with
                                    | 6 -> Some (float d.Numbers.[3])
                                    | _ -> None
            }
        )

    let stock (exchange:Exchange) (ticker:string) (startDate:DateTime option) (endDate:DateTime option) =
        let transf = match exchange with
                        | LSE -> lseData
                        | EURONEXT -> euroNextData

        let format (date:DateTime) = date.ToString("yyyy-MM-dd")

        let startVal =
            match startDate with
                | Some date -> format date
                | None -> format (DateTime.UtcNow.AddYears -1)

        let endVal =
            match endDate with
                | Some date -> format date
                | None -> format DateTime.UtcNow

        let url = sprintf "https://www.quandl.com/api/v3/datasets/%s/%s.json?start_date=%s&end_date=%s%s" (Tools.caseString exchange) ticker startVal endVal authKey
        transf (getWebStream url)
