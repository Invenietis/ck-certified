echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Debug\Plugins\KeyScroller.dll ..\..\..\..\Output\Debug\KeyScroller.dll
mkdir ..\..\..\..\Output\Debug\en-US
copy ..\..\..\..\Output\Debug\Plugins\en-US\KeyScroller.resources.dll ..\..\..\..\Output\Debug\en-US\KeyScroller.resources.dll

echo ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\KeyScroller.resources.dll /out:..\..\Plugins\Accessibility\KeyScroller\Locale\KeyScroller.resources.Debug.txt

echo ------------------------ clean ------------------------
del en-US\KeyScroller.resources.dll
del LocBaml.exe
del KeyScroller.dll

pause