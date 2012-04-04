rem  ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

rem  ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\SimpleSkin.dll ..\..\..\..\Output\Release\SimpleSkin.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\SimpleSkin.resources.dll ..\..\..\..\Output\Release\en-US\SimpleSkin.resources.dll

rem  ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate en-US\SimpleSkin.resources.dll /trans:..\..\..\Plugins\Accessibility\SimpleSkin\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

rem  ------------------------ clean ------------------------
del en-US\SimpleSkin.resources.dll
del LocBaml.exe
del SimpleSkin.dll

pause