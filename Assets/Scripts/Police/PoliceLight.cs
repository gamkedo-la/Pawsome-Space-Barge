using System;
using UnityEngine;

public class PoliceLight : MonoBehaviour
{
    public float rotationSpeed;
    public Vector3 rotationAxis = Vector3.up;

    public Gradient lightColor;
    public float lightCyclingSpeed = 1f;

    public Light spotLight;

    private float l;
    private Material material;
    private float intensity;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        intensity = material.GetColor(EmissionColor).maxColorComponent;
        // Debug.Log("Intensity: " + intensity);
    }

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed*Time.deltaTime);
        
        l = Mathf.Repeat(l + Time.deltaTime * lightCyclingSpeed, 1f);
        spotLight.color = lightColor.Evaluate(l);
        
        material.SetColor(EmissionColor, spotLight.color * intensity);
    }
}