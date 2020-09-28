using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCar
{
	public class InputController : MonoBehaviour
	{
		[Tooltip("How much to limit horizontal input rate of change")]
		public float _limitedHorizontalChangeRate = 1f; //Default change rate is 1
		[SerializeField]
		public float vertical { get; private set; }
		[SerializeField]
		public float horizontal { get; private set; }
		public float rawHorizontal { get; private set; }
		public bool driftInput { get; private set; }

		void Start()
		{
			vertical = 0;
			horizontal = 0;
			rawHorizontal = 0;
			driftInput = false;
		}

		// Update is called once per frame
		void Update()
		{
			vertical = Input.GetAxis("Vertical");
			horizontal = Mathf.Lerp(horizontal, Input.GetAxis("Horizontal"), _limitedHorizontalChangeRate * Time.deltaTime);
			rawHorizontal = Input.GetAxis("Horizontal");
			driftInput = Input.GetKey(KeyCode.LeftControl);
		}

		public void SetChangeRate(float value)
		{
			_limitedHorizontalChangeRate = value;
		}

	}
}