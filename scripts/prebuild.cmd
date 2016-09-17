@echo off

cls
".paket\paket.bootstrapper.exe"
".paket\paket.exe" restore
cd Pricer.Fabled
call npm install
cd ..