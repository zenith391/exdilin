using System;
using Blocks;
using UnityEngine;

public class Arrow
{
	private UIArrow uiArrow;

	public TrackingState state;

	public TileObject tile;

	public TileObject tile2;

	public Block block;

	public Block block2;

	public Vector3 offset;

	public Panel panel;

	public Vector3 world;

	public Vector3 world2;

	public Vector3 faceNormal;

	public Vector3 screen;

	public Vector3 screen2;

	public float bounce;

	public Vector3 from;

	public Vector3 to;

	private int swipe;

	public Arrow()
	{
		uiArrow = Blocksworld.UI.Overlay.CreateArrow();
	}

	public void Show(bool show, int swipe = 0)
	{
		uiArrow.Show(show);
		uiArrow.SetSwipeMode(swipe);
		this.swipe = swipe;
	}

	public bool IsShowing()
	{
		return uiArrow.IsShowing();
	}

	public void Update()
	{
		if (!IsShowing())
		{
			return;
		}
		switch (state)
		{
		case TrackingState.TileBlock:
			if (tile != null && block.go != null)
			{
				Position(tile.GetCenterPosition(), Util.WorldToScreenPoint(block.go.transform.position, z: true), 40f, 40f);
			}
			break;
		case TrackingState.TileWorld:
			if (tile != null)
			{
				Position(tile.GetCenterPosition(), Util.WorldToScreenPoint(world, z: true), 40f, 40f);
			}
			break;
		case TrackingState.TileOffset:
			if (tile != null)
			{
				Vector3 centerPosition5 = tile.GetCenterPosition();
				Position(centerPosition5, centerPosition5 + offset, 0f, 0f);
			}
			break;
		case TrackingState.TilePanelBottom:
			if (tile != null)
			{
				Vector3 centerPosition4 = tile.GetCenterPosition();
				Position(to: new Vector3((!(Blocksworld.scriptPanel.position.x + Blocksworld.scriptPanel.width / 2f > (float)(Blocksworld.screenWidth / 2))) ? (Blocksworld.scriptPanel.position.x + Blocksworld.scriptPanel.width + 160f) : (Blocksworld.scriptPanel.position.x - 160f), panel.position.y + 0.5f * panel.height, centerPosition4.z), from: centerPosition4, boxWidth: 0f, boxHeight: 0f);
			}
			break;
		case TrackingState.TilePanelOffset:
			if (tile != null)
			{
				Vector3 centerPosition7 = tile.GetCenterPosition();
				Position(centerPosition7, panel.position + offset, 0f, 0f);
			}
			break;
		case TrackingState.ScreenPanelOffset:
			Position(screen, panel.position + offset, 0f, 0f);
			break;
		case TrackingState.Tile2Screen:
			if (tile != null)
			{
				Vector3 centerPosition6 = tile.GetCenterPosition();
				Position(centerPosition6, screen, 0f, 0f);
			}
			break;
		case TrackingState.Tile2Tile:
			if (tile != null && tile2 != null)
			{
				Vector3 centerPosition2 = tile.GetCenterPosition();
				Vector3 centerPosition3 = tile2.GetCenterPosition();
				Position(centerPosition2, centerPosition3 + offset, 0f, 0f);
			}
			break;
		case TrackingState.Button2World:
			if (tile != null)
			{
				Position(tile.GetCenterPosition(), Util.WorldToScreenPointSafe(world), 0f, 0f);
			}
			break;
		case TrackingState.BlockBlockOffset:
			if (block.go != null && block2.go != null)
			{
				Position(Util.WorldToScreenPoint(block.go.transform.position, z: true), Util.WorldToScreenPoint(block2.go.transform.position + offset, z: true), 0f, 0f);
			}
			break;
		case TrackingState.OffsetBlock:
			if (block.go != null)
			{
				Vector3 vector2 = Util.WorldToScreenPoint(block.go.transform.position, z: true);
				Position(vector2 + offset, vector2, 0f, 0f);
			}
			break;
		case TrackingState.BlockOffsetWorld:
			if (block.go != null)
			{
				Position(Util.WorldToScreenPoint(block.go.transform.position + offset, z: true), Util.WorldToScreenPoint(world, z: true), 0f, 0f);
			}
			break;
		case TrackingState.TBoxFaceWorld:
			if (block.go != null)
			{
				Position(Util.WorldToScreenPoint(block.go.transform.position + faceNormal, z: true), Util.WorldToScreenPoint(world, z: true), 0f, 0f);
			}
			break;
		case TrackingState.PanelOffsetScreen:
			Position(panel.position + offset, screen, 0f, 0f);
			break;
		case TrackingState.Block2Screen:
			if (block.go != null)
			{
				Position(Util.WorldToScreenPoint(block.go.transform.position, z: true), screen, 0f, 0f);
			}
			break;
		case TrackingState.World2Screen:
			Position(Util.WorldToScreenPoint(world, z: true), screen, 0f, 0f);
			break;
		case TrackingState.Screen2World:
			Position(screen, Util.WorldToScreenPoint(world, z: true), 0f, 0f);
			break;
		case TrackingState.Screen2Screen:
			Position(screen, screen2, 0f, 0f);
			break;
		case TrackingState.Screen2Block:
			if (block.go != null)
			{
				Position(screen, Util.WorldToScreenPoint(block.go.transform.position, z: true), 0f, 0f);
			}
			break;
		case TrackingState.Screen2ScreenBounce:
		{
			float num = bounce * Mathf.Abs(20f * Mathf.Sin(5f * Time.time));
			Position(screen + num * Vector3.up, screen2 + num * Vector3.up, 0f, 0f);
			break;
		}
		case TrackingState.WorldOffset:
			Position(Util.WorldToScreenPoint(world, z: true), Util.ClampToScreen(Util.WorldToScreenPoint(world + offset, z: true)), 0f, 0f);
			break;
		case TrackingState.Block2World:
			Position(Util.WorldToScreenPoint(block.go.transform.position, z: true), Util.WorldToScreenPoint(world, z: true), 0f, 0f);
			break;
		case TrackingState.MoveButtonHelper:
		{
			Vector3 centerPosition = TBox.tileButtonMove.GetCenterPosition();
			Vector3 vector = Util.WorldToScreenPointSafe(world) + offset;
			Position(centerPosition, vector, 0f, 0f);
			break;
		}
		}
	}

