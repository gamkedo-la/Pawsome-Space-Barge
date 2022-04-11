using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player Settings")]
public class PlayerSettings : ScriptableObject
{
    // first run?
    [SerializeField] public bool firstRun = true;

    // dialog switches
    [SerializeField, ReadOnly] public bool mafiaMad;
    [SerializeField, ReadOnly] public bool playerSelectBarge;
    [SerializeField, ReadOnly] public bool tooEasy;


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
