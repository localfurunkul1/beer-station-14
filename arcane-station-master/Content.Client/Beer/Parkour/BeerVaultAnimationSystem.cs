using System.Numerics;
using Content.Goobstation.Shared.Sprinting;
using Content.Shared.Beer.Parkour;
using Content.Shared.Climbing.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Animations;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Client.Beer.Parkour;

public sealed class BeerVaultAnimationSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    private const string AnimKey = "beer-vault";
    private const float Duration = 0.45f;
    private const float ArcHeight = 0.75f;
    private const float PredictiveClearance = 1.0f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeAllEvent<BeerVaultEvent>(OnVault);
        SubscribeLocalEvent<ClimbableComponent, StartCollideEvent>(OnPredictiveCollide);
    }

    private void OnPredictiveCollide(Entity<ClimbableComponent> climbable, ref StartCollideEvent args)
    {
        if (args.OtherEntity != _player.LocalEntity)
            return;
        if (!TryComp<SprinterComponent>(args.OtherEntity, out var sprinter) || !sprinter.IsSprinting)
            return;
        if (_anim.HasRunningAnimation(args.OtherEntity, AnimKey))
            return;
        if (TerminatingOrDeleted(climbable))
            return;

        var playerXform = Transform(args.OtherEntity);
        var playerWorld = _xform.GetWorldPosition(args.OtherEntity);
        var targetWorld = _xform.GetWorldPosition(climbable.Owner);
        var diff = targetWorld - playerWorld;
        var dist = diff.Length();
        if (dist < 0.01f)
            return;

        var dir = diff / dist;
        var landingWorld = targetWorld + dir * PredictiveClearance;
        var fromOffset = playerWorld - landingWorld;

        _xform.SetMapCoordinates(args.OtherEntity, new MapCoordinates(landingWorld, playerXform.MapID));
        PlayVault(args.OtherEntity, fromOffset);
    }

    private void OnVault(BeerVaultEvent ev)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var user = GetEntity(ev.User);
        if (TerminatingOrDeleted(user))
            return;

        if (_anim.HasRunningAnimation(user, AnimKey))
            return;

        PlayVault(user, new Vector2(ev.FromX, ev.FromY));
    }

    private void PlayVault(EntityUid user, Vector2 from)
    {
        if (!TryComp<SpriteComponent>(user, out var sprite))
            return;

        var baseAngle = sprite.Rotation;
        var mid = from * 0.5f + new Vector2(0f, ArcHeight);
        var end = Vector2.Zero;

        var halfArc = Duration * 0.5f;

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(Duration),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(from, 0f),
                        new AnimationTrackProperty.KeyFrame(mid, halfArc),
                        new AnimationTrackProperty.KeyFrame(end, halfArc),
                    }
                },
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(baseAngle, 0f),
                        new AnimationTrackProperty.KeyFrame(baseAngle + Angle.FromDegrees(180), Duration * 0.45f),
                        new AnimationTrackProperty.KeyFrame(baseAngle + Angle.FromDegrees(360), Duration * 0.45f),
                        new AnimationTrackProperty.KeyFrame(baseAngle, 0.05f),
                    }
                }
            }
        };

        _anim.Play(user, animation, AnimKey);
    }
}
