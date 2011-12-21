rem ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

rem ------------------------ parse with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
LocBaml /parse en-US\CK.WPF.Controls.resources.dll /out:..\..\Library\WPF\CK.WPF.Controls\Locale\Controls.resources.Release.txt

rem ------------------------ clean ------------------------
del LocBaml.exe

pause