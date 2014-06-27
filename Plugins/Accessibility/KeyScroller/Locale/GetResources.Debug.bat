echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\Scroller.dll ..\..\..\..\Output\Debug\Scroller.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\Scroller.resources.dll ..\..\..\..\Output\Debug\en-US\Scroller.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\Scroller.resources.dll /out:..\..\Plugins\Accessibility\KeyScroller\Locale\Scroller.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\Scroller.resources.dll
del LocBaml.exe
del Scroller.dll

pause