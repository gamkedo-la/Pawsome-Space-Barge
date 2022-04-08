using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    [SerializeField] private Flowchart pauseDialog;
    [SerializeField] private Flowchart restart;

    [SerializeField] private bool pauseOnDialog = false;


    [Header("Player Settings")]
    [SerializeField] private PlayerSettings settings;


    [Header("Alert Networks")]
    [SerializeField] private AlertEvent policeEvent;
    [SerializeField] private AlertEvent pirateEvent;


    [Header("Pause Overlay")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mafiaPanel;
    [SerializeField] private GameObject commercialPanel;
    [SerializeField] private GameObject minimap;



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
        pausePanel.SetActive(false);
    }


    void Update()
    {
        if (settings.firstRun == true)
        {
            StartDialog(tutorial, pauseOnDialog ? true : false );
            settings.firstRun = false;
        }
    }



    // ************************************** Game Controls ***************************************
    /// <summary>
    /// Exits game.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }


    /// <summary>
    /// Returns to title screen.
    /// </summary>
    public void ExitToTitle()
    {
        // TODO
    }


    /// <summary>
    /// Secondary handler for OnPause input event. Passed context from ShipActions.cs
    /// </summary>
    /// <param name="context"></param>
    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("Pause button pushed.");
        if (!gamePaused)
        {
            PauseGame();
        }
    }


    /// <summary>
    /// Pauses game and disables extraneous objects.
    /// </summary>
    public void PauseGame()
    {
        TogglePause(PauseState.Paused);

        // disable health bars and minimap
        mafiaPanel.SetActive(false);
        commercialPanel.SetActive(false);
        minimap.SetActive(false);

        // enable panel
        pausePanel.SetActive(true);

        // start pause dialog
        StartDialog(pauseDialog, false);
    }


    /// <summary>
    /// Resumes Game from paused state.
    /// </summary>
    public void ResumeGame()
    {
        // enable health bars and minimap
        mafiaPanel.SetActive(true);
        commercialPanel.SetActive(true);
        minimap.SetActive(true);

        // disable panel and dialog
        pausePanel.SetActive(false);

        DialogDone(pauseDialog);
    }


    /// <summary>
    /// Toggles paused state, or sets passed PausedState.
    /// </summary>
    /// <param name="enterState"></param>
    private void TogglePause(PauseState? enterState=null)
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



    // ***************************************** Dialog *******************************************
    /// <summary>
    /// Disable dialog object and resumes game if paused.
    /// </summary>
    /// <param name="chart"></param>
    public void DialogDone(Flowchart chart)
    {
        chart.gameObject.SetActive(false);
        if (Time.timeScale == 0) TogglePause(PauseState.Playing);
    }


    /// <summary>
    /// Starts specified Fungus dialog.
    /// </summary>
    /// <param name="dialog"></param>
    /// <param name="pause"></param>
    private void StartDialog(Flowchart dialog, bool pause=false)
    {
        if (pause) TogglePause(PauseState.Paused);
        dialog.gameObject.SetActive(true);
        dialog.SendFungusMessage("start");
    }



    // ************************************** Alert Network ***************************************
    /// <summary>
    /// Returns correct alert network object for enemy initialization.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public AlertEvent GetAlertNetwork(EnemyType type)
    {
        return type == EnemyType.Police ? policeEvent : pirateEvent;
    }
}

/// <summary>
/// Game paused states. Can be passed as value: Time.timeScale = (int)
/// </summary>
public enum PauseState
{
    Paused,
    Playing
}
