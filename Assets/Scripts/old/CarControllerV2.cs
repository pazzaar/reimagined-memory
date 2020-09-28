using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CarState { DRIVE, REVERSE, BRAKE, PARK, DRIFT }

namespace test
{
	public class CarControllerV2 : MonoBehaviour
	{
		[Header("Car performance variables")]
		[Tooltip("Y - Acceleration strength, X - Current speed")]
		public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 50, 80, 0);
		[Tooltip("Y - Steer strength, X - Current speed")]
		public AnimationCurve steeringCurve = AnimationCurve.Linear(0.0f, 1f, 60, .4f);
		public float carGrip = 1;
		public float wheelRadius = .37f;
		[Tooltip("How much do wheel meshs rotate when steering")]
		public float visualWheelSteerRotation = 20;
		[Tooltip("This is so the car doesn't turn on the spot, it needs to be moving to start turning")]
		public float fullTurnSpeed = 5;
		public float downforce = 100;
		public float steerPower = 5;
		public Transform steeringTransform;
		public float angularTraction = 10;
		public float sidewaysTraction = 1;

		[Header("Drift variables")]
		public float inputThreshold = .01f;
		public float minDriftSpeed = 30;
		[Tooltip("How effective is horizontal input when drifting")]
		public float driftStrength = .5f;
		public float driftStrengthVisualMultipler = 12;
		[Tooltip("How much is always drifting")]
		public float driftOffset = 1;
		public float visualDriftOffsetVisualMultipler = 10;
		[Tooltip("How fast does the visual mesh rotate to 'drift'")]
		public float driftEnableSpeed = .3f;
		[Tooltip("How much horizontal input in the other direction to cancel drifting")]
		public float driftCancelHorizontalInput = .3f;
		public float limitedHorizontalChangeRate = 10f;
		public float limitedHorizontalChangeRateDrift = 3f;

		[Header("Telemetry")]
		public bool debug = true;
		public float currentSpeed;
		public float verticalInput;
		public float horizontalInput;
		public CarState state = CarState.PARK;
		public float _dampDriftOffset = 0;
		public float _dampDriftStrength = 1;
		public float _dampVisualHorizontalMultipler = 0;
		public float slidingAngle;
		public bool driftingRight;

		//Private stuff
		private float _dampDriftOffsetTarget = 0;
		private float _dampDriftOffsetVelocity = 0;
		private float _dampDriftStrengthTarget = 0;
		private float _dampDriftStrengthVelocity = 0;
		private float _dampVisualHorizontalMultiplerTarget = 0;
		private float _dampVisualHorizontalMultiplerVelocity = 0;
		private float _wheelPower = 1;
		private bool addGrip;

		//Gameobjects
		private GameObject visualMesh;
		public GameObject[] wheels;
		public GameObject[] wheelMeshs;
		public GameObject bodyMesh;
		private Rigidbody rb;

		//Inputs
		private float _vertical;
		private float _horizontal;
		public float _limitedHorizontalChangeRate = 1f; //temporary value
		private bool _driftInput;

		public float debugTurn;

		void Start()
		{
			rb = GetComponent<Rigidbody>();
			visualMesh = transform.Find("Mesh").gameObject;
			var com = transform.Find("CenterOfMass") as Transform;
			rb.centerOfMass = com.localPosition;

			Debug.Assert(rb != null); //must be set
			Debug.Assert(visualMesh != null); //must be set
			Debug.Assert(bodyMesh != null); //must be set
			//Debug.Assert(wheels != null); //must be set
			Debug.Assert(wheelMeshs != null); //must be set
		}

