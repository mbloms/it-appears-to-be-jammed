using System.Collections.Generic;
using UnityEngine;

public class GraphicalRoadnet {

    public float roadWidth = 1;
    public float roadThickness = 0.5f;

    private List<int[]> intersec;
    private List<int[]> roads;

    public GraphicalRoadnet(List<int[]> intersec, List<int[]> roads)
    {
        this.intersec = intersec;
        this.roads = roads;

        // draw intersections
        foreach (int[] elem in intersec)
        {
            CreateIntersection(elem[0], elem[1]);
        }
        // draw roads
        foreach (int[] elem in roads)
        {
            CreateRoad(elem[0], elem[1], elem[2], elem[3]);
        }
    }

    // create the intersection-object
    void CreateIntersection(float x, float z) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().material.color = Color.red;
        cube.transform.position = new Vector3(x, 0, z);
        cube.transform.localScale = new Vector3(roadWidth, roadThickness, roadWidth);
    }

    // create the road objects
    void CreateRoad(float x1, float z1, float x2, float z2) {
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

}
