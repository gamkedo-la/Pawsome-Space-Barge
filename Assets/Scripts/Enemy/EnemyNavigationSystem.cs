using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNavigationSystem : MonoBehaviour
{
    private GameObject barge;
    public GameObject Barge => barge;

    [Tooltip("Maximum range at which the barge can be detected")]
    [SerializeField][Min(0f)] private float bargeDetectionRange = 500f;

    [Tooltip("Maximum range at which the barge can be boarded/pushed")]
    [SerializeField] [Min(0f)] private float bargeContactRange = 20f;

    [Tooltip("Required time to complete a scan for the barge")]
    [SerializeField] [Min(0.1f)] private float scanInterval = 3f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float asteroidStunTime = 7f;

    [SerializeField] [Min(0)] [Tooltip("Stun time, in seconds.")]
    float playerStunTime = 0f;

    private Rigidbody2D rb2d;

    private float timer = 0;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }


    void Start()
    {
        StartCoroutine(ScanForBarge());
    }


    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }


    public bool IsBargeContact()
    {
        return barge != null && Vector3.Distance(transform.position, barge.transform.position) <= bargeContactRange;
    }


    private IEnumerator ScanForBarge()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanInterval);
            barge = GameObject.FindGameObjectWithTag("Barge");
            if (barge == null) continue;
            
            if (
                Vector3.Distance(transform.position, barge.transform.position) > bargeDetectionRange
                || timer > 0
            )
            {
                barge = null;
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (timer <= 0)
        {
            if (other.gameObject.CompareTag("Asteroid"))
            {
                timer = asteroidStunTime;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                timer = playerStunTime;
            }
        }
    }
}
