namespace Pricer.Tests

open Pricer
open System
open System.Reflection
open System.IO
open FsUnit
open NUnit.Framework
open Pricer.Core

[<TestFixture>]
type MonteCarloTests() = 
    
    let callPrice = 2.7172467445106512
    let callDelta = 0.54373433554702966
    let europeanPutPrice = 2.0542675521718747
    let europeanDeltaPrice = -0.44565561894046612


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


    [<Test>]
    [<Ignore>]
    member this.``monte carlo pricing european call`` () =
        let price = MonteCarloPricer.run stock europeanCall 5000
        price.Premium |> should equal callPrice
        price.Delta |> should equal callDelta

    // American and European call have the same price
    [<Test>]
    [<Ignore>]
    member this.``monte carlo american call`` () = 
        let price = MonteCarloPricer.run stock americanCall 5000
        price.Premium |> should equal callPrice
        price.Delta |> should equal callDelta
        
    
    [<Test>]
    [<Ignore>]
    member this.``monte carlo euroean put`` () =        
        let price = MonteCarloPricer.run stock europeanPut 5000
        price.Premium |> should equal europeanPutPrice
        price.Delta |> should equal europeanDeltaPrice

    // American put has higher premium then european put and as well higher delta
    [<Test>]
    [<Ignore>]
    member this.``monte carlo american put`` () =        
        let price = MonteCarloPricer.run stock americanPut 5000
        price.Premium |> should equal 2.3156625779008477
        price.Delta |> should equal -0.52646992694089489