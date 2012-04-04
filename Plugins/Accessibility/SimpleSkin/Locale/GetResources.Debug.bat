rem ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

copy ..\..\..\..\Output\Debug\Plugins\SimpleSkin.dll ..\..\..\..\Output\Debug\SimpleSkin.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\SimpleSkin.resources.dll ..\..\..\..\Output\Debug\en-US\SimpleSkin.resources.dll

rem  ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\SimpleSkin.resources.dll /out:..\..\Plugins\Accessibility\SimpleSkin\Locale\SimpleSkin.resources.Debug.txt

rem  ------------------------ clean ------------------------
del en-US\SimpleSkin.resources.dll
del LocBaml.exe
del SimpleSkin.dll
pause