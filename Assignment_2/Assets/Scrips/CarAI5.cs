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

        public enum Formation {Drive, Attack}

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

        private readonly float EPSILON = 0.01f;
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
        public Formation formation;

        public bool leader;
        public bool inFormation = false;
        public bool reachedTurret = false;
        int carNumber;
        Vector3 offset = Vector3.zero;
        float angle;

        private void Start()
        {

            Time.timeScale = 1;
            maxVelocity = 10f;
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
            crashCheckTime = 0.25f;
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
            angle = 90;

            finalPathIndex = 0;
            finalPath = pathFinder.GetComponent<CreateGridCost>().path[finalPathIndex];
            currentPathIndex = 15;

            Debug.Log(finalPath.Count);
        }

        private void Stop()
        {
            crashed = false;
            crashTime = 0;
            if (Math.Abs(m_Car.GetComponent<Rigidbody>().velocity.magnitude) > EPSILON)
            {
                m_Car.Move(0f, 0f, 0f, 1);
            }
        }




        private void FixedUpdate()
        {

            bool allInFormation = true;
            foreach (GameObject car in friends)
            {
                if (!car.GetComponent<CarAI5>().inFormation)
                {
                    allInFormation = false;
                }
            }

            Vector3 followPoint;
            if (leader)
            {

                if (currentPathIndex >= finalPath.Count - 5)
                {
                    if (!reachedTurret)
                    {
                        foreach (GameObject car in friends)
                        {
                            car.GetComponent<CarAI5>().inFormation = false;
                        }
                        reachedTurret = true;
                    }

                    inFormation = true;

                }

                if (inFormation && !allInFormation)
                {
                    Stop();
                    return;
                }

                if (Vector3.Distance(m_Car.transform.position, finalPath[currentPathIndex]) < 2f &&
                    currentPathIndex < finalPath.Count - 1)
                {
                    currentPathIndex++;

                }
                if (currentPathIndex >= finalPath.Count - 1)
                {
                    finalPathIndex++;
                    finalPath = pathFinder.GetComponent<CreateGridCost>().path[finalPathIndex];
                    currentPathIndex = 0;
                    reachedTurret = false;

                    foreach(GameObject car in friends)
                    {
                        car.GetComponent<CarAI5>().inFormation = false;
                    }
                }
                followPoint = finalPath[currentPathIndex];
            }
            else
            {
                bool leaderInFormation = false;

                foreach (GameObject car in friends)
                {
                    if (car.GetComponent<CarAI5>().leader && car.GetComponent<CarAI5>().inFormation)
                    {
                        leaderInFormation = true;
                        break;
                    }
                }

                if (!leaderInFormation)
                {
                    return;
                }

                followPoint = FindFollowPoint();

                if (Vector3.Distance(transform.position, followPoint) < 2f)
                {
                    inFormation = true;
                }

                if (inFormation && !allInFormation)
                {
                    Stop();
                    return;
                }
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
                if (crashTime <= crashCheckTime)
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


            int dir = -1;
            if(carNumber == (leaderCarNumber + 2) % 3)
            {
                dir = 1;
            }



            offset = Vector3.Lerp(offset, Quaternion.AngleAxis(angle * dir, leaderCar.up) * -leaderCar.forward * 5f, Time.deltaTime / invLerpSpeed);

            return leaderCar.position + offset;
        }

        private bool CheckSpacing(Transform leaderCar, float spacing)
        {
            Vector3 pos = leaderCar.position + Quaternion.AngleAxis(-angle, leaderCar.up) * -Vector3.forward * spacing;
            Vector3 pos2 = leaderCar.position + Quaternion.AngleAxis(angle, leaderCar.up) * -Vector3.forward * spacing;
            return terrain_manager.myInfo.traversability[terrain_manager.myInfo.get_i_index(pos.x), terrain_manager.myInfo.get_j_index(pos.z)] > 0.5f
                    || terrain_manager.myInfo.traversability[terrain_manager.myInfo.get_i_index(pos2.x), terrain_manager.myInfo.get_j_index(pos2.z)] > 0.5f;
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
            if(!Application.isPlaying || leader)
            {
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
