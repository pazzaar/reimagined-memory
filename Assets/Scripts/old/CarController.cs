using UnityEngine;
using System.Collections;
using LivioDeLaCruz.RacingTest5.car;

namespace LivioDeLaCruz.RacingTest5.car
{
	public enum CarState { DRIVE, REVERSE, BRAKE, PARK, DRIFT }

	public class CarController : MonoBehaviour
	{
		//inspector fields

		[Header("Controls: Acceleration")]
		public float accelerationStrength;
		public float maxSpeed;
		public float inputThreshold;
		public float dragStrength;
		public float brakeStrength;

		[Header("Controls: Steering")]
		public float maxSteeringAngleSlow;
		public float maxSteeringAngleFast;
		public float minDriftSpeed;
		public float driftSteeringRange;
		public float driftVisualAngle;
		public float driftVisualAngleRange;

		[Header("Visuals")]
		public GameObject visualsParent;
		public CarWheel[] wheels;
		public float wheelRadius;

		[Header("Telemetry")]
		public float currentSpeed = 0;
		public float currentSteeringAngle = 0;
		public bool lockSteering = false;
		public float axleDistance;
		public CarState state = CarState.PARK;
		public Vector3 forward;

		public Vector3 velocity = Vector3.zero;
		public float velocityMagnitude = 0;
		public Vector3 acceleration = Vector3.zero;
		public float accelerationMagnitude = 0;

		//internals

		private Rigidbody rb;

		private float currentRadius = 0;
		private float innerRadius = 0;
		private float currentDriftDelta = 0;
		private bool isDriftingRight = false;
		private Vector3 currentCircleCenter = Vector3.zero;

		private Vector3 lastPosition = Vector3.zero;

		void Start()
		{
			rb = GetComponent<Rigidbody>();
			updateWheelData();
			axleDistance = calculateAxleDistance();
		}

		private float calculateAxleDistance()
		{
			float low = 777, high = 777; //What?
			foreach (CarWheel w in wheels)
			{
				float z = w.transform.position.z;
				if (low == 777 || z < low)
					low = z;
				if (high == 777 || z > high)
					high = z;
			}
			return high - low;
		}


		void Update()
		{
		}
		
		void FixedUpdate()
		{
			lastPosition = transform.position;

			//telemetry
			forward = transform.forward;

			//update speed controls
			float input = Input.GetAxis("Vertical");
			bool brakes = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			if (brakes && input > inputThreshold && currentSpeed > minDriftSpeed && (Input.GetAxis("Horizontal") != 0 || state == CarState.DRIFT))
				maintainDriftSpeed();
			else if (brakes || (currentSpeed < 0 && input > inputThreshold) || (currentSpeed > 0 && input < -inputThreshold))
				slowDown(true);
			else if (input < inputThreshold && input > -inputThreshold)
				slowDown();
			else
				accelerate(input);

			//update turning controls
			if (!lockSteering)
			{
				//Ok so to clear this up, we get the horizontal input, change that based on how fast ur going (current speed out of max speed)
				//and then we want to apply that ratio to somewhere in our steering range. E.G. if slow speed angle is 8 and fast speed angle is 2, then
				// I want ratio to fall somewhere between that. So I find multipler from 0-1, and multiply that on 6 (difference between 8 and 2). I then
				// add the 2 to get that to between 2 and 8. So if multipler is 50%, then I get 1/3*6 which is 3, then add 2 so it is 5, which falls between 2 and 8.
				currentSteeringAngle = (state != CarState.DRIFT ? Input.GetAxis("Horizontal") : (isDriftingRight ? 1 : -1)) * ((currentSpeed / maxSpeed) * (maxSteeringAngleFast - maxSteeringAngleSlow) + maxSteeringAngleSlow); 
	
				if (state == CarState.DRIFT)
				{
					//Ok so if ur drifting, you wanna add a little either to the left or to the right to make ur turning circle wider or narrower.
					//So use some random defined range above and add it
					currentDriftDelta = Input.GetAxis("Horizontal") * driftSteeringRange;
					currentSteeringAngle = currentSteeringAngle + currentDriftDelta;
				}
			}

            if(Input.GetKey(KeyCode.Space))
            {
				//LOL WUT????
                rb.AddForce(transform.up * -50);
                rb.AddTorque(transform.up * 10);
            }

			rb.AddForce(transform.up * -50);

			//update physics
			updateCarPosition();

			if (Input.GetKey(KeyCode.L))
				lockSteering = true;
			else if(Input.GetKey(KeyCode.U))
				lockSteering = false;

			updateWheelData();
		}

