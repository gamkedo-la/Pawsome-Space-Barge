using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProgressBar : MonoBehaviour
{
    private AsyncOperation loadOperation;

    [SerializeField] private Slider progressBar;
    [SerializeField] private Button playButton;

    private float currentValue;
    private float targetValue;
    private bool loadComplete = false;

    [SerializeField, Range(0, 1)] private float progressMultiplier = 0.25f;


    private void Awake()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }
    // Start is called before the first frame update
    void Start()
    {
        progressBar.value = currentValue = targetValue = 0;

        loadOperation = SceneManager.LoadSceneAsync(1);

        loadOperation.allowSceneActivation = false;

        playButton.interactable = false;
        playButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        targetValue = loadOperation.progress / 0.9f;

        currentValue = Mathf.MoveTowards(currentValue, targetValue, progressMultiplier * Time.deltaTime);

        progressBar.value = currentValue;

        if (!loadComplete && Mathf.Approximately(currentValue, 1))
        {
            LoadComplete();
        }
    }


    private void LoadComplete()
    {
        playButton.gameObject.SetActive(true);
        playButton.interactable = true;
        Debug.Log("Scene load complete");
        loadComplete = true;
    }


    public void PlayButtonClicked()
    {
        Debug.Log("Play button clicked!");
        loadOperation.allowSceneActivation = true;
    }
}
