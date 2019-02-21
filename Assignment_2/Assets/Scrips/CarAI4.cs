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
        float angle;
        float spacing;
        Vector3 offset = Vector3.zero;

        private void Start()
        {
            Time.timeScale = 1;
            maxVelocity = 30;
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

            angle = 90;
            spacing = 18f;

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
        }

        private void FixedUpdate()
        {
            previousPoint = followPoint;
            followPoint = FindFollowPoint();
            float pointVelocity = Vector3.Distance(previousPoint, followPoint) / Time.deltaTime;

            steerDirection = SteerInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);
            accelerationDirection = AccelerationInput(m_Car.transform.position, m_Car.transform.eulerAngles.y, followPoint);


            if ((m_Car.CurrentSpeed >= pointVelocity && Vector3.Distance(followPoint, m_Car.transform.position) < 5) ||
                m_Car.CurrentSpeed >= maxVelocity)

                if (m_Car.CurrentSpeed >= pointVelocity + Vector3.Distance(followPoint, m_Car.transform.position))
                {
                    accelerationDirection = 0;
                }
                else if (m_Car.CurrentSpeed >= maxVelocity)
                    m_Car.Move(-steerDirection, brake, accelerationDirection * acceleration, handBrake);
            if (Vector3.Distance(followPoint, m_Car.transform.position) < 5)
            {
                m_Car.Move(steerDirection, brake, accelerationDirection * acceleration, handBrake);
            }
            else
            {
                m_Car.Move(-steerDirection, brake, accelerationDirection * acceleration, handBrake);
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

            Transform leader = GameObject.FindWithTag("leader").transform;

            float actualSpacingLeft = spacing;
            float actualSpacingRight = spacing;
            while(terrain_manager.myInfo.traversability[terrain_manager.myInfo.get_i_index((leader.position + Quaternion.AngleAxis(angle, leader.up) * -leader.forward * 3 * actualSpacingLeft).x), terrain_manager.myInfo.get_j_index((leader.position + Quaternion.AngleAxis(angle, leader.up) * -leader.forward * 3 * actualSpacingLeft).z)] > 0.5f)
            {
                actualSpacingLeft -= spacing / 3f;
            }
            while (terrain_manager.myInfo.traversability[terrain_manager.myInfo.get_i_index((leader.position + Quaternion.AngleAxis(-angle, leader.up) * -leader.forward * 3 * actualSpacingRight).x), terrain_manager.myInfo.get_j_index((leader.position + Quaternion.AngleAxis(-angle, leader.up) * -leader.forward * 3 * actualSpacingRight).z)] > 0.5f)
            {
                actualSpacingRight -= spacing / 3f;
            }

            switch (carNumber)
            {
                case 0:
                    offset = Vector3.Lerp(offset, Quaternion.AngleAxis(angle, leader.up) * -leader.forward * actualSpacingLeft, Time.deltaTime);
                    break;
                case 1:
                    offset = Vector3.Lerp(offset, Quaternion.AngleAxis(angle, leader.up) * -leader.forward * 3 * actualSpacingLeft, Time.deltaTime);
                    break;
                case 2:
                    offset = Vector3.Lerp(offset, Quaternion.AngleAxis(-angle, leader.up) * -leader.forward * actualSpacingRight, Time.deltaTime);
                    break;
                default:
                    offset = Vector3.Lerp(offset, Quaternion.AngleAxis(-angle, leader.up) * -leader.forward * 3 * actualSpacingRight, Time.deltaTime);
                    break;
            }

            return leader.position + offset;
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
            float car_length = 4.47f, car_width = 2.43f, car_high = 2f;
            float scale = 1f;
            Vector3 cube_size = new Vector3(car_width * scale, car_high * scale, car_length * scale);

            Gizmos.color = Color.blue;

            Transform leader = GameObject.FindWithTag("leader").transform;
            Gizmos.DrawSphere(leader.position + offset, 1f);

        }
    }
}
