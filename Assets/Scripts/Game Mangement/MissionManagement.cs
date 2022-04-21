using UnityEngine;
using Variables;

public class MissionManagement : MonoBehaviour
{
    private Barge bargeHealthScript;
    private GameObject barge => GameManagement.Instance.Barge;
    private OrbitalRigidbody orrb;

    [Header("Mission Variables")]
    [SerializeField, Tooltip("Mission Type ScriptableObject")] private IntVariable missionScriptable;

    [Header("Barge UI Panels")]
    [SerializeField, Tooltip("Mafia canvas health panel.")] private GameObject mafiaHealthPanel;
    [SerializeField, Tooltip("Commercial canvas health panel.")] private GameObject commercialHealthPanel;

    [Header("Enemies")]
    [SerializeField, Tooltip("Orbital Purrtrol parent object.")] private GameObject orbitalPurrtrol;
    [SerializeField, Tooltip("Pirate fleet parent object.")] private GameObject pirateFleet;
    [SerializeField, Tooltip("Pirate damage per second")] private float pirateDamage = 1f;
    [SerializeField, Tooltip("Pirate damage per second")] private float purrtrolForce = 500f;


    public MissionType missionType => (MissionType)missionScriptable.Value;
    public int bargeHealth => bargeHealthScript.bargeHealth.Value;


    private void Awake()
    {
        mafiaHealthPanel.SetActive(false);
        commercialHealthPanel.SetActive(false);
    }


    private static float distanceCheckInterval = 2;
    private float distanceCheckTimer = 0;
    private void Update()
    {
        if (GameManagement.Instance.enemyContactsCount > 0)
        {
            if (missionType == MissionType.Commercial)
            {
                float damage = pirateDamage * Time.deltaTime * GameManagement.Instance.enemyContactsCount;
                if (bargeHealthScript != null) bargeHealthScript.ApplyDamage(damage);
            }
            else // MissionType.Mafia
            {
                if (orrb != null || GameManagement.Instance.Barge.TryGetComponent<OrbitalRigidbody>(out orrb))
                {
                    float force = purrtrolForce * Time.deltaTime * GameManagement.Instance.enemyContactsCount;
                    orrb.AddEnemyForce(force);
                }
            }
        }

        if (!GameManagement.Instance.DialogActive)
        {
            distanceCheckTimer += Time.deltaTime;

            if (distanceCheckTimer > distanceCheckInterval)
            {
                distanceCheckTimer -= distanceCheckInterval;

                if (Vector2.Distance(GameManagement.Instance.Barge.transform.position, Vector2.zero) > GameManagement.Instance.BargeOrbitalBody.MaxOrbitRadius * 0.8f)
                {
                    Debug.Log("Barge pushed out of orbit.");
                    GameManagement.Instance.MissionFailed();
                }
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
