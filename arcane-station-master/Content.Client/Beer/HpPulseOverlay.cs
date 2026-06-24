using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Beer;

public sealed class HpPulseOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "BeerHpPulse";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly ShaderInstance _shader;
    private float _strength;
    private TimeSpan _lastUpdate;

    public HpPulseOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index(Shader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_cfg.GetCVar(CCVars.BeerShadersEnabled))
            return false;

        var now = _timing.RealTime;
        var dt = (float)(now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;
        if (dt < 0f || dt > 0.5f)
            dt = 0f;

        var target = 0f;
        if (_player.LocalEntity is { } player &&
            _ent.TryGetComponent<MobThresholdsComponent>(player, out var thresholds) &&
            _ent.TryGetComponent<DamageableComponent>(player, out var damageable))
        {
            float? critThreshold = null;
            foreach (var (key, value) in thresholds.Thresholds)
            {
                if (key.ToString().Contains("Critical", StringComparison.OrdinalIgnoreCase))
                {
                    critThreshold = (float)value;
                    break;
                }
            }

            if (critThreshold is { } crit && crit > 0f)
            {
                var totalDmg = (float)damageable.TotalDamage;
                var ratio = totalDmg / crit;
                if (ratio > 0.55f)
                    target = MathF.Min(1f, (ratio - 0.55f) / 0.45f);
            }
        }

        _strength += (target - _strength) * MathF.Min(1f, dt * 3f);
        return _strength > 0.02f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("Intensity", _strength);
        var handle = args.WorldHandle;
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
