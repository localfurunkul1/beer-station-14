using Content.Shared.Damage;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Client.Beer;

public sealed class BloodSplatterSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private BloodSplatterOverlay _splatter = default!;

    public override void Initialize()
    {
        _splatter = new BloodSplatterOverlay { ZIndex = 5 };
        _overlay.AddOverlay(_splatter);
        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay(_splatter);
    }

    private void OnDamageChanged(EntityUid uid, DamageableComponent component, DamageChangedEvent args)
    {
        if (_player.LocalEntity != uid || !args.DamageIncreased || args.DamageDelta == null)
            return;

        var total = (float)args.DamageDelta.GetTotal();
        if (total < 5f)
            return;

        _splatter.AddSplatter(total);
    }
}
