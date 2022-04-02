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

    // alert networks
    [SerializeField] private AlertEvent policeEvent;
    [SerializeField] private AlertEvent pirateEvent;

    public AlertEvent GetAlertNetwork(EnemyType type)
    {
        return type == EnemyType.Police ? policeEvent : pirateEvent;
    }

    // misc
    [SerializeField] public bool pauseOnDialog = false;


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
        //
    }


    void Update()
    {
        if (settings.firstRun == true)
        {
            StartDialog(tutorial, pauseOnDialog ? true : false );
            settings.firstRun = false;
        }
    }


    public void DialogDone(Flowchart chart)
    {
        chart.gameObject.SetActive(false);
        Time.timeScale = 1;
    }


    private void StartDialog(Flowchart dialog, bool pause=false)
    {
        if (pause) { Time.timeScale = 0; }
        dialog.gameObject.SetActive(true);
        dialog.SendFungusMessage("start");
    }
}
