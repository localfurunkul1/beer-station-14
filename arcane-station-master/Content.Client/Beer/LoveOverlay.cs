using Content.Shared._Arcane.ERP;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Beer;

public sealed class LoveOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "BeerLove";

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

    public LoveOverlay()
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
            _ent.TryGetComponent<ArousalComponent>(player, out var arousal) &&
            arousal.MaxArousal > 0f)
        {
            var ratio = arousal.LastValue / arousal.MaxArousal;
            if (ratio > 0.08f)
                target = MathF.Min(1f, (ratio - 0.08f) / 0.85f);
        }

        _strength += (target - _strength) * MathF.Min(1f, dt * 1.8f);
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
