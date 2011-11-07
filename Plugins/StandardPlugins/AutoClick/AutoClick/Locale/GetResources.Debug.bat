echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\..\Setup\LocBaml.exe ..\..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\..\Output\Debug\Plugins\AutoClick.dll ..\..\..\..\..\Output\Debug\AutoClick.dll
mkdir ..\..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\..\Output\Debug\Plugins\en-US\AutoClick.resources.dll ..\..\..\..\..\Output\Debug\en-US\AutoClick.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\..\Output\Debug\
LocBaml /parse en-US\AutoClick.resources.dll /out:..\..\Plugins\StandardPlugins\AutoClick\AutoClick\Locale\AutoClick.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\AutoClick.resources.dll
del LocBaml.exe
del AutoClick.dll

pause