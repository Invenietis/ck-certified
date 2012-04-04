echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\AutoClick.dll ..\..\..\..\Output\Release\AutoClick.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\AutoClick.resources.dll ..\..\..\..\Output\Release\en-US\AutoClick.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\AutoClick.resources.dll /out:..\..\Plugins\Accessibility\AutoClick\Locale\AutoClick.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\AutoClick.resources.dll
del LocBaml.exe
del AutoClick.dll

pause