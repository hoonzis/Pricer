namespace Pricer.Fabled

open Fable.Core
open Fable.Import.Browser
open Fable.Import

type Value = {
    X: int
    Y: float
}

type LineData = {
    Linename: string
    Values: Value list
}

type LineOptions = {
    Width: int
}

module KoExtensions =
    type Charting = 
        abstract drawLineChart: LineData list -> LineOptions -> unit