using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularCar
{
	class ForceSteering : MonoBehaviour
	{
		public CarControllerV3 control;
		public Rigidbody rb;
		public InputController input;
		public Transform steeringTransform;

		public CarState state = CarState.DRIVE;
		private bool driftingRight;

		[Header("Steering")]
		[Tooltip("Y - Steer strength, X - Current speed")]
		public AnimationCurve steeringCurve = AnimationCurve.Linear(0.0f, 1f, 60, .4f);
		[Tooltip("This is so the car doesn't turn on the spot, it needs to be moving to start turning")]
		public float fullTurnSpeed = 5;
		public float steerPower = 15;
		public float angularTraction = 15;

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
		public float verticalInput;
		public float horizontalInput;
		public float _dampDriftOffset = 0;
		public float _dampDriftStrength = 1;
		public float _dampVisualHorizontalMultipler = 0;
		public float slidingAngle;
		public float turningPower;

		//This is for smoothly transitioning between DRIVE and DRIFT mode
		private float _dampDriftOffsetTarget = 0;
		private float _dampDriftOffsetVelocity = 0;
		private float _dampDriftStrengthTarget = 0;
		private float _dampDriftStrengthVelocity = 0;
		private float _dampVisualHorizontalMultiplerTarget = 0;
		private float _dampVisualHorizontalMultiplerVelocity = 0;
		private float _wheelPower = 1;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			input = this.GetComponent<InputController>();
			steeringTransform = control.steeringTransform;
			rb = control.rb;

			Debug.Assert(control != null, "Must have a controller");
			Debug.Assert(input != null, "Must have an input controller");
		}

		private void FixedUpdate()
		{
			// If drifting criteria fulfilled, then drift
			if (input.driftInput && input.vertical > inputThreshold && control.currentSpeed > minDriftSpeed && (input.horizontal != 0 || state == CarState.DRIFT) && _wheelPower == 1)
			{
				Drift(); //This function changes _driftOffset and _driftStrength up to the target values
			}
			else if (state == CarState.DRIFT && input.vertical > inputThreshold && control.currentSpeed > minDriftSpeed && _wheelPower == 1)
			{
				if ((driftingRight && input.horizontal > -driftCancelHorizontalInput) || (!driftingRight && input.horizontal < driftCancelHorizontalInput))
				{
					Drift();
				}
				else
				{
					StopDrift();
				}
			}
			else
			{
				StopDrift(); //Resets them back
			}

			Turn();
		}

		private void Turn()
		{
			horizontalInput = (input.horizontal * _dampDriftStrength) + _dampDriftOffset; //add _driftOffset to horizontal input 

			var steerSpeed = steeringCurve.Evaluate(control.currentSpeed);
			turningPower = horizontalInput * (Mathf.Clamp(control.currentSpeed, -fullTurnSpeed, fullTurnSpeed) / fullTurnSpeed) * _wheelPower;

			RaycastHit hit;
			Vector3 down = -steeringTransform.transform.up;

			if (Physics.Raycast(steeringTransform.transform.position, down, out hit, 1f))
			{
				var x = Vector3.Cross(rb.transform.forward, hit.point - steeringTransform.transform.position);
				var something = Vector3.Reflect(x, hit.normal).normalized;

				Debug.DrawLine(steeringTransform.transform.position, hit.point);
				Debug.DrawRay(steeringTransform.transform.position, 5 * something);
				Debug.DrawRay(steeringTransform.transform.position, steeringTransform.transform.right, Color.blue);
				something = steeringTransform.transform.right;


				//Steer
				rb.AddForceAtPosition(turningPower * something * steerPower * control.wheelPower, steeringTransform.position, ForceMode.Acceleration);
			}
			rb.AddTorque(-new Vector3(0.0f, rb.angularVelocity.y, 0.0f) * angularTraction * control.wheelPower, ForceMode.Acceleration);

			//Rotate the car mesh so its 'drifting' lol
			var visualTurnAngle = Quaternion.AngleAxis(Mathf.Rad2Deg * ((input.horizontal * (_dampDriftStrength * driftStrengthVisualMultipler)) + (_dampDriftOffset * visualDriftOffsetVisualMultipler)) * Time.deltaTime * _dampVisualHorizontalMultipler, control.visualMesh.transform.up);
			control.visualMesh.transform.localRotation = visualTurnAngle;

			//Damp stuff so the velocity changes all smoothly transition
			_dampDriftOffset = Mathf.SmoothDamp(_dampDriftOffset, _dampDriftOffsetTarget, ref _dampDriftOffsetVelocity, driftEnableSpeed);
			_dampDriftStrength = Mathf.SmoothDamp(_dampDriftStrength, _dampDriftStrengthTarget, ref _dampDriftStrengthVelocity, driftEnableSpeed);
			_dampVisualHorizontalMultipler = Mathf.SmoothDamp(_dampVisualHorizontalMultipler, _dampVisualHorizontalMultiplerTarget, ref _dampVisualHorizontalMultiplerVelocity, driftEnableSpeed);
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
					input.SetChangeRate(limitedHorizontalChangeRateDrift);
			}
			else if (Input.GetAxis("Horizontal") > 0)
			{
				if (!driftingRight)
				{
					input.SetChangeRate(limitedHorizontalChangeRate);
				}
				else
				{
					input.SetChangeRate(limitedHorizontalChangeRateDrift);
				}
			}
			else if (Input.GetAxis("Horizontal") < 0)
			{
				if (driftingRight)
				{
					input.SetChangeRate(limitedHorizontalChangeRate);
				}
				else
				{
					input.SetChangeRate(limitedHorizontalChangeRateDrift);
				}
			}

			state = CarState.DRIFT;
		}

		private void StopDrift()
		{
			state = CarState.DRIVE;
			_dampDriftOffsetTarget = 0;
			_dampDriftStrengthTarget = 1;
			_dampVisualHorizontalMultiplerTarget = 0;
			input.SetChangeRate(limitedHorizontalChangeRate);
		}
	}
}