		private void slowDown(bool isBraking = false)
		{
			float decel = (isBraking ? brakeStrength : dragStrength);
			if (currentSpeed > 0)
			{
				currentSpeed -= decel;
				if (currentSpeed < 0)
				{
					currentSpeed = 0;
					state = CarState.PARK;
				}
				else
				{
					state = CarState.DRIVE;
				}
			}
			else if (currentSpeed < 0)
			{
				currentSpeed += decel;
				if (currentSpeed > 0)
				{
					currentSpeed = 0;
					state = CarState.PARK;
				}
				else
				{
					state = CarState.REVERSE;
				}
			}
			else
			{
				state = CarState.PARK;
			}
			if(isBraking && state != CarState.PARK)
			{
				state = CarState.BRAKE;
			}
		}

		private void accelerate(float input)
		{
			currentSpeed += input * accelerationStrength;
			if (currentSpeed > maxSpeed)
				currentSpeed = maxSpeed;
			else if (currentSpeed < -maxSpeed)
				currentSpeed = -maxSpeed;
			if (currentSpeed > 0)
				state = CarState.DRIVE;
			else
				state = CarState.REVERSE;
		}

		private void maintainDriftSpeed()
		{
			if(state != CarState.DRIFT)
			{
				isDriftingRight = Input.GetAxis("Horizontal") > 0;
			}
			state = CarState.DRIFT;
		}

		private void updateWheelData()
		{
			foreach (CarWheel w in wheels)
			{
				w.state = state;
				if (w.steers)
					w.steeringAngle = currentSteeringAngle;
				if (Application.isEditor)
					w.wheelRadius = wheelRadius;
			}
		}

		private void updateCarPosition()
		{
			if (currentSteeringAngle == 0 && state != CarState.DRIFT)
			{
				transform.position += transform.forward * currentSpeed * Time.deltaTime;
				currentRadius = 0;
				innerRadius = 0;
			}
			else
			{
				//DEBUGGING
				if (!lockSteering)
					currentRadius = calculateSteeringRadius();

				float neg = 1;
				if (currentSteeringAngle < 0)
					neg = -1;
				float circumference = 2 * Mathf.PI * currentRadius; //calc circumference from radius... ez
				float distanceToTravel = currentSpeed * Time.deltaTime;
				float percentRevolutions = distanceToTravel / circumference;
				float degreesToTravel = percentRevolutions * 360f;

				float rightToTravel = currentRadius * (1 - Mathf.Cos(degreesToTravel * Mathf.PI / 180));
				float forwardToTravel = currentRadius * Mathf.Sin(degreesToTravel * Mathf.PI / 180);
				transform.position += transform.right * rightToTravel * neg;
				transform.position += transform.forward * forwardToTravel;

				transform.Rotate(new Vector3(0, degreesToTravel * neg, 0));
			}

			if (state == CarState.DRIFT)
			{
				visualsParent.transform.localRotation = Quaternion.Euler(new Vector3(0, driftVisualAngle * (isDriftingRight ? 1 : -1) + Input.GetAxis("Horizontal") * driftVisualAngleRange, 0));
			}
			else
			{
				visualsParent.transform.localRotation = Quaternion.Euler(Vector3.zero);
			}

			Vector3 lastVelocity = velocity;
			velocity = (transform.position - lastPosition) / Time.deltaTime;
			velocityMagnitude = velocity.magnitude;

			acceleration = (velocity - lastVelocity) / Time.deltaTime;
			accelerationMagnitude = acceleration.magnitude;


		}

		private float calculateSteeringRadius()
		{
			innerRadius = (axleDistance / 2) / Mathf.Cos((90 - currentSteeringAngle) * Mathf.PI / 180); //Do some trig and find the radius??
			int index = 0; //front left wheel
			if (currentSteeringAngle > 0)
				index = 1; //front right wheel
			CarWheel w = wheels[index]; //Workout which way we're turning, so do we want the inner circle coming out of the left or the right tire, depending on ur direction
			currentCircleCenter = w.transform.position + w.transform.right * innerRadius; //Get wheel position, point right, and add the 
			return (currentCircleCenter - transform.position).magnitude;
		}

		void OnDrawGizmos()
		{
			UnityEditor.Handles.color = Color.blue;
			UnityEditor.Handles.DrawWireDisc(currentCircleCenter, transform.up, currentRadius);
			UnityEditor.Handles.color = Color.cyan;
			UnityEditor.Handles.DrawWireDisc(currentCircleCenter, transform.up, innerRadius);
		}

	}
}