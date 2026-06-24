// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using TimedDespawnComponent = Robust.Shared.Spawners.TimedDespawnComponent;

namespace Content.Client.Projectiles;

public sealed class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ImpactEffectEvent>(OnProjectileImpact);
    }

    private void OnProjectileImpact(ImpactEffectEvent ev)
    {
        var coords = GetCoordinates(ev.Coordinates);

        if (Deleted(coords.EntityId))
            return;

        var ent = Spawn(ev.Prototype, coords);

        if (_cfg.GetCVar(CCVars.BeerBulletFxEnabled))
        {
            if (TryComp<SpriteComponent>(ent, out var mainSprite))
            {
                mainSprite.Scale = mainSprite.Scale * 1.35f;
                mainSprite.Color = Color.FromSrgb(new Color(
                    Math.Clamp(mainSprite.Color.R * 1.4f, 0f, 1f),
                    Math.Clamp(mainSprite.Color.G * 1.25f, 0f, 1f),
                    Math.Clamp(mainSprite.Color.B * 0.95f, 0f, 1f),
                    mainSprite.Color.A));
            }

            for (var i = 0; i < 4; i++)
            {
                var off = coords.Offset(new System.Numerics.Vector2(
                    (_random.NextFloat() - 0.5f) * 0.45f,
                    (_random.NextFloat() - 0.5f) * 0.45f));
                var spark = Spawn(ev.Prototype, off);
                if (TryComp<SpriteComponent>(spark, out var sparkSprite))
                {
                    var s = 0.55f + _random.NextFloat() * 0.55f;
                    sparkSprite.Scale = sparkSprite.Scale * s;
                    sparkSprite.Color = sparkSprite.Color.WithAlpha(0.45f + _random.NextFloat() * 0.35f);
                }
            }
        }

        if (TryComp<SpriteComponent>(ent, out var sprite))
        {
            sprite[EffectLayers.Unshaded].AutoAnimated = false;
            _sprite.LayerMapTryGet((ent, sprite), EffectLayers.Unshaded, out var layer, false);
            var state = _sprite.LayerGetRsiState((ent, sprite), layer);
            var lifetime = 0.5f;

            if (TryComp<TimedDespawnComponent>(ent, out var despawn))
                lifetime = despawn.Lifetime;

            var anim = new Animation()
            {
                Length = TimeSpan.FromSeconds(lifetime),
                AnimationTracks =
                {
                    new AnimationTrackSpriteFlick()
                    {
                        LayerKey = EffectLayers.Unshaded,
                        KeyFrames =
                        {
                            new AnimationTrackSpriteFlick.KeyFrame(state.Name, 0f),
                        }
                    }
                }
            };

            _player.Play(ent, anim, "impact-effect");
        }
    }
}