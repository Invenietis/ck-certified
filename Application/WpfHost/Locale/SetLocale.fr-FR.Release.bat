echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\Setup\LocBaml.exe ..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Create fr-FR ------------------------
cd ..\..\..\Output\Release\
mkdir fr-FR

LocBaml /generate en-US\CiviKey.resources.dll /trans:..\..\Application\WpfHost\Locale\fr-FR.txt /cult:fr-FR /out:fr-FR

pause