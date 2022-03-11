using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player Settings")]
public class PlayerSettings : ScriptableObject
{
    // first run?
    public bool firstRun = true;

    // dialog switches
    public bool mafiaMad { get; set; }
    public bool playerSelectBarge { get; set; }
    public bool tooEasy { get; set; }

    
    public PlayerSettings()
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
}
