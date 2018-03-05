using System.Collections.Generic;
using UnityEngine;

internal class Car
{
    private static string[] car_models = { @"car_1" };
    private static float scale_factor = 0.3f;
    private static float right_lane_offset = 0.25f;
    private static float constant_speed = 0.01f;

    /* proof of concept starting point */
    private Vector3 position = new Vector3(0.0f, 0.175f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector3 direction = new Vector3(0, 0, 1);

    private int destination_id;
    private Vector2 destination;

    private GameObject model;
    private LogicalRoadnet network;

    public Car(LogicalRoadnet network)
    {
        this.network = network;

        // pick a starting lane
        int source_id = Deterministic.random.Next(network.connection.Length);
        destination_id = network.connection[source_id][Deterministic.random.Next(network.connection[source_id].Count)];
        destination = network.intersection_coordinates[destination_id];
        Debug.Log(destination.x);
        Debug.Log(destination.y);

        // pick a car model
        int model_index = Deterministic.random.Next(car_models.Length);
        model = LoadPrefab(car_models[model_index]);

        // set position
        ApplyLaneOffset();
        model.transform.position = position;
        model.transform.localScale = scale;
        model.transform.Rotate(direction);

        Drive();
    }

    public void Drive()
    {
        if (AtIntersection())
        {
            position.x = destination.x;
            position.z = destination.y;
            // take new path
            destination_id = network.connection[destination_id][Deterministic.random.Next(network.connection[destination_id].Count)];
            destination = network.intersection_coordinates[destination_id];
            UpdateDirection();
        }
        UpdatePosition();
        UpdateModel();
    }

    private bool AtIntersection()
    {
        if (direction.x == 0 && direction.z == 1)
        {
            // heading north
            if (destination.y <= position.z) return true; 
        }
        else if (direction.x == 0 && direction.z == -1)
        {
            // heading south
            if (destination.y >= position.z) return true;
        }
        else if (direction.x == 1 && direction.z == 0)
        {
            // heading east
            if (destination.x <= position.x) return true;
        }
        else if (direction.x == -1 && direction.z == 0)
        {
            // heading west
            if (destination.x >= position.x) return true;
        }
        return false;
    }

    private void UpdateDirection()
    {
        if (destination.x >= position.x)
        {
            // heading east
            direction.x = 1.0f;
            direction.z = 0.0f;
        }
        else if (destination.x <= position.x)
        {
            // heading west
            direction.x = -1.0f;
            direction.z = 0.0f;
        }
        else if (destination.y >= position.z)
        {
            // heading north
            direction.x = 0.0f;
            direction.z = 1.0f;
        }
        else if (destination.y <= position.z)
        {
            // heading south
            direction.x = 0.0f;
            direction.z = -1.0f;
        }
    }

    private void UpdatePosition()
    {
        // initially no change
        float x_modifier = 0.0f;
        float z_modifier = 0.0f;
        // determine what direction is increased
        if (direction.x == 0 && direction.z == 1) z_modifier = constant_speed;
        else if (direction.x == 0 && direction.z == -1) z_modifier = -constant_speed;
        else if (direction.x == 1 && direction.z == 0) x_modifier = constant_speed;
        else if (direction.x == -1 && direction.z == 0) x_modifier = -constant_speed;
        // update positional data
        position.x = position.x + x_modifier;
        position.z = position.z + z_modifier;
    }

    private void UpdateModel()
    {
        model.transform.position = position;
        model.transform.Rotate(new Vector3(direction.x * 90, direction.y * 90, direction.z * 90 ));
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