rem ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

rem ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
LocBaml /parse en-US\CK.WPF.Controls.resources.dll /out:..\..\Library\WPF\CK.WPF.Controls\Locale\Controls.resources.Debug.txt

rem ------------------------ clean ------------------------
del LocBaml.exe

pause