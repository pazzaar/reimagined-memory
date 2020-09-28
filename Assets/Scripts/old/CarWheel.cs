using UnityEngine;
using System.Collections;

namespace LivioDeLaCruz.RacingTest5.car
{

	public class CarWheel : MonoBehaviour
	{
		//Inspector parameters
		public bool steers = false;
		public bool motors = false;

		//data passed from CarController
		private float _wheelRadius = 0.26f;
		public float wheelRadius
		{
			set { _wheelRadius = value;  }
			get { return _wheelRadius; }
		}

		private CarState _state;
		public CarState state
		{
			set { _state = value; }
			get { return _state; }
		}

		//internal state stuff
		private Vector3 lastPosition;
		private float _steeringAngle;
		private float wheelRotation = 0;

		//telemetry
		public Vector3 steerForce;

		void Start()
		{
			lastPosition = transform.position;
		}
		
		void Update()
		{
			Vector3 dist = transform.position - lastPosition;
			float fullRotationDistance = 2 * Mathf.PI * wheelRadius;
			float distFraction = dist.magnitude / fullRotationDistance;
			if (state == CarState.REVERSE)
				distFraction *= -1;
			wheelRotation += distFraction * 360 % 360;
			transform.localRotation = Quaternion.Euler(new Vector3(wheelRotation, _steeringAngle, 0));

			/*if (rb)
			{
				steerForce = Vector3.Dot(dist, transform.forward) * transform.forward * steeringForce;
				rb.AddForceAtPosition(steerForce, transform.localPosition);
			}*/

			lastPosition = transform.position;
		}
		
		void FixedUpdate()
		{
		
		}

		void OnDrawGizmos()
		{
			UnityEditor.Handles.color = Color.green;
			UnityEditor.Handles.DrawWireDisc(transform.position, transform.right, _wheelRadius);
		}

		public float steeringAngle
		{
			set
			{
				_steeringAngle = value;
			}
		}
	}
}