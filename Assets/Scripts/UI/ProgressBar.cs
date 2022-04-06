using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    private AsyncOperation loadOperation;

    [SerializeField] private Slider progressBar;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject loadingTextObj;
    private TextMeshPro loadingText;

    private float currentValue;
    private float targetValue;
    private bool loadComplete = false;

    [SerializeField, Range(0, 1)] private float progressMultiplier = 0.25f;


    private void Awake()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }


    private void Start()
    {
        loadingText = loadingTextObj.GetComponent<TextMeshPro>();
        progressBar.value = currentValue = targetValue = 0;

        loadOperation = SceneManager.LoadSceneAsync(1);

        loadOperation.allowSceneActivation = false;

        playButton.interactable = false;
        playButton.gameObject.SetActive(false);
    }


    private void Update()
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
        loadingTextObj.SetActive(false);
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
