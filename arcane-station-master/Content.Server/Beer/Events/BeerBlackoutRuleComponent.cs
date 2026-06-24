namespace Content.Server.Beer.Events;

[RegisterComponent, Access(typeof(BeerBlackoutRule))]
public sealed partial class BeerBlackoutRuleComponent : Component
{
    [DataField]
    public TimeSpan LightsOffDelay = TimeSpan.FromSeconds(11);

    [DataField]
    public TimeSpan BlackoutDuration = TimeSpan.FromMinutes(3);

    [ViewVariables]
    public TimeSpan StartTime;

    [ViewVariables]
    public bool LightsOff;

    [ViewVariables]
    public bool LightsRestored;

    [ViewVariables]
    public EntityUid? AffectedStation;

    [ViewVariables]
    public List<EntityUid> AffectedApcs = new();
}
