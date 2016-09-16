namespace Pricer.Fabled

open System
open System.Text.RegularExpressions

module Tools = 
    let parseDate exp = 
        let groups = Regex.Match(exp, @"([0-9]+)-([0-9]+)\-([0-9]+)").Groups
        let year = int groups.[1].Value
        let month = int groups.[2].Value
        let day = int groups.[3].Value
        new DateTime(year, month, day)


    let toDate (date:DateTime) = 
        sprintf "%i-%02i-%02i" date.Year date.Month date.Day