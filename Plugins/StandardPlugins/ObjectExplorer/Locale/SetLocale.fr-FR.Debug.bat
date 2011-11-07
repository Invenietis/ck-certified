echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\ObjectExplorer.dll ..\..\..\..\Output\Debug\ObjectExplorer.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\ObjectExplorer.resources.dll ..\..\..\..\Output\Debug\en-US\ObjectExplorer.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir Plugins\fr-FR
LocBaml /generate en-US\ObjectExplorer.resources.dll /trans:..\..\Plugins\StandardPlugins\ObjectExplorer\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\ObjectExplorer.resources.dll
del LocBaml.exe
del ObjectExplorer.dll

pause