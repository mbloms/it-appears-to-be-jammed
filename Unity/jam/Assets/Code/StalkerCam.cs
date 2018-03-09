using UnityEngine;

internal class StalkerCam : MonoBehaviour
{
    public GameObject go;
    public bool ready = false;
 
    void Update()
    {
        if (ready) {
            transform.position = go.transform.position + new Vector3(-1.0f, 0.5f, 0.0f);
        }
    }
}