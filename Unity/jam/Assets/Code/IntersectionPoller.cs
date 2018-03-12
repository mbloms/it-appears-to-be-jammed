using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

internal class IntersectionPoller
{
    private readonly Intersection target;

    //Quadrants to be acquired.
    private readonly bool q1;
    private readonly bool q2;
    private readonly bool q3;
    private readonly bool q4;

    private bool locked;
    private bool auto;
    private int time;
    
    private readonly Car car;
    private LinkedList<Car> current_queue;
    private LinkedList<Car> next_queue;

    public IntersectionPoller(Intersection target, Car car, string from, string to)
    {
        this.target = target;
        this.car = car;
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

    public LinkedList<Car> GetQueue()
    {
        return current_queue;
    }

    public bool AlreadyAcquired()
    {
        return locked;
    }

    public bool Acquire()
    {
        bool lock_acquired = target.Acquire(q1, q2, q3, q4);
        if (lock_acquired)
        {
            locked = true;
        }
        return lock_acquired;
    }

    public void Free()
    {
        if (locked)
        {
            locked = false;
            target.Free(q1,q2,q3,q4);
        }
    }
    /**
     * Lock in `time` iterations. 
     */
    public void Free(int time)
    {
        auto = true;
        this.time = time;
    }

    public void Update()
    {
        if (auto)
        {
            time--;
            if (time == 0) { Free(); }
        }
    }
}
