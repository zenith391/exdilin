using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000129 RID: 297
public class CopyScriptAnimationCommand : Command
{
	// Token: 0x0600140F RID: 5135 RVA: 0x0008C044 File Offset: 0x0008A444
	public override void Execute()
	{
		this.step++;
		if (this.step >= this.animationSteps)
		{
			this.parent.transform.localScale = Vector3.one;
			this.parent.transform.DetachChildren();
			foreach (Tile tile in this.tiles)
			{
				tile.Show(false);
			}
			this.tiles.Clear();
			this.done = true;
			this.step = int.MaxValue;
			Sound.PlayOneShotSound("Copy Script Done", 1f);
			if (this.endCommand != null)
			{
				Blocksworld.AddFixedUpdateCommand(this.endCommand);
			}
		}
		if (this.step == (int)Mathf.Round((float)this.animationSteps / 3f))
		{
			Sound.PlayOneShotSound("Copy Script Transfer", 1f);
		}
		float num = Mathf.Clamp((float)this.step / (float)this.animationSteps, 0f, 1f);
		Vector3 position = (1f - num) * this.startPos + num * this.targetPos;
		this.parent.transform.position = position;
		this.parent.transform.localScale = Vector3.one * Mathf.Max(0.01f, 1f - num);
	}

	// Token: 0x06001410 RID: 5136 RVA: 0x0008C1D4 File Offset: 0x0008A5D4
	public bool Animating()
	{
		return this.step <= this.animationSteps;
	}

	// Token: 0x06001411 RID: 5137 RVA: 0x0008C1E8 File Offset: 0x0008A5E8
	public void SetTiles(List<List<Tile>> tiles)
	{
		this.step = 0;
		this.endCommand = null;
		this.tiles.Clear();
		if (this.parent == null)
		{
			this.parent = new GameObject("Script copy animation object");
		}
		this.parent.layer = 8;
		this.parent.transform.localScale = Vector3.one;
		this.startPos = Vector3.zero;
		int num = 0;
		foreach (List<Tile> list in tiles)
		{
			foreach (Tile tile in list)
			{
				if (tile.tileObject != null)
				{
					Tile tile2 = tile.Clone();
					tile2.Show(true);
					tile2.Enable(true);
					Vector3 position = tile2.tileObject.GetPosition();
					this.startPos += position;
					tile2.MoveTo(position, false);
					this.tiles.Add(tile2);
					num++;
				}
			}
		}
		this.startPos /= (float)num;
		this.startPos.z = -0.5f;
		this.parent.transform.position = this.startPos;
		foreach (Tile tile3 in this.tiles)
		{
			if (!(tile3.tileObject == null))
			{
				tile3.SetParent(this.parent.transform);
				Vector3 localPosition = tile3.tileObject.GetLocalPosition();
				localPosition.z = 0f;
				tile3.tileObject.SetLocalPosition(localPosition);
			}
		}
		UIQuickSelect quickSelect = Blocksworld.UI.QuickSelect;
		if (quickSelect != null)
		{
			Vector2 vector = quickSelect.ScriptRectCenter();
			this.targetPos.x = vector.x;
			this.targetPos.y = vector.y;
			float magnitude = (this.targetPos - this.startPos).magnitude;
			this.animationSteps = (int)Mathf.Max(2f, magnitude / 50f);
		}
	}

	// Token: 0x04000FA9 RID: 4009
	private int step = int.MaxValue;

	// Token: 0x04000FAA RID: 4010
	private int animationSteps = 40;

	// Token: 0x04000FAB RID: 4011
	private GameObject parent;

	// Token: 0x04000FAC RID: 4012
	private List<Tile> tiles = new List<Tile>();

	// Token: 0x04000FAD RID: 4013
	private Vector3 targetPos = new Vector3(800f, 100f, -0.5f);

	// Token: 0x04000FAE RID: 4014
	private Vector3 startPos = default(Vector3);

	// Token: 0x04000FAF RID: 4015
	public DelegateCommand endCommand;
}
