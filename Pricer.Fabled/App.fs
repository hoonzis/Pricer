module PricerApp

open System
open Pricer
open Fable.Core
open Fable.Import

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

type LegViewModel(l:Leg) = 
    let leg = l
    member __.strike =
         match leg.Definition with  
                | Option option -> option.Strike 
                | _ -> 0.0

type StrategyViewModel(strat:Strategy) =
    let strategy = strat

    member __.legs = strat.Legs |> List.map (fun l -> new LegViewModel(l))
    member __.name = strat.Name
    

type StategyListViewModel(strategyList: Strategy list) = 
    let strategies: StrategyViewModel list = strategyList |> List.map (fun s -> new StrategyViewModel(s))

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

    // We'll use a typed object to represent our viewmodel instead of
    // a JS dynamic object to take advantage of static type checking
    
        
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
        
    // Now instantiate the type and create a Vue view model
    // using the helper method
    let app = VueHelper.createFromObj(StategyListViewModel(examples), extraOpts)