		void FixedUpdate()
		{
			_vertical = Input.GetAxis("Vertical");
			_horizontal = Mathf.Lerp(_horizontal, Input.GetAxis("Horizontal"), _limitedHorizontalChangeRate * Time.deltaTime);
			_driftInput = Input.GetKey(KeyCode.LeftControl);

			currentSpeed = transform.InverseTransformDirection(rb.velocity).z;

			_wheelPower = 1;
			Downforce();
			Grip();

			//If drifting criteria fulfilled, then drift
			if (_driftInput && _vertical > inputThreshold && currentSpeed > minDriftSpeed && (_horizontal != 0 || state == CarState.DRIFT) && _wheelPower == 1)
			{
				Drift(); //This function changes _driftOffset and _driftStrength up to the target values
			}
			else if (state == CarState.DRIFT && _vertical > inputThreshold && currentSpeed > minDriftSpeed && _wheelPower == 1)
			{
				if ((driftingRight && _horizontal > -driftCancelHorizontalInput) || (!driftingRight && _horizontal < driftCancelHorizontalInput))
				{
					Drift(); 
				}
				else
				{
					Accelerate();
				}
			}
			else
			{
				Accelerate(); //Resets them back
			}

			horizontalInput = (_horizontal * _dampDriftStrength) + _dampDriftOffset; //add _driftOffset to horizontal input 
			verticalInput = _vertical;

			//Add acceleration to car
			float forwardPower = verticalInput * accelerationCurve.Evaluate(currentSpeed) * _wheelPower;
			Vector3 vel = rb.velocity;
			vel += forwardPower * Time.deltaTime * rb.transform.forward;
			rb.velocity = vel;
			
			var steerSpeed = steeringCurve.Evaluate(currentSpeed);

			//rotate car to point in correct direction
			//That clamp/fullturnspeed stuff is to stop the car spinning on the spot when its going too slowly.
			var turningPower = horizontalInput * steerSpeed * (Mathf.Clamp(currentSpeed, -fullTurnSpeed, fullTurnSpeed) / fullTurnSpeed) * _wheelPower;
			var turnAngle = Quaternion.AngleAxis(Mathf.Rad2Deg * turningPower * Time.deltaTime, rb.transform.up); //This is the same as... --->
			rb.transform.Rotate(turnAngle.eulerAngles);

			//Rotate the car velocity (basically make the direction of the velocity point to wherever the car is pointed towards
			var gripTurnAngle = Quaternion.AngleAxis(Mathf.Rad2Deg * turningPower * Time.deltaTime * carGrip * _wheelPower, rb.transform.up); //This! but we add grip to this one as well. <---
			Vector3 v = rb.velocity;
			v = gripTurnAngle * v;
			rb.velocity = v;


			//debugTurn = turningPower;

			//If the car's actual velocity is not in line with the rigidbody's velocity (0.01 radians away), slowly align it
			if (slidingAngle > 0.01 && _wheelPower == 1)
				rb.velocity = Vector3.RotateTowards(rb.velocity, rb.transform.forward, .025f, 0); //Yeah these are magic numbers, sue me. Rotate the velocity vector back at .01 radians every update

			//Rotate the car mesh so its 'drifting' lol
			var visualTurnAngle = Quaternion.AngleAxis(Mathf.Rad2Deg * ((_horizontal * (_dampDriftStrength * driftStrengthVisualMultipler)) + (_dampDriftOffset * visualDriftOffsetVisualMultipler)) * Time.deltaTime * _dampVisualHorizontalMultipler, visualMesh.transform.up);
			visualMesh.transform.localRotation = visualTurnAngle;

			//Rotate the body a little to give the illusion of weight shifting
			//Want to lean only a a degree or so either way, so I divide forward power by the maximum possible forward power (Evalute 0)
			var leanAngleForwards = Quaternion.AngleAxis(Mathf.Rad2Deg * (forwardPower / accelerationCurve.Evaluate(0)) * Time.deltaTime, Vector3.forward); 
			var leanAngleSide = Quaternion.AngleAxis(Mathf.Rad2Deg * turningPower * Time.deltaTime, Vector3.right);
			bodyMesh.transform.localRotation = leanAngleForwards * leanAngleSide;

			//Damp stuff so the velocity changes all smoothly transition
			_dampDriftOffset = Mathf.SmoothDamp(_dampDriftOffset, _dampDriftOffsetTarget, ref _dampDriftOffsetVelocity, driftEnableSpeed);
			_dampDriftStrength = Mathf.SmoothDamp(_dampDriftStrength, _dampDriftStrengthTarget, ref _dampDriftStrengthVelocity, driftEnableSpeed);
			_dampVisualHorizontalMultipler = Mathf.SmoothDamp(_dampVisualHorizontalMultipler, _dampVisualHorizontalMultiplerTarget, ref _dampVisualHorizontalMultiplerVelocity, driftEnableSpeed);

			//Debug stuff
			if (debug)
			{
				Debug.DrawRay(rb.position + (rb.velocity * Time.deltaTime), transform.up, Color.red);
				Debug.DrawLine(rb.position + (rb.velocity * Time.deltaTime), rb.position + rb.velocity, Color.green);
			}
		}

