using System.Collections.Generic;
using UnityEngine;

internal class LogicalRoadnet
{
    public List<Intersection> intersections;

    public LogicalRoadnet(List<int[]> intersection_data, List<int[]> road_data)
    {
        Intersection.Roadnet = this;
        // assign all intersections an id
        intersections = new List<Intersection>();
        for (int i = 0; i < intersection_data.Count; i++)
        {
            int[] coord = intersection_data[i];
            intersections.Add(new Intersection(new Vector3(coord[0], 0, coord[1])));
        }

        // add all roads as connections on each intersection
        foreach (int[] elem in road_data)
        {
            int inter_1 = -1; 
            int inter_2 = -1;

            // find the indices of both intersections in the road
            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].coordinates.x == elem[0] &&
                    intersections[i].coordinates.z == elem[1])
                {
                    inter_1 = i;
                }
                else
                if (intersections[i].coordinates.x == elem[2] && 
                    intersections[i].coordinates.z == elem[3])
                {
                    inter_2 = i;
                }
            }
            // all roads are bidirectional (and lead to rome)
            intersections[inter_1].AddConnection(inter_2);
            intersections[inter_2].AddConnection(inter_1);
        }
    }
}
