using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Beer;

public sealed class BeerSprintEffectSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    private SprintShakeOverlay _shake = default!;

    public override void Initialize()
    {
        _shake = new SprintShakeOverlay();
        _overlay.AddOverlay(_shake);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay(_shake);
    }
}
