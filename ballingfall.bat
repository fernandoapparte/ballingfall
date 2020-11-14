echo off
echo INSTRUCCIONES: Este bat reconfigura el proyecto para Balling Fall. Para compilar pasar el parametro -compile
echo Si se compila, el ejecutable queda en folder other. Revisar stdout.log tiene info importante del resultado.
echo on

set prjpath =%cd%
@echo %prjpath%
"C:\Program Files\Unity2017.4.22.LTS\Editor\Unity.exe" -quit -batchmode -logfile stdout.log  -projectPath "D:\ESPACIO-TRABAJO\20170502-KMFactory\PROD\539-helix-jumpy\20201010-balling-fall-v1_0\" %prjpath% -executeMethod compile.ballingfall %1
