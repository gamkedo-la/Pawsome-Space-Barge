using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player Settings")]
public class PlayerSettings : ScriptableObject
{
    // first run?
    [SerializeField] public bool firstRun = true;

    // dialog switches
    [SerializeField, ReadOnly] public bool mafiaMad = false;
    [SerializeField, ReadOnly] public bool playerSelectBarge = false;
    [SerializeField, ReadOnly] public bool tooEasy = false;


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
        }
        catch
        {
            newSettings.Reset();
        }

        return newSettings;
    }
}
