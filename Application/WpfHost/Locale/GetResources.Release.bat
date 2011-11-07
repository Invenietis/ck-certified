echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\Setup\LocBaml.exe ..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Create en-US folder ------------------------
cd ..\..\..\Output\Release\
mkdir en-US

echo ------------------------ parse with LocBaml ------------------------

LocBaml /parse en-US\CiviKey.resources.dll /out:..\..\Application\WpfHost\Locale\CiviKey.resources.Release.txt

echo ------------------------ clean ------------------------
del LocBaml.exe
pause