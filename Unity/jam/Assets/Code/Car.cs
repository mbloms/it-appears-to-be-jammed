using System;
using System.Collections.Generic;
using UnityEngine;

internal class Car
{
    private static string[] car_models = { @"car_1", @"car_2", @"car_3", @"car_4", @"Police_car", @"Taxi" };
    private static float scale_factor = GraphicalRoadnet.roadWidth * 0.3f;
    private static float right_lane_offset = GraphicalRoadnet.roadWidth * 0.25f;

    private static float speed_scaler = 0.0005f;
    private static float max_speed = 60.0f;
    private static float acceleration = 0.25f;//6.0f * speed_scaler;
    private static float retardation = 10.0f;//40.0f * speed_scaler;
    private float speed = 0.0f;

    private float intersection_speed = max_speed * 0.5f;
    private float approach_distance = GraphicalRoadnet.roadWidth * 2;

    private Vector3 position = new Vector3(0.0f, GraphicalRoadnet.roadThickness + 0.055f, 0.0f);
    private Vector3 scale = new Vector3(scale_factor, scale_factor, scale_factor);
    private Vector2 direction = new Vector2(1, 0);

    private Intersection source;
    private Intersection destination;
    private LinkedList<Car> current_queue;
    private LinkedList<Car> previous_queue;
    private string from,to;

    public bool turning;

    private bool waiting = false;
    private int wait_counter;
    private int wait_threshold;
    private Vector3 turn_position;
    private int right_turn_delay = 50;

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
        poller = source.getPoller(this, from, to);

        return destination;
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
        
        if (source == origin.getEast())
        {
            from = "east";
        }
        else if (source == origin.getWest())
        {
            from = "west";
        }
        else if (source == origin.getNorth())
        {
            from = "north";
        }
        else if (source == origin.getSouth())
        {
            from = "south";
        }

        poller = origin.getPoller(this, from, to);
        
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
            // Log("waiting for OK to drive to " + destination.coordinates);

