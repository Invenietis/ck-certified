echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\BasicScroll.dll ..\..\..\..\Output\Debug\BasicScroll.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\BasicScroll.resources.dll ..\..\..\..\Output\Debug\en-US\BasicScroll.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir Plugins\fr-FR
LocBaml /generate Plugins\en-US\BasicScroll.resources.dll /trans:..\..\Plugins\Accessibility\BasicScroll\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\BasicScroll.resources.dll
del LocBaml.exe
del BasicScroll.dll
pause