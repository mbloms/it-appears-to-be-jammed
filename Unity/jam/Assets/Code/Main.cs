using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Main : MonoBehaviour {
    
    void Start () {
        List<int[]> intersections = ReadCSV(@"Assets/Data/intersections.csv");
        List<int[]> roads = ReadCSV(@"Assets/Data/roads.csv");

        // create the physical road network
        new GraphicalRoadnet(intersections, roads);

        // create the logical road network
        new LogicalRoadnet(intersections, roads);
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
		
	}
}
