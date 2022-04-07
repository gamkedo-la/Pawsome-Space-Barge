using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private float currentValue;
    private float targetValue;
    private bool loadComplete = false;



    [Header("Progress Bar")]
    [SerializeField, Tooltip("Slider to adjust as progress bar.")]
    private Slider progressBar;

    [SerializeField, Range(0, 1), Tooltip("Multiplier for bar progress speed.")]
    private float progressMultiplier = 0.25f;



    private void OnEnable()
    {
        progressBar.value = currentValue = targetValue = 0;
        loadComplete = false;
    }


    public void StartLoading(AsyncOperation operation, System.Action callback)
    {
        StartCoroutine(RunProgressBar(callback, operation));
    }


    private IEnumerator RunProgressBar(System.Action callback, AsyncOperation operation)
    {
        while (!loadComplete)
        {
            yield return null;

            targetValue = operation.progress / 0.9f;

            currentValue = Mathf.MoveTowards(currentValue, targetValue, progressMultiplier * Time.deltaTime);

            progressBar.value = currentValue;

            if (Mathf.Approximately(currentValue, 1))
            {
                loadComplete = true;
            }
        }

        callback();
        yield break;
    }
}
