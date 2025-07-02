using System.Collections.Generic;
using UnityEngine;

public class BlockWithTagAlongSameLineCountModelFeatureType : FirstTileRowIntModelFeatureType
{
	public string tagName;

	public Vector3 localDirection = Vector3.forward;

	private List<Vector3> directions = new List<Vector3>();

	public override void Reset()
	{
		base.Reset();
		directions.Clear();
	}

	protected override void Update(List<Tile> firstRow)
	{
		Tile tile = firstRow[1];
		string stringArg = Util.GetStringArg(tile.gaf.Args, 0, string.Empty);
		if (!EntityTagsRegistry.BlockHasTag(stringArg, tagName))
		{
			return;
		}
		Tile tile2 = firstRow[3];
		Quaternion quaternion = Quaternion.Euler(Util.GetVector3Arg(tile2.gaf.Args, 0, Vector3.zero));
		Vector3 vector = quaternion * localDirection;
		int num = 1;
		foreach (Vector3 direction in directions)
		{
			float f = Vector3.Dot(vector, direction);
			if (Mathf.Abs(f) > 0.001f)
			{
				num++;
			}
		}
		value = Mathf.Max(value, num);
		directions.Add(vector);
	}
}
