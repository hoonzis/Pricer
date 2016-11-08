namespace Pricer.Fabled

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import
open Fable.Import.Browser

module Tools = 
    let parseDate exp = 
        let groups = Regex.Match(exp, @"([0-9]+)-([0-9]+)\-([0-9]+)").Groups
        let year = int groups.[1].Value
        let month = int groups.[2].Value
        let day = int groups.[3].Value
        new DateTime(year, month, day)


    let toDate (date:DateTime) = 
        sprintf "%i-%02i-%02i" date.Year date.Month date.Day

module DateUtils = 

    [<Emit("new Date($0)")>]
    let fromTicks (ticks: int): DateTime = jsNative

module RangeUtils = 
    [<Emit("[$0,$1]")>]
    let range (left: float) (right: float): float array = jsNative

module NumberUtils = 
    let toFixed n (d:int) = (Fable.Import.JS.Number.Create n).toFixed(float d)