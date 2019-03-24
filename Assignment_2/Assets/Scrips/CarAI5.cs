using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI5 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;
        public GameObject pathFinder;

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
        private int currentPathIndex = 0;
        private readonly int distanceOffset = 3;
        private List<Vector3> finalPath;
        private int finalPathIndex;
        Vector3 previousPoint;
        Vector3 followPoint;

        public bool leader;
        int carNumber;
        Vector3 offset = Vector3.zero;
        public float angle;

        private void Start()
        {

            Time.timeScale = 1;
            maxVelocity = leader ? 10f : 20f;
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
            // get the car controller
            m_Car = GetComponent<CarController>();
            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();

            InitializeCSpace();

            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            carNumber = 0;
            for (int i = 0; i < friends.Length; ++i)
            {
                if (friends[i].name == this.name)
                {
                    carNumber = i;
                    break;
                }
            }
            angle = 120;

            finalPathIndex = 0;
            finalPath = pathFinder.GetComponent<CreateGridCost>().path[finalPathIndex];

        }


        private void FixedUpdate()
        {

            bool leaderAlive = false;

            foreach (GameObject car in friends)
            {
                if (car == null) continue;
                if (car.GetComponent<CarAI5>().leader)
                {
                    leaderAlive = true;
                }
            }

            foreach (GameObject car in friends)
            {
                if (!leaderAlive) {
                    leader = true;
                    break;
                }
            }

            previousPoint = followPoint;
            followPoint = FindFollowPoint();
            float pointVelocity = Vector3.Distance(previousPoint, followPoint) / Time.deltaTime;

            if (leader)
            {
                followPoint = FindLeaderFollowPoint();

                if (Vector3.Distance(transform.position, followPoint) < 2f)
                {
                    foreach (GameObject car in friends)
                    {
                        if (car == null) continue;
                        car.GetComponent<CarAI5>().finalPathIndex++;
                        car.GetComponent<CarAI5>().finalPath = pathFinder.GetComponent<CreateGridCost>().path[finalPathIndex];
                    }
                    followPoint = FindLeaderFollowPoint();
                }


            }
            else
            {
                followPoint = FindFollowPoint();
            }

            if (!crashed)
            {
                if(!leader || currentPathIndex < finalPath.Count - 1) 
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
                }
                steerDirection = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);
                accelerationDirection = AccelerationInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);

                if (m_Car.CurrentSpeed >= pointVelocity + Vector3.Distance(followPoint, m_Car.transform.position))
                {
                    accelerationDirection = 0;
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
                    steerDirection = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);
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

        private bool IsPathFree(Vector3 position, Vector3 target)
        {
            Vector3 direction = target - position;
            float distance = Vector3.Distance(position, target);
            RaycastHit hit;
            bool isHit = Physics.Raycast(position + new Vector3(0f,2f,0f), direction, out hit, distance);
            return !isHit;
        }


        private Vector3 FindLeaderFollowPoint()
        {

            for(int i = finalPath.Count - 1; i >= 0; i--)
            {
                if(IsPathFree(transform.position, finalPath[i]))
                {
                    return finalPath[i];
                }
            }

            return finalPath[finalPath.Count - 1];
        }

        private Vector3 FindFollowPoint()
        {
            Transform leaderCar = transform;
            int leaderCarNumber = 0;
            foreach(GameObject friend in friends)
            {
                if(friend.GetComponent<CarAI5>().leader)
                {
                    leaderCar = friend.transform;
                    leaderCarNumber = friend.GetComponent<CarAI5>().carNumber;
                    break;
                }
            }

            float invLerpSpeed = 1f;

            int dir = 1;
            if(carNumber == (leaderCarNumber + 2) % 3)
            {
                dir = -1;
            }

            offset = Vector3.Lerp(offset, Quaternion.AngleAxis(dir * angle, leaderCar.up) * -leaderCar.forward * 4.5f, Time.deltaTime / invLerpSpeed);


            return leaderCar.position + offset;
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
            if(!Application.isPlaying)
            {
                return;
            }

            if (leader)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(followPoint, Vector3.one * 3);
                return;
            }

            Transform leaderCar = transform;
            foreach (GameObject friend in friends)
            {
                if (friend.GetComponent<CarAI5>().leader)
                {
                    leaderCar = friend.transform;
                    break;
                }
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(leaderCar.position + offset, 1);
        }
    }
}
