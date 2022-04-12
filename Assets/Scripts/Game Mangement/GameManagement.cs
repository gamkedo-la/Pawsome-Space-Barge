using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Fungus;

public class GameManagement : MonoBehaviour
{
    [HideInInspector] public static GameManagement Instance;

    // barge
    private GameObject barge;
    private OrbitalBody bargeOrbitalBody;
    public OrbitalBody BargeOrbitalBody => bargeOrbitalBody;

    // pause state switch
    private bool gamePaused = false;

    /// <summary> List of enemies currently pursuing barge. </summary>
    private HashSet<GameObject> lockedOnEnemies = new HashSet<GameObject>();



    // ****************************** Management Scripts Accessors ********************************
    private PlayerInputManager playerInputManager;
    public PlayerInputManager InputManager => playerInputManager;
    private CameraManagement cameraManager;
    public CameraManagement CameraManager => cameraManager;
    private SoundManagement soundManager;
    public SoundManagement SoundManager => soundManager;



    [Header("Fungus Flowcharts")]
    [SerializeField] private Flowchart tutorial;
    [SerializeField] private Flowchart mission;
    [SerializeField] private Flowchart warnings;
    [SerializeField] private Flowchart pauseDialog;
    [SerializeField] private Flowchart missionFail;
    [SerializeField] private Flowchart missionSuccess;

    [SerializeField] private bool pauseOnDialog = false;


    [Header("Player Settings")]
    [SerializeField] private PlayerSettings settings;


    [Header("Alert Networks")]
    [SerializeField] private AlertEvent policeEvent;
    [SerializeField] private AlertEvent pirateEvent;


    [Header("Overlays")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject missionFailPanel;
    [SerializeField] private GameObject missionSuccessPanel;

    [SerializeField, Tooltip("UI objects to disable when pause or mission failure overlays are active.")]
    private GameObject[] gameUI;



    // **************************************** Accessors *****************************************
    public Vector2 bargeVelocity => bargeOrbitalBody.Velocity;
    public bool GamePaused => gamePaused;



    // ************************************* Monobehaviours ***************************************
    private void Awake()
    {
        // setup singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // TODO: check for save data and load, else create new data
        // initialize player settings if not present
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PlayerSettings>();
        }
    }

