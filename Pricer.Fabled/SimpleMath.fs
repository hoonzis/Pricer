namespace Pricer.Fabled

module SimpleMath =
    
    let signOf(x:double) = 
        if x < 0.0 then -1.0 else 1.0

    let cdf(x:double) = 
        let a1 = 0.254829592
        let a2 = -0.284496736
        let a3 = 1.421413741
        let a4 = -1.453152027
        let a5 = 1.061405429
        let p = 0.3275911
        
        let absX = abs x

        
        let t = 1.0 / (1.0 + p*absX);
        let y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*exp(-absX*absX);
 
        let result = (signOf x)*y
        result