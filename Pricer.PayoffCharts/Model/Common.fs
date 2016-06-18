namespace Pricer.PayoffCharts.Model

open Pricer.Core
open Pricer.MarketData
open System

type RequestType =
    | StockAnalysis
    | OptionAnalysis


type ChartLine<'a> = {
    Linename:string
    Values: ('a*double) list
}

type OptionAnalysisData = {
    Calls : ChartLine<double> seq
    Puts : ChartLine<double> seq
    AmericanVsEuropean: ChartLine<DateTime> seq
    Options: Leg seq
}

type StockAnalysisData = {
    Averages : ChartLine<DateTime> seq
}

type PayOffAnalysis = {
    Legs: Leg seq
    LegPayoffs : ChartLine<double> seq
    StrategyPayoff: ChartLine<double>
}

type StockAnalysisQuery = {
    StockInfo : StockInfo
    Type: RequestType
    Stock: StockRefData
}

type StrategyQuery = {
    Name: String
    Stock: StockInfo
}