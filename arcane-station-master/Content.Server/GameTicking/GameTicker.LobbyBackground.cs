// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.GameTicking.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.GameTicking;

// Goobstation - this file is heavily modified to add credits for lobby backgrounds
public sealed partial class GameTicker
{
    [ViewVariables]
    public ProtoId<LobbyBackgroundPrototype>? LobbyBackground { get; private set; }

    [ViewVariables]
    private List<ProtoId<LobbyBackgroundPrototype>> _lobbyBackgrounds = [];

    private static readonly string[] WhitelistedBackgroundExtensions = new[] {"png", "jpg", "jpeg", "webp"};

    private void InitializeLobbyBackground()
    {
        foreach (var prototype in _prototypeManager.EnumeratePrototypes<LobbyBackgroundPrototype>())
        {
            if (!WhitelistedBackgroundExtensions.Contains(prototype.Background.Extension))
            {
                _sawmill.Warning($"Lobby background '{prototype.ID}' has an invalid extension '{prototype.Background.Extension}' and will be ignored.");
                continue;
            }

            _lobbyBackgrounds.Add(prototype.ID);
        }

        _sawmill.Info($"Loaded {_lobbyBackgrounds.Count} lobby backgrounds");
        RandomizeLobbyBackground();
    }

    private void RandomizeLobbyBackground() {
        if (_lobbyBackgrounds.Count == 0)
        {
            LobbyBackground = null;
            _sawmill.Warning("RandomizeLobbyBackground: list is empty");
            return;
        }

        var prev = LobbyBackground;
        ProtoId<LobbyBackgroundPrototype> next;
        var attempts = 0;
        do
        {
            next = _robustRandom.Pick(_lobbyBackgrounds);
            attempts++;
        } while (_lobbyBackgrounds.Count > 1 && next == prev && attempts < 8);

        LobbyBackground = next;
        _sawmill.Info($"RandomizeLobbyBackground: pool={_lobbyBackgrounds.Count} prev={prev} new={next}");
    }
}
