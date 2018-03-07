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
                case "right":
                break;
                case "forward":
                break;
                case "left":
                break;
            }
            break;

            case "v":
            switch (to)
            {
                case "right":
                break;
                case "forward":
                break;
                case "left":
                break;
            }
            break;

            case "n":
            switch (to)
            {
                case "right":
                break;
                case "forward":
                break;
                case "left":
                break;
            }
            break;

            case "e":
            switch (to)
            {
                case "right":
                break;
                case "forward":
                break;
                case "left":
                break;
            }
            break;
        }
    }

    public void Update()
    {
        if (ready)
        {
            foreach (Car car in cars)
            {
                car.Drive();
            }
        }
    }
}