using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Hello World!");
        
    }

    // Update is called once per frame
    public float rotateSpeed = 10;
    void Update()
    {
        transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);
    }
    
}
