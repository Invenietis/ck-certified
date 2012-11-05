echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\BasicScroll.dll ..\..\..\..\Output\Release\BasicScroll.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\BasicScroll.resources.dll ..\..\..\..\Output\Release\en-US\BasicScroll.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\BasicScroll.resources.dll /out:..\..\Plugins\Accessibility\BasicScroll\Locale\BasicScroll.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\BasicScroll.resources.dll
del LocBaml.exe
del BasicScroll.dll

pause