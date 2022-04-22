using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGUI : MonoBehaviour
{
    public float secondsUntilItVanishes = 5f;
    
    void Start()
    {
        StartCoroutine(HideLater());
    }
    IEnumerator HideLater()
    {
        yield return new WaitForSeconds(secondsUntilItVanishes);
        Debug.Log("hiding tutorial.");
        gameObject.SetActive(false);
    }

    public void tutorialPopup() {
        Debug.Log("displaying tutorial...");
        gameObject.SetActive(true);
        StartCoroutine(HideLater());
    }

}
