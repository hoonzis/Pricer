namespace Pricer.Fabled

open System
open Pricer.Core
open Fable.Core
open Fable.Import
open Fable.Core.JsInterop
open System.Text.RegularExpressions

module PayoffCharts =
    
    let pricer = new SimplePricer()
    let payoffsGenerator = new PayoffsGenerator(pricer)
    

    type LegViewModel(l:Leg) = 
        let mutable leg =l 
        // All these things are strings, because they come from text fields later and vuejs will give us string :(
        let mutable strike = "0.0"
        let mutable expiry = "test"
        let mutable kind = "Option"
        let mutable direction = "Buy"
        let mutable delta = "0.0"   
        let mutable premium = "0.0"

        do 
            match l.Definition with
                    | Option opt -> 
                        strike <- opt.Strike.ToString()
                        expiry <- opt.Expiry |> Tools.toDate
                        direction <- opt.BuyVsSell
                        kind <- opt.Kind.ToString()
                    | Cash cash -> 
                        kind <- "Cash"
                        direction <- cash.BuyVsSell
                    | _ -> ()

            match l.Pricing with
                    | Some price -> 
                        delta <- NumberUtils.toFixed price.Delta 2
                        premium <-  NumberUtils.toFixed price.Premium 2
                    | _ -> ()
        
        member __.getLeg = 
            if kind = "Cash" then 
                {
                    Definition = Cash { 
                        Direction = direction |> Transforms.stringToDirection
                        Price = float strike
                    }
                    Pricing = None
                }
            else
                {
                    Definition = Option { 
                        Direction = direction |> Transforms.stringToDirection
                        Expiry = Tools.parseDate expiry
                        Strike = float strike
                        PurchaseDate = DateTime.Now
                        Kind = if kind = "Put" then Put else Call
                        Style = European
                    }
                    Pricing = None
                }

    type StrategyViewModel(strategy) =
        let mutable legs = strategy.Legs |> List.map (fun l -> LegViewModel(l)) |> Array.ofList
        let mutable name = strategy.Name
        let mutable stock = new StockViewModel(strategy.Stock)
        
        member __.addLeg(event) = 
            let  newLeg: Leg = {
                Definition = Option {
                    Direction = 1.0
                    Strike = 100.0
                    Expiry = DateTime.Now
                    Kind = Call
                    Style = European
                    PurchaseDate = DateTime.Now
                }
                Pricing = None
            }

            legs <- (legs |> Array.append [|new LegViewModel(newLeg)|])
        
        member __.removeLeg(leg:LegViewModel) =
            legs <- (legs |> Array.filter (fun l -> l.getLeg <> leg.getLeg))

        member __.generatePayoff() = 
            let newStrategy = {
                Name = name
                Legs = legs |> Seq.map (fun l -> l.getLeg) |> List.ofSeq
                Stock = stock.buildStock
            }
            let data = payoffsGenerator.getStrategyData newStrategy
            legs <- data.LegsSeries |> Seq.map (fun (l, payoff) -> new LegViewModel(l)) |> Array.ofSeq
            FinanceCharting.drawPayoff data "#payoffChart"



    type StrategyListViewModel(examples) = 
        // let mutable strategies = examples |> List.map (fun s -> new StrategyViewModel(s)) |> Array.ofList
        let mutable selectedStrategy: StrategyViewModel option = None

        member x.strategies = examples |> List.map (fun s -> new StrategyViewModel(s)) |> Array.ofList
        member x.select (strat: StrategyViewModel) = 
            strat.generatePayoff()
            selectedStrategy <- Some strat
            
        member x.strategy = selectedStrategy
       
    

    let extraOpts =
        createObj [
            "el" ==> ".payoffapp"           
        ]

    let exampleStock = {
        CurrentPrice = 3000.0
        Volatility = 0.23
        Rate = 0.02
    }

    let vm = StrategyListViewModel(StrategiesExamples.strategiesForStock exampleStock (DateTime.Now.AddDays 180.0))
    vm.select vm.strategies.[4] 
    vm.strategy.Value.generatePayoff()
    let app = VueHelper.createFromObj(vm, extraOpts)