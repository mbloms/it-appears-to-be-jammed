using UnityEngine;

internal class HeliCam : MonoBehaviour
{
    public GameObject go;
    public bool ready = false;

    void Update()
    {
        if (ready)
        {
            transform.position = go.transform.position + new Vector3(0.0f, 1.0f, 0.0f); ;
        }
    }
}