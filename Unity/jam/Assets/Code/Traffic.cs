using System;
using System.Collections.Generic;
using UnityEngine;

internal class Traffic : MonoBehaviour
{
    private List<Car> cars = new List<Car>();
    private bool ready = false;
    private GameObject marker, marker_large;

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

        // set up the marker over car #0
        marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.GetComponent<Renderer>().material.color = Color.magenta;
        marker.transform.localScale = new Vector3(GraphicalRoadnet.roadWidth / 6, GraphicalRoadnet.roadWidth / 6, GraphicalRoadnet.roadWidth / 6);
        marker_large = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker_large.GetComponent<Renderer>().material.color = Color.magenta;
        marker_large.transform.localScale = new Vector3(GraphicalRoadnet.roadWidth * 2, GraphicalRoadnet.roadWidth * 2, GraphicalRoadnet.roadWidth * 2);
    }

    public void Update()
    {
        if (ready)
        {
            foreach (Car car in cars)
            {
                car.Drive();
            }

            // move marker
            Vector3 marker_position = cars[0].model.transform.position;
            marker_position.y = 0.25f;
            marker.transform.position = marker_position;
            marker_position.y = 5.0f;
            marker_large.transform.position = marker_position;
        }
    }
}