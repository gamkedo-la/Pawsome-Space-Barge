using UnityEngine;
using Variables;

public class MissionManagement : MonoBehaviour
{
    private Barge bargeHealthScript;
    private GameManagement gm => GameManagement.Instance;
    private GameObject barge => GameManagement.Instance.Barge;
    private OrbitalBody bargeOrbitalBody => GameManagement.Instance.BargeOrbitalBody;
    private OrbitalRigidbody bargeOrbitalRigidbody;
    private GameObject deliveryWindow => GameManagement.Instance.DeliveryWindow;
    private OrbitalBody deliveryWindowOrbitalBody => GameManagement.Instance.DeliveryWindowOrbitalBody;


    [Header("Mission Variables")]
    [SerializeField, Tooltip("Mission Type ScriptableObject")] private IntVariable missionScriptable;
    [SerializeField, Tooltip("Mission direction; inwards or outwards.")] private MissionDirection missionDirection;

    [Header("Barge UI Panels")]
    [SerializeField, Tooltip("Mafia canvas health panel.")] private GameObject mafiaHealthPanel;
    [SerializeField, Tooltip("Commercial canvas health panel.")] private GameObject commercialHealthPanel;
    // [SerializeField, Tooltip("Tutorial Tooltip.")] private GameObject tutorialTooltip;

    [Header("Enemies")]
    [SerializeField, Tooltip("Orbital Purrtrol parent object.")] private GameObject orbitalPurrtrol;
    [SerializeField, Tooltip("Pirate fleet parent object.")] private GameObject pirateFleet;
    [SerializeField, Tooltip("Pirate damage per second")] private float pirateDamage = 1f;
    [SerializeField, Tooltip("Pirate damage per second")] private float purrtrolForce = 500f;


    public MissionType MissionType => (MissionType)missionScriptable.Value;
    public MissionDirection Direction => missionDirection;
    public int bargeHealth => bargeHealthScript.bargeHealth.Value;


    private void Awake()
    {
        mafiaHealthPanel.SetActive(false);
        commercialHealthPanel.SetActive(false);
    }


    private void Update()
    {
        if (GameManagement.Instance.enemyContactsCount > 0)
        {
            if (MissionType == MissionType.Commercial)
            {
                float damage = pirateDamage * Time.deltaTime * GameManagement.Instance.enemyContactsCount;
                if (bargeHealthScript != null) bargeHealthScript.ApplyDamage(damage);
            }
            else // MissionType.Mafia
            {
                if (bargeOrbitalRigidbody != null || barge.TryGetComponent<OrbitalRigidbody>(out bargeOrbitalRigidbody))
                {
                    float force = purrtrolForce * Time.deltaTime * GameManagement.Instance.enemyContactsCount;
                    bargeOrbitalRigidbody.AddEnemyForce(force);
                }
            }
        }
    }


    private void SetupMission()
    {
        barge.SendMessage("InitializeBarge");

        // if (tutorialTooltip) tutorialTooltip.SetActive(true);

        if (MissionType == MissionType.Mafia)
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


    /// <summary> Valid inner orbital positions, at 6, 3, 12, 9 o'clock. </summary>
    private Vector2[] innerOrbitalPositions =
    {
        new Vector2( 0,    -2250),
        new Vector2( 2250,  0   ),
        new Vector2( 0,     2250),
        new Vector2(-2250,  0   )
    };

    /// <summary> Valid outer orbital positions, at 6, 3, 12, 9 o'clock.  </summary>
    private Vector2[] outerOrbitalPositions =
    {
        new Vector2( 0,    -6750),
        new Vector2( 6750,  0   ),
        new Vector2( 0,     6750),
        new Vector2(-6750,  0   )
    };


    /// <summary>
    /// Wrapper for setup of barge and delivery window locations and orbits.
    /// </summary>
    public void SetupOrbitalThings(bool firstRun=false)
    {
        if (firstRun)
        {
            missionDirection = MissionDirection.Inwards;
            SetOrbitalPositions(1);
            return;
        }

        if ( ( gm.MafiaDeliveries > 1 || gm.CommercialEarnings > 1600000 ) && !gm.PlayerSelectBarge )
        {
            missionDirection = MissionDirection.Outwards;
            SetOrbitalPositions();
        }
        else if (gm.PlayerSelectBarge)
        {
            int whatDirection = Random.Range(0,2);
            missionDirection = (MissionDirection)whatDirection;
            // missionDirection = MissionDirection.Inwards;

            int random = Random.Range(0, innerOrbitalPositions.Length);
            SetOrbitalPositions(random);
        }
        else
        {
            missionDirection = MissionDirection.Inwards;
            SetOrbitalPositions(1);
        }
    }


    /// <summary>
    /// Sets orbit for delivery window and barge.
    /// </summary>
    /// <param name="positionIndex">Position index for delivery window.</param>
    /// <param name="direction"></param>
    private void SetOrbitalPositions(int positionIndex=0)
    {
        if (missionDirection == MissionDirection.Inwards)
        {
            deliveryWindow.transform.position = innerOrbitalPositions[positionIndex];
            barge.transform.position = outerOrbitalPositions[0];

            Physics2D.SyncTransforms();

            deliveryWindowOrbitalBody.SetNewOrbit();
            bargeOrbitalBody.SetNewOrbit();
        }
        else // MissionDirection.Outwards
        {
            deliveryWindow.transform.position = outerOrbitalPositions[3];
            barge.transform.position = innerOrbitalPositions[0];

            Physics2D.SyncTransforms();

            deliveryWindowOrbitalBody.SetNewOrbit();
            bargeOrbitalBody.SetNewOrbit();
        }
    }
}


public enum MissionDirection
{
    Inwards,
    Outwards
}
