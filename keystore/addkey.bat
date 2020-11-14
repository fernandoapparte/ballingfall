@echo off
rem si falta un parámetro ir a las instrucciones
if "%1"=="" goto instrucciones
if "%2"=="" goto instrucciones

keytool -genkey -noprompt -v -keystore MAIN-KEYSTORE.keystore -storepass ballingfall  -dname "CN=YOUR NAME HERE, OU=OU, O=YOUR COMPANY, L=YOUR CITY,S=YOUR CITY,C=US" -alias %1 -keypass %2 -keyalg RSA -keysize 2048 -validity 10000
echo *****************************
echo Key  %1 con clave %2 agregada
goto end
:instrucciones
echo addkey genera un keystore en la carpeta donde uno esté parado. 
echo dicho keystore se llama MAIN-KEYSTORE.keystore
echo LA CLAVE DEL KEYSTORE ES: ballingfall
echo Parametros: addkey alias password
echo A la salida, generará el Keystore con la clave indicada y le agrega el alias y password indicado.

:end
