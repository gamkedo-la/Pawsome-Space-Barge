/// <summary>
/// Implements Barge class, fixes MissionType as Commercial.
/// </summary>
public class MafiaBarge : Barge
{
    public override MissionType type
    {
        get { return MissionType.Mafia; }
    }
}
