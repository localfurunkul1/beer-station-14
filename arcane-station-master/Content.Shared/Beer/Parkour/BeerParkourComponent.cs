using Robust.Shared.GameStates;

namespace Content.Shared.Beer.Parkour;

[RegisterComponent, NetworkedComponent]
public sealed partial class BeerParkourComponent : Component
{
    [ViewVariables]
    public TimeSpan NextVaultTime = TimeSpan.Zero;
}
