rem ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

copy ..\..\..\..\Output\Release\Plugins\SimpleSkin.dll ..\..\..\..\Output\Release\SimpleSkin.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\SimpleSkin.resources.dll ..\..\..\..\Output\Release\en-US\SimpleSkin.resources.dll

rem  ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\SimpleSkin.resources.dll /out:..\..\Plugins\Accessibility\SimpleSkin\Locale\SimpleSkin.resources.Release.txt

rem  ------------------------ clean ------------------------
del en-US\SimpleSkin.resources.dll
del LocBaml.exe
del SimpleSkin.dll