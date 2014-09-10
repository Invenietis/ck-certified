echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\KeyboardEditor.dll ..\..\..\..\Output\Debug\KeyboardEditor.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\KeyboardEditor.resources.dll ..\..\..\..\Output\Debug\en-US\KeyboardEditor.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir Plugins\fr-FR
LocBaml /generate en-US\KeyboardEditor.resources.dll /trans:..\..\Plugins\Advanced\ContextEditor\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\KeyboardEditor.resources.dll
del LocBaml.exe
del KeyboardEditor.dll

pause