		void Update()
		{
			
		}

		/*
		private void WheelPower()
		{
			_wheelPower = 0;
			for (int i = 0; i < wheels.Length; i++)
			{
				if (debug)
					Debug.DrawRay(wheels[i].transform.position + (rb.velocity * Time.deltaTime), -wheels[i].transform.up * wheelRadius, Color.green);

				if (Physics.Raycast(wheels[i].transform.position + (rb.velocity * Time.deltaTime), -wheels[i].transform.up, out RaycastHit hit, wheelRadius))
				{
					if (i < 2)
					{
						var turnAngle = Quaternion.AngleAxis(Mathf.Rad2Deg * _horizontal * (Mathf.Clamp(currentSpeed, 0, fullTurnSpeed) / fullTurnSpeed) * steeringCurve.Evaluate(currentSpeed) * visualWheelSteerRotation * Time.deltaTime, visualMesh.transform.up);
						//wheelMeshs[i].transform.localRotation = turnAngle;
					}
					wheelMeshs[i].transform.Rotate(0, 0, Mathf.Rad2Deg * (-currentSpeed / wheelRadius) * Time.deltaTime, Space.Self);
					_wheelPower += .25f;
				}
			}
		}
		*/

		private void Downforce()
		{
			rb.AddForce(-transform.up * downforce * currentSpeed * _wheelPower);
		}

		private void Grip()
		{
			var rigidBodyDirection = rb.velocity.normalized;
			var rigidBodyDirectionTarget = rb.transform.forward.normalized * Mathf.Sign(currentSpeed);
			slidingAngle = Mathf.Deg2Rad * Vector3.Angle(rigidBodyDirection, rigidBodyDirectionTarget);
		}

		private void Drift()
		{
			if (state != CarState.DRIFT)
			{
				if (Input.GetAxis("Horizontal") > 0)
				{
					driftingRight = true;
					_dampDriftOffsetTarget = driftOffset + driftStrength;
				}
				else
				{
					driftingRight = false;
					_dampDriftOffsetTarget = -(driftOffset + driftStrength);
				}

				_dampVisualHorizontalMultiplerTarget = 1;
				_dampDriftStrengthTarget = driftStrength;
				//_limitedHorizontalChangeRate = limitedHorizontalChangeRateDrift;
			}

			if (Input.GetAxis("Horizontal") == 0) //Anoying workaround for when horizontal input is nothing
			{
				if (state == CarState.DRIFT)
					_limitedHorizontalChangeRate = limitedHorizontalChangeRateDrift;
			}
			else if (Input.GetAxis("Horizontal") > 0)
			{
				if (!driftingRight)
				{
					_limitedHorizontalChangeRate = limitedHorizontalChangeRate;
				}
				else
				{
					_limitedHorizontalChangeRate = limitedHorizontalChangeRateDrift;
				}
			}
			else if (Input.GetAxis("Horizontal") < 0)
			{
				if (driftingRight)
				{
					_limitedHorizontalChangeRate = limitedHorizontalChangeRate;
				}
				else
				{
					_limitedHorizontalChangeRate = limitedHorizontalChangeRateDrift;
				}
			}

			state = CarState.DRIFT;
		}

		private void Accelerate()
		{
			state = CarState.DRIVE;
			_dampDriftOffsetTarget = 0;
			_dampDriftStrengthTarget = 1; 
			_dampVisualHorizontalMultiplerTarget = 0;
			_limitedHorizontalChangeRate = limitedHorizontalChangeRate;
		}
	}
}