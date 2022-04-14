using System.Collections;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    [SerializeField, Tooltip("How fast to flash, or max speed if randomized.")]
    private float flashSpeed = 1f;

    [SerializeField, Tooltip("Randomize speed between 0.1 and flashSpeed.")]
    private bool randomizeSpeed = false;

    [ReadOnly, SerializeField, Tooltip("Set in Awake()")]
    private TextMeshProUGUI flashingText;


    private void Awake()
    {
        flashingText = GetComponent<TextMeshProUGUI>();
    }


    private void OnEnable()
    {
        StartCoroutine(AnimateFlashingText());
    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }


    private IEnumerator AnimateFlashingText()
    {

        Color currentColor = flashingText.color;

        Color invisibleColor = flashingText.color;
        invisibleColor.a = 0; //Set Alpha to 0

        if (randomizeSpeed)
        {
            flashSpeed = Random.Range(0.1f, flashSpeed);
        }

        float oldFlashSpeed = flashSpeed;
        float counter = 0;

        while (true)
        {
            //Hide Slowly
            while (counter < oldFlashSpeed)
            {
                //Reset counter when Speed is changed
                if (oldFlashSpeed != flashSpeed)
                {
                    counter = 0;
                    oldFlashSpeed = flashSpeed;
                }

                counter += Time.unscaledDeltaTime;
                flashingText.color = Color.Lerp(currentColor, invisibleColor, counter / oldFlashSpeed);
                yield return null;
            }

            yield return null;


            //Show Slowly
            while (counter > 0)
            {
                //Reset counter when Speed is changed
                if (oldFlashSpeed != flashSpeed)
                {
                    counter = 0;
                    oldFlashSpeed = flashSpeed;
                }

                counter -= Time.unscaledDeltaTime;
                flashingText.color = Color.Lerp(currentColor, invisibleColor, counter / oldFlashSpeed);
                yield return null;
            }
        }
    }
}
