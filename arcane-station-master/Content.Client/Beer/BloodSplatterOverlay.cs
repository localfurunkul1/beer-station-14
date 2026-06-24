using System.Numerics;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Beer;

public sealed class BloodSplatterOverlay : Overlay
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IResourceCache _resCache = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private static readonly string[] TexturePaths =
    {
        "/Textures/Beer/Blood/blood1.png",
        "/Textures/Beer/Blood/blood2.png",
    };

    private readonly Texture[] _textures;

    private struct Spot
    {
        public int TextureIndex;
        public Vector2 Uv;
        public float Size;
        public float Rotation;
        public float Strength;
        public float Decay;
    }

    private readonly Spot[] _spots = new Spot[5];
    private int _next;

    public BloodSplatterOverlay()
    {
        IoCManager.InjectDependencies(this);
        _textures = new Texture[TexturePaths.Length];
        for (var i = 0; i < TexturePaths.Length; i++)
            _textures[i] = _resCache.GetResource<TextureResource>(new ResPath(TexturePaths[i])).Texture;
    }

    public void AddSplatter(float amount)
    {
        var size = MathHelper.Clamp(0.08f + amount * 0.005f, 0.08f, 0.20f);
        _spots[_next] = new Spot
        {
            TextureIndex = _random.Next(_textures.Length),
            Uv = new Vector2(_random.NextFloat(0.08f, 0.92f), _random.NextFloat(0.08f, 0.92f)),
            Size = size,
            Rotation = _random.NextFloat() * MathF.Tau,
            Strength = 1f,
            Decay = 1f / (1.8f + _random.NextFloat() * 1.4f),
        };
        _next = (_next + 1) % _spots.Length;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_cfg.GetCVar(CCVars.BeerShadersEnabled) || !_cfg.GetCVar(CCVars.BeerBloodSplatterEnabled))
            return false;

        var dt = (float)_timing.FrameTime.TotalSeconds;
        var any = false;
        for (var i = 0; i < _spots.Length; i++)
        {
            if (_spots[i].Strength > 0f)
            {
                _spots[i].Strength = MathF.Max(0f, _spots[i].Strength - _spots[i].Decay * dt);
                if (_spots[i].Strength > 0f) any = true;
            }
        }
        return any;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var viewport = args.ViewportBounds;
        var refDim = MathF.Min(viewport.Width, viewport.Height);

        for (var i = 0; i < _spots.Length; i++)
        {
            var s = _spots[i];
            if (s.Strength <= 0f)
                continue;

            var tex = _textures[s.TextureIndex];
            var pixelSize = s.Size * refDim;
            var half = pixelSize * 0.5f;
            var center = new Vector2(
                viewport.Left + s.Uv.X * viewport.Width,
                viewport.Top + s.Uv.Y * viewport.Height);

            var box = new UIBox2(
                center.X - half,
                center.Y - half,
                center.X + half,
                center.Y + half);

            var alpha = MathF.Min(1f, s.Strength * 1.5f);
            handle.DrawTextureRect(tex, box, new Color(1f, 1f, 1f, alpha));
        }
    }
}
