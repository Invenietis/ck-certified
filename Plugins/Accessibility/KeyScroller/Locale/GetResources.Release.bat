echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\Scroller.dll ..\..\..\..\Output\Release\Scroller.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\Scroller.resources.dll ..\..\..\..\Output\Release\en-US\Scroller.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\Scroller.resources.dll /out:..\..\Plugins\Accessibility\KeyScroller\Locale\Scroller.resources.Release.txt

echo ------------------------ clean ------------------------
del en-US\Scroller.resources.dll
del LocBaml.exe
del Scroller.dll

pause