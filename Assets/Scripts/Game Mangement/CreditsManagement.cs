using UnityEngine;
using UnityEngine.SceneManagement;
using Fungus;
using TMPro;
using UnityEngine.UI;

public class CreditsManagement : MonoBehaviour
{
    [HideInInspector] public static CreditsManagement Instance;
    [SerializeField] private Flowchart allDone;
    [SerializeField] private TMP_Text combinedCreditsText;

    [SerializeField] private Text mafiaText;
    [SerializeField] private Text commercialText;
    [SerializeField] private Text creditsText;

    private SoundManagement soundManager;
    private string textSpacer = "\n\n\n\n\n\n\n\n\n\n";

#if UNITY_EDITOR
    [SerializeField] private GameSuccess successReason;
#endif



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

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 1;
    }

    private void Start()
    {

    #if UNITY_EDITOR
        if (SoundManagement.Instance == null)
        {
            Instantiate(Resources.Load("SoundManager", typeof(GameObject)), transform.position, Quaternion.identity);

            soundManager = SoundManagement.Instance;
            soundManager.successReason = successReason;
        }
    #endif

        if (SoundManagement.Instance.successReason == GameSuccess.Mafia)
        {
            combinedCreditsText.text = mafiaText.text + textSpacer + creditsText.text;
        }
        else // GameSuccess.Commercial
        {
            combinedCreditsText.text = commercialText.text + textSpacer + creditsText.text;
        }
    }


    public void CreditsDone()
    {
        allDone.gameObject.SetActive(true);
        allDone.SetIntegerVariable("completionMethod", SoundManagement.Instance.successReason == GameSuccess.Mafia ? 0 : 1);
        allDone.SendFungusMessage("start");
    }


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
        // ResumeGame(); // cleanup, else game is paused with no recourse.
        SceneManager.LoadScene(0);
    }
}
