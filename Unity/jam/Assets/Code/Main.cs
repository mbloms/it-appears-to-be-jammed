using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Main : MonoBehaviour {

    private static float deltaTime = 0.0f;
    public static float FPS = 30.0f;
    
    void Start () {
        List<int[]> intersections = ReadCSV(@"Assets/Data/intersections.csv");
        List<int[]> roads = ReadCSV(@"Assets/Data/roads.csv");

        // create the physical road network
        GraphicalRoadnet graphics = new GraphicalRoadnet(intersections, roads);

        // create the logical road network
        LogicalRoadnet network = new LogicalRoadnet(intersections, roads);

        // create traffic object that runs the cars in the network
        Traffic traffic = this.gameObject.AddComponent<Traffic>();
        traffic.Init(network);
    }

    // return rows of csv values as a list, where each row is represented as an array of ints
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
	
	// Update is called once per frame
	void Update () {
        /** FPS calculation inspired by: http://wiki.unity3d.com/index.php?title=FramesPerSecond */
        //deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //FPS = 1.0f / deltaTime;
	    //Debug.Log("FPS: " + FPS);
	}
}
