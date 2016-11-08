namespace Pricer.Tests

open Pricer
open System
open System.Reflection
open System.IO
open FsUnit
open NUnit.Framework
open Pricer.Core

[<TestFixture>]
type PayoffTests() = 
    
    let mathProvider = new MathNetProvider()
    let bsPricer = new BlackScholesPricer(mathProvider)
    let fullPricer = new FullPricer()
    let payoffGenerator = new PayoffsGenerator(fullPricer)
    
    [<Test>]
    member this.``simple payoff test`` () =
        let strat = {
            Name = "Test strat"
            Legs = 
                [
                    {
                        Definition = Option TestData.europeanCall
                        Pricing = None
                    }
                ]
            Stock = TestData.stock
        }
        let data = payoffGenerator.getStrategyData strat
        data.LegsSeries |> Seq.length |> should equal 1
        let firstLeg, legsData = data.LegsSeries |> Seq.head
        let x,y = legsData |> Seq.head
        //this is the minimal value
        y |> should equal -2.8237329844670001

    [<Test>]
    member this.``butterlfy payoff tests`` () =        
        let strat = StrategiesExamples.butterfly TestData.stock (DateTime.Now.AddDays 50.0)
        let data = payoffGenerator.getStrategyData strat
        data.LegsSeries |> Seq.length |> should equal 4

    [<Test>]
    member this.``cash leg payoff test`` () =
        let strat = {
            Name = "Test strat"
            Legs = 
                [
                    {
                        Definition = Cash {
                            Price = 1.0
                            Direction = 1.0
                        }
                        Pricing = None
                    }
                ]
            Stock = TestData.stock
        }
        let data = payoffGenerator.getStrategyData strat
        data.LegsSeries |> Seq.length |> should equal 1
        let leg,legData = data.LegsSeries |> Seq.head
        match leg.Definition with
                | Cash cl -> cl.Price |> should equal 1.0
                | _ -> failwith "Not good"
                   
        // points - min,max and the only strike
        data.StrategySerie |> should haveLength 3        

    [<Test>]
    member this.``getting strategy data of strategy with no legs`` () =
        let strategy = {
            Stock = TestData.stock
            Name = "Test"
            Legs = List.empty
        }

        let data = payoffGenerator.getStrategyData strategy
        data.StrategySerie |> List.ofSeq |> should haveLength 0
        
        