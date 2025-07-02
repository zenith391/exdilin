using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShapeCollisionMatrixEntry
{
	public string shapeCategory;

	public string[] noCollides;

	public List<string> prefabsWithShapes = new List<string>();

	[HideInInspector]
	public List<string> noCollideList = new List<string>();
}
