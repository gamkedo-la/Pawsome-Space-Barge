using UnityEngine;
using Variables;

public class MissionTypeActivator : MonoBehaviour
{
    public IntVariable missionType;

    public MissionType activeMissionType;

    private void Awake()
    {
        if (missionType.Value != (int)activeMissionType)
        {
            gameObject.SetActive(false);
        }
    }
}