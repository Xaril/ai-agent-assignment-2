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
        

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;

        public Dictionary<String, Vector3> initial_positions;

        private void Start()
        {
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            Point startPoint = new Point((int) transform.position.x, (int) transform.position.z);
            Point endPoint = new Point(360, 320);
            
            // get the car controller
            
            m_Car = GetComponent<CarController>();

            PathGenerator p = new PathGenerator(terrain_manager);
            p.GetPath(startPoint, endPoint, transform.rotation.eulerAngles.y);


            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            Debug.Log(string.Format("Number of friends: {0}", friends.Length));

            initial_positions = GetInitPos(friends);
        }

        private Dictionary<String, Vector3> GetInitPos(GameObject[] friends)
        {
            Dictionary<String, Vector3> initial_positions = new Dictionary<String, Vector3>();

            for (int i = 0; i < friends.Length; i++)
            {
                Debug.Log(string.Format("Car name: {0}", friends[i].name));
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

            return;
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


        }
    }
}
