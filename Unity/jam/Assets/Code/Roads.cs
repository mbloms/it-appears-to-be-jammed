using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Roads : MonoBehaviour {

    // Use this for initialization
    public float roadWidth = 1;
    public float roadThickness = 0.5f;
    void Start () {
        // draw intersections
        List<int[]> intersec = ReadCSV( @"Assets/Data/intersections.csv");
        foreach (int[] elem in intersec) {
            Intersection(elem[0], elem[1]);
        }

        // draw roads
        List<int[]> roads = ReadCSV(@"Assets/Data/roads.csv");
        foreach (int[] elem in roads)
        {
            Road(elem[0], elem[1], elem[2], elem[3]);
        }
    }

    List<int[]> ReadCSV(string path) {
        using (var reader = new StreamReader(path))
        {
            List<int[]> elements = new List<int[]>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                int[] values = Array.ConvertAll(line.Split(','), s => int.Parse(s));
                elements.Add(values);
            }
            return elements;
        }
    }

    void Intersection(float x, float z) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().material.color = Color.red;
        cube.transform.position = new Vector3(x, 0, z);
        cube.transform.localScale = new Vector3(roadWidth, roadThickness, roadWidth);
    }

    void Road(float x1, float z1, float x2, float z2) {
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
