using UnityEngine;

public struct OrbitalStatHolder
{
    public Vector2 PositionPci;
    public Vector2 VelocityPci;
    public float a;
    public float e;
    public float v;
    public float w;
    public float T;
    public float rp;
    public float ra;

    public OrbitalStatHolder(Vector2 pPci, Vector2 vPci, float a, float e, float v, float w, float T, float rp, float ra)
    {
        PositionPci = pPci;
        VelocityPci = vPci;
        this.a = a;
        this.e = e;
        this.v = v;
        this.w = w;
        this.T = T;
        this.rp = rp;
        this.ra = ra;
    }    
}
