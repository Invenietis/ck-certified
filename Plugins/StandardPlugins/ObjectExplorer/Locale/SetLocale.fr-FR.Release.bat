echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\ObjectExplorer.dll ..\..\..\..\Output\Release\ObjectExplorer.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\ObjectExplorer.resources.dll ..\..\..\..\Output\Release\en-US\ObjectExplorer.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate en-US\ObjectExplorer.resources.dll /trans:..\..\Plugins\StandardPlugins\ObjectExplorer\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\ObjectExplorer.resources.dll
del LocBaml.exe
del ObjectExplorer.dll

pause