            /** if you are at the top of the queue */
            if (current_queue.First.Value == this)
            {
                /** when the lock is acquired*/
                if (poller.AlreadyAcquired())
                {
                    // When the animation is done.
                    if (wait_counter >= wait_threshold)
                    {
                        // update the cars appearance
                        position.x = source.coordinates.x;
                        position.z = source.coordinates.z;
                        UpdateDirection();

                        // apply offset
                        if (direction.x == 1) position.x += GraphicalRoadnet.roadWidth;    // heading east
                        if (direction.x == -1) position.x -= GraphicalRoadnet.roadWidth;   // heading west
                        if (direction.y == 1) position.z += GraphicalRoadnet.roadWidth;    // heading north
                        if (direction.y == -1) position.z -= GraphicalRoadnet.roadWidth;   // heading south
                        model.transform.position = position;

                        // 1. leave the previous queue
                        previous_queue = current_queue;
                        previous_queue.Remove(this);

                        // 2. enter the new queue and set `from`.
                        if (source == destination.getEast())
                        {
                            current_queue = destination.EQ;
                        }
                        else if (source == destination.getWest())
                        {
                            current_queue = destination.WQ;
                        }
                        else if (source == destination.getNorth())
                        {
                            current_queue = destination.NQ;
                        }
                        else if (source == destination.getSouth())
                        {
                            current_queue = destination.SQ;
                        }
                        current_queue.AddLast(this);

                        // 3. Stop waiting
                        waiting = false;
                        turning = false;
                        poller.Free();

                    } else {
                        /** animate movement */
                        AnimateTurn();
                    }
                }
                else if (!DestinationQueueFull())
                {
                    /** attempt to acquire lock until success */
                    if (poller.Acquire())
                    {
                        turning = true;
                        wait_threshold = right_turn_delay * TypeOfTurn();
                        turn_position = position;
                        wait_counter = 0;
                    }
                }
            }
        }
        /** this event is triggered in the frame that the car arrives at the destination */
        else if (HasArrived())
        {
            // Log("arrived at destination " + destination.coordinates);

            // Get a new destination and store the current intersection in `source
            Intersection next_hop = NextDestination(destination, source);
            source = destination;
            destination = next_hop;

            // enter the next state; waiting for OK at the intersection
            waiting = true;     
        }
        /** continue driving towards next destination*/
        else
        {
            if (speed < 0)
            {
                speed++;
            }
            // Log("driving old:" + previous_queue.Count + " cur:" + current_queue.Count);
            else
            {

                if (StartToBrake() && NextCar() != null && !NextCar().turning)
                {
                    Retard();
                }
                else
                {
                    Accelerate();
                }

                float distance = DistanceNextCar();
                if (distance == -1 || distance > GraphicalRoadnet.roadWidth || NextCar().turning)
                {
                    UpdatePosition();
                }
                else
                {
                    speed = -30f;
                }
            }
        }
    }

    private Car NextCar()
    {
        Car next_car = null;
        LinkedListNode<Car> next = current_queue.Find(this).Previous;
        if (next != null) next_car = next.Value;
        return next_car;
    }

    private void Retard()
    {
        speed = Mathf.Max(speed - retardation, 0.0f);
    }

    private void Accelerate()
    {
        speed = Mathf.Min(speed + acceleration, max_speed);
    }

    private bool StartToBrake()
    {
        float brake_distance = speed * speed_scaler * 50;//(speed * speed) / (2 * retardation);
        float distance_next = DistanceNextCar();

        if (distance_next == -1)
        {
            // no car infront
            return false;
        }
        else
        {
            return brake_distance > distance_next;
        }

    }

    private void AnimateTurn()
    {
        wait_counter++;
        int turn = TypeOfTurn();
        
        if (turn == 1)
        {
            AnimateRight();
        }
        else if (turn == 2)
        {
            AnimateForward();
        }
        else if (turn == 3)
        {
            AnimateLeft();
        }
        else
        {
            throw new InvalidOperationException("Undefined type of turn");
        }
    }

    // Turn the car right in an intersection
    private void AnimateRight()
    {
        float angle_deg = 90.0f / wait_threshold;
        float angle_rad = (Mathf.PI / 2) / wait_threshold * wait_counter;

        float radius = GraphicalRoadnet.roadWidth;

        if (from == "north" && to == "west")
        {
            turn_position.x = position.x - (radius - right_lane_offset) * (1 - Mathf.Cos(angle_rad));
            turn_position.z = position.z - (radius - right_lane_offset) * Mathf.Sin(angle_rad);
        }
        else if (from == "south" && to == "east")
        {
            turn_position.x = position.x + (radius - right_lane_offset) * (1 - Mathf.Cos(angle_rad));
            turn_position.z = position.z + (radius - right_lane_offset) * Mathf.Sin(angle_rad);
        }
        else if (from == "west" && to == "south")
        {
            turn_position.x = position.x + (radius - right_lane_offset) * (Mathf.Cos(angle_rad - Mathf.PI / 2));
            turn_position.z = position.z - (radius - right_lane_offset) + ((radius - right_lane_offset) * Mathf.Sin(angle_rad + Mathf.PI / 2));
        }
        else if (from == "east" && to == "north")
        {
            turn_position.x = position.x - (radius - right_lane_offset) * (Mathf.Cos(angle_rad - Mathf.PI / 2));
            turn_position.z = position.z + (radius - right_lane_offset) + ((radius - right_lane_offset) * Mathf.Sin(angle_rad - Mathf.PI / 2));
        }

        // Apply animation
        turn_position.y = position.y;
        model.transform.position = turn_position;
        model.transform.Rotate(new Vector3(0, angle_deg, 0));
    }

    // Turn the car left in an intersection
    private void AnimateLeft()
    {
        float angle_deg = -90.0f / wait_threshold;
        float angle_rad = -(Mathf.PI / 2) / wait_threshold * wait_counter;

        float radius = GraphicalRoadnet.roadWidth;

        if (from == "west" && to == "north")
        {
            turn_position.x = position.x - right_lane_offset + (radius - right_lane_offset) * (1-Mathf.Cos(angle_rad - Mathf.PI / 2));
            turn_position.z = position.z + (radius - right_lane_offset) + (radius - right_lane_offset) * Mathf.Sin(angle_rad - Mathf.PI / 2);
        }
        else if (from == "east" && to == "south")
        {
            turn_position.x = position.x - (2 * radius - 3 * right_lane_offset) * (Mathf.Cos(angle_rad + Mathf.PI / 2));
            turn_position.z = position.z - (2 * radius - 3 * right_lane_offset) + (2 * radius - 3 * right_lane_offset) * Mathf.Sin(angle_rad + Mathf.PI / 2);
        }
        /*else if (from == "south" && to == "west")
         {
             turn_position.x = position.x + (radius - right_lane_offset) * (Mathf.Cos(angle_rad - Mathf.PI / 2));
             turn_position.z = position.z - (radius - right_lane_offset) + ((radius - right_lane_offset) * Mathf.Sin(angle_rad + Mathf.PI / 2));
         }
         else if (from == "north" && to == "east")
         {
             turn_position.x = position.x - (radius - right_lane_offset) * (Mathf.Cos(angle_rad - Mathf.PI / 2));
             turn_position.z = position.z + (radius - right_lane_offset) + ((radius - right_lane_offset) * Mathf.Sin(angle_rad - Mathf.PI / 2));
         }*/

        // Apply animation
        turn_position.y = position.y;
        model.transform.position = turn_position;
        model.transform.Rotate(new Vector3(0, angle_deg, 0));
    }

    // Move the car forward in an intersection
    private void AnimateForward()
    {
        float step = GraphicalRoadnet.roadWidth * 2 / wait_threshold;
        if (direction.x == 1) position.x += step;    // heading east
        if (direction.x == -1) position.x -= step;   // heading west
        if (direction.y == 1) position.z += step;    // heading north
        if (direction.y == -1) position.z -= step;   // heading south
        model.transform.position = position;
    }

    /** 
     * Returns one of the following integers
     * 1: right turn
     * 2: straight ahead 
     * 3: left turn
     */
    private int TypeOfTurn()
    {
        // came from south
        if (from == "south")
        {
            if (to == "north") return 2;    // dead ahead
            if (to == "east") return 1;     // right
            if (to == "west") return 3;     // left
        }
        // came from north
        if (from == "north")
        {
            if (to == "south") return 2;    // dead ahead
            if (to == "west") return 1;     // right
            if (to == "east") return 3;     // left
        }
        // came from west
        if (from == "west")
        {
            if (to == "east") return 2;     // dead ahead
            if (to == "south") return 1;    // right
            if (to == "north") return 3;    // left
        }
        // came from east
        if (from == "east")
        {
            if (to == "west") return 2;     // dead ahead
            if (to == "north") return 1;    // right
            if (to == "south") return 3;    // left
        }
        return 0;
    }

    private float DistanceNextCar()
    {
        LinkedListNode<Car> next = current_queue.Find(this).Previous;
        if (next != null)
        {
            Car next_car = next.Value;
            if (this.position.x == next_car.position.x)
            {
                // traveling north/south
                return Mathf.Abs(this.position.z - next_car.position.z);
            }
            else if (this.position.z == next_car.position.z)
            {
                // traveling north/south
                return Mathf.Abs(this.position.x - next_car.position.x);
            }
            else
            {
                throw new InvalidOperationException("Cars in same queue but different axes.");
            }
        }
        return -1;
    }

    private bool DestinationQueueFull()
    {
        LinkedList<Car> queue = null;
        if (source == destination.getEast()) { queue = destination.EQ; }
        else if (source == destination.getWest()) { queue = destination.WQ; }
        else if (source == destination.getNorth()) { queue = destination.NQ; }
        else if (source == destination.getSouth()) { queue = destination.SQ; }

        if (queue.Last != null)
        {
            float distance = -1; 
            if (source == destination.getEast() || source == destination.getWest())
            {
                // traveling on the x-axis
                distance = Mathf.Abs(this.position.x - queue.Last.Value.position.x);
            }
            else if (source == destination.getNorth() || source == destination.getSouth())
            {
                // traveling on the z-axis
                distance = Mathf.Abs(this.position.z - queue.Last.Value.position.z);
            }
            return distance < (GraphicalRoadnet.roadWidth + right_lane_offset);
        }

        return false;
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
        position.x = position.x + x_modifier * speed_scaler;
        position.z = position.z + z_modifier * speed_scaler;
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
