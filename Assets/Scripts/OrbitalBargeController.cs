using UnityEngine;

public class OrbitalBargeController : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private OrbitalBody orbitalBody;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orbitalBody = GetComponent<OrbitalBody>();
    }

    private void Update()
    {
        rb2d.MovePosition(orbitalBody.Position);
    }
}