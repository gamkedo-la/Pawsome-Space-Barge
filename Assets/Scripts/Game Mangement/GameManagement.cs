using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Fungus;

public class GameManagement : MonoBehaviour
{
    [HideInInspector] public static GameManagement Instance;
    private GameObject barge;
    private OrbitalBody bargeOrbitalBody;
    private bool gamePaused = false;


    [Header("Fungus Flowcharts")]
    [SerializeField] private Flowchart tutorial;
    [SerializeField] private Flowchart mission;
    [SerializeField] private Flowchart warnings;
    [SerializeField] private Flowchart restart;

    [SerializeField] private bool pauseOnDialog = false;


    [Header("Player Settings")]
    [SerializeField] private PlayerSettings settings;


    [Header("Alert Networks")]
    [SerializeField] private AlertEvent policeEvent;
    [SerializeField] private AlertEvent pirateEvent;



    // **************************************** Accessors *****************************************
    public Vector2 bargeVelocity => bargeOrbitalBody.Velocity;
    public bool GamePaused => gamePaused;



    // ************************************* Monobehaviours ***************************************
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
        barge = GameObject.FindGameObjectWithTag("Barge");
        bargeOrbitalBody = barge.GetComponent<OrbitalBody>();
    }


    void Update()
    {
        if (settings.firstRun == true)
        {
            StartDialog(tutorial, pauseOnDialog ? true : false );
            settings.firstRun = false;
        }
    }



    // ***************************************** Dialog *******************************************
    public void DialogDone(Flowchart chart)
    {
        chart.gameObject.SetActive(false);
        TogglePause(PauseState.Playing);
    }


    private void StartDialog(Flowchart dialog, bool pause=false)
    {
        if (pause) { TogglePause(PauseState.Paused); }
        dialog.gameObject.SetActive(true);
        dialog.SendFungusMessage("start");
    }



    // ************************************** Alert Network ***************************************
    public AlertEvent GetAlertNetwork(EnemyType type)
    {
        return type == EnemyType.Police ? policeEvent : pirateEvent;
    }


    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("Pause button pushed.");
        TogglePause();
    }

    public void TogglePause(PauseState? enterState=null)
    {
        if (enterState != null)
        {
            Time.timeScale = (int)enterState;
        }
        else
        {
            Time.timeScale = Time.timeScale == 1 ? 0 : 1;
        }

        gamePaused = Time.timeScale > 0 ? false : true;
    }
}

public enum PauseState
{
    Paused,
    Playing
}
