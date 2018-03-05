using System.Collections.Generic;
using UnityEngine;

internal class LogicalRoadnet
{
    public List<int>[] connection;
    public List<Vector2> intersection_coordinates;

    public LogicalRoadnet(List<int[]> intersection_data, List<int[]> road_data)
    {
        // assign all intersections an id
        Dictionary<string, int> intersection = new Dictionary<string, int>();
        intersection_coordinates = new List<Vector2>();
        for (int i = 0; i < intersection_data.Count; i++)
        {
            int[] coord = intersection_data[i];
            intersection[coord[0] + "," + coord[1]] = i;
            intersection_coordinates.Add(new Vector2(coord[1], coord[0]));
        }

        // initialize the connection list
        this.connection = new List<int>[intersection.Keys.Count];
        for (int i = 0; i < intersection.Keys.Count; i++)
        {
            connection[i] = new List<int>();
        }

        // add all roads as connections on each intersection
        foreach (int[] elem in road_data)
        {
            // add roads as two opposite unidirectional lanes
            int inter_1 = intersection[elem[0] + "," + elem[1]];
            int inter_2 = intersection[elem[2] + "," + elem[3]];
            this.connection[inter_1].Add(inter_2);
            this.connection[inter_2].Add(inter_1);
        }
    }
}