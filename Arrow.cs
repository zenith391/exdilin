using System;
using Blocks;
using UnityEngine;

// Token: 0x020001E7 RID: 487
public class Arrow
{
	// Token: 0x060018C9 RID: 6345 RVA: 0x000AEBAE File Offset: 0x000ACFAE
	public Arrow()
	{
		this.uiArrow = Blocksworld.UI.Overlay.CreateArrow();
	}

	// Token: 0x060018CA RID: 6346 RVA: 0x000AEBCB File Offset: 0x000ACFCB
	public void Show(bool show, int swipe = 0)
	{
		this.uiArrow.Show(show);
		this.uiArrow.SetSwipeMode(swipe);
		this.swipe = swipe;
	}

	// Token: 0x060018CB RID: 6347 RVA: 0x000AEBEC File Offset: 0x000ACFEC
	public bool IsShowing()
	{
		return this.uiArrow.IsShowing();
	}

	// Token: 0x060018CC RID: 6348 RVA: 0x000AEBFC File Offset: 0x000ACFFC
	public void Update()
	{
		if (!this.IsShowing())
		{
			return;
		}
		switch (this.state)
		{
		case TrackingState.TileBlock:
			if (this.tile != null && this.block.go != null)
			{
				this.Position(this.tile.GetCenterPosition(), Util.WorldToScreenPoint(this.block.go.transform.position, true), 40f, 40f, false);
			}
			break;
		case TrackingState.TileWorld:
			if (this.tile != null)
			{
				this.Position(this.tile.GetCenterPosition(), Util.WorldToScreenPoint(this.world, true), 40f, 40f, false);
			}
			break;
		case TrackingState.TileOffset:
			if (this.tile != null)
			{
				Vector3 centerPosition = this.tile.GetCenterPosition();
				this.Position(centerPosition, centerPosition + this.offset, 0f, 0f, false);
			}
			break;
		case TrackingState.TilePanelBottom:
			if (this.tile != null)
			{
				Vector3 centerPosition2 = this.tile.GetCenterPosition();
				float x;
				if (Blocksworld.scriptPanel.position.x + Blocksworld.scriptPanel.width / 2f > (float)(Blocksworld.screenWidth / 2))
				{
					x = Blocksworld.scriptPanel.position.x - 160f;
				}
				else
				{
					x = Blocksworld.scriptPanel.position.x + Blocksworld.scriptPanel.width + 160f;
				}
				this.Position(centerPosition2, new Vector3(x, this.panel.position.y + 0.5f * this.panel.height, centerPosition2.z), 0f, 0f, false);
			}
			break;
		case TrackingState.TilePanelOffset:
			if (this.tile != null)
			{
				Vector3 centerPosition3 = this.tile.GetCenterPosition();
				this.Position(centerPosition3, this.panel.position + this.offset, 0f, 0f, false);
			}
			break;
		case TrackingState.ScreenPanelOffset:
			this.Position(this.screen, this.panel.position + this.offset, 0f, 0f, false);
			break;
		case TrackingState.Tile2Screen:
			if (this.tile != null)
			{
				Vector3 centerPosition4 = this.tile.GetCenterPosition();
				this.Position(centerPosition4, this.screen, 0f, 0f, false);
			}
			break;
		case TrackingState.Tile2Tile:
			if (this.tile != null && this.tile2 != null)
			{
				Vector3 centerPosition5 = this.tile.GetCenterPosition();
				Vector3 centerPosition6 = this.tile2.GetCenterPosition();
				this.Position(centerPosition5, centerPosition6 + this.offset, 0f, 0f, false);
			}
			break;
		case TrackingState.Button2World:
			if (this.tile != null)
			{
				this.Position(this.tile.GetCenterPosition(), Util.WorldToScreenPointSafe(this.world), 0f, 0f, false);
			}
			break;
		case TrackingState.BlockBlockOffset:
			if (this.block.go != null && this.block2.go != null)
			{
				this.Position(Util.WorldToScreenPoint(this.block.go.transform.position, true), Util.WorldToScreenPoint(this.block2.go.transform.position + this.offset, true), 0f, 0f, false);
			}
			break;
		case TrackingState.OffsetBlock:
			if (this.block.go != null)
			{
				Vector3 a = Util.WorldToScreenPoint(this.block.go.transform.position, true);
				this.Position(a + this.offset, a, 0f, 0f, false);
			}
			break;
		case TrackingState.BlockOffsetWorld:
			if (this.block.go != null)
			{
				this.Position(Util.WorldToScreenPoint(this.block.go.transform.position + this.offset, true), Util.WorldToScreenPoint(this.world, true), 0f, 0f, false);
			}
			break;
		case TrackingState.TBoxFaceWorld:
			if (this.block.go != null)
			{
				this.Position(Util.WorldToScreenPoint(this.block.go.transform.position + this.faceNormal, true), Util.WorldToScreenPoint(this.world, true), 0f, 0f, false);
			}
			break;
		case TrackingState.PanelOffsetScreen:
			this.Position(this.panel.position + this.offset, this.screen, 0f, 0f, false);
			break;
		case TrackingState.Block2Screen:
			if (this.block.go != null)
			{
				this.Position(Util.WorldToScreenPoint(this.block.go.transform.position, true), this.screen, 0f, 0f, false);
			}
			break;
		case TrackingState.World2Screen:
			this.Position(Util.WorldToScreenPoint(this.world, true), this.screen, 0f, 0f, false);
			break;
		case TrackingState.Screen2World:
			this.Position(this.screen, Util.WorldToScreenPoint(this.world, true), 0f, 0f, false);
			break;
		case TrackingState.Screen2Screen:
			this.Position(this.screen, this.screen2, 0f, 0f, false);
			break;
		case TrackingState.Screen2Block:
			if (this.block.go != null)
			{
				this.Position(this.screen, Util.WorldToScreenPoint(this.block.go.transform.position, true), 0f, 0f, false);
			}
			break;
		case TrackingState.Screen2ScreenBounce:
		{
			float d = this.bounce * Mathf.Abs(20f * Mathf.Sin(5f * Time.time));
			this.Position(this.screen + d * Vector3.up, this.screen2 + d * Vector3.up, 0f, 0f, false);
			break;
		}
		case TrackingState.WorldOffset:
			this.Position(Util.WorldToScreenPoint(this.world, true), Util.ClampToScreen(Util.WorldToScreenPoint(this.world + this.offset, true)), 0f, 0f, false);
			break;
		case TrackingState.Block2World:
			this.Position(Util.WorldToScreenPoint(this.block.go.transform.position, true), Util.WorldToScreenPoint(this.world, true), 0f, 0f, false);
			break;
		case TrackingState.MoveButtonHelper:
		{
			Vector3 centerPosition7 = TBox.tileButtonMove.GetCenterPosition();
			Vector3 vector = Util.WorldToScreenPointSafe(this.world) + this.offset;
			this.Position(centerPosition7, vector, 0f, 0f, false);
			break;
		}
		}
	}

