using System.Collections.Generic;
using UnityEngine;

internal class Car
{
    private static string[] car_models = { @"car_1", @"car_2", @"car_3", @"car_4", @"Police_car", @"Taxi" };
    private static float scale_factor = GraphicalRoadnet.roadWidth * 0.3f;
    private static float right_lane_offset = GraphicalRoadnet.roadWidth * 0.25f;

    private static float speed_scaler = 0.0005f; 
    private static float max_speed = 60.0f * speed_scaler;
    private static float acceleration = 6.0f * speed_scaler;
    private static float retardation = 20.0f * speed_scaler;
    private float speed = max_speed;

    /* proof of concept starting point */
    private Vector3 position = new Vector3(0.0f, GraphicalRoadnet.roadThickness + 0.055f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector2 direction = new Vector2(1, 0);

    private int source_id;
    private int destination_id;

    private Vector3 destination;

    public GameObject model;
    private LogicalRoadnet network;

    public Car(LogicalRoadnet network)
    {
        this.network = network;

        // pick a starting lane
        source_id = Deterministic.random.Next(network.connection.Length);
        destination_id = network.connection[source_id][Deterministic.random.Next(network.connection[source_id].Count)];
        destination = network.intersection_coordinates[destination_id];

        // pick a car model
        int model_index = Deterministic.random.Next(car_models.Length);
        model = LoadPrefab(car_models[model_index]);

        // move car to starting position
        position = position + network.intersection_coordinates[source_id];
        Debug.Log("src: " + position);

        // set position
        model.transform.position = position;
        model.transform.localScale = scale;
        UpdateDirection();

        Debug.Log("dest0: " + destination);
        Debug.Log(AtNextIntersection());
    }

    public void Drive()
    {
        //ChangeSpeed();

        if (AtNextIntersection())
        {
            position.x = destination.x;
            position.z = destination.z;

            int next = network.connection[destination_id][Deterministic.random.Next(network.connection[destination_id].Count)];
            while (next == source_id) {
                next = network.connection[destination_id][Deterministic.random.Next(network.connection[destination_id].Count)];
            }

            // take new path
            source_id = destination_id;
            destination_id = next;
            destination = network.intersection_coordinates[destination_id];

            Debug.Log("next: " + destination);
            UpdateDirection();
            model.transform.position = position;
        }
        else
        {
            UpdatePosition();
        }
    }

	private void ChangeSpeed(int target_speed)
    {
        Accelerate();
		if (speed < target_speed)
		{
			speed = Mathf.Min(speed + acceleration * target_speed, target_speed);
		}
		else if(speed > target_speed)
		{
			speed = Mathf.Max(speed + retardation * target_speed.CompareTo, target_speed);
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
        else if (destination.x + GraphicalRoadnet.roadWidth <= position.x && destination.z == position.z)
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
        else if (destination.z + GraphicalRoadnet.roadWidth <= position.z && destination.x == position.x)
        {
            // heading south
            direction.x = 0.0f;
            direction.y = -1.0f;
            model.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        ApplyLaneOffset();
    }

    private void UpdatePosition()
    {
        // initially no change
        float x_modifier = 0.0f;
        float z_modifier = 0.0f;
        // determine what direction is increased
        if (direction.x == 0 && direction.y == 1) z_modifier = speed;
        else if (direction.x == 0 && direction.y == -1) z_modifier = -speed;
        else if (direction.x == 1 && direction.y == 0) x_modifier = speed;
        else if (direction.x == -1 && direction.y == 0) x_modifier = -speed;
        // update positional data
        position.x = position.x + x_modifier;
        position.z = position.z + z_modifier;
        //  update actual model
        model.transform.position = position;
    }

    private void ApplyLaneOffset()
    {
        if (direction.x == 0 && direction.y == 1) position.x += right_lane_offset;
        else if (direction.x == 0 && direction.y == -1) position.x -= right_lane_offset;
        else if (direction.x == 1 && direction.y == 0) position.z -= right_lane_offset;
        else if (direction.x == -1 && direction.y == 0) position.z += right_lane_offset;
    }

    private GameObject LoadPrefab(string v)
    {
        var obj = Resources.Load(v);
        GameObject loadedPrefab = GameObject.Instantiate(obj) as GameObject;
        return loadedPrefab;
    }
    
}