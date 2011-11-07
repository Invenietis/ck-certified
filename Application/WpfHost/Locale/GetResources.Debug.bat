echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\Setup\LocBaml.exe ..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Create en-US folder ------------------------
cd ..\..\..\Output\Debug\
mkdir en-US

echo ------------------------ parse with LocBaml ------------------------

LocBaml /parse en-US\CiviKey.resources.dll /out:..\..\Application\WpfHost\Locale\CiviKey.resources.Debug.txt

echo ------------------------ clean ------------------------
del LocBaml.exe
pause