echo ------------------------ LocBaml to output ------------------------
copy ..\..\Setup\LocBaml.exe ..\..\Output\Release\LocBaml.exe

echo ------------------------ Create en-US folder ------------------------
cd ..\..\Output\Release\
mkdir en-US

echo ------------------------ parse with LocBaml ------------------------

LocBaml /parse en-US\Host.Services.resources.dll /out:..\..\Host.Services\Locale\Host.Services.resources.Release.txt

echo ------------------------ clean ------------------------
del LocBaml.exe
pause