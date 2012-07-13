echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\AutoClick.dll ..\..\..\..\Output\Debug\AutoClick.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\AutoClick.resources.dll ..\..\..\..\Output\Debug\en-US\AutoClick.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir Plugins\fr-FR
LocBaml /generate Plugins\en-US\AutoClick.resources.dll /trans:..\..\Plugins\Accessibility\AutoClick\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\AutoClick.resources.dll
del LocBaml.exe
del AutoClick.dll
pause