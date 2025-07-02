using System;
using System.Collections.Generic;

[Serializable]
public class UnassignedShapeCollisionMatrixEntry
{
	public string unassignedShapeCategory;

	public List<string> prefabsWithShapes = new List<string>();
}
