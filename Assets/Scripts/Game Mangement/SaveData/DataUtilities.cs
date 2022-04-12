using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class DataUtilities
{
    private static string path = Application.persistentDataPath + "/pawsome.meow";


    /// <summary>
    /// Saves player settings to disk
    /// </summary>
    /// <param name="settings"></param>
    public static void SavePlayerData(PlayerSettings settings)
    {
        Debug.Log("Saving Data...");
        Debug.Log($"{path}");

        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            PlayerSaveData data = new PlayerSaveData(settings);

            formatter.Serialize(stream, data);
        }

        if (!File.Exists(path))
        {
            Debug.Log("Error saving data to disk.");
        }
    }


    /// <summary>
    /// Loads player settings data from persistentDataPath.
    /// If none is present, new data is created.
    /// </summary>
    /// <returns></returns>
    public static PlayerSettings LoadPlayerData()
    {
        if (File.Exists(path))
        {
            Debug.Log("Save File Found.");
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                PlayerSaveData data = formatter.Deserialize(stream) as PlayerSaveData;

                var thing1 = PlayerSettings.ConvertFromData(data);

                return thing1;
            }
        }
        else
        {
            Debug.Log("No Save File Found, creating new settings.");
            PlayerSettings newSettings = ScriptableObject.CreateInstance<PlayerSettings>(); //new PlayerSettings();
            SavePlayerData(newSettings);

            return newSettings;
        }
    }
}
