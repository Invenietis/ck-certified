echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\KeyScroller.dll ..\..\..\..\Output\Release\KeyScroller.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\KeyScroller.resources.dll ..\..\..\..\..\Output\Release\en-US\KeyScroller.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate Plugins\en-US\KeyScroller.resources.dll /trans:..\..\Plugins\Accessibility\KeyScroller\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\KeyScroller.resources.dll
del LocBaml.exe
del KeyScroller.dll
pause