namespace Pricer.Fabled

open System
open Pricer.Core
open Fable.Core
open Fable.Import
open Fable.Core.JsInterop
open System.Text.RegularExpressions

[<Erase>]
module Lib =
    let [<Global>] Vue: obj = failwith "JS only"
    let [<Global>] Router: obj = failwith "JS only"


// This helper uses JS reflection to convert a class instancegfa
// to the options' format required by Vue
module VueHelper =
    let createFromObj(data: obj, extraOpts: obj) =
        let methods = obj()
        let computed = obj()
        let proto = JS.Object.getPrototypeOf data
        for k in JS.Object.getOwnPropertyNames proto do
            let prop = JS.Object.getOwnPropertyDescriptor(proto, k)
            match prop.value with
            | Some f ->
                methods?(k) <- f
            | None ->
                computed?(k) <- JsInterop.createObj [
                    "get" ==> prop?get
                    "set" ==> prop?set
                ]
        extraOpts?data <- data
        extraOpts?computed <- computed
        extraOpts?methods <- methods
        JsInterop.createNew Lib.Vue extraOpts

module Main =
    
    
    let pricer = new SimplePricer()
    let payoffsGenerator = new PayoffsGenerator(pricer)

    type StockViewModel(s:StockInfo) = 
        let mutable rate = s.Rate
        let mutable volatility = s.Volatility
        let mutable currentPrice = s.CurrentPrice

        member __.buildStock = {
            Rate = rate
            Volatility = volatility
            CurrentPrice = currentPrice
        }

    type LegViewModel(l:Leg) = 
        let mutable leg = l
        // All these things are strings, because they come from text fields later and vuejs will give us string :(
        let mutable strike = "0.0"
        let mutable expiry = "test"
        let mutable kind = "Option"
        let mutable direction = "Buy"       

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
            Charting.drawPayoff data



    type StrategyListViewModel(examples) = 
        let mutable strategies = examples |> List.map (fun s -> new StrategyViewModel(s)) |> Array.ofList
        let mutable selectedStrategy: StrategyViewModel option = None

        member __.allStrategies = strategies
        member __.select strat = 
            selectedStrategy <- Some strat
            selectedStrategy.Value.generatePayoff()

        member __.strategy = selectedStrategy
        
    type Directives =
        abstract ``todo-focus``: obj option -> unit
            
    let extraOpts =
        createObj [
            "el" ==> ".payoffapp"           
            "directives" ==> {
                new Directives with
                    member this.``todo-focus`` x =
                        match x with
                        | None -> ()
                        | Some _ ->
                            let el = this?el
                            Lib.Vue?nextTick$(fun () ->
                                el?focus$() |> ignore)
                            |> ignore
            } 
        ]

    let vm = StrategyListViewModel(StrategiesExamples.exampleStrategies)
    vm.select vm.allStrategies.[4]
    vm.strategy.Value.generatePayoff()
    let app = VueHelper.createFromObj(vm, extraOpts)