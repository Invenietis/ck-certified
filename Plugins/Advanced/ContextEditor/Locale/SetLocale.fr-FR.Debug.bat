echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\ContextEditor.dll ..\..\..\..\Output\Debug\ContextEditor.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\ContextEditor.resources.dll ..\..\..\..\Output\Debug\en-US\ContextEditor.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir Plugins\fr-FR
LocBaml /generate en-US\ContextEditor.resources.dll /trans:..\..\Plugins\Advanced\ContextEditor\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\ContextEditor.resources.dll
del LocBaml.exe
del ContextEditor.dll

pause