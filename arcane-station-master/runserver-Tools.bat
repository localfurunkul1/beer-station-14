@echo off
chcp 65001 >nul
set "PATH=C:\Users\impero\.dotnet;%PATH%"
set "DOTNET_ROOT=C:\Users\impero\.dotnet"

set "SRV_NAME=🧃ПИВНАЯ СТАНЦИЯ🍻 [+18][ПАРКУР]✅✅[БЕЗ БАНОВ][СОЦИАЛЬНЫЙ РЕЙТИНГ]‼‼"
set "SRV_DESC=18+ Medium/Low RP. Паркур, социальный рейтинг, без банов, отзывчивые админы. Discord: discord.gg/m3BpKrdHMR"

dotnet run --project Content.Goobstation.Server --configuration Tools -- ^
  --cvar game.hostname="%SRV_NAME%" ^
  --cvar game.desc="%SRV_DESC%" ^
  --cvar game.role_timers=false ^
  --cvar hub.advertise=false ^
  --cvar hub.tags="rp;18+;russian;parkour" ^
  --cvar infolinks.discord="https://discord.gg/m3BpKrdHMR"

pause
