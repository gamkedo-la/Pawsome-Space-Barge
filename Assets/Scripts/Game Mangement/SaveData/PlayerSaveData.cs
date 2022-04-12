/// <summary>
/// Data wrapper for serializing PlayerSettings to disk.
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public bool firstRun;
    public bool mafiaMad;
    public bool playerSelectBarge;
    public bool tooEasy;


    public PlayerSaveData(PlayerSettings settings)
    {
        firstRun = settings.firstRun;
        mafiaMad = settings.mafiaMad;
        playerSelectBarge = settings.playerSelectBarge;
        tooEasy = settings.tooEasy;
    }
}
