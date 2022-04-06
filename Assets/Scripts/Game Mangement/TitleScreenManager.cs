using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private Flowchart kitty;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SeeKitty()
    {   kitty.gameObject.SetActive(true);
        kitty.SendFungusMessage("start");
    }
}
