using UnityEngine;
using Variables;

public class MissionManagement : MonoBehaviour
{
    private Barge bargeHealthScript;
    private GameObject barge => GameManagement.Instance.Barge;


    [SerializeField, Tooltip("Mission Type ScriptableObject")] private IntVariable missionScriptable;
    [SerializeField, Tooltip("Mafia canvas health panel.")] private GameObject mafiaHealthPanel;
    [SerializeField, Tooltip("Commercial canvas health panel.")] private GameObject commercialHealthPanel;
    [SerializeField, Tooltip("Orbital Purrtrol parent object.")] private GameObject orbitalPurrtrol;
    [SerializeField, Tooltip("Pirate fleet parent object.")] private GameObject pirateFleet;


    public MissionType missionType => (MissionType)missionScriptable.Value;
    public int bargeHealth => bargeHealthScript.bargeHealth.Value;


    private void Awake()
    {
        mafiaHealthPanel.SetActive(false);
        commercialHealthPanel.SetActive(false);
    }


    [SerializeField, Tooltip("Pirate damage per second")] private float pirateDamage = 1f;
    private void Update()
    {
        if (GameManagement.Instance.enemyContactsCount > 0)
        {
            if (missionType == MissionType.Commercial)
            {
                float damage = pirateDamage * Time.deltaTime * GameManagement.Instance.enemyContactsCount;
                bargeHealthScript.ApplyDamage(damage);
            }
            else
            {
                // apply force
            }
        }
    }


    private void SetupMission()
    {
        barge.SendMessage("InitializeBarge");

        if (missionType == MissionType.Mafia)
        {
            bargeHealthScript = barge.GetComponent<MafiaBarge>();

            mafiaHealthPanel.SetActive(true);
            commercialHealthPanel.SetActive(false);

            orbitalPurrtrol.SetActive(true);
            pirateFleet.SetActive(false);
        }
        else
        {
            bargeHealthScript = barge.GetComponent<CommercialBarge>();

            commercialHealthPanel.SetActive(true);
            mafiaHealthPanel.SetActive(false);

            orbitalPurrtrol.SetActive(false);
            pirateFleet.SetActive(true);
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
        SetupMission();
    }
}
