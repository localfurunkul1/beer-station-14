using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Beer;

public sealed class BeerLightFlickerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;

    private const float TickInterval = 1.0f;
    private const float FlickerChance = 0.01f;

    private TimeSpan _nextTick;

    private readonly List<ActiveFlicker> _active = new();

    private struct ActiveFlicker
    {
        public EntityUid Light;
        public TimeSpan NextStep;
        public int StepsLeft;
        public bool TargetState;
    }

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;

        for (var i = _active.Count - 1; i >= 0; i--)
        {
            var f = _active[i];
            if (Deleted(f.Light) || !TryComp<PoweredLightComponent>(f.Light, out var lightComp))
            {
                _active.RemoveAt(i);
                continue;
            }

            if (now < f.NextStep)
                continue;

            _light.SetState(f.Light, f.TargetState, lightComp);

            if (f.StepsLeft <= 0)
            {
                _light.SetState(f.Light, true, lightComp);
                _active.RemoveAt(i);
                continue;
            }

            f.StepsLeft--;
            f.TargetState = !f.TargetState;
            f.NextStep = now + TimeSpan.FromMilliseconds(40 + _random.Next(130));
            _active[i] = f;
        }

        if (now < _nextTick)
            return;
        _nextTick = now + TimeSpan.FromSeconds(TickInterval);

        var query = EntityQueryEnumerator<PoweredLightComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.On)
                continue;
            if (IsAlreadyFlickering(uid))
                continue;
            if (_random.NextFloat() > FlickerChance)
                continue;

            _active.Add(new ActiveFlicker
            {
                Light = uid,
                NextStep = now,
                StepsLeft = 5 + _random.Next(8),
                TargetState = false,
            });
        }
    }

    private bool IsAlreadyFlickering(EntityUid uid)
    {
        foreach (var f in _active)
        {
            if (f.Light == uid)
                return true;
        }
        return false;
    }
}
