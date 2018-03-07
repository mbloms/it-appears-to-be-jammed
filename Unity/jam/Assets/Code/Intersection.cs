using System.Collections.Generic;
using UnityEngine;

public class Intersection
{
    public Vector3 coordinates;
    public List<int> connections = new List<int>();
    private IntersectionPoller låck;

    public Intersection(Vector3 vector3)
    {
        this.coordinates = vector3;
    }
}