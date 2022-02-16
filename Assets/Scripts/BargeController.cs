using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BargeController : MonoBehaviour
{
    [HideInInspector] private Rigidbody2D rb2d;

    [Range(-1,1)]
    [SerializeField] private float movementCoefficient = -0.3f;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();       
    }

    void FixedUpdate()
    {
        Vector2 newPosition =
            rb2d.position +
            new Vector2(0, AsteroidField.instance.fieldSpeed * movementCoefficient * Time.deltaTime);

        rb2d.MovePosition(newPosition);
    }
}
