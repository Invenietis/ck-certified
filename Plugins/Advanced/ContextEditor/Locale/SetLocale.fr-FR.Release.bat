echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\ContextEditor.dll ..\..\..\..\Output\Release\ContextEditor.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\ContextEditor.resources.dll ..\..\..\..\Output\Release\en-US\ContextEditor.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate en-US\ContextEditor.resources.dll /trans:..\..\Plugins\Advanced\ContextEditor\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\ContextEditor.resources.dll
del LocBaml.exe
del ContextEditor.dll

pause