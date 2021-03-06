using UnityEngine;

public class TextCrawler : MonoBehaviour
{
    public float scrollSpeed = 20f;

    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 localVectorUp = transform.TransformDirection(0, 1, 0);

        pos += localVectorUp * scrollSpeed * Time.deltaTime;
        transform.position = pos;
    }
}
