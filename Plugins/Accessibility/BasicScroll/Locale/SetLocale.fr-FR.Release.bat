echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\BasicScroll.dll ..\..\..\..\Output\Release\BasicScroll.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\BasicScroll.resources.dll ..\..\..\..\..\Output\Release\en-US\BasicScroll.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate Plugins\en-US\BasicScroll.resources.dll /trans:..\..\Plugins\Accessibility\BasicScroll\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\BasicScroll.resources.dll
del LocBaml.exe
del BasicScroll.dll
pause