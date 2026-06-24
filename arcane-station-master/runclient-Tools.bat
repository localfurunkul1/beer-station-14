REM SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
REM SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
REM SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
REM
REM SPDX-License-Identifier: AGPL-3.0-or-later

@echo off
set "PATH=C:\Users\impero\.dotnet;%PATH%"
set "DOTNET_ROOT=C:\Users\impero\.dotnet"
dotnet run --project Content.Goobstation.Client --configuration Tools
