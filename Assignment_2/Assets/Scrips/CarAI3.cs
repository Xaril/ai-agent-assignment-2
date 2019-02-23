using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI3 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;

        public Dictionary<String, Vector3> initial_positions;
        public List<Vector3> vrpPath;
        private int currentPathIndex = 0;
        private float distanceOffset = 5f;
        private List<Vector3> finalPath;

        public float maxVelocity;
        public float acceleration;
        private float timeStep;
        private int numberOfSteps;
        private float time;

        private float steerDirection;
        private float accelerationDirection;
        private float brake;
        private float handBrake;

        private bool crashed;
        private float crashTime;
        private float crashCheckTime;
        private float crashDirection;
        private Vector3 previousPosition;
        private ConfigurationSpace configurationSpace;

        private static int[][] cost_matrix; // is symmetric

        private void Start()
        {
            Time.timeScale = 1;
            maxVelocity = 20;
            acceleration = 1f;

            timeStep = 0.05f;
            numberOfSteps = 5;
            time = 0;

            steerDirection = 0;
            accelerationDirection = 0;
            brake = 0;
            handBrake = 0;
            crashed = false;
            crashTime = 0;
            crashCheckTime = 0.5f;
            crashDirection = 0;
            previousPosition = Vector3.up;

            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            m_Car = GetComponent<CarController>();

            InitializeCSpace();


            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            Debug.Log(string.Format("Number of friends: {0}", friends.Length));

            int carNumber = 0;
            for (int i = 0; i < friends.Length; ++i)
            {
                if (friends[i].name == this.name)
                {
                    carNumber = i;
                    break;
                }
            }

            List<Vector3> enemy_positions = new List<Vector3>();
            for(int i = 0; i < enemies.Length; ++i)
            {
                enemy_positions.Add(enemies[i].transform.position);
            }

            //InitCostMatrix(enemy_positions);
            

            List<Vector3> route = TSP.GenerateCarPaths(TSP.GreedyPath(enemy_positions))[carNumber];

            Point startPoint = new Point((int)transform.position.x, (int)transform.position.z);
            Point endPoint = new Point((int)route[0].x, (int)route[0].z);


            PathGenerator aStar = new PathGenerator(terrain_manager,null);
            List<Vector3> startPath = aStar.GetPath(startPoint, endPoint, transform.rotation.eulerAngles.y);
            
            finalPath = startPath;
            vrpPath = new List<Vector3>();
            for (int i = 0; i < route.Count; ++i)
            {
                Point a = new Point((int)route[i].x, (int)route[i].z);
                Point b = new Point((int)route[(i + 1) % route.Count].x, (int)route[(i + 1) % route.Count].z);

                List<Vector3> p;
                if (i == 0)
                {
                    float dir = Quaternion.LookRotation(new Vector3(a.x, 0, a.y) - startPath[startPath.Count - 2]).eulerAngles.y;
                    p = aStar.GetPath(a, b, dir);
                }
                else
                {
                    float dir = Quaternion.LookRotation(new Vector3(a.x, 0, a.y) - vrpPath[vrpPath.Count - 2]).eulerAngles.y;
                    p = aStar.GetPath(a, b, dir);
                }

                vrpPath.AddRange(p);
            }

        }

        private void InitCostMatrix(List<Vector3> points)
        {
            if (cost_matrix != null)
            {
                return;
            }
            cost_matrix = new int[enemies.Length][];

            for (int i = 0; i < enemies.Length; i++)
            {
                cost_matrix[i] = new int[enemies.Length];
            }
            
            PathGenerator aStar = new PathGenerator(terrain_manager);

            // Compute every cost
            int count = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < enemies.Length; i++)
            {
                for (int j = 0; j < i; j++)
                {

                    if (i == j)
                    {
                        continue;
                    }

                    count++;
                    Point start_point = new Point((int)points[i].x, (int)points[i].z);
                    Point end_point = new Point((int)points[j].x, (int)points[j].z);
                    Debug.Log(string.Format("i: {0}, j: {1}", i, j));
                    Debug.Log(string.Format("({0},{1}) ({2}, {3})", start_point.x, start_point.y, end_point.x, end_point.y));
                    Debug.Log(string.Format("n: {0}", count));
                    
                    List<Vector3> path = aStar.GetPath(start_point, end_point, 0f);

                    cost_matrix[i][j] = cost_matrix[j][i] = path.Count;
                    Debug.Log(string.Format("From {0} to {1}: {2}",
                        points[i], points[j], cost_matrix[i][j]));
                }
            }
            sw.Stop();
            Debug.Log(string.Format("Time elapsed: {0}", sw.Elapsed));

        }

        private void FixedUpdate()
        {
            return;
            if (Vector3.Distance(finalPath[currentPathIndex], transform.position) <= distanceOffset)
            {
                currentPathIndex++;
                if (currentPathIndex + 3 >= finalPath.Count)
                {
                    finalPath = vrpPath;
                    currentPathIndex = 0;
                }
            }

            if (!crashed)
            {
                time += Time.deltaTime;
                if (time >= crashCheckTime && Vector3.Distance(m_Car.transform.position, terrain_manager.myInfo.start_pos) > 5)
                {
                    time = 0;
                    if (Vector3.Distance(previousPosition, m_Car.transform.position) < 0.1f)
                    {
                        crashed = true;
                        if (Physics.BoxCast(
                            m_Car.transform.position,
                            new Vector3(configurationSpace.BoxSize.x / 2, configurationSpace.BoxSize.y / 2, 0.5f),
                            m_Car.transform.forward,
                            Quaternion.LookRotation(m_Car.transform.forward),
                            configurationSpace.BoxSize.z / 2
                        ))
                        {
                            crashDirection = -1;
                        }
                        else
                        {
                            crashDirection = 1;
                        }

                    }
                    else
                    {
                        previousPosition = m_Car.transform.position;
                    }
                }
                steerDirection = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, finalPath[currentPathIndex]);
                accelerationDirection = AccelerationInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, finalPath[currentPathIndex]);

                if (m_Car.CurrentSpeed >= maxVelocity)
                {
                    accelerationDirection = 0;
                }

                if (accelerationDirection < 0)
                {
                    m_Car.Move(-steerDirection, brake, accelerationDirection * acceleration, handBrake);
                }
                else
                {
                    m_Car.Move(steerDirection, accelerationDirection * acceleration, -brake, handBrake);
                }
            }
            else
            {
                crashTime += Time.deltaTime;
                if (crashTime <= 1f)
                {
                    steerDirection = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, finalPath[currentPathIndex]);
                    if (crashDirection > 0)
                    {
                        m_Car.Move(steerDirection, acceleration, 0, 0);
                    }
                    else
                    {
                        m_Car.Move(-steerDirection, 0, -acceleration, 0);
                    }
                }
                else
                {
                    crashTime = 0;
                    crashed = false;
                }
            }
        }

        //Determines steer angle for the car
        private float SteerInput(Vector3 position, float theta, Vector3 point)
        {
            Vector3 direction = Quaternion.Euler(0, theta, 0) * Vector3.forward;
            Vector3 directionToPoint = point - position;
            float angle = Vector3.Angle(direction, directionToPoint) * Mathf.Sign(-direction.x * directionToPoint.z + direction.z * directionToPoint.x);
            float steerAngle = Mathf.Clamp(angle, -m_Car.m_MaximumSteerAngle, m_Car.m_MaximumSteerAngle) / m_Car.m_MaximumSteerAngle;
            return steerAngle;
        }

        //Determines acceleration for the car
        private float AccelerationInput(Vector3 position, float theta, Vector3 point)
        {
            Vector3 direction = Quaternion.Euler(0, theta, 0) * Vector3.forward;
            Vector3 directionToPoint = point - position;
            return Mathf.Clamp(direction.x * directionToPoint.x + direction.z * directionToPoint.z, -1, 1);
        }

        //Get size of car collider to be used with C space
        private void InitializeCSpace()
        {
            Quaternion carRotation = m_Car.transform.rotation;
            m_Car.transform.rotation = Quaternion.identity;
            configurationSpace = new ConfigurationSpace();
            BoxCollider carCollider = GameObject.Find("ColliderBottom").GetComponent<BoxCollider>();
            configurationSpace.BoxSize = carCollider.transform.TransformVector(carCollider.size);
            m_Car.transform.rotation = carRotation;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            Gizmos.color = Color.red;
            for (int i = 0; i <= currentPathIndex; ++i)
            {
                if (finalPath[i] != null)
                {
                    continue;
                }
                Gizmos.DrawCube(finalPath[i], Vector3.one);
            }
        }
    }
}
