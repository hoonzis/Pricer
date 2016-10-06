namespace Pricer.Fabled

open System
open Pricer.Core

module SimpleMath = 
    let signOf(x:double) = 
        if x < 0.0 then -1.0 else 1.0

    let erf1(z: double) =
        let t = 1.0 / (1.0 + 0.5 * abs z)
        let param = -z*z - 1.26551223 + t * ( 1.00002368 + t * ( 0.37409196 + t * ( 0.09678418 + t * (-0.18628806 +  t * ( 0.27886807 +  t * (-1.13520398 +  t * ( 1.48851587 +  t * (-0.82215223 +  t * ( 0.17087277)))))))))
        // use Horner's method
        let ans = 1.0 - t * exp param 
        if z >= 0.0 then ans else -ans;
    
    // http://www.johndcook.com/erf_and_normal_cdf.pdf
    //  http://www.johndcook.com/blog/csharp_phi/
    let erf2(x:double) = 
        let a1 = 0.254829592
        let a2 = -0.284496736
        let a3 = 1.421413741
        let a4 = -1.453152027
        let a5 = 1.061405429
        let p = 0.3275911
        
        let sign = signOf x
        let absX = abs x / sqrt (2.0)

        let t = 1.0 / (1.0 + p*absX);
        let y = 1.0 - ((((a5*t + a4)*t + a3)*t + a2)*t + a1)*t*exp(-absX*absX);
 
        let result = 0.5 * (1.0 + sign*y)
        result

    let cdf z = 
        0.5 * (1.0 + erf1(z / sqrt 2.0))

    
type SimpleMathProvider() =

    interface IMathProvider with
        member this.cdf x =  SimpleMath.cdf x
    