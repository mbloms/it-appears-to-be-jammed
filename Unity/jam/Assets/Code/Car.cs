using System.Collections.Generic;
using UnityEngine;

internal class Car
{
    private static string[] car_models = { @"car_1" };
    private static float scale_factor = 0.3f;
    //private static float right_lane_offset = 0.25f;
    private static float constant_speed = 0.05f;

    /* proof of concept starting point */
    private Vector3 position = new Vector3(0.0f, 0.175f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector2 direction = new Vector2(1, 0);

    private int destination_id;
    private Vector3 destination;

    public GameObject model;
    private LogicalRoadnet network;

    public Car(LogicalRoadnet network)
    {
        this.network = network;

        // pick a starting lane
        int source_id = Deterministic.random.Next(network.connection.Length);
        destination_id = network.connection[source_id][Deterministic.random.Next(network.connection[source_id].Count)];
        destination = network.intersection_coordinates[destination_id];

        // pick a car model
        int model_index = Deterministic.random.Next(car_models.Length);
        model = LoadPrefab(car_models[model_index]);

        // move car to starting position
        position = position + network.intersection_coordinates[source_id];
        Debug.Log("src: " + position);

        // set position
        ApplyLaneOffset();
        model.transform.position = position;
        model.transform.localScale = scale;
        UpdateDirection();

        Debug.Log("dest0: " + destination);
        Debug.Log(AtNextIntersection());
    }

    public void Drive()
    {
        if (AtNextIntersection())
        {
            position.x = destination.x;
            position.z = destination.z;
            model.transform.position = position;

            // take new path
            destination_id = network.connection[destination_id][Deterministic.random.Next(network.connection[destination_id].Count)];
            destination = network.intersection_coordinates[destination_id];

            Debug.Log("next: " + destination);
            UpdateDirection();
        }
        else
        {
            UpdatePosition();
        }
    }

    private bool AtNextIntersection()
    {
        // east
        if (position.x >= destination.x && direction.x == 1 && direction.y == 0)
        {
            return true;
        }
        // west
        if (position.x <= destination.x && direction.x == -1 && direction.y == 0)
        {
            return true;
        }
        // north
        if (position.z >= destination.z && direction.x == 0 && direction.y == 1)
        {
            return true;
        }
        // south
        if (position.z <= destination.z && direction.x == 0 && direction.y == -1)
        {
            return true;
        }
        return false;
    }

    private void UpdateDirection()
    {
        if (destination.x - GraphicalRoadnet.roadWidth >= position.x && destination.z == position.z)
        {
            // heading east
            direction.x = 1.0f;
            direction.y = 0.0f;
            model.transform.rotation = Quaternion.Euler(0, 90, 0);

        }
        else if (destination.x - GraphicalRoadnet.roadWidth <= position.x && destination.z == position.z)
        {
            // heading west
            direction.x = -1.0f;
            direction.y = 0.0f;
            model.transform.rotation = Quaternion.Euler(0, 270, 0);
        }

        if (destination.z - GraphicalRoadnet.roadWidth >= position.z && destination.x == position.x)
        {
            // heading north
            direction.x = 0.0f;
            direction.y = 1.0f;
            model.transform.rotation = Quaternion.Euler(0, 0, 0);

        }
        else if (destination.z - GraphicalRoadnet.roadWidth <= position.z && destination.x == position.x)
        {
            // heading south
            direction.x = 0.0f;
            direction.y = -1.0f;
            model.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void UpdatePosition()
    {
        // initially no change
        float x_modifier = 0.0f;
        float z_modifier = 0.0f;
        // determine what direction is increased
        if (direction.x == 0 && direction.y == 1) z_modifier = constant_speed;
        else if (direction.x == 0 && direction.y == -1) z_modifier = -constant_speed;
        else if (direction.x == 1 && direction.y == 0) x_modifier = constant_speed;
        else if (direction.x == -1 && direction.y == 0) x_modifier = -constant_speed;
        // update positional data
        position.x = position.x + x_modifier;
        position.z = position.z + z_modifier;
        //  update actual model
        model.transform.position = position;
    }

    private void ApplyLaneOffset()
    {
        // offset for driving on the *right* side of the road
        //position.x += right_lane_offset;
    }

    private GameObject LoadPrefab(string v)
    {
        var obj = Resources.Load(v);
        GameObject loadedPrefab = GameObject.Instantiate(obj) as GameObject;
        return loadedPrefab;
    }
    
}