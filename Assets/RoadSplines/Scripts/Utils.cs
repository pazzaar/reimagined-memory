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
}