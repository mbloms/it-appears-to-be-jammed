using System;
using System.Collections.Generic;
using UnityEngine;

internal class IntersectionPoller
{
    private readonly Intersection target;

    //Quadrants to be acquired.
    private readonly bool q1;
    private readonly bool q2;
    private readonly bool q3;
    private readonly bool q4;
    private readonly Car car;
    private LinkedList<Car> current_queue;
    private LinkedList<Car> next_queue;

    public IntersectionPoller(Intersection target, Car car, string from, string to)
    {
        this.target = target;
        this.car = car;
        switch (to)
        {
            case "south":
                next_queue = target.getSouth().NQ;
                break;
            case "west":
                next_queue = target.getWest().EQ;
                break;
            case "north":
                next_queue = target.getNorth().SQ;
                break;
            case "east":
                next_queue = target.getEast().WQ;
                break;                
        }
        switch (from)
        {
            case "south":
                current_queue = target.SQ;
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

            case "west":
                current_queue = target.WQ;
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

            case "north":
                current_queue = target.NQ;
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

            case "east":
                current_queue = target.EQ;
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

    public LinkedList<Car> GetQueue()
    {
        return current_queue;
    }

    public bool Acquire()
    {
        if (target.Acquire(q1, q2, q3, q4))
        {
            next_queue.AddLast(car);
            return true;
        }
        return false;
    }

    public void Free()
    {
        target.Free(q1,q2,q3,q4);
        current_queue.Remove(car);
    }

    public void Update()
    {
        
    }
}
