fable ../Pricer.Core/Pricer.Core.fsproj -o CompiledJs/Pricer.Core
fable Pricer.Fabled.fsproj --refs Pricer.Core=Pricer.Core\ -o CompiledJs