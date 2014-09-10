:This .bat gets the en-US BAML resources of the configured Civikey plugin, translates it to the specified culture and merges it with the compiled resx of the same culture.
:if

:STEP 1 - Configure the variables
set containsRESX=true
set compileMode=Debug
set culture=fr-FR
set pluginName=KeyboardEditor
set tempFolderName=TEMP

:NOTE : These paths are relative to the folder into which this .bat file is located.
set pluginPath=..\..\Plugins\Advanced\ContextEditor\
set outputPath=..\..\..\..\Output\%compileMode%\
set objPath=..\obj\%compileMode%\
:Path to the Setup Folder, containg LocBaml.exe
set setupPath=..\..\..\..\Setup\

:LocBaml needs to be copied next to the dlls that are going to be localized
copy %setupPath%LocBaml.exe %outputPath%LocBaml.exe

mkdir %outputPath%%tempFolderName%
mkdir %outputPath%en-US

:The dll of the plugin is needed to generate the localized BAML
copy %outputPath%Plugins\%pluginName%.dll %outputPath%%pluginName%.dll

if %containsRESX% EQU true (
:STEP 2 : if needed, find the RESX compiled files linked to the plugin and copy them into the temporary folder
:NOTE : we need the .resources files, not the final assembly in order to merge the BAML file and the RESX files together into one final assembly.
copy %objPath%%pluginName%.Resources.R.%culture%.resources %outputPath%%tempFolderName%\%pluginName%.Resources.R.%culture%.resources
copy %objPath%%pluginName%.Resources.Images.%culture%.resources %outputPath%%tempFolderName%\%pluginName%.Resources.Images.%culture%.resources
)

:Moving to the release path to simplify the rest of the localization process calls
cd %outputPath%
mkdir Plugins\%culture%
mkdir %tempFolderName%

if %containsRESX% EQU true (
:Generating the localized BAML file (.resources) into the temporary folder
LocBaml /generate %pluginPath%obj\%compileMode%\%pluginName%.g.en-US.resources /trans:%pluginPath%Locale\%culture%.txt /cult:%culture% /out:%tempFolderName%

:Linking the BAML file with the RESX file(s)
:STEP 4 - make sure you embed all the RESX compiled files linked to your plugin.
al /template:Plugins\%pluginName%.dll /embed:%tempFolderName%\%pluginName%.g.%culture%.resources /embed:%tempFolderName%\%pluginName%.Resources.Images.%culture%.resources /embed:%tempFolderName%\%pluginName%.Resources.R.%culture%.resources /culture:%culture% /out:Plugins\%culture%\%pluginName%.resources.dll 

) else (
:There are no resx files, just localize the BAML and output it in the right folder
LocBaml /generate %pluginPath%obj\%compileMode%\en-US\%pluginName%.resources.dll /trans:%pluginPath%Locale\%culture%.txt /cult:%culture% /out:Plugins\%culture%
)

:Cleaning the release folder
del %pluginName%.dll
del LocBaml.exe
rmdir %tempFolderName% /s /q 
echo. & echo. & echo. & echo. & echo. & echo %pluginName% LOCALIZATION PROCESS FINISHED & echo. & echo. & echo. & echo.

pause