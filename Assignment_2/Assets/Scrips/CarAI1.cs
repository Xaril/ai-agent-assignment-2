using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI1 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject mstCreator;
        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;

        public Dictionary<String, Vector3> initial_positions;
        private List<Vector3> mstPath;
        private PIDController velocityController;
        private PIDController steeringController;
        private int currentPathIndex = 0;
        private float maxVelocity = 1f;
        private GameObject emptyGO;
        private float distanceOffset = 5f;
        private List<Vector3> finalPath;

        private void Start()
        {
            emptyGO = new GameObject();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            m_Car = GetComponent<CarController>();
            

            velocityController = new VelocityController(Time.fixedDeltaTime);
            steeringController = new SteeringController(Time.fixedDeltaTime);


            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            Debug.Log(string.Format("Number of friends: {0}", friends.Length));


            initial_positions = GetInitPos(friends);

            foreach (var pos in initial_positions)
            {
                Debug.Log(pos.ToString());
            }

            mstPath = mstCreator.GetComponent<CreateMST>().path;
            Point startPoint = new Point((int)transform.position.x, (int)transform.position.z);
            Point endPoint = new Point((int)mstPath[0].x, (int)mstPath[0].z);

            

            PathGenerator aStar = new PathGenerator(terrain_manager);
            List<Vector3> startPath = aStar.GetPath(startPoint, endPoint, transform.rotation.eulerAngles.y);
            finalPath = startPath;

        }

        private Dictionary<String, Vector3> GetInitPos(GameObject[] friends)
        {
            Dictionary<String, Vector3> initial_positions = new Dictionary<String, Vector3>();

            for (int i = 0; i < friends.Length; i++)
            {
                switch (friends[i].name)
                {
                    case "ArmedCar":
                        initial_positions.Add(friends[i].name,
                            new Vector3(
                                terrain_manager.myInfo.get_x_pos(8),
                                0f,
                                terrain_manager.myInfo.get_z_pos(9)));
                        break;
                    case "ArmedCar (1)":
                        initial_positions.Add(friends[i].name,
                            new Vector3(
                                terrain_manager.myInfo.get_x_pos(8),
                                0f,
                                terrain_manager.myInfo.get_z_pos(10)));
                        break;
                    case "ArmedCar (2)":
                        initial_positions.Add(friends[i].name,
                            new Vector3(
                                terrain_manager.myInfo.get_x_pos(8),
                                0f,
                                terrain_manager.myInfo.get_z_pos(11)));
                        break;
                    default:
                        break;
                }
            }
            return initial_positions;
        }

        private void FixedUpdate()
        {

            if (Vector3.Distance(finalPath[currentPathIndex], transform.position) <= distanceOffset)
            {
                currentPathIndex++;
                if(currentPathIndex + 3 >= finalPath.Count)
                {
                    finalPath = mstPath;
                    currentPathIndex = 0;
                }
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = finalPath[currentPathIndex];
            cube.GetComponent<BoxCollider>().enabled = false;
            cube.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0);


            /// Lazy to calculate angle between two objects, using empty object and invoking LookAt function to do that for me.
            emptyGO.transform.position = transform.position;
            emptyGO.transform.rotation = transform.rotation;
            Transform t = emptyGO.transform;
            t.LookAt(finalPath[currentPathIndex]);

            float x = t.rotation.eulerAngles.y;
            /// x - 180 for oposite ange. Moving backwards.
            float angle = Mathf.DeltaAngle(0f, x);

            /// Setting target velocity and sendind negative to footbrake to move backwards.
            float velocity = velocityController.GetOutput(GetComponent<Rigidbody>().velocity.magnitude, maxVelocity);
            float steering = steeringController.GetOutput(transform.rotation.eulerAngles.y, angle);
            m_Car.Move(steering, velocity, 0f, 0f);

            /*
            // Execute your path here
            // ...

            Vector3 destination = initial_positions[m_Car.name];

            Vector3 direction = (destination - transform.position).normalized;
            Debug.Log(direction.y);
            bool is_to_the_right = Vector3.Dot(direction, transform.right) > 0f;
            bool is_to_the_front = Vector3.Dot(direction, transform.forward) > 0f;

            float steering = 0f;
            float acceleration = 0;

            if (is_to_the_right && is_to_the_front)
            {
                steering = 1f;
                acceleration = 1f;
            }
            else if (is_to_the_right && !is_to_the_front)
            {
                steering = -1f;
                acceleration = -1f;
            }
            else if (!is_to_the_right && is_to_the_front)
            {
                steering = -1f;
                acceleration = 1f;
            }
            else if (!is_to_the_right && !is_to_the_front)
            {
                steering = 1f;
                acceleration = -1f;
            }

            // this is how you access information about the terrain
            int i = terrain_manager.myInfo.get_i_index(transform.position.x);
            int j = terrain_manager.myInfo.get_j_index(transform.position.z);
            float grid_center_x = terrain_manager.myInfo.get_x_pos(i);
            float grid_center_z = terrain_manager.myInfo.get_z_pos(j);

            Debug.DrawLine(transform.position, new Vector3(grid_center_x, 0f, grid_center_z));


            // this is how you control the car
            //Debug.Log("Steering:" + steering + " Acceleration:" + acceleration);
            m_Car.Move(steering, acceleration, acceleration, 0f);
            //m_Car.Move(0f, -1f, 1f, 0f);

            */
        }
    }
}
