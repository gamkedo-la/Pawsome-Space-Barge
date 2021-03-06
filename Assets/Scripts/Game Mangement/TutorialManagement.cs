using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManagement : MonoBehaviour
{
    private GameManagement gm => GameManagement.Instance;

    [Header("Barge Hints")]
    [SerializeField, Tooltip("Side pointing arrows object.")] private GameObject sideArrows;
    [SerializeField, Tooltip("Fore-Aft pointing arrows object.")] private GameObject foreAftArrows;
    [SerializeField, Tooltip("Tutorial Tooltip.")] private GameObject bargeTip;

    [Header("Minimap Hints")]
    [SerializeField, Tooltip("Player minimap hint object.")] private GameObject playerTooltip;



    public void DisplayBargeTip()
    {
        bargeTip.SetActive(true);
    }



    public void DisplaySideArrows()
    {
        sideArrows.SetActive(true);
    }
    public void HideSideArrows()
    {
        sideArrows.SetActive(false);
    }


    public void DispalyForeAftArrrows()
    {
        foreAftArrows.SetActive(true);
    }
    public void HideForeAftArrrows()
    {
        foreAftArrows.SetActive(false);
    }


    public void DispalyPlayerTooltip()
    {
        playerTooltip.SetActive(true);
    }
    public void HidePlayerTooltip()
    {
        playerTooltip.SetActive(false);
    }
}
