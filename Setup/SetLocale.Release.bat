@ECHO OFF
cls
cd ..
FOR /D /r %%G IN ("Locale*") DO ( 
	Echo Run folder: %%G
	FOR /f %%F IN ('dir /b %%G\SetLocale.*Release.bat') DO (
		Echo Run files: %%F
		cd %%G
		call %%F
	)
)
pause