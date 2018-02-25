using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roads : MonoBehaviour {

    // Use this for initialization
    public float roadWidth = 1;
    public float roadThickness = 0.2f;
    void Start () {
        intersection(0, 0);
        intersection(0, 3);
        intersection(2, 3);
        road(0, 0, 0, 3);
        road(0, 3, 2, 3);
    }

    void intersection(float x, float z) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().material.color = Color.red;
        cube.transform.position = new Vector3(x, 0, z);
        cube.transform.localScale = new Vector3(roadWidth, roadThickness, roadWidth);
    }

    void road(float x1, float z1, float x2, float z2) {
        // determine the direction on the x-axis
        float xdir = 0;
        if (x1 < x2) { xdir = 1; }
        else if (x1 > x2) { xdir = -1; }

        // determine the direction on the z-axis
        float zdir = 0;
        if (z1 < z2) { zdir = 1; }
        else if (z1 > z2) { zdir = -1; }

        // calculate the number of road segments
        float segments = Mathf.Max(Mathf.Abs(x1 - x2), Mathf.Abs(z1 - z2)) / roadWidth - 1;

        for (int i = 0; i < segments; i++) {
            float xpos = x1 + i * xdir + roadWidth * xdir;
            float zpos = z1 + i * zdir + roadWidth * zdir;
            // draw the cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<Renderer>().material.color = Color.white;
            cube.transform.position = new Vector3(xpos, 0, zpos);
            cube.transform.localScale = new Vector3(roadWidth, roadThickness, roadWidth);
        } 
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
