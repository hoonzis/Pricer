namespace Pricer

open Pricer.Core
open MathNet.Numerics.Distributions

type MathNetProvider() = 
    let normal = new Normal()
    interface IMathProvider with 
        member this.cdf x = normal.CumulativeDistribution x
