echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\ContextEditor.dll ..\..\..\..\Output\Debug\ContextEditor.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\ContextEditor.resources.dll ..\..\..\..\Output\Debug\en-US\ContextEditor.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\ContextEditor.resources.dll /out:..\..\Plugins\Advanced\ContextEditor\Locale\ContextEditor.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\ContextEditor.resources.dll
del LocBaml.exe
del ContextEditor.dll

pause