using System.Collections.Generic;
using UnityEngine;

internal class Car
{
    private static string[] car_models = { @"car_1", @"car_2", @"car_3", @"car_4", @"Police_car", @"Taxi" };
    private static float scale_factor = GraphicalRoadnet.roadWidth * 0.3f;
    private static float right_lane_offset = GraphicalRoadnet.roadWidth * 0.25f;

    private static float speed_scaler = 0.0005f;
    private static float max_speed = 60.0f * speed_scaler;
    private static float acceleration = 0.01f;//6.0f * speed_scaler;
    private static float retardation = 0.25f;//40.0f * speed_scaler;
    private float speed = 0.0f;

    private float intersection_speed = max_speed * 0.1f;
    private float approach_distance = GraphicalRoadnet.roadWidth * 2;

    /* proof of concept starting point */
    private Vector3 position = new Vector3(0.0f, GraphicalRoadnet.roadThickness + 0.055f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector2 direction = new Vector2(1, 0);

    private Intersection source;
    private Intersection destination;

    private IntersectionPoller poller;

    public GameObject model;
    private LogicalRoadnet network;

    public Car(LogicalRoadnet network)
    {
        this.network = network;

        // pick a starting intersection
        source = network.intersections[Deterministic.random.Next(network.intersections.Count)];
        destination = NextDestination(source, null);

        // pick a car model
        int model_index = Deterministic.random.Next(car_models.Length);
        model = LoadPrefab(car_models[model_index]);

        // move car to starting position
        position = position + source.coordinates;
        Debug.Log("src: " + position);

        // set position
        model.transform.position = position;
        model.transform.localScale = scale;
        UpdateDirection();

        Debug.Log(AtNextIntersection());
    }

    private Intersection NextDestination(Intersection origin, Intersection excluding)
    {
        // naïve solution: choose next destination at random
        List<Intersection> options = new List<Intersection>();
        Intersection east = origin.getEast();
        if (east != null && east != excluding)
        {
            options.Add(east);
        }
        Intersection west = origin.getWest();
        if (west != null && west != excluding)
        {
            options.Add(west);
        }
        Intersection north = origin.getNorth();
        if (north != null && north != excluding)
        {
            options.Add(north);
        }
        Intersection south = origin.getSouth();
        if (south != null && south != excluding)
        {
            options.Add(south);
        }

        return options[Deterministic.random.Next(options.Count)];
    }

    public Intersection getDestination()
    {
        return destination;
    }

    public void Drive()
    {
        if (AtNextIntersection())
        {
            position.x = destination.coordinates.x;
            position.z = destination.coordinates.z;

            // the previous destination becomes the new source intersection
            Intersection next_dest = NextDestination(destination, source);
            source = destination;
            destination = next_dest;

            // update the cars appearance
            UpdateDirection();
            model.transform.position = position;
            Debug.Log("from: " + source.coordinates + " to " + destination.coordinates);

        }
        else
        {
            if(ApproachingIntersection())
            {
                Debug.Log("aproachinf");
                ChangeSpeed(intersection_speed);
            }
            else
            {
                ChangeSpeed(max_speed);
            }
            UpdatePosition();
        }
    }

    private void ChangeSpeed(float target_speed)
    {
        Debug.Log(speed/speed_scaler + ":" + target_speed/ speed_scaler);
        if (speed < target_speed)
        {
            speed = Mathf.Min(speed + acceleration * target_speed, target_speed);
        }
        else if (speed > target_speed)
        {
            speed = Mathf.Max(speed - retardation * target_speed, target_speed);
        }
    }

    private bool ApproachingIntersection()
    {
        bool approched = false;
        // east
        if (!approched &&
            position.x >= (destination.coordinates.x - approach_distance) &&
            direction.x == 1 && direction.y == 0)
        {
            approched = true;
        }
        // west
        if (!approched &&
            position.x <= (destination.coordinates.x + approach_distance) &&
            direction.x == -1 && direction.y == 0)
        {
            approched = true;
        }
        // north
        if (!approched &&
            position.z >= (destination.coordinates.z - approach_distance) &&
            direction.x == 0 && direction.y == 1)
        {
            approched = true;
        }
        // south
        if (!approched &&
            position.z <= (destination.coordinates.z + approach_distance) && direction.x == 0 &&
            direction.y == -1)
        {
            approched = true;
        }
        return approched;
    }

    private bool AtNextIntersection()
    {
        bool arrived = false;
        // east
        if (!arrived &&
            position.x >= destination.coordinates.x &&
            direction.x == 1 && direction.y == 0)
        {
            arrived = true;
        }
        // west
        if (!arrived &&
            position.x <= destination.coordinates.x &&
            direction.x == -1 && direction.y == 0)
        {
            arrived = true;
        }
        // north
        if (!arrived &&
            position.z >= destination.coordinates.z &&
            direction.x == 0 && direction.y == 1)
        {
            arrived = true;
        }
        // south
        if (!arrived &&
            position.z <= destination.coordinates.z && direction.x == 0 &&
            direction.y == -1)
        {
            arrived = true;
        }
        return arrived;
    }

    private void UpdateDirection()
    {
        if (destination.coordinates.x - GraphicalRoadnet.roadWidth >= position.x &&
            destination.coordinates.z == position.z)
        {
            // heading east
            direction.x = 1.0f;
            direction.y = 0.0f;
            model.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        if (destination.coordinates.x + GraphicalRoadnet.roadWidth <= position.x &&
            destination.coordinates.z == position.z)
        {
            // heading west
            direction.x = -1.0f;
            direction.y = 0.0f;
            model.transform.rotation = Quaternion.Euler(0, 270, 0);
        }

        if (destination.coordinates.z - GraphicalRoadnet.roadWidth >= position.z &&
            destination.coordinates.x == position.x)
        {
            // heading north
            direction.x = 0.0f;
            direction.y = 1.0f;
            model.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        if (destination.coordinates.z + GraphicalRoadnet.roadWidth <= position.z &&
            destination.coordinates.x == position.x)
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