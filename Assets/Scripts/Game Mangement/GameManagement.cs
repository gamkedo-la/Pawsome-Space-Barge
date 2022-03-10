using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class GameManagement : MonoBehaviour
{
    [SerializeField] private Flowchart tutorial;

    [SerializeField] private PlayerSettings settings;

    private void Awake()
    {
        if (settings == null)
        {
            settings = new PlayerSettings();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (tutorial != null)
        {
            var dialog = GameObject.Instantiate(tutorial, Vector3.zero, Quaternion.identity);
            dialog.SendFungusMessage("start");
            // dialog.ExecuteBlock("Tutorial Start");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
