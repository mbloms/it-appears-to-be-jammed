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
                case "forward":
                q3 = true;
                case "right":
                q2 = true;
            }
            break;

            case "v":
            switch (to)
            {
                case "left":
                q1 = true;
                case "forward":
                q4 = true;
                case "right":
                q3 = true;
            }
            break;

            case "n":
            switch (to)
            {
                case "left":
                q2 = true;
                case "forward":
                q1 = true;
                case "right":
                q4 = true;
            }
            break;

            case "e":
            switch (to)
            {
                case "left":
                q3 = true;
                case "forward":
                q2 = true;
                case "right":
                q1 = true;
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