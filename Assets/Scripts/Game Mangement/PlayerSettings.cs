using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player Settings")]
public class PlayerSettings : ScriptableObject
{
    // first run?
    public bool firstRun = true;

    // dialog switches
    public bool mafiaMad = false;
    public bool playerSelectBarge = false;
    public bool tooEasy = false;

    // game state variables
    public int commercialEarnings;
    public int mafiaDeliveries;
    public int bargesDelivered;
    public int bargesLost;


    PlayerSettings()
    {
        Reset();
    }


    public void Reset()
    {
        firstRun = true;
        mafiaMad = false;
        playerSelectBarge = false;
        tooEasy = false;

        commercialEarnings = 0;
        mafiaDeliveries = 0;
        bargesDelivered = 0;
        bargesLost = 0;
    }


    public void SaveGame()
    {
    
    #if !UNITY_EDITOR
        DataUtilities.SavePlayerData(this);
    #endif

    }


    public static PlayerSettings ConvertFromData(PlayerSaveData data)
    {
        PlayerSettings newSettings = ScriptableObject.CreateInstance<PlayerSettings>();

        try
        {
            newSettings.firstRun = data.firstRun;
            newSettings.mafiaMad = data.mafiaMad;
            newSettings.playerSelectBarge = data.playerSelectBarge;
            newSettings.tooEasy = data.tooEasy;
            newSettings.commercialEarnings = data.commercialEarnings;
            newSettings.mafiaDeliveries = data.mafiaDeliveries;
            newSettings.bargesDelivered = data.bargesDelivered;
            newSettings.bargesLost = data.bargesLost;
        }
        catch
        {
            newSettings.Reset();
        }

        return newSettings;
    }
}
