using System;
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
    private string from,to;

    private bool waiting = false;
    private int wait_counter; 

    private IntersectionPoller poller;

    public GameObject model;
    private LogicalRoadnet network;

    private bool debugging = false;

    public Car(LogicalRoadnet network, bool debugging)
    {
        this.network = network;
        this.debugging = debugging;

        
        // pick a starting intersection
        source = network.intersections[Deterministic.random.Next(network.intersections.Count)];
        destination = InitialDestination(source);

        // pick a car model
        int model_index = Deterministic.random.Next(car_models.Length);
        model = LoadPrefab(car_models[model_index]);

        // move car to starting position
        position = position + source.coordinates;

        // set position
        model.transform.position = position;
        model.transform.localScale = scale;
        UpdateDirection();
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
        if (destination == east)
        {
            current_queue = destination.WQ;
            from = "west";
        }

        if (destination == west)
        {
            current_queue = destination.EQ;
            from = "east";
        }

        if (destination == north)
        {
            current_queue = destination.SQ;
            from = "south";
        }

        if (destination == south)
        {
            current_queue = destination.NQ;
            from = "north";
        }

        current_queue.AddLast(this);    // join the current queue
        return destination;
    }

    private Intersection NextDestination(Intersection origin)
    {
        return NextDestination(origin, null);
    }

    private Intersection NextDestination(Intersection origin, Intersection excluding)
    {
        Intersection east = origin.getEast();
        Intersection west = origin.getWest();
        Intersection north = origin.getNorth();
        Intersection south = origin.getSouth();

        List<Intersection> options = new List<Intersection>();
        if (east != null && east != excluding) { options.Add(east); }
        if (west != null && west != excluding) { options.Add(west); }
        if (north != null && north != excluding) { options.Add(north); }
        if (south != null && south != excluding) { options.Add(south); }
        
        /** Naïve solution: pick the next destination at random */
        Intersection next_hop = options[Deterministic.random.Next(options.Count)];
        if (next_hop == origin) { throw new InvalidOperationException("next_hop can't be same as origin"); }

        if (next_hop == origin.getEast())
        {
            to = "east";
        }
        else if (next_hop == origin.getWest())
        {
            to = "west";
        }
        else if (next_hop == origin.getNorth())
        {
            to = "north";
        }
        else if (next_hop == origin.getSouth())
        {
            to = "south";
        }
        
        return next_hop;
    }

    public Intersection getDestination()
    {
        return destination;
    }

    public void Drive()
    {
        if (poller != null)
        {
            poller.Update();
        }
        /** if waiting for OK to drive */
        if (waiting)
        {
            Log("waiting for OK to drive to " + destination.coordinates);
            if (poller.Acquire())
            {
                // update the cars appearance
                position.x = source.coordinates.x;
                position.z = source.coordinates.z;
                model.transform.position = position;
                UpdateDirection();

                // 1. leave the previous queue
                previous_queue = current_queue;
                previous_queue.Remove(this);

                // 2. enter the new queue
                //    and set `from`.
                if (source == destination.getEast())
                {
                    current_queue = destination.EQ;
                    // If our new queue is east of the destination,
                    // then we're also comming from east of the destination.
                    from = "east";
                }
                else if (source == destination.getWest())
                {
                    current_queue = destination.WQ;
                    from = "west";
                }
                else if (source == destination.getNorth())
                {
                    current_queue = destination.NQ;
                    from = "north";
                }
                else if (source == destination.getSouth())
                {
                    current_queue = destination.SQ;
                    from = "south";
                }
                current_queue.AddLast(this);

                // 3. Stop waiting
                waiting = false;
                
            }
            else if (current_queue.First.Value == this)
            {
                wait_counter--;
            }

        }
        /** this event is triggered in the frame that the car arrives at the destination */
        else if (HasArrived())
        {
            if (poller != null) {poller.Free();}
            Log("arrived at destination " + destination.coordinates);

            // Get a new destination and store the current intersection in `source
            Intersection next_hop = NextDestination(destination, source);
            source = destination;
            destination = next_hop;

            // enter the next state; waiting for OK at the intersection
            waiting = true;     
            wait_counter = 100;

            poller = source.getPoller(this,from, to);
        }
        /** continue driving towards next destination*/
        else
        {
            Log("driving old:" + previous_queue.Count + " cur:" + current_queue.Count);
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

    private bool HasArrived()
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

    private void Log(string s)
    {
        if (this.debugging) { Debug.Log(s); }
    }

}
