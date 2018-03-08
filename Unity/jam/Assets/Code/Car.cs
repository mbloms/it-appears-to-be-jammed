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
    private float speed = 0.0f;

    /* proof of concept starting point */
    private Vector3 position = new Vector3(0.0f, GraphicalRoadnet.roadThickness + 0.055f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector2 direction = new Vector2(1, 0);

    private int source_id;
    private int destination_id;

    private Intersection destination;

    private IntersectionPoller poller;

    public GameObject model;
    private LogicalRoadnet network;

    public Car(LogicalRoadnet network)
    {
        this.network = network;

        // pick a starting lane
        source_id = Deterministic.random.Next(network.intersections.Count);
        destination_id = network.intersections[source_id].connections[
            Deterministic.random.Next(network.intersections[source_id].connections.Count)];
        destination = network.intersections[destination_id];

        // pick a car model
        int model_index = Deterministic.random.Next(car_models.Length);
        model = LoadPrefab(car_models[model_index]);

        // move car to starting position
        position = position + network.intersections[source_id].coordinates;
        Debug.Log("src: " + position);

        // set position
        model.transform.position = position;
        model.transform.localScale = scale;
        UpdateDirection();

        Debug.Log(AtNextIntersection());
    }

    public Intersection getDestination()
    {
        return network.intersections[destination_id];
    }
    
    public void Drive()
    {
        ChangeSpeed(max_speed);

        if (AtNextIntersection())
        {
            position.x = destination.x;
            position.z = destination.z;

            List<int> options = network.intersections[destination_id].connections;
            int next_id = options[Deterministic.random.Next(options.Count)];
            while (next_id == source_id) {
                next_id = options[Deterministic.random.Next(options.Count)];
            }

            // take new path
            source_id = destination_id;
            destination_id = next_id;
            destination = network.intersections[destination_id].coordinates;

            Debug.Log("from: " + network.intersections[source_id].coordinates + " to " + destination);
            UpdateDirection();
            model.transform.position = position;
        }
        else
        {
            UpdatePosition();
        }
    }

	private void ChangeSpeed(float target_speed)
    {
		if (speed < target_speed)
		{
			speed = Mathf.Min(speed + acceleration * target_speed, target_speed);
		}
		else if(speed > target_speed)
		{
			speed = Mathf.Max(speed + retardation * target_speed, target_speed);
        }
    }

    private bool AtNextIntersection()
    {
        bool arrived = false;
        // east
        if (!arrived &&
            position.x >= destination.x &&
            direction.x == 1 && direction.y == 0)
        {
            arrived = true;
        }
        // west
        if (!arrived &&
            position.x <= destination.x &&
            direction.x == -1 && direction.y == 0)
        {
            arrived = true;
        }
        // north
        if (!arrived &&
            position.z >= destination.z &&
            direction.x == 0 && direction.y == 1)
        {
            arrived = true;
        }
        // south
        if (!arrived &&
            position.z <= destination.z && direction.x == 0 &&
            direction.y == -1)
        {
            arrived = true;
        }
        return arrived;
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