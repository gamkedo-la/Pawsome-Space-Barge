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

    private void OnEnable()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        cameraManager = GetComponent<CameraManagement>();
        soundManager = GetComponent<SoundManagement>();
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
        Debug.Log("Pausing game.");

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
        Debug.Log("Resuming game.");

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
}


/// <summary>
/// Game paused states. Can be passed as value: Time.timeScale = (int)
/// </summary>
public enum PauseState
{
    Paused,
    Playing
}
