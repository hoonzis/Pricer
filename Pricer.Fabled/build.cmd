call npm install
robocopy "node_modules" "../CompiledJs/node_modules" /e
robocopy "." "../CompiledJs" index.html
fable Pricer.Fabled.fsproj --refs Pricer=..\Pricer\ -o ../CompiledJs/Pricer.Fabled