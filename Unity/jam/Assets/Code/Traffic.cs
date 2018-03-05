using System;
using System.Collections.Generic;
using UnityEngine;

internal class Traffic : MonoBehaviour
{
    private List<Car> cars = new List<Car>();
    private bool ready = false;

    public void Init(LogicalRoadnet network)
    {
        cars.Add(new Car(network));
        ready = true;
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