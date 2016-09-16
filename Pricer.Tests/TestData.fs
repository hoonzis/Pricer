namespace Pricer.Tests

open System
open Pricer.Core

module TestData =
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