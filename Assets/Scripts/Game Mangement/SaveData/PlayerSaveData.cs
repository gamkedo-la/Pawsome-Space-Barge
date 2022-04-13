/// <summary>
/// Data wrapper for serializing PlayerSettings to disk.
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public bool firstRun;

    // dialog switches
    public bool mafiaMad;
    public bool playerSelectBarge;
    public bool tooEasy;

    // game state variables
    public int commercialEarnings;
    public int mafiaDeliveries;
    public int bargesDelivered;
    public int bargesLost;


    public PlayerSaveData(PlayerSettings settings)
    {
        firstRun = settings.firstRun;
        mafiaMad = settings.mafiaMad;
        playerSelectBarge = settings.playerSelectBarge;
        tooEasy = settings.tooEasy;

        commercialEarnings = settings.commercialEarnings;
        mafiaDeliveries = settings.mafiaDeliveries;
        playerSelectBarge = settings.playerSelectBarge;
        bargesLost = settings.bargesLost;
    }
}
