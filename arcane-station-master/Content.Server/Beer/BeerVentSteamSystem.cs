using System.Numerics;
using Content.Server.Atmos.Piping.Unary.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Beer;

public sealed class BeerVentSteamSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private TimeSpan _nextTick;
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(6.0);
    private const float VentSpawnChance = 0.08f;
    private const float ScrubberSpawnChance = 0.05f;

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextTick)
            return;
        _nextTick = _timing.CurTime + TickInterval;

        var ventQuery = EntityQueryEnumerator<GasVentPumpComponent, TransformComponent>();
        while (ventQuery.MoveNext(out var uid, out var vent, out var xform))
        {
            if (!vent.Enabled)
                continue;
            if (_random.NextFloat() > VentSpawnChance)
                continue;

            SpawnPuff(xform);
        }

        var scrubberQuery = EntityQueryEnumerator<GasVentScrubberComponent, TransformComponent>();
        while (scrubberQuery.MoveNext(out var uid, out var scrubber, out var xform))
        {
            if (!scrubber.Enabled)
                continue;
            if (_random.NextFloat() > ScrubberSpawnChance)
                continue;

            SpawnPuff(xform);
        }
    }

    private void SpawnPuff(TransformComponent xform)
    {
        var coords = xform.Coordinates.Offset(new Vector2(
            (_random.NextFloat() - 0.5f) * 0.12f,
            0.05f + _random.NextFloat() * 0.08f));
        Spawn("BeerVentSteam", coords);
    }
}
