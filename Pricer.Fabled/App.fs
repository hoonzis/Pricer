module PricerApp

open System
open Pricer
open Fable.Core
open Fable.Import
open Pricer.StrategiesExamples

let option = {
    Strike = 250.0
    Expiry = DateTime.Now.AddDays(90.)
    Direction = 1.0
    Kind = Call
    Style = European
    PurchaseDate = DateTime.Now
}

let examples = StrategiesExamples.exampleStrategies

// Use this dummy module to hold references to Vue and Router objects
// exposed globally by loading the corresponding libraries with HTML script tags
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
    type LegViewModel(l:Leg) = 
        let mutable leg = l
        member __.strike
            with get() = 
                 match leg.Definition with  
                        | Option option -> option.Strike.ToString()
                        | _ -> ""
            and set(s:string) = 
                let strike = float s
                let newLegDefinition = match leg.Definition with  
                                                  | Option option -> { option with Strike = strike}
                                                  | _ -> failwith "we should not be able to modify strike while not working on option"
                let newLeg = {
                    Definition = Option newLegDefinition
                    Pricing = leg.Pricing
                }
                leg <- newLeg
                
    type StrategyViewModel(strat) =
        let mutable strategy: Strategy = strat

        let mutable newLeg: Leg = {
            Definition = Option {
                Direction = 1.0
                Strike = 100.0
                Expiry = new DateTime()
                Kind = Call
                Style = European
                PurchaseDate = DateTime.Now
            }
            Pricing = None
        }
        member __.legs = strategy.Legs |> List.map (fun l -> LegViewModel(l)) |> Array.ofList
        member __.name = strategy.Name
        
        member __.Strategy
            with get() = strategy
            and set(v) = strategy <- v

        member __.addLeg(event) = 
            // let newLegs = strategy.Strategy.Legs @ [newLeg]
            let newLegs = [
                yield! strategy.Legs
                yield newLeg
            ]
            
            let newStrategy = {strategy with Legs = newLegs } 
            strategy <- newStrategy

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

    let vm = StrategyListViewModel(examples)
    let app = VueHelper.createFromObj(vm, extraOpts)