	public void Position(Vector3 from, Vector3 to, float boxWidth, float boxHeight, bool inverseMovement = false)
	{
		this.from = from;
		this.to = to;
		bool flag = boxWidth != 0f && to.x >= from.x - boxWidth / 2f && to.x <= from.x + boxWidth / 2f && to.y >= from.y - boxHeight / 2f && to.y <= from.y + boxHeight / 2f;
		uiArrow.SetSwipeMode((!flag) ? swipe : 3);
		if (boxWidth != 0f)
		{
			from = FindBoxEdge(from, to, boxWidth, boxHeight);
		}
		Blocksworld.UI.Overlay.SetArrowEndpoints(uiArrow, from, to);
		if (inverseMovement)
		{
			this.from = to;
			this.to = from;
		}
	}

	private Vector3 FindBoxEdge(Vector3 from, Vector3 to, float boxWidth, float boxHeight)
	{
		float num = boxWidth / 2f;
		float num2 = boxHeight / 2f;
		float num3 = to.x - from.x;
		float num4 = to.y - from.y;
		Vector2 vector = ((num * Math.Abs(num4) > num2 * Math.Abs(num3)) ? ((num4 > 0f) ? new Vector2(num2 * num3 / num4, num2) : ((!(num4 < 0f)) ? Vector2.zero : new Vector2((0f - num2) * num3 / num4, 0f - num2))) : ((num3 > 0f) ? new Vector2(num, num * num4 / num3) : ((!(num3 < 0f)) ? Vector2.zero : new Vector2(0f - num, (0f - num) * num4 / num3))));
		return new Vector3(from.x + vector.x, from.y + vector.y, 0f);
	}
}
