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
    public GameObject Barge => barge;
    private OrbitalBody bargeOrbitalBody;
    public OrbitalBody BargeOrbitalBody => bargeOrbitalBody;

    // pause state switch
    private bool gamePaused = false;

    // dialog state switch
    private bool dialogActive = false;

    /// <summary> List of enemies currently pursuing barge. </summary>
    private HashSet<GameObject> lockedOnEnemies = new HashSet<GameObject>();



    // ****************************** Management Scripts Accessors ********************************
    private PlayerInputManager playerInputManager;
    public PlayerInputManager InputManager => playerInputManager;
    private CameraManagement cameraManager;
    public CameraManagement CameraManager => cameraManager;
    private SoundManagement soundManager = SoundManagement.Instance;
    public SoundManagement SoundManager => soundManager;
    private MissionManagement missionManager;
    public MissionManagement MissionManager => missionManager;


    [Header("Success Conditions")]
     // 2400000 means 3 missions at 80% health, 4 at 60%, this feels fair and less certain than 
    [SerializeField, Tooltip("Earnings to trigger commercial success ending.")]
    private int commecialSuccess = 2400000;

    [SerializeField, Tooltip("Number of mafia barges delivered to trigger mafia success.")]
    private int mafiaSuccess = 3;



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
    [ReadOnly] public int enemyContactsCount = 0;


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
    }

    private void OnEnable()
    {
        
    #if UNITY_EDITOR
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<PlayerSettings>();
        }
    #else
        settings = DataUtilities.LoadPlayerData();
    #endif

        playerInputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponent<CameraManagement>();

    #if UNITY_EDITOR
        if (SoundManagement.Instance == null)
        {
            Instantiate(Resources.Load("SoundManager", typeof(GameObject)), transform.position, Quaternion.identity);

            soundManager = SoundManagement.Instance;
        }
#endif

        missionManager = GetComponent<MissionManagement>();

        InitializeUI();

        if (SoundManagement.Instance != null) SoundManager.SetAmbientSound(null);

        InputManager.DisableJoining();
    }


    private void OnDisable()
    {
        SavePlayerSettings();
    }


    void Start()
    {
        barge = GameObject.FindGameObjectWithTag("Barge");
        bargeOrbitalBody = barge.GetComponent<OrbitalBody>();

        // gameplay
        StartMission();

        // for screenshot
        // DisableGameUI();
        // CameraManager.ToggleDialogCamera();
    }


    /// <summary>
    /// Ensure UI is in correct initial state, or use to reset UI display.
    /// </summary>
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

    #if UNITY_EDITOR
        // stop the editor playmode
        UnityEditor.EditorApplication.isPlaying = false;
    #elif UNITY_WEBGL
        // reload itch.io page
        Application.OpenURL("https://esklarski.itch.io/pawsome-space-barge");
    #else
        // close application
        Application.Quit();
    #endif

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


    /// <summary>
    /// Check player game completion condtions.
    /// </summary>
    private void GameCompletionCheck()
    {
        if  (settings.commercialEarnings >= commecialSuccess)
        {
            // trigger game end sequence

            Debug.Log("Commercial success!");
        }

        if (settings.mafiaDeliveries >= mafiaSuccess)
        {
            // trigger game end sequence

            Debug.Log("A force to be feared...");
        }
    }


    // Called from mission success panel. Maybe unneccessary...
    public void NextMission()
    {
        // TODO mission setup

        // for now just reload scene
        RestartMission();
    }



    // ***************************************** Dialog *******************************************
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


    /// <summary> Starts appropriate intro dialog on scene load. </summary>
    private void StartMission()
    {
        CameraManager.ToggleDialogCamera();

        if (settings.firstRun)
        {
            StartCoroutine(RunDialog(tutorial, 2));
        }
        else
        {
            mission.SetBooleanVariable("tooEasy", settings.tooEasy);
            mission.SetBooleanVariable("playerSelectBarge", settings.playerSelectBarge);
            mission.SetBooleanVariable("mafiaMad", settings.mafiaMad);

            StartCoroutine(RunDialog(mission, 2f));
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

        SavePlayerSettings();

        missionManager.SwitchBarge(mafiaMad ? MissionType.Commercial : MissionType.Mafia);

        InputManager.EnableJoining();

        CameraManager.ToggleDialogCamera();
    }


    /// <summary>
    /// Wraps up mission dialog. Takes a 0 for Mafia mission, 1 for Commercial mission.
    /// </summary>
    /// <param name="missionType"></param>
    public void MissionDialogDone(int missionType, bool tooEasy)
    {
        DialogDone(mission);

        settings.tooEasy = tooEasy;

        SavePlayerSettings();

        missionManager.SwitchBarge((MissionType)missionType);

        InputManager.EnableJoining();

        CameraManager.ToggleDialogCamera();
    }


    /// <summary> Called when mission is failed. </summary>
    public void MissionFailed()
    {
        Debug.Log("Mission Failed.");

        TogglePause(PauseState.Paused);

        DisableGameUI();

        // enable panel
        missionFailPanel.SetActive(true);

        // Update player settings
        settings.bargesLost++;

        // save player data
        SavePlayerSettings();

        // start pause dialog
        StartDialog(missionFail, false);
    }


    /// <summary> Called when barge hits delivery window. </summary>
    public void MissionSuccess()
    {
        Debug.Log("Mission Accomplished!");

        TogglePause(PauseState.Paused);

        DisableGameUI();

        // // enable panel
        missionSuccessPanel.SetActive(true);

        // update player settings
        settings.bargesDelivered++;

        if (missionManager.missionType == MissionType.Mafia)
        {
            settings.mafiaDeliveries++;
        }
        else
        {
            settings.commercialEarnings += missionManager.bargeHealth * 10;
        }

        // save player data
        SavePlayerSettings();

        GameCompletionCheck();

        // // start pause dialog
        StartDialog(missionSuccess, false);
    }


    /// <summary> Starts dialog after delay. </summary>
    private IEnumerator RunDialog(Flowchart dialog, float delay)
    {
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


    /// <summary>
    /// Saves settings data to disk.
    /// </summary>
    private void SavePlayerSettings()
    {

    #if !UNITY_EDITOR
        settings.SaveGame();
    #endif

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
