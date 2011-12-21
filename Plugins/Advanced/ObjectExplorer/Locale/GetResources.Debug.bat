echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\ObjectExplorer.dll ..\..\..\..\Output\Debug\ObjectExplorer.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\ObjectExplorer.resources.dll ..\..\..\..\Output\Debug\en-US\ObjectExplorer.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\ObjectExplorer.resources.dll /out:..\..\Plugins\StandardPlugins\ObjectExplorer\Locale\ObjectExplorer.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\ObjectExplorer.resources.dll
del LocBaml.exe
del ObjectExplorer.dll

pause