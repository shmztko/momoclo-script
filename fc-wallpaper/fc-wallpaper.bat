@echo off
set LOG_FILE=fc-wallpaper.log

echo %date% %time% >> %LOG_FILE%
cd >> %LOG_FILE%

for /F "usebackq" %%i in (` bundle exec ruby fc-wallpaper.rb %LOG_FILE% `) do (
	.\bin\ChangeWallpaper.exe %%i >> %LOG_FILE%
)