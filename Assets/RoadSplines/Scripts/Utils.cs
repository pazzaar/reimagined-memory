using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public struct Pair<T>
	{
		private T first;
		private T second;

		public T First
		{
			get { return first; }
			set { first = value; }
		}

		public T Second
		{
			get { return second; }
			set { second = value; }
		}

		public Pair(T first, T second)
		{
			this.first = first;
			this.second = second;
		}

		public override string ToString()
		{
			return "[" + First + "," + Second + "]";
		}
	}

	public struct ExtrudeShapePoint
	{
		private Vector3 point;
		private bool softNormal;

		public Vector3 Point
		{
			get { return point; }
			set { point = value; }
		}

		public bool SoftNormal
		{
			get { return softNormal; }
			set { softNormal = value; }
		}

		public ExtrudeShapePoint(Vector3 point, bool softNormal = false)
		{
			this.point = point;
			this.softNormal = softNormal;
		}

		public override string ToString()
		{
			return "[" + point + "," + softNormal + "]";
		}
	}
}