using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCar
{
	public class Friction : MonoBehaviour
	{
		public CarControllerV3 control;
		public Rigidbody rb;
		public InputController input;
		public Transform steeringTransform;

		[Tooltip("Sideways grip - about 15 is full grip")]
		public float sidewaysTraction = 15;
		[Tooltip("Rolling resistance - about .4 is good")]
		public float rollingResistance = .4f;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			input = this.GetComponent<InputController>();
			steeringTransform = control.steeringTransform;
			rb = control.rb;
		}

		// Update is called once per frame
		private void FixedUpdate()
		{
			Vector3 contrarySidewaysVelocity = -Vector3.Project(rb.velocity, transform.right);
			if (contrarySidewaysVelocity.sqrMagnitude > 0.0f)
			{
				rb.AddForce(contrarySidewaysVelocity * sidewaysTraction * control.wheelPower, ForceMode.Acceleration);
			}

			Vector3 wheelsRollingResistanceForce = -(rb.velocity * rollingResistance);
			rb.AddForce(wheelsRollingResistanceForce * control.wheelPower, ForceMode.Acceleration);
		}
	}
}