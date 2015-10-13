module OptionsTests

open OptionsPricing
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
        let price = Options.binomialPrice 120.0 130.0 0.03 1.15
        price |> should equal 2.792773459184565

    [<Test>]
    member this.``test binomial recursive - simple implementation`` () =
        let price = Options.binomialRecursiveCall 120.0 121.0 0.03 1.15 3
        price.Premium |> should equal 5.5955087326342348

        
    [<Test>]
    member this.``binomial pricing European call in BS setting`` () =        
        let price = Options.binomial stock europeanCall 1000
        price.Premium |> should equal 2.7287226062956242

    [<Test>]
    member this.``binomial pricing American call in BS setting`` () =        
        let price = Options.binomial stock americanCall 1000
        price.Premium |> should equal 2.7287226062956242
        

    [<Test>]
    member this.``black sholes euroean call`` () =        
        let price = Options.blackScholes stock europeanCall
        price.Premium |> should equal 2.8237329844670001

    [<Test>]
    member this.``black sholes euroean put`` () =        
        let price = Options.blackScholes stock europeanPut
        price.Premium |> should equal 1.9705489779045706

    [<Test>]
    member this.``binomial euroean put in BS setting`` () =        
        let price = Options.binomial stock europeanPut 1000
        price.Premium |> should equal 2.0622848528435895

    //american put has higher value then european put
    [<Test>]
    member this.``binomial american put in BS setting`` () =        
        let price = Options.binomial stock americanPut 1000
        price.Premium |> should equal 2.3197098602325328


    [<Test>]
    member this.``end node price generation tests`` () =   
        let periods = 100000
        let up = 1.25
        let uu = 1.25**2.0
        let lowest = (100.0*(0.8**(float periods)))
        let highest = lowest * ((1.25*1.25)**(float periods-1.0))
        
        let prices = Options.generateEndNodePrices 100.0 1.25 periods
        prices.[0] |> should equal lowest
        prices.[1] |> should equal (lowest*1.25*1.25)

        let prices2 = Options.generateEndNodePrices2 100.0 1.25 periods
        prices2.[0] |> should equal lowest
        prices2.[1] |> should equal (lowest*uu)
        prices2.[2] |> should equal (lowest*(up**4.0))

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
        let (strategy,legs) = Options.getStrategyData strat
        legs |> Seq.length |> should equal 1
        let firstLeg, legData = legs |> Seq.head
        let x,y = legData.Head
        //this is the minimal value
        y |> should equal -2.8237329844670001

    [<Test>]
    member this.``butterlfy payoff tests`` () =        
        let strat = StrategiesExamples.butterfly stock
        let strategy,legs = Options.getStrategyData strat
        legs |> Seq.length |> should equal 4

    [<Test>]
    member this.``cash leg payoff test`` () =        
        let strat = {
            Name = "Test strat"
            Legs = 
                [
                    {
                        Definition = Cash {
                            Strike = 1.0
                            Direction = 1.0
                        }
                        Pricing = None
                    }
                ]
            Stock = stock
        }
        let strategy,legs = Options.getStrategyData strat
        legs |> Seq.length |> should equal 1
        let leg,legData = legs |> Seq.head
        match leg.Definition with
                | Option ol -> ol.Kind |> should equal Call
                | Cash cl -> cl.Strike |> should equal 1.0
        
        //we have 3 points - min,max and the only strike
        strategy |> should haveLength 3