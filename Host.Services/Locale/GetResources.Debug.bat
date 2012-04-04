echo ------------------------ LocBaml to output ------------------------
copy ..\..\Setup\LocBaml.exe ..\..\Output\Debug\LocBaml.exe

echo ------------------------ Create en-US folder ------------------------
cd ..\..\Output\Debug\
mkdir en-US

echo ------------------------ parse with LocBaml ------------------------

LocBaml /parse en-US\Host.Services.resources.dll /out:..\..\Host.Services\Locale\Host.Services.resources.Debug.txt

echo ------------------------ clean ------------------------
del LocBaml.exe
pause