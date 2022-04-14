using UnityEngine;
using Variables;

public class MissionManagement : MonoBehaviour
{
    private Barge bargeHealthScript;
    private GameObject barge => GameManagement.Instance.Barge;


    [SerializeField, Tooltip("Mission Type ScriptableObject")] private IntVariable missionScriptable;
    [SerializeField, Tooltip("Mafia canvas health panel.")] private GameObject mafiaHealthPanel;
    [SerializeField, Tooltip("Commercial canvas health panel.")] private GameObject commercialHealthPanel;


    public MissionType missionType => (MissionType)missionScriptable.Value;



    private void SetupBarge()
    {
        barge.SendMessage("InitializeBarge");

        if (missionType == MissionType.Mafia)
        {
            bargeHealthScript = barge.GetComponent<MafiaBarge>();
            mafiaHealthPanel.SetActive(true);
            commercialHealthPanel.SetActive(false);
        }
        else
        {
            bargeHealthScript = barge.GetComponent<CommercialBarge>();
            commercialHealthPanel.SetActive(true);
            mafiaHealthPanel.SetActive(false);
        }
    }


    /// <summary>
    /// Switches the barge to the current type,
    /// reseting the health and setting up the UI canvas.
    /// </summary>
    /// <param name="mission"></param>
    public void SwitchBarge(MissionType mission)
    {
        missionScriptable.Value = (int)mission;
        SetupBarge();
    }
}
