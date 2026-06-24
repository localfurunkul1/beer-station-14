using Robust.Shared.Serialization;

namespace Content.Shared.Beer.Parkour;

[Serializable, NetSerializable]
public sealed class BeerVaultEvent : EntityEventArgs
{
    public NetEntity User;
    public float FromX;
    public float FromY;

    public BeerVaultEvent(NetEntity user, float fromX, float fromY)
    {
        User = user;
        FromX = fromX;
        FromY = fromY;
    }
}