    private void OnEnable()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponent<CameraManagement>();
        soundManager = GetComponent<SoundManagement>();
        InitializeUI();
    }


    private void OnDisable()
    {
        // save data
    }


    void Start()
    {
        barge = GameObject.FindGameObjectWithTag("Barge");
        bargeOrbitalBody = barge.GetComponent<OrbitalBody>();

        if (settings.firstRun)
        {
            StartCoroutine(RunDialog(tutorial, 2));
        }
    }


    // void Update()
    // {
    //     //
    // }


    private void InitializeUI()
    {
        // put flowcharts in order
        tutorial.gameObject.SetActive(false);
        // mission.gameObject.SetActive(false);
        missionFail.gameObject.SetActive(false);
        missionSuccess.gameObject.SetActive(false);
        pauseDialog.gameObject.SetActive(false);
        warnings.gameObject.SetActive(true);

        // ensure overlays are off
        pausePanel.SetActive(false);
        missionFailPanel.SetActive(false);
        missionSuccessPanel.SetActive(false);

        // enable gameplay ui elements
        EnableGameUI();

        // and in case of logic error, make sure game is running
        TogglePause(PauseState.Playing);
    }



    // ************************************** Game Controls ***************************************
    /// <summary>
    /// Exits game.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Exiting game.");
        Application.Quit();
    }


    /// <summary>
    /// Returns to title screen.
    /// </summary>
    public void ExitToTitle()
    {
        Debug.Log("Exiting to Title.");
        ResumeGame(); // cleanup, else game is paused with no recourse.
        SceneManager.LoadScene(0);
    }


    // TODO: 
    /// <summary>
    /// Restarts mission.
    /// </summary>
    public void RestartMission()
    {
        Debug.Log("Restarting mission.");
        ResumeGame(); // cleanup, else game is paused with no recourse.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    /// <summary>
    /// Secondary handler for OnPause input event. Passed context from ShipActions.cs
    /// </summary>
    /// <param name="context"></param>
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!gamePaused && !tutorial.IsActive())
        {
            PauseGame();
        }
    }


    /// <summary>
    /// Pauses game and disables extraneous objects.
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("Pausing game.");

        TogglePause(PauseState.Paused);

        DisableGameUI();

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
        Debug.Log("Resuming game.");

        EnableGameUI();

        // disable panel and dialog
        pausePanel.SetActive(false);

        if ( pauseDialog.IsActive() )
        {
            DialogDone(pauseDialog);
        }
        else if ( missionFail.IsActive() )
        {
            DialogDone(missionFail);
        }
        else if ( missionSuccess.IsActive() )
        {
            DialogDone(missionSuccess);
        }
        else
        {
            // fallback, reset UI
            InitializeUI();
        }
    }


    public void MissionFailed()
    {
        Debug.Log("Mission Failed.");

        TogglePause(PauseState.Paused);

        DisableGameUI();

        // enable panel
        missionFailPanel.SetActive(true);

        // start pause dialog
        StartDialog(missionFail, false);
    }


    public void MissionSuccess()
    {
        Debug.Log("Mission Accomplished!");

        TogglePause(PauseState.Paused);

        DisableGameUI();

        // // enable panel
        missionSuccessPanel.SetActive(true);

        // TODO save player data

        // // start pause dialog
        StartDialog(missionSuccess, false);
    }


    public void NextMission()
    {
        // TODO mission setup

        // for now just reload scene
        RestartMission();
    }



    // ***************************************** Dialog *******************************************
    bool dialogActive = false;


    /// <summary>
    /// Disable dialog object and resumes game if paused.
    /// </summary>
    /// <param name="chart"></param>
    public void DialogDone(Flowchart chart)
    {
        dialogActive = false;
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
        if (!dialogActive)
        {
            if (pause || pauseOnDialog) TogglePause(PauseState.Paused);
            dialogActive = true;
            dialog.gameObject.SetActive(true);
            dialog.SendFungusMessage("start");
        }
    }


    /// <summary>
    /// Switches firstRun off, and closes tutorial dialog.
    /// Called when tutorial dialog ends.
    /// </summary>
    public void TutorialDone(bool mafiaMad, bool playerSelectBarge, bool tooEasy)
    {
        DialogDone(tutorial);

        settings.mafiaMad = mafiaMad;
        settings.playerSelectBarge = playerSelectBarge;
        settings.tooEasy = tooEasy;

        settings.firstRun = false;
    }


    /// <summary>
    /// Starts dialog after delay.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunDialog(Flowchart dialog, float delay)
    {
        settings.Reset();
        yield return new WaitForSeconds(delay);
        StartDialog(dialog);
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


    /// <summary>
    /// Notify game management of enemy targeting.
    /// Adds pursuingEnemy to lockedOnEnemies List, and notifies sound manager.
    /// </summary>
    /// <param name="pursuingEnemy"></param>
    public void NotifyPursuit(GameObject pursuingEnemy, EnemyType type)
    {
        lockedOnEnemies.Add(pursuingEnemy);
        soundManager.SetPursuitAmbient();

        // these strings must match those in the flowchart, there is probably a more robust way to do this...
        string enemy = type == EnemyType.Police ? "police" : "pirate";

        warnings.SendFungusMessage(enemy);
    }


    /// <summary>
    /// Notify game management of lost target.
    /// Pusuing pursuingEnemy to lockedOnEnemies List, and notifies sound manager.
    /// </summary>
    /// <param name="pursuingEnemy"></param>
    public void CancelPursuit(GameObject evadedEnemy)
    {
        lockedOnEnemies.Remove(evadedEnemy);

        if (lockedOnEnemies.Count == 0)
        {
            soundManager.CancelPursuitAmbient();
        }
    }



    // **************************************** Utilities *****************************************
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


    /// <summary>
    /// Diables in game UI elements
    /// </summary>
    private void DisableGameUI()
    {
        foreach (var uiItem in gameUI)
        {
            uiItem.SetActive(false);
        }
    }


    /// <summary>
    /// Enables in game UI elements
    /// </summary>
    private void EnableGameUI()
    {
        foreach (var uiItem in gameUI)
        {
            uiItem.SetActive(true);
        }
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
