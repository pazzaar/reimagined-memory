using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
	class old
	{
	}
}

/*
		Quaternion turnAngle = Quaternion.AngleAxis(turningPower, rb.transform.up);
		Vector3 fwd = turnAngle * rb.transform.forward;

		Vector3 movement = fwd * forwardPower;
		Vector3 adjustedVelocity = rb.velocity + movement * Time.deltaTime;
		adjustedVelocity.y = rb.velocity.y; // Do I need this?
		
		rb.velocity = adjustedVelocity;


		// manual angular velocity coefficient
		float angularVelocitySteering = .4f;
		float angularVelocitySmoothSpeed = 20f;

		var angularVel = rb.angularVelocity;
		angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.deltaTime * angularVelocitySmoothSpeed);
		rb.angularVelocity = angularVel;
		// rotate rigidbody's velocity as well to generate immediate velocity redirection
		// manual velocity steering coefficient
		float velocitySteering = 25f;
		// rotate our velocity based on current steer value
		rb.velocity = Quaternion.Euler(0f, turningPower * velocitySteering * Time.deltaTime, 0f) * rb.velocity;
		*/


/*
//FROM that youtube video on making mobile friendly car physics
// Update is called once per frame
void FixedUpdate()
{
	verticalInput = Input.GetAxis("Vertical");
	horizontalInput = Input.GetAxis("Horizontal");
	driftInput = Input.GetKey(KeyCode.LeftControl);
	currentSpeed = getVelocity();

	if (driftInput && verticalInput > inputThreshold && currentSpeed > minDriftSpeed && (horizontalInput != 0 || state == CarState.DRIFT))
	{
		state = CarState.DRIFT;
		//maintainDriftSpeed();
	}

	var myRight = transform.right;
	var velo = rb.velocity;
	var tempVEC = new Vector3(velo.x, 0f, velo.z);
	var flatVelo = tempVEC;

	var dir = transform.TransformDirection(carFwd);

	tempVEC = new Vector4(dir.x, 0, dir.z);

	var flatDir = Vector3.Normalize(tempVEC);

	var relativeVelocity = transform.InverseTransformDirection(flatVelo);

	var slideSpeed = Vector3.Dot(myRight, flatVelo);

	var mySpeed = flatVelo.magnitude;

	var rev = Mathf.Sign(Vector3.Dot(flatVelo, flatDir));

	var engineForce = (flatDir * (power * verticalInput) * carMass);

	var actualTurn = horizontalInput;

	if (rev < 0.1f)
		actualTurn = -actualTurn;

	var turnVec = (((carUp * turnSpeed) * actualTurn) * carMass) * 800f;

	var actualGrip = Mathf.Lerp(100f, carGrip, mySpeed * 0.02f);

	var imp = myRight * (-slideSpeed * carMass * actualGrip);

	rb.AddForce(engineForce * Time.deltaTime);

	if (mySpeed > maxSpeedToTurn)
		rb.AddTorque(turnVec * Time.deltaTime);
	else if (mySpeed < maxSpeedToTurn)
		return;

	rb.AddForce(imp * Time.deltaTime);
}
*/
