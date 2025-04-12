using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001C1 RID: 449
public class BlockWithTagAlongSameLineCountModelFeatureType : FirstTileRowIntModelFeatureType
{
	// Token: 0x06001814 RID: 6164 RVA: 0x000AAB33 File Offset: 0x000A8F33
	public override void Reset()
	{
		base.Reset();
		this.directions.Clear();
	}

	// Token: 0x06001815 RID: 6165 RVA: 0x000AAB48 File Offset: 0x000A8F48
	protected override void Update(List<Tile> firstRow)
	{
		Tile tile = firstRow[1];
		string stringArg = Util.GetStringArg(tile.gaf.Args, 0, string.Empty);
		if (!EntityTagsRegistry.BlockHasTag(stringArg, this.tagName))
		{
			return;
		}
		Tile tile2 = firstRow[3];
		Quaternion rotation = Quaternion.Euler(Util.GetVector3Arg(tile2.gaf.Args, 0, Vector3.zero));
		Vector3 vector = rotation * this.localDirection;
		int num = 1;
		foreach (Vector3 rhs in this.directions)
		{
			float f = Vector3.Dot(vector, rhs);
			if (Mathf.Abs(f) > 0.001f)
			{
				num++;
			}
		}
		this.value = Mathf.Max(this.value, num);
		this.directions.Add(vector);
	}

	// Token: 0x04001309 RID: 4873
	public string tagName;

	// Token: 0x0400130A RID: 4874
	public Vector3 localDirection = Vector3.forward;

	// Token: 0x0400130B RID: 4875
	private List<Vector3> directions = new List<Vector3>();
}
