namespace Pricer.PayoffCharts.Model

open Pricer
open System

type RequestType =
    | StockAnalysis
    | OptionAnalysis


type ChartLine<'a> = {
    Linename:string
    Values: ('a*double) list
}

type Payoff = {
    Data : ChartLine<double>
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
    Stock: Stock
}

type StrategyQuery = {
    Name: String
    Stock: StockInfo
}