using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class GameManagement : MonoBehaviour
{
    // Singleton
    [HideInInspector] public static GameManagement Instance;

    // using Fungus for non-game UI
    [SerializeField] private Flowchart title;
    [SerializeField] private Flowchart tutorial;
    [SerializeField] private Flowchart mission;
    [SerializeField] private Flowchart restart;
    [SerializeField] private Flowchart ending;

    // player settings + saved data
    [SerializeField] private PlayerSettings settings;


    private void Awake()
    {
        // setup singleton
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // initialize player settings if not present
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PlayerSettings>();
        }
    }


    void Start()
    {
        if (settings.firstRun == true)
        {
            tutorial.SendFungusMessage("start");
            settings.firstRun = false;
        }
    }


    void Update()
    {
        
    }


    public void DialogDone(Flowchart chart)
    {
        Debug.Log("Dialog Done");
    }
}
