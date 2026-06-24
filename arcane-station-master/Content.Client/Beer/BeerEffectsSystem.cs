using Robust.Client.Graphics;

namespace Content.Client.Beer;

public sealed class BeerEffectsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    private FisheyeOverlay _fisheye = default!;
    private WarmFilterOverlay _warm = default!;
    private BloomOverlay _bloom = default!;
    private BrightnessOverlay _brightness = default!;
    private VignetteOverlay _vignette = default!;
    private HpPulseOverlay _hpPulse = default!;
    private LoveOverlay _love = default!;

    public override void Initialize()
    {
        _bloom = new BloomOverlay { ZIndex = 0 };
        _brightness = new BrightnessOverlay { ZIndex = 1 };
        _warm = new WarmFilterOverlay { ZIndex = 2 };
        _fisheye = new FisheyeOverlay { ZIndex = 3 };
        _vignette = new VignetteOverlay { ZIndex = 4 };
        _hpPulse = new HpPulseOverlay { ZIndex = 6 };
        _love = new LoveOverlay { ZIndex = 7 };
        _overlay.AddOverlay(_bloom);
        _overlay.AddOverlay(_brightness);
        _overlay.AddOverlay(_warm);
        _overlay.AddOverlay(_fisheye);
        _overlay.AddOverlay(_vignette);
        _overlay.AddOverlay(_hpPulse);
        _overlay.AddOverlay(_love);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay(_love);
        _overlay.RemoveOverlay(_hpPulse);
        _overlay.RemoveOverlay(_vignette);
        _overlay.RemoveOverlay(_fisheye);
        _overlay.RemoveOverlay(_warm);
        _overlay.RemoveOverlay(_brightness);
        _overlay.RemoveOverlay(_bloom);
    }
}
