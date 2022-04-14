using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrbitalStats : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI positionX;
    [SerializeField] TextMeshProUGUI positionY;
    [SerializeField] TextMeshProUGUI velocityX;
    [SerializeField] TextMeshProUGUI velocityY;
    [SerializeField] TextMeshProUGUI velocityMag;

    [SerializeField] TextMeshProUGUI a;
    [SerializeField] TextMeshProUGUI e;
    [SerializeField] TextMeshProUGUI v;
    [SerializeField] TextMeshProUGUI w;
    [SerializeField] TextMeshProUGUI T;
    [SerializeField] TextMeshProUGUI rp;
    [SerializeField] TextMeshProUGUI ra;


    private OrbitalStatHolder stats;

    private void OnEnable()
    {
        stats = GameManagement.Instance.BargeOrbitalBody.GetOrbitalStats();

        positionX.text = $"X: {stats.PositionPci.x}";
        positionY.text = $"Y: {stats.PositionPci.y}";
        velocityX.text = $"X: {stats.VelocityPci.x}";
        velocityY.text = $"Y: {stats.PositionPci.y}";

        a.text = $"α: {stats.a}";
        e.text = $"ε: {stats.e}";
        v.text = $"ν: {stats.v}";
        w.text = $"ω: {stats.w}";
        T.text = $"Τ: {stats.T}";
        rp.text = $"rp: {stats.rp}";
        ra.text = $"ra: {stats.ra}";

        velocityMag.text = $"{stats.VelocityPci.magnitude} m/s²";
    }
}
