echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\KeyboardEditor.dll ..\..\..\..\Output\Debug\KeyboardEditor.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\KeyboardEditor.resources.dll ..\..\..\..\Output\Debug\en-US\KeyboardEditor.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\KeyboardEditor.resources.dll /out:..\..\Plugins\Advanced\ContextEditor\Locale\KeyboardEditor.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\KeyboardEditor.resources.dll
del LocBaml.exe
del KeyboardEditor.dll

pause