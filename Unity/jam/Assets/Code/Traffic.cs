using System;
using System.Collections.Generic;
using UnityEngine;

internal class Traffic : MonoBehaviour
{
    private List<Car> cars = new List<Car>();
    private bool ready = false;

    public void Init(LogicalRoadnet network)
    {
        // add cars
        cars.Add(new Car(network, true));
        int number_of_cars = 99;
        for (int i = 0; i < number_of_cars; i++)
        {
            cars.Add(new Car(network, false));
        }
        // traffic is ready 
        ready = true;

        // init stalkercam
        StalkerCam stalker = GameObject.Find("stalker").AddComponent(typeof(StalkerCam)) as StalkerCam;
        stalker.go = cars[0].model;
        stalker.ready = true;

        // init helicam
        HeliCam heli = GameObject.Find("heli").AddComponent(typeof(HeliCam)) as HeliCam;
        heli.go = cars[0].model;
        heli.ready = true;

        cars[0].debugging = true;
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