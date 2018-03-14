using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

internal class IntersectionPoller
{
    private readonly Intersection target;

    //Quadrants to be acquired.
    private bool q1;
    private bool q2;
    private bool q3;
    private bool q4;

    private bool locked;

    public IntersectionPoller(Intersection target, string from, string to)
    {
        this.target = target;
        switch (from)
        {
            case "north":
            switch (to)
            {
                case "left":
                case "east":
                q4 = true;
                goto case "forward";
                case "forward":
                case "south":
                q3 = true;
                goto case "right";
                case "right":
                case "west":
                q2 = true;
                break;
            }
            break;

            case "west":
            switch (to)
            {
                case "left":
                case "north":
                q1 = true;
                goto case "forward";
                case "forward":
                case "east":
                q4 = true;
                goto case "right";
                case "right":
                case "south":
                q3 = true;
                break;
            }
            break;

            case "south":
            switch (to)
            {
                case "left":
                case "west":
                q2 = true;
                goto case "forward";
                case "forward":
                case "north":
                q1 = true;
                goto case "right";
                case "right":
                case "east":
                q4 = true;
                break;
            }
            break;

            case "east":
            switch (to)
            {
                case "left":
                case "south":
                q3 = true;
                goto case "forward";
                case "forward":
                case "west":
                q2 = true;
                goto case "right";
                case "right":
                case "north":
                q1 = true;
                break;
            }
            break;
        }
    }

    public bool Acquire()
    {
        if (target.Acquire(q1, q2, q3, q4))
        {
            locked = true;
        }
        return locked;
    }

    public void Free()
    {
        if (locked)
        {
            locked = false;
            target.Free(q1,q2,q3,q4);
        }
    }

    public void FreePartial(bool f1, bool f2, bool f3, bool f4)
    {
        if (!locked) return;
        
        target.Free(q1 && f1, q2 && f2, q3 && f3, q4 && f4);
        
        if (f1) {q1 = false;}
        if (f2) {q2 = false;}
        if (f3) {q3 = false;}
        if (f4) {q4 = false;}
        
        if (!(q1||q2||q3||q4)){
            locked = false;
        }
    }
}
