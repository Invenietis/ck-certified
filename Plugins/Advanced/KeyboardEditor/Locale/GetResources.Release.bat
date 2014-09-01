echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\KeyboardEditor.dll ..\..\..\..\Output\Release\KeyboardEditor.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\KeyboardEditor.resources.dll ..\..\..\..\Output\Release\en-US\KeyboardEditor.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\KeyboardEditor.resources.dll /out:..\..\Plugins\Advanced\ContextEditor\Locale\KeyboardEditor.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\KeyboardEditor.resources.dll
del LocBaml.exe
del KeyboardEditor.dll

pause