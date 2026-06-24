using Content.Server.GameTicking.Rules;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Timing;

namespace Content.Server.Beer.Events;

public sealed class BeerBlackoutRule : StationEventSystem<BeerBlackoutRuleComponent>
{
    [Dependency] private readonly ApcSystem _apc = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    protected override void Started(EntityUid uid, BeerBlackoutRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station))
            return;

        component.AffectedStation = station;
        component.StartTime = _timing.CurTime;
    }

    protected override void ActiveTick(EntityUid uid, BeerBlackoutRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.AffectedStation == null)
            return;

        var elapsed = _timing.CurTime - component.StartTime;

        if (!component.LightsOff && elapsed >= component.LightsOffDelay)
        {
            CutPower(component);
            component.LightsOff = true;
        }
        else if (component.LightsOff && !component.LightsRestored && elapsed >= component.LightsOffDelay + component.BlackoutDuration)
        {
            RestorePower(component);
            component.LightsRestored = true;
        }
    }

    protected override void Ended(EntityUid uid, BeerBlackoutRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        if (!component.LightsRestored)
            RestorePower(component);
    }

    private void CutPower(BeerBlackoutRuleComponent component)
    {
        var query = AllEntityQuery<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            if (!apc.MainBreakerEnabled)
                continue;
            if (CompOrNull<StationMemberComponent>(xform.GridUid)?.Station != component.AffectedStation)
                continue;
            _apc.ApcToggleBreaker(apcUid, apc);
            component.AffectedApcs.Add(apcUid);
        }
    }

    private void RestorePower(BeerBlackoutRuleComponent component)
    {
        foreach (var apcUid in component.AffectedApcs)
        {
            if (Deleted(apcUid))
                continue;
            if (TryComp<ApcComponent>(apcUid, out var apc) && !apc.MainBreakerEnabled)
                _apc.ApcToggleBreaker(apcUid, apc);
        }
        component.AffectedApcs.Clear();
    }
}
