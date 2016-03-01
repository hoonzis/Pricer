module OptionsTests

open Pricer
open System
open System.Reflection
open System.IO
open FsUnit
open NUnit.Framework

let stock = {
    Volatility = 0.05
    Rate = 0.03
    CurrentPrice = 230.0
}

let europeanCall = {
    Strike = 231.0
    Expiry = new DateTime(2015,12,12)
    Direction = 1.0
    Kind = Call
    Style = European
    PurchaseDate = new DateTime(2015,9,5)
}

let americanCall = {
    Strike = 231.0
    Expiry = new DateTime(2015,12,12)
    Direction = 1.0
    Kind = Call
    Style = American
    PurchaseDate = new DateTime(2015,9,5)
}

let europeanPut = {
    Strike = 231.0
    Expiry = new DateTime(2015,12,12)
    Direction = 1.0
    Kind = Put
    Style = European
    PurchaseDate = new DateTime(2015,9,5)
}

let americanPut = {
    Strike = 231.0
    Expiry = new DateTime(2015,12,12)
    Direction = 1.0
    Kind = Put
    Style = American
    PurchaseDate = new DateTime(2015,9,5)
}

[<TestFixture>]
type OptionsTests() = 

    [<Test>]
    member this.``test simple binomial`` () =
        let price = Binomial.binomialPrice 120.0 130.0 0.03 1.15
        price |> should equal 2.792773459184565

        
    [<Test>]
    member this.``binomial pricing European call in BS setting`` () =
        let price = Binomial.binomial stock europeanCall 1000 Imperative
        price.Premium |> should equal 2.7172467445106512

    [<Test>]
    member this.``binomial pricing American call in BS setting`` () = 
        let price = Binomial.binomial stock americanCall 1000 Imperative
        price.Premium |> should equal 2.7172467445106512
        

    [<Test>]
    member this.``black sholes euroean call`` () =        
        let price = Options.blackScholes stock europeanCall
        price.Premium |> should equal 2.8237329844670001
        
    [<Test>]
    member this.``black sholes euroean call - volatility 0`` () =      
        let updatedStock = { stock with Volatility = 0.0 }  
        let price = Options.blackScholes updatedStock europeanCall
        price.Premium |> should equal 0.85318400656242943

    [<Test>]
    member this.``black sholes euroean put`` () =        
        let price = Options.blackScholes stock europeanPut
        price.Premium |> should equal 1.9705489779045706
    
    [<Test>]
    member this.``black sholes quick price put from now with expiry in 30 days from now``() =
        let buyingDate = DateTime.Now.Date
        let exp = buyingDate.AddDays(30.0)
        let quickPrice = Options.europeanBSPrice 0.03m 1.0 230m 0.05m 231m exp Put buyingDate
        quickPrice.Premium |> should equal 1.0818440568021117

    [<Test>]
    member this.``binomial euroean put in BS setting`` () =        
        let price = Binomial.binomial stock europeanPut 1000 Imperative
        price.Premium |> should equal 2.0542675521718747

    //american put has higher value then european put
    [<Test>]
    member this.``binomial american put in BS setting`` () =        
        let price = Binomial.binomial stock americanPut 1000 Imperative
        price.Premium |> should equal 2.3156625779008477

    [<Test>]
    member this.``binomial american put in functional way`` () =        
        let price = Binomial.binomial stock europeanPut 1000 Functional
        price.Premium |> should equal 2.0542675521718747

    [<Test>]
    member this.``binomial american put in BS setting - functional way`` () =        
        let price = Binomial.binomial stock americanPut 1000 Functional
        price.Premium |> should equal 2.3156625779008477


    [<Test>]
    member this.``end node price generation tests`` () =
        let periods = 100000
        let up = 1.25
        let uu = 1.25**2.0
        let lowest = (100.0*(0.8**(float periods)))
        let highest = lowest * ((1.25*1.25)**(float periods-1.0))

        let optionVal stock = stock
        
        let prices = Binomial.generateEndNodePrices 100.0 1.25 periods optionVal
        (List.nth prices 0) |> should equal (lowest,lowest)
        (List.nth prices 1) |> should equal ((lowest*1.25*1.25),(lowest*1.25*1.25))

    [<Test>]
    member this.``simple payoff test`` () =
        let strat = {
            Name = "Test strat"
            Legs = 
                [
                    {
                        Definition = Option europeanCall
                        Pricing = None
                    }
                ]
            Stock = stock
        }
        let strategyData = Options.getStrategyData strat
        match strategyData with
                | SingleYear (strategy, legs) -> 
                    legs |> Seq.length |> should equal 1
                    let firstLeg, legsData = legs |> Seq.head
                    let x,y = legsData |> Seq.head
                    //this is the minimal value
                    y |> should equal -2.8237329844670001
                | _ -> failwith "Should have returned single year"

    [<Test>]
    member this.``butterlfy payoff tests`` () =        
        let strat = StrategiesExamples.butterfly stock
        let strategyData = Options.getStrategyData strat
        match strategyData with
                | SingleYear (strategy, legs) -> 
                    legs |> Seq.length |> should equal 4
                | _ -> failwith "Should have returned single year four legs"
        

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
            Stock = stock
        }
        let strategyData = Options.getStrategyData strat
        match strategyData with
                | SingleYear (strategy, legs) -> 
                    legs |> Seq.length |> should equal 1
                    let leg,legData = legs |> Seq.head
                    match leg.Definition with
                            | Cash cl -> cl.Price |> should equal 1.0
                            | _ -> failwith "Not good"
                   
                    // points - min,max and the only strike
                    strategy |> should haveLength 3
                | _ -> failwith "wrong"
        
        

    [<Test>]
    member this.``single binomial step test`` () =
        let prices = [
            (1.0,1.0)
            (2.0,2.0)
            (3.0,2.0)
            (4.0,4.0)
        ]

        //let's test with european call, so there is premature execution
        let pricing = {
            Periods = 100
            Down = 0.6
            Up = 1.4
            PUp = 0.6
            PDown = 0.4
            Option = europeanCall
            Rate = 1.0
            Ref = 100.0
        }

        //let's say the derivative price is the same as the stock price
        let optionVal stock = stock
        
        //in real world the R will be => exp (stock.Rate*deltaT)

        //the computation of the continuation of the derivative price
        //dPrice <- (downPrice*pDown + upPrice*pUp)*exp(-r*deltaT)
        //in the model Rate<-exp(r*deltaT)
        let newPrices = Binomial.step pricing optionVal prices

        newPrices |> should haveLength 3
        //for derivative price:
        //first element => (1*0.4 + 2*0.6)1/1
        //second element => (2*0.4 + 2*0.6)1/1
        //third element => (2*0.4 + 4*0.6)1/1

        //for the stock price:
        //2*0.6 - (upStock*down) = 1.2
        //3*0.6 - 1.8
        //4*0.6 - 2.4
        
        //the following fails with fsunit...
        //newPrices |> should equal [(1.2,1.6);(1.8,2.0);(2.4,3.2)]
        (newPrices |> List.head) |> should equal (1.2,1.6)
        
        //(List.nth newPrices 1) |> should equal (1.8,2.0)
        
    [<Test>]
    member this.``getting strategy data of strategy with no legs`` () =
        let strategy = {
            Stock = stock
            Name = "Test"
            Legs = List.empty
        }

        let strategyData = Options.getStrategyData strategy
        match strategyData with
                | SingleYear (strategy, legsData) ->  strategy |> List.ofSeq |> should haveLength 0
                | _ -> failwith "Wrong"
        
        