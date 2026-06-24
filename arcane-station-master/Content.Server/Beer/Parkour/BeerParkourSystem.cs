using System.Numerics;
using Content.Goobstation.Shared.Sprinting;
using Content.Shared.Beer.Parkour;
using Content.Shared.Climbing.Components;
using Content.Shared.Stunnable;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Beer.Parkour;

public sealed class BeerParkourSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(1.5);
    private const float VaultClearance = 1.0f;
    private static readonly SoundPathSpecifier VaultSound =
        new("/Audio/_Goobstation/Effects/Sprinting/sprint_puff.ogg");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClimbableComponent, StartCollideEvent>(OnClimbableCollide);
    }

    private void OnClimbableCollide(Entity<ClimbableComponent> climbable, ref StartCollideEvent args)
    {
        var sprinterUid = args.OtherEntity;

        if (!TryComp<SprinterComponent>(sprinterUid, out var sprinter) || !sprinter.IsSprinting)
            return;

        if (TerminatingOrDeleted(sprinterUid) || TerminatingOrDeleted(climbable))
            return;

        if (HasComp<KnockedDownComponent>(sprinterUid))
            return;

        var cooldown = EnsureComp<BeerParkourComponent>(sprinterUid);
        if (_timing.CurTime < cooldown.NextVaultTime)
            return;

        var playerXform = Transform(sprinterUid);
        var targetXform = Transform(climbable.Owner);

        if (playerXform.MapID != targetXform.MapID)
            return;

        var playerWorld = _xform.GetWorldPosition(sprinterUid);
        var targetWorld = _xform.GetWorldPosition(climbable.Owner);
        var diff = targetWorld - playerWorld;
        var dist = diff.Length();
        if (dist < 0.01f)
            return;

        var dir = diff / dist;
        var landingWorld = targetWorld + dir * VaultClearance;
        var landingCoords = new MapCoordinates(landingWorld, playerXform.MapID);

        cooldown.NextVaultTime = _timing.CurTime + Cooldown;

        var fromOffset = playerWorld - landingWorld;
        var filter = Filter.Pvs(sprinterUid, entityManager: EntityManager);
        RaiseNetworkEvent(new BeerVaultEvent(GetNetEntity(sprinterUid), fromOffset.X, fromOffset.Y), filter);

        _xform.SetMapCoordinates(sprinterUid, landingCoords);
        _audio.PlayPvs(VaultSound, sprinterUid, AudioParams.Default.WithVolume(-3f));
    }
}
