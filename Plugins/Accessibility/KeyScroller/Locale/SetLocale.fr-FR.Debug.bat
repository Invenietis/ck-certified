echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\KeyScroller.dll ..\..\..\..\Output\Debug\KeyScroller.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\KeyScroller.resources.dll ..\..\..\..\Output\Debug\en-US\KeyScroller.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir Plugins\fr-FR
LocBaml /generate Plugins\en-US\KeyScroller.resources.dll /trans:..\..\Plugins\Accessibility\KeyScroller\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\KeyScroller.resources.dll
del LocBaml.exe
del KeyScroller.dll
pause