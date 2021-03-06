﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

internal class Intersection
{
    //Is the quadrant locked?
    private bool qne = false;
    private bool qnv = false;
    private bool qsv = false;
    private bool qse = false;

    public LinkedList<Car> NQ = new LinkedList<Car>();
    public LinkedList<Car> WQ = new LinkedList<Car>();
    public LinkedList<Car> SQ = new LinkedList<Car>();
    public LinkedList<Car> EQ = new LinkedList<Car>();

    public static LogicalRoadnet roadnet;

    public Vector3 coordinates;
    
    private Intersection north;
    private Intersection west;
    private Intersection south;
    private Intersection east;
    

    public Intersection(Vector3 vector3)
    {
        this.coordinates = vector3;
    }

    public IntersectionPoller getPoller(string from, string to)
    {
        return new IntersectionPoller(this, from, to);
    }

    public Intersection getNorth() {return north;}
    public Intersection getWest() {return west;}
    public Intersection getSouth() {return south;}
    public Intersection getEast() {return east;}
    
    /*
    Try to acquire locks for the specified quadrants.
    Arguments: true to acquire lock, false else.
    Returns: true if locks acquired, false else.
     */
    public bool Acquire(bool q1, bool q2, bool q3, bool q4)
    {
        if ( qne && q1
          || qnv && q2
          || qsv && q3
          || qse && q4)
        {
            return false;
        }
        qne |= q1;
        qnv |= q2;
        qsv |= q3;
        qse |= q4;
        return true;
    }
    /*
    Frees locks that are true in arguments.
     */
    public void Free(bool q1, bool q2, bool q3, bool q4)
    {
        if (q1) {qne = false;}
        if (q2) {qnv = false;}
        if (q3) {qsv = false;}
        if (q4) {qse = false;}
    }

    public void AddConnection(int connection_id)
    {
        Intersection connection = roadnet.intersections[connection_id];
        if (this.coordinates.x > connection.coordinates.x)
        {
            this.west = connection;
        }
        else
        if (this.coordinates.x < connection.coordinates.x)
        {
            this.east = connection;
        }
        else
        if (this.coordinates.z > connection.coordinates.z)
        {
            this.south = connection;
        }
        else
        if (this.coordinates.z < connection.coordinates.z)
        {
            this.north = connection;
        }
    }

    public LinkedList<Car> GetLane(Intersection source)
    {
        if (this.coordinates.x > source.coordinates.x)
        {
            return EQ;
        }
        else
        if (this.coordinates.x < source.coordinates.x)
        {
            return WQ;
        }
        else
        if (this.coordinates.z > source.coordinates.z)
        {
            return NQ;
        }
        else
        if (this.coordinates.z < source.coordinates.z)
        {
            return SQ;
        }
        throw new ArgumentException("Du har fel.");
    }

    public void Update()
    {
        
    }
}
