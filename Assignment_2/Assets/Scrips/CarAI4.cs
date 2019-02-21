using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarAI4 : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use

        public GameObject terrain_manager_game_object;
        TerrainManager terrain_manager;

        public GameObject[] friends;
        public GameObject[] enemies;

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

        Vector3 followPoint;
        Vector3 previousPoint;
        int carNumber;

        int index_leader;

        private void Start()
        {
            Time.timeScale = 1;
            maxVelocity = 200;
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
            crashCheckTime = 1f;
            crashDirection = 0;
            previousPosition = Vector3.up;

            terrain_manager = terrain_manager_game_object.GetComponent<TerrainManager>();
            m_Car = GetComponent<CarController>();

            InitializeCSpace();


            // note that both arrays will have holes when objects are destroyed
            // but for initial planning they should work
            friends = GameObject.FindGameObjectsWithTag("Player");
            enemies = GameObject.FindGameObjectsWithTag("Enemy");

            carNumber = 0;
            for (int i = 0; i < friends.Length; ++i)
            {
                // We might find a better way to identify the replay car
                if (friends[i].name == "ReplayCar (2)")
                {
                    this.index_leader = i;
                }
                if (friends[i].name == this.name)
                {
                    carNumber = i;
                    break;
                }
            }
        }

        private void FixedUpdate()
        {
            previousPoint = followPoint;
            followPoint = GameObject.FindGameObjectsWithTag("Player")[index_leader].transform.position; // FindFollowPoint();
            float pointVelocity = Vector3.Distance(previousPoint, followPoint) / Time.deltaTime;

            steerDirection = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);
            accelerationDirection = AccelerationInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);

            if ((m_Car.CurrentSpeed >= pointVelocity && Vector3.Distance(followPoint, m_Car.transform.position) < 1) || 
                m_Car.CurrentSpeed >= maxVelocity)
            {
                accelerationDirection = 0;
            }

            if (accelerationDirection < 0)
            {
                m_Car.Move(steerDirection, brake, accelerationDirection * acceleration, handBrake);
            }
            else
            {
                m_Car.Move(steerDirection, accelerationDirection * acceleration, -brake, handBrake);
            }
        }

        private Vector3 FindFollowPoint()
        {
            Vector3 averagePosition = Vector3.zero;
            float averageAngle = 0;
            foreach(GameObject friend in friends)
            {
                averagePosition += friend.transform.position;
                averageAngle += friend.transform.eulerAngles.y;
            }
            averagePosition /= friends.Length;
            averageAngle /= friends.Length;
            averageAngle *= Mathf.Deg2Rad;

            Vector3 offset;
            switch(carNumber)
            {
                case 0:
                    offset = new Vector3(0, 0, -25);//new Vector3(-10 * Mathf.Cos(averageAngle), 0, 10 * Mathf.Sin(averageAngle));
                    break;
                case 1:
                    offset = new Vector3(0, 0, -12.5f);
                    break;
                case 2:
                    offset = new Vector3(0, 0, 12.5f);
                    break;
                default:
                    offset = new Vector3(0, 0, 25);//new Vector3(10 * Mathf.Cos(averageAngle), 0, 10 * Mathf.Sin(averageAngle));
                    break;
            }

            return GameObject.FindWithTag("Point").transform.position + offset;
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

        //private void PerfectFormationPositions()
        //{

        //}

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            

            Gizmos.color = Color.blue;
            float car_length = 4.47f, car_width = 2.43f, car_high = 2f;
            float scale = 1.5f;
            var car_leader = GameObject.FindGameObjectsWithTag("Player")[index_leader];
            Vector3 pos_leader = car_leader.transform.position;

            Gizmos.matrix = car_leader.transform.localToWorldMatrix;

            Vector3 cube_size = new Vector3(car_width*scale, car_high*scale, car_length*scale);

            Gizmos.DrawWireCube(Vector3.zero, cube_size);




        }
    

    }

    
}
