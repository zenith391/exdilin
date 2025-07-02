using UnityEngine;

public class DragTileGestureCommand : DragGestureCommand
{
	public DragTileGestureCommand(TileObject tile, Vector2 targetPos, float endDelay = 0f, float speed = 0.3f)
	{
		base.targetPos = targetPos;
		if (tile != null)
		{
			Vector3 position = tile.GetPosition();
			startPos = new Vector2(position.x + 37.5f, position.y + 37.5f);
			base.endDelay = endDelay;
		}
		base.speed = speed;
	}
}
