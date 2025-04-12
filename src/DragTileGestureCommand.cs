using System;
using UnityEngine;

// Token: 0x0200013A RID: 314
public class DragTileGestureCommand : DragGestureCommand
{
	// Token: 0x06001443 RID: 5187 RVA: 0x0008E6A0 File Offset: 0x0008CAA0
	public DragTileGestureCommand(TileObject tile, Vector2 targetPos, float endDelay = 0f, float speed = 0.3f)
	{
		this.targetPos = targetPos;
		if (tile != null)
		{
			Vector3 position = tile.GetPosition();
			this.startPos = new Vector2(position.x + 37.5f, position.y + 37.5f);
			this.endDelay = endDelay;
		}
		this.speed = speed;
	}
}
