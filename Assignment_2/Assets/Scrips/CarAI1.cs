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

        private void Start()
        {
            Time.timeScale = 1;
            maxVelocity = 150;
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


            initial_positions = GetInitPos(friends);

            foreach (var pos in initial_positions)
            {
                Debug.Log(pos.ToString());
            }

            int carNumber = 0;
            for(int i = 0; i < friends.Length; ++i)
            {
                if(friends[i].name == this.name)
                {
                    carNumber = i;
                    break;
                }
            }

            mstPath = mstCreator.GetComponent<CreateMST>().paths[carNumber];
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
                if (Mathf.Abs(steerDirection) < 0.2f)
                {
                    steerDirection = 0;
                }
                brake = 0;
                if (Mathf.Abs(steerDirection) > 0.8f && m_Car.CurrentSpeed > maxVelocity / 10)
                {
                    accelerationDirection = 0;
                    if (m_Car.CurrentSpeed > maxVelocity / 5)
                    {
                        handBrake = 1;
                    }
                    else
                    {
                        handBrake = 0;
                    }
                }
                else
                {
                    accelerationDirection = AccelerationInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, finalPath[currentPathIndex]);
                    handBrake = 0;
                }
                if (currentPathIndex < finalPath.Count - 1)
                {
                    int stepsToCheck = Mathf.Min(3 + (int)(m_Car.CurrentSpeed * 1.6f * 1.6f * m_Car.CurrentSpeed / 500), finalPath.Count - 1 - currentPathIndex);
                    for (int i = 1; i <= stepsToCheck; ++i)
                    {
                        float steerCheck = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, finalPath[currentPathIndex + i]);
                        if (Mathf.Abs(steerCheck) > 0.8f && (m_Car.CurrentSpeed * 1.6f * 1.6f * m_Car.CurrentSpeed) >= Vector3.Distance(m_Car.transform.position, finalPath[currentPathIndex + i]) * 125 * 0.8f)
                        {
                            accelerationDirection = 0;
                            brake = 1;
                            break;
                        }
                    }
                }

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
            for(int i = 0; i <= currentPathIndex; ++i)
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
