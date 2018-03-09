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

    private Vector3 position = new Vector3(0.0f, GraphicalRoadnet.roadThickness + 0.055f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector2 direction = new Vector2(1, 0);

    private Intersection source;
    private Intersection destination;
    private LinkedList<Car> current_queue;
    private LinkedList<Car> previous_queue;

    private bool waiting = false;
    private int wait_counter; 

    private IntersectionPoller poller;

    public GameObject model;
    private LogicalRoadnet network;

    public Car(LogicalRoadnet network)
    {
        this.network = network;

        // pick a starting intersection
        source = network.intersections[Deterministic.random.Next(network.intersections.Count)];
        destination = InitialDestination(source);

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

    private Intersection InitialDestination(Intersection source)
    {
        // naïve solution: pick first destination at random

        Intersection east = source.getEast();
        Intersection west = source.getWest();
        Intersection north = source.getNorth();
        Intersection south = source.getSouth();

        List<Intersection> options = new List<Intersection>();
        if (east != null) { options.Add(east); }
        if (west != null) { options.Add(west); }
        if (north != null) { options.Add(north); }
        if (south != null) { options.Add(south); }

        /**
         * Choose a "previous source" retroactively from the first available interface.
         * This is just to pouplate the previous_queue object in a sane way
         */
        Intersection previous = options[0];
        if (previous == east) { previous_queue = source.EQ; }
        if (previous == west) { previous_queue = source.WQ; }
        if (previous == north) { previous_queue = source.NQ; }
        if (previous == south) { previous_queue = source.SQ; }

        /**
         * Pick a first destination from the second available interface (there will always be at least 2)
         * E.g. if the destination is on the north interface, join the southern queue
         */
        Intersection destination = options[1];
        if (destination == east) { current_queue = destination.WQ; }
        if (destination == west) { current_queue = destination.EQ; }
        if (destination == north) { current_queue = destination.SQ; }
        if (destination == south) { current_queue = destination.NQ; }

        current_queue.AddLast(this);    // join the current queue
        return destination;
    }

    private Intersection NextDestination(Intersection origin, Intersection excluding)
    {
        // naïve solution: choose next destination at random
        List<Intersection> options = new List<Intersection>();
        Intersection east = origin.getEast();
        if (east != null)
        {
            options.Add(east);
        }
        Intersection west = origin.getWest();
        if (west != null)
        {
            options.Add(west);
        }
        Intersection north = origin.getNorth();
        if (north != null)
        {
            options.Add(north);
        }
        Intersection south = origin.getSouth();
        if (south != null)
        {
            options.Add(south);
        }

        // remove from old queue
        if (current_queue != null)
        {
            current_queue.RemoveFirst();
        }

        Intersection next_hop = options[Deterministic.random.Next(options.Count)];
        if (next_hop == east) { current_queue = origin.EQ; }
        if (next_hop == west) { current_queue = origin.WQ; }
        if (next_hop == north) { current_queue = origin.NQ; }
        if (next_hop == south) { current_queue = origin.SQ; }

        // enter the queue
        Debug.Log("AddLast");
        current_queue.AddLast(this);

        return next_hop;
    }

    public Intersection getDestination()
    {
        return destination;
    }

    public void Drive()
    {
        if (waiting)
        {
            if (wait_counter > 0)
            {
                wait_counter--;
            }
            else
            {
                waiting = false;
                position.x = source.coordinates.x;
                position.z = source.coordinates.z;

                // update the cars appearance
                UpdateDirection();
                model.transform.position = position;
                Debug.Log("from: " + source.coordinates + " to " + destination.coordinates);            }
        }
        else if (AtNextIntersection())
        {

            if (current_queue.First.Value == this)
            {
                model.transform.position = position;
                waiting = true;
                wait_counter = 100;
                
                // the previous destination becomes the new source intersection
                Intersection next_dest = NextDestination(destination, source);
                source = destination;
                destination = next_dest;
            }
        }
        else
        {
            if(ApproachingIntersection())
            {
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
        //Debug.Log(speed/speed_scaler + ":" + target_speed/ speed_scaler);
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
        float offset = GraphicalRoadnet.roadWidth;
        bool arrived = false;
        // east
        if (!arrived &&
            position.x >= destination.coordinates.x - offset &&
            direction.x == 1 && direction.y == 0)
        {
            arrived = true;
        }
        // west
        if (!arrived &&
            position.x <= destination.coordinates.x + offset &&
            direction.x == -1 && direction.y == 0)
        {
            arrived = true;
        }
        // north
        if (!arrived &&
            position.z >= destination.coordinates.z - offset &&
            direction.x == 0 && direction.y == 1)
        {
            arrived = true;
        }
        // south
        if (!arrived &&
            position.z <= destination.coordinates.z + offset && 
            direction.x == 0 && direction.y == -1)
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
