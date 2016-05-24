namespace Pricer.PayoffCharts.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open Pricer
open Pricer.PayoffCharts.Model

[<RoutePrefix("api/pricing")>]
type PricingController() =
    inherit ApiController()

    //given complete strategy  (stock and legs, returns the payoff chart data)
    member x.Put([<FromBody>] strategy:Strategy) : IHttpActionResult = 
        let data = Options.getStrategyData strategy

        let buildLines (data:(Leg*(float*float) list) seq)= 
            data |> Seq.map (fun (leg,linedata) -> 
                {
                    Linename = leg.Definition.Name
                    Values = linedata
                })

        let payoff = 
            match data with
                | SingleYear (strategyData, legsData) ->
                    {
                        Legs = legsData |> Seq.map (fun (leg,_) -> leg)
                        LegPayoffs = buildLines legsData
                        StrategyPayoff = 
                        {    
                            Linename = "Strategy"
                            Values = strategyData
                        }
                    }
                | MultiYear (strategyPerYear) ->
                    let firstYearStrategy = strategyPerYear |> Seq.head
                    {
                        Legs = []
                        LegPayoffs = []
                        StrategyPayoff = 
                        {    
                            Linename = "Strategy"
                            Values = firstYearStrategy
                        }
                    }
        x.Ok(payoff) :> _

    //given stock and strategy name returns the example strategy
    member x.Post([<FromBody>] query:StrategyQuery) : IHttpActionResult = 
        let strategy = StrategiesExamples.getStrategy query.Name query.Stock
        x.Ok(strategy) :> _