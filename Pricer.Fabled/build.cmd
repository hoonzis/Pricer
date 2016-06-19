robocopy "node_modules" "../CompiledJs/node_modules" /e
robocopy "." "../CompiledJs" index.html
fable Pricer.Fabled.fsproj --refs Pricer.Core=..\Pricer.Core\ -o ../CompiledJs/Pricer.Fabled