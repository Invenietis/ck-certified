echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\BasicScroll.dll ..\..\..\..\Output\Debug\BasicScroll.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\BasicScroll.resources.dll ..\..\..\..\Output\Debug\en-US\BasicScroll.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\BasicScroll.resources.dll /out:..\..\Plugins\Accessibility\BasicScroll\Locale\BasicScroll.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\BasicScroll.resources.dll
del LocBaml.exe
del BasicScroll.dll

pause