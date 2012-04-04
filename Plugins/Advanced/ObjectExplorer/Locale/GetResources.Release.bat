echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\ObjectExplorer.dll ..\..\..\..\Output\Release\ObjectExplorer.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\ObjectExplorer.resources.dll ..\..\..\..\Output\Release\en-US\ObjectExplorer.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\ObjectExplorer.resources.dll /out:..\..\Plugins\Advanced\ObjectExplorer\Locale\ObjectExplorer.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\ObjectExplorer.resources.dll
del LocBaml.exe
del ObjectExplorer.dll

pause