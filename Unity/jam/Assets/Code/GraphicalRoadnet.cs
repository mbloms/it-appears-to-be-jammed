using System.Collections.Generic;
using UnityEngine;

public class GraphicalRoadnet {

    public static float roadWidth = 0.5f;
    public static float roadThickness = 0.001f;
    private static float lineWidth = 0.05f * roadWidth;

    private static Color color_tarmac = new Color(0.15f, 0.15f, 0.15f);

    public GraphicalRoadnet(List<int[]> intersec, List<int[]> roads)
    {
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
        GameObject edges = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edges.GetComponent<Renderer>().material.color = Color.white;
        edges.transform.position = new Vector3(x, 0, z);
        edges.transform.localScale = new Vector3(roadWidth * 1.05f, roadThickness * 1.2f, roadWidth * 1.05f);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Renderer>().material.color = color_tarmac;
        cube.transform.position = new Vector3(x, 0, z);
        cube.transform.localScale = new Vector3(roadWidth - lineWidth, roadThickness*1.4f, roadWidth - lineWidth);
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
        float segments = Mathf.Max(Mathf.Abs(x1 - x2), Mathf.Abs(z1 - z2));

        for (int i = 0; i*roadWidth < segments; i++) {
            float xpos = x1 + i * xdir * roadWidth + roadWidth * xdir;
            float zpos = z1 + i * zdir * roadWidth + roadWidth * zdir;

            // draw the road segment
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<Renderer>().material.color = color_tarmac;
            cube.transform.position = new Vector3(xpos, 0, zpos);
            cube.transform.localScale = new Vector3(roadWidth * 1.05f, roadThickness, roadWidth * 1.05f);

            // draw line in the middle of the segment
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.GetComponent<Renderer>().material.color = Color.white;
            line.transform.position = new Vector3(xpos, 0, zpos);
            line.transform.localScale = new Vector3(Mathf.Max(lineWidth * 6 * xdir, lineWidth), roadThickness * 1.3f, Mathf.Max(lineWidth * 6 * zdir, lineWidth));

        }
    }

}
