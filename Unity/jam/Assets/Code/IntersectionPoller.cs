using System;
using System.Collections.Generic;
using UnityEngine;

internal class IntersectionPoller
{
    private Intersection target;

    //Quadrants to be acquired.
    private readonly bool q1;
    private readonly bool q2;
    private readonly bool q3;
    private readonly bool q4;

    public IntersectionPoller(Intersection target, string from, string to)
    {
        this.target = target;
        switch (from)
        {
            case "s":
            switch (to)
            {
                case "left":
                case "east":
                q4 = true;
                goto case "forward";
                case "forward":
                case "north":
                q3 = true;
                goto case "right";
                case "right":
                case "west":
                q2 = true;
                break;
            }
            break;

            case "v":
            switch (to)
            {
                case "left":
                case "south":
                q1 = true;
                goto case "forward";
                case "forward":
                case "east":
                q4 = true;
                goto case "right";
                case "right":
                case "north":
                q3 = true;
                break;
            }
            break;

            case "n":
            switch (to)
            {
                case "left":
                case "west":
                q2 = true;
                goto case "forward";
                case "forward":
                case "south":
                q1 = true;
                goto case "right";
                case "right":
                case "east":
                q4 = true;
                break;
            }
            break;

            case "e":
            switch (to)
            {
                case "left":
                case "north":
                q3 = true;
                goto case "forward";
                case "forward":
                case "west":
                q2 = true;
                goto case "right";
                case "right":
                case "south":
                q1 = true;
                break;
            }
            break;
        }
    }

    public bool Acquire()
    {
        return target.Acquire(q1,q2,q3,q4);
    }

    public void Free()
    {
        target.Free(q1,q2,q3,q4);
    }

    public void Update()
    {
        
    }
}
