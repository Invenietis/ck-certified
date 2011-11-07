rem ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Debug\LocBaml.exe

rem ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Debug\
mkdir fr-FR
LocBaml /generate en-US\CK.WPF.Controls.resources.dll /trans:..\..\Library\WPF\CK.WPF.Controls\Locale\fr-FR.txt /cult:fr-FR /out:fr-FR

rem ------------------------ clean ------------------------
del LocBaml.exe

pause