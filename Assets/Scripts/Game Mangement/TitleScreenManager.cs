using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private Flowchart kitty;
    [SerializeField] private Button firstOfficerButton;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    public IEnumerator FirstOfficerPause()
    {
        yield return new WaitForSeconds(0.5f);
        firstOfficerButton.interactable = true;
    }
}
