using System;
using System.Collections.Generic;
using UnityEngine;

internal class IntersectionPoller
{
    private Intersection target;

    //Quadrants to be acquired.
    private bool q1;
    private bool q2;
    private bool q3;
    private bool q4;

    public IntersectionPoller(string from, string to)
    {
        switch (from)
        {
            case "s":
            switch (to)
            {
                case "left":
                q4 = true;
                goto case "forward";
                case "forward":
                q3 = true;
                goto case "right";
                case "right":
                q2 = true;
                break;
            }
            break;

            case "v":
            switch (to)
            {
                case "left":
                q1 = true;
                goto case "forward";
                case "forward":
                q4 = true;
                goto case "right";
                case "right":
                q3 = true;
                break;
            }
            break;

            case "n":
            switch (to)
            {
                case "left":
                q2 = true;
                goto case "forward";
                case "forward":
                q1 = true;
                goto case "right";
                case "right":
                q4 = true;
                break;
            }
            break;

            case "e":
            switch (to)
            {
                case "left":
                q3 = true;
                goto case "forward";
                case "forward":
                q2 = true;
                goto case "right";
                case "right":
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