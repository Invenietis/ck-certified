echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\ContextEditor.dll ..\..\..\..\Output\Release\ContextEditor.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\ContextEditor.resources.dll ..\..\..\..\Output\Release\en-US\ContextEditor.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\ContextEditor.resources.dll /out:..\..\Plugins\Advanced\ContextEditor\Locale\ContextEditor.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\ContextEditor.resources.dll
del LocBaml.exe
del ContextEditor.dll

pause