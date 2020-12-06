# Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass -Force;
Set-ExecutionPolicy Bypass -Scope Process -Force; 

cd C:\GIT
git clone https://github.com/thepirat000/photo-restoration.git

CD photo-restoration\src\photo-api
publish.cmd