	// Token: 0x060018CD RID: 6349 RVA: 0x000AF398 File Offset: 0x000AD798
	public void Position(Vector3 from, Vector3 to, float boxWidth, float boxHeight, bool inverseMovement = false)
	{
		this.from = from;
		this.to = to;
		bool flag = boxWidth != 0f && to.x >= from.x - boxWidth / 2f && to.x <= from.x + boxWidth / 2f && to.y >= from.y - boxHeight / 2f && to.y <= from.y + boxHeight / 2f;
		this.uiArrow.SetSwipeMode((!flag) ? this.swipe : 3);
		if (boxWidth != 0f)
		{
			from = this.FindBoxEdge(from, to, boxWidth, boxHeight);
		}
		Blocksworld.UI.Overlay.SetArrowEndpoints(this.uiArrow, from, to);
		if (inverseMovement)
		{
			this.from = to;
			this.to = from;
		}
	}

	// Token: 0x060018CE RID: 6350 RVA: 0x000AF494 File Offset: 0x000AD894
	private Vector3 FindBoxEdge(Vector3 from, Vector3 to, float boxWidth, float boxHeight)
	{
		float num = boxWidth / 2f;
		float num2 = boxHeight / 2f;
		float num3 = to.x - from.x;
		float num4 = to.y - from.y;
		Vector2 zero;
		if (num * Math.Abs(num4) > num2 * Math.Abs(num3))
		{
			if (num4 > 0f)
			{
				zero = new Vector2(num2 * num3 / num4, num2);
			}
			else if (num4 < 0f)
			{
				zero = new Vector2(-num2 * num3 / num4, -num2);
			}
			else
			{
				zero = Vector2.zero;
			}
		}
		else if (num3 > 0f)
		{
			zero = new Vector2(num, num * num4 / num3);
		}
		else if (num3 < 0f)
		{
			zero = new Vector2(-num, -num * num4 / num3);
		}
		else
		{
			zero = Vector2.zero;
		}
		return new Vector3(from.x + zero.x, from.y + zero.y, 0f);
	}

	// Token: 0x040013CE RID: 5070
	private UIArrow uiArrow;

	// Token: 0x040013CF RID: 5071
	public TrackingState state;

	// Token: 0x040013D0 RID: 5072
	public TileObject tile;

	// Token: 0x040013D1 RID: 5073
	public TileObject tile2;

	// Token: 0x040013D2 RID: 5074
	public Block block;

	// Token: 0x040013D3 RID: 5075
	public Block block2;

	// Token: 0x040013D4 RID: 5076
	public Vector3 offset;

	// Token: 0x040013D5 RID: 5077
	public Panel panel;

	// Token: 0x040013D6 RID: 5078
	public Vector3 world;

	// Token: 0x040013D7 RID: 5079
	public Vector3 world2;

	// Token: 0x040013D8 RID: 5080
	public Vector3 faceNormal;

	// Token: 0x040013D9 RID: 5081
	public Vector3 screen;

	// Token: 0x040013DA RID: 5082
	public Vector3 screen2;

	// Token: 0x040013DB RID: 5083
	public float bounce;

	// Token: 0x040013DC RID: 5084
	public Vector3 from;

	// Token: 0x040013DD RID: 5085
	public Vector3 to;

	// Token: 0x040013DE RID: 5086
	private int swipe;
}
