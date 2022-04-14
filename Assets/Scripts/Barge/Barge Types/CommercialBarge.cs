/// <summary>
/// Implements Barge class, fixes MissionType as Commercial.
/// </summary>
public class CommercialBarge : Barge
{
    public override MissionType type
    {
        get { return MissionType.Commercial; }
    }
}
