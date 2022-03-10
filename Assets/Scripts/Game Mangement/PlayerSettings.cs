using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player Settings")]
public class PlayerSettings : ScriptableObject
{
    // first run?
    private bool firstRun = true;

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
        mafiaMad = false;
        playerSelectBarge = false;
        tooEasy = false;
    }
}
