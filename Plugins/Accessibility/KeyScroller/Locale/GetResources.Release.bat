echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\KeyScroller.dll ..\..\..\..\Output\Release\KeyScroller.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\KeyScroller.resources.dll ..\..\..\..\Output\Release\en-US\KeyScroller.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\KeyScroller.resources.dll /out:..\..\Plugins\Accessibility\KeyScroller\Locale\KeyScroller.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\KeyScroller.resources.dll
del LocBaml.exe
del KeyScroller.dll

pause