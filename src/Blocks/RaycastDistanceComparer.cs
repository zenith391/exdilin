using System.Collections.Generic;
using UnityEngine;

namespace Blocks;

public class RaycastDistanceComparer : IComparer<RaycastHit>
{
	private Vector3 _from;

	public RaycastDistanceComparer(Vector3 from)
	{
		_from = from;
	}

	public int Compare(RaycastHit a, RaycastHit b)
	{
		Vector3 vector = _from - a.point;
		Vector3 vector2 = _from - b.point;
		return (int)(vector.sqrMagnitude - vector2.sqrMagnitude);
	}
}
