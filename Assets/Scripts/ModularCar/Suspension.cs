using System;
using UnityEngine;

namespace ModularCar
{
	public class Suspension : MonoBehaviour
	{
		public CarControllerV3 control;
		public Rigidbody rb;
		public InputController input;
		public Transform steeringTransform;

		public bool debug;
		
		public bool springsInitialized;
		private Spring2[] springs;
		public float mass = 1;
		public float radius = .4f;
		public float maxSuspension = .05f;
		public float springy = 30000;
		public float damper = 3000;

		public float springFLForce;
		public float springFRForce;
		public float springBLForce;
		public float springBRForce;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			input = this.GetComponent<InputController>();
			rb = control.rb;

			InitializeSprings(control.wheels[0], control.wheels[1], control.wheels[2], control.wheels[3]);

			Debug.Assert(control != null, "Must have a controller");
			Debug.Assert(input != null, "Must have an input controller");
		}

		private void FixedUpdate()
		{
			if (springsInitialized)
			{
				foreach (var spring in springs)
				{
					GetGround(spring);
				}
			}
		}

		public void InitializeSprings(Transform frontLeftPosition, Transform frontRightPosition, Transform rearLeftPosition, Transform rearRightPosition)
		{
			if (springsInitialized)
			{
				throw new InvalidOperationException("Springs already initialized");
			}

			springs = new Spring2[4];
			springs[0] = new Spring2(frontLeftPosition);
			springs[1] = new Spring2(frontRightPosition);
			springs[2] = new Spring2(rearLeftPosition);
			springs[3] = new Spring2(rearRightPosition);

			springsInitialized = true;
		}

		void GetGround(Spring2 spring)
		{
			Vector3 downwards = spring.transform.TransformDirection(-Vector3.up);
			RaycastHit hit;

			// down = local downwards direction
			Vector3 down = spring.transform.TransformDirection(Vector3.down);

			if (Physics.Raycast(spring.transform.position, downwards, out hit, radius + maxSuspension))
			{
				// the velocity at point of contact
				Vector3 velocityAtTouch = rb.GetPointVelocity(hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float compression = hit.distance / (maxSuspension + radius);
				compression = -compression + 1;

				// final force
				Vector3 force = -downwards * compression * springy;
				// velocity at point of contact transformed into local space

				Vector3 t = spring.transform.InverseTransformDirection(velocityAtTouch);

				// local x and z directions = 0
				t.z = t.x = 0;

				// back to world space * -damping
				Vector3 damping = spring.transform.TransformDirection(t) * -damper;
				Vector3 finalForce = force + damping;

				rb.AddForceAtPosition(finalForce, hit.point);

				//if (graphic) graphic.position = transform.position + (down * (hit.distance - radius));

			}
			else
			{
				//if (graphic) graphic.position = transform.position + (down * maxSuspension);
			}

		}

	}


	public struct Spring2
	{
		public Transform transform;

		public Spring2(Transform spring)
		{
			this.transform = spring;
		}
	}
}