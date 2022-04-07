using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    [HideInInspector] public static TitleScreenManager Instance;


    private AsyncOperation loadOperation;
    private bool loadComplete = false;
    private TextMeshProUGUI playButtonText;
    private ProgressBar progressBar;


    [SerializeField, Tooltip("Settings flowchart.")]
    private Flowchart kitty;

    [SerializeField, Tooltip("Dialog call button. To be disabled when dialog open.")]
    private Button firstOfficerButton;


    [Header("Buttons")]
    [SerializeField, Tooltip("Button to be disabled until load complete.")]
    private Button playButton;

    [SerializeField, Tooltip("Button text color when enabled.")]
    private Color buttonEnabledTextColor;

    [SerializeField, Tooltip("Button text color when disabled.")]
    private Color buttonDisabledTextColor;

    [SerializeField, Tooltip("Loading indicator text.")]
    private TextMeshProUGUI loadingText;


    [Header("Blinking Text Settings")]
    [SerializeField]
    public float animSpeedInSec = 1f;



    // ********************************** Monobehaviours **********************************
    private void Awake()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        playButtonText = playButton.GetComponentInChildren<TextMeshProUGUI>();

        progressBar = GetComponent<ProgressBar>();
    }


    private void OnEnable()
    {
        playButton.interactable = false;
        playButtonText.color = buttonDisabledTextColor;
    }


    private void Start()
    {
        System.Action loadCompleteAction = new System.Action(LoadingComplete);

        loadOperation = SceneManager.LoadSceneAsync(1);
        loadOperation.allowSceneActivation = false;

        progressBar.StartLoading(loadOperation, loadCompleteAction);
        StartCoroutine(AnimateLoadingText());
    }



    // ********************************** Public Methods **********************************
    public void SeeKitty()
    {
        kitty.gameObject.SetActive(true);
        firstOfficerButton.interactable = false;
        kitty.SendFungusMessage("start");
    }


    public void ByeKitty()
    {
        StartCoroutine(FirstOfficerPause());
    }


    public void ExitGame()
    {
        Application.Quit();
    }


    public void PlayButtonClicked()
    {
        if (loadComplete)
        {
            loadOperation.allowSceneActivation = true;
        }
    }



    // ********************************** Private Methods *********************************
    private void LoadingComplete()
    {
        loadingText.enabled = false;
        playButton.interactable = true;
        playButtonText.color = buttonEnabledTextColor;
        loadComplete = true;
    }


    private IEnumerator FirstOfficerPause()
    {
        yield return new WaitForSeconds(0.5f);
        firstOfficerButton.interactable = true;
    }


    private IEnumerator AnimateLoadingText()
    {

        Color currentColor = loadingText.color;

        Color invisibleColor = loadingText.color;
        invisibleColor.a = 0; //Set Alpha to 0

        float oldAnimSpeedInSec = animSpeedInSec;
        float counter = 0;

        while (!loadComplete)
        {
            //Hide Slowly
            while (counter < oldAnimSpeedInSec)
            {
                if (loadComplete)
                {
                    yield break;
                }

                //Reset counter when Speed is changed
                if (oldAnimSpeedInSec != animSpeedInSec)
                {
                    counter = 0;
                    oldAnimSpeedInSec = animSpeedInSec;
                }

                counter += Time.deltaTime;
                loadingText.color = Color.Lerp(currentColor, invisibleColor, counter / oldAnimSpeedInSec);
                yield return null;
            }

            yield return null;


            //Show Slowly
            while (counter > 0)
            {
                if (loadComplete)
                {
                    yield break;
                }

                //Reset counter when Speed is changed
                if (oldAnimSpeedInSec != animSpeedInSec)
                {
                    counter = 0;
                    oldAnimSpeedInSec = animSpeedInSec;
                }

                counter -= Time.deltaTime;
                loadingText.color = Color.Lerp(currentColor, invisibleColor, counter / oldAnimSpeedInSec);
                yield return null;
            }
        }
    }
}
