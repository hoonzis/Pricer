namespace Pricer.Full

open Pricer
open MathNet.Numerics.Distributions

type MathNetProvider() = 
    let normal = new Normal()
    interface IMathProvider with 
        member this.cdf x = normal.CumulativeDistribution x
