echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\KeyboardEditor.dll ..\..\..\..\Output\Release\KeyboardEditor.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\KeyboardEditor.resources.dll ..\..\..\..\Output\Release\en-US\KeyboardEditor.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate en-US\KeyboardEditor.resources.dll /trans:..\..\Plugins\Advanced\ContextEditor\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\KeyboardEditor.resources.dll
del LocBaml.exe
del KeyboardEditor.dll

pause