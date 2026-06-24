using Content.Shared.Atmos;
using Content.Shared.Nutrition.Components;
using Robust.Shared.GameObjects;

namespace Content.Server.Beer.Lighting;

public sealed class BeerSmokableLightSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SmokableComponent, IgnitedEvent>(OnIgnited);
        SubscribeLocalEvent<SmokableComponent, ExtinguishedEvent>(OnExtinguished);
    }

    private void OnIgnited(EntityUid uid, SmokableComponent comp, ref IgnitedEvent args)
    {
        if (!_light.TryGetLight(uid, out var light))
            return;
        _light.SetEnabled(uid, true, light);
    }

    private void OnExtinguished(EntityUid uid, SmokableComponent comp, ref ExtinguishedEvent args)
    {
        if (!_light.TryGetLight(uid, out var light))
            return;
        _light.SetEnabled(uid, false, light);
    }
}
