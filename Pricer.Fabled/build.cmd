call fable ../Pricer.Core/Pricer.Core.fsproj -o ../Pricer.Fabled/CompiledJs/Pricer.Core
call fable Pricer.Fabled.fsproj --refs Pricer.Core=Pricer.Core\ -o CompiledJs