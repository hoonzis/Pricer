namespace Pricer.Fabled

open System
open Pricer.Core
open Fable.Core
open Fable.Import
open Fable.Providers.Regex

[<Erase>]
module Lib =
    let [<Global>] Vue: obj = failwith "JS only"
    let [<Global>] Router: obj = failwith "JS only"


// This helper uses JS reflection to convert a class instance
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
                computed?(k) <- createObj [
                    "get" ==> prop?get
                    "set" ==> prop?set
                ]
        extraOpts?data <- data
        extraOpts?computed <- computed
        extraOpts?methods <- methods
        createNew Lib.Vue extraOpts

module Main =
    let dateToString (date:DateTime) = 
        sprintf "%i-%0i-%0i" date.Year date.Month date.Day
    
    let pricer = new SimplePricer()
    let payoffsGenerator = new PayoffsGenerator(pricer)


    type LegViewModel(l:Leg) = 
        let mutable leg = l
        let mutable strike = 0.0
        let mutable expiry = "test"
        let mutable kind = "Option"
        let mutable direction = "Buy"       
        
        let getDirection direction = if direction = 1.0 then "Buy" else "Sell"
        let getKind kind = if kind = Put then "Put" else "Call"
        do 
            match l.Definition with
                    | Option opt -> 
                        strike <- opt.Strike
                        expiry <- opt.Expiry |> dateToString
                        direction <- opt.Direction |> getDirection
                        kind <- opt.Kind |> getKind
                    | Cash cash -> 
                        kind <- "Cash"
                        direction <- cash.Direction |> getDirection
                    | _ -> ()
        
        member __.getLeg = 
            if kind = "Cash" then 
                {
                    Definition = Cash { 
                        Direction = 1.0
                        Price = strike
                    }
                    Pricing = None
                }
            else
                {
                    Definition = Option { 
                        Direction = 1.0
                        Expiry = DateTime.Now
                        Strike = strike
                        PurchaseDate = DateTime.Now
                        Kind = if kind = "Put" then Put else Call
                        Style = European
                    }
                    Pricing = None
                }    

    type StrategyViewModel(strategy) =
        let mutable legs = strategy.Legs |> List.map (fun l -> LegViewModel(l)) |> Array.ofList
        
        let buildStrategy = 
            {
                Name = strategy.Name
                Legs = legs |> Seq.map (fun l -> l.getLeg) |> List.ofSeq
                Stock = strategy.Stock
            }
        member __.name = strategy.Name
        
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

        member __.generatePayoff() = 
            let newStrategy = buildStrategy
            let data = payoffsGenerator.getStrategyData newStrategy
            ()




    type StrategyListViewModel(examples) = 
        let mutable strategies = examples |> List.map (fun s -> new StrategyViewModel(s)) |> Array.ofList
        let mutable selectedStrategy: StrategyViewModel option = None

        member self.select strat = selectedStrategy <- Some strat
        member self.selectedName = 
            match selectedStrategy with
                    | Some strat -> strat.name
                    | _ -> "No strategy selected"
                   
        
    type Directives =
        abstract ``todo-focus``: obj option -> unit
            
    let extraOpts =
        createObj [
            "el" ==> ".todoapp"           
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
    let app = VueHelper.createFromObj(vm, extraOpts)