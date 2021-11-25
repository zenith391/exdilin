using System;
using Blocks;
using UnityEngine;

// Token: 0x020002BF RID: 703
public class Target
{
	// Token: 0x06002042 RID: 8258 RVA: 0x000ED4F0 File Offset: 0x000EB8F0
	public Target()
	{
		this._worldSpaceTargetObj = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("GUI/Prefab Target") as GameObject);
		this._worldSpaceTargetObj.SetLayer(Layer.Default, false);
		this._overlayTargetObj = Blocksworld.UI.Overlay.CreateTargetOverlayObject();
		this.Show(false);
	}

	// Token: 0x06002043 RID: 8259 RVA: 0x000ED546 File Offset: 0x000EB946
	public void Destroy()
	{
		UnityEngine.Object.Destroy(this._worldSpaceTargetObj);
		Blocksworld.UI.Overlay.RemoveOverlayObject(this._overlayTargetObj);
	}

	// Token: 0x06002044 RID: 8260 RVA: 0x000ED568 File Offset: 0x000EB968
	public void TargetTile(TileObject tile)
	{
		this.SetToOverlay();
		this.tile = tile;
		this.timer = 0f;
		this.state = TargetState.Tile;
		this.Show(true);
	}

	// Token: 0x06002045 RID: 8261 RVA: 0x000ED590 File Offset: 0x000EB990
	public void TargetSurface(Vector3 point, Vector3 normal)
	{
		this.SetToWorldSpace();
		this._worldSpaceTargetObj.transform.position = point;
		this._worldSpaceTargetObj.transform.LookAt(this._worldSpaceTargetObj.transform.position + normal);
		this.timer = 0f;
		this.state = TargetState.Surface;
		this.Show(true);
	}

	// Token: 0x06002046 RID: 8262 RVA: 0x000ED5F3 File Offset: 0x000EB9F3
	public void TargetBlock(Block block)
	{
		this.SetToOverlay();
		this.block = block;
		this.timer = 0f;
		this.state = TargetState.Block;
		this.Show(true);
	}

	// Token: 0x06002047 RID: 8263 RVA: 0x000ED61B File Offset: 0x000EBA1B
	public void TargetPanelOffset(Panel panel, Vector3 offset)
	{
		this.SetToOverlay();
		this.panel = panel;
		this.offset = offset;
		this.timer = 0f;
		this.state = TargetState.PanelOffset;
		this.Show(true);
	}

	// Token: 0x06002048 RID: 8264 RVA: 0x000ED64A File Offset: 0x000EBA4A
	public void TargetWorld(Vector3 world)
	{
		this.SetToOverlay();
		this.world = world;
		this.timer = 0f;
		this.state = TargetState.World;
		this.Show(true);
	}

	// Token: 0x06002049 RID: 8265 RVA: 0x000ED672 File Offset: 0x000EBA72
	public void TargetScreen(Vector3 screen)
	{
		this.SetToOverlay();
		this.screen = screen;
		this.timer = 0f;
		this.state = TargetState.Screen;
		this.Show(true);
	}

	// Token: 0x0600204A RID: 8266 RVA: 0x000ED69C File Offset: 0x000EBA9C
	public void TargetButton(TILE_BUTTON button)
	{
		this.SetToOverlay();
		Transform transformForButton = Blocksworld.UI.GetTransformForButton(button);
		if (transformForButton != null)
		{
			Vector3 vector = Util.CenterOfRectTransform(transformForButton);
			this.TargetScreen(new Vector3(vector.x / NormalizedScreen.scale, vector.y / NormalizedScreen.scale, 1f));
		}
	}

	// Token: 0x0600204B RID: 8267 RVA: 0x000ED6F8 File Offset: 0x000EBAF8
	public void TargetControl(UIInputControl.ControlType control)
	{
		this.SetToOverlay();
		Transform transformForControl = Blocksworld.UI.Controls.GetTransformForControl(control);
		if (transformForControl != null)
		{
			Vector3 vector = Util.CenterOfRectTransform(transformForControl);
			this.TargetScreen(new Vector3(vector.x / NormalizedScreen.scale, vector.y / NormalizedScreen.scale, 1f));
		}
	}

	// Token: 0x0600204C RID: 8268 RVA: 0x000ED75C File Offset: 0x000EBB5C
	public void TargetMoverControl()
	{
		this.SetToOverlay();
		Transform transformForLeftMover = Blocksworld.UI.Controls.GetTransformForLeftMover();
		if (transformForLeftMover != null)
		{
			Vector3 vector = Util.CenterOfRectTransform(transformForLeftMover);
			this.TargetScreen(new Vector3(vector.x / NormalizedScreen.scale, vector.y / NormalizedScreen.scale, 1f));
		}
	}

	// Token: 0x0600204D RID: 8269 RVA: 0x000ED7BC File Offset: 0x000EBBBC
	public void TargetArrowEnd(Arrow arrow)
	{
		this.SetToOverlay();
		this._arrowTarget = arrow;
		this.state = TargetState.ArrowEnd;
	}

	// Token: 0x0600204E RID: 8270 RVA: 0x000ED7D4 File Offset: 0x000EBBD4
	public void Update()
	{
		if (!this._visible)
		{
			return;
		}
		this.timer = 5f * Time.time;
		float num = 1f;
		Vector3 pos = Vector3.zero;
		switch (this.state)
		{
		case TargetState.Tile:
			num = 80f + 20f * Mathf.Sin(this.timer);
			if (this.tile.IsShowing())
			{
				Vector3 centerPosition = this.tile.GetCenterPosition();
				pos = new Vector3(centerPosition.x, centerPosition.y, -0.5f);
				this._overlayTargetObj.SetActive(true);
			}
			else
			{
				this._overlayTargetObj.SetActive(false);
			}
			break;
		case TargetState.Block:
			num = 80f + 20f * Mathf.Sin(this.timer);
			if (this.block != null && this.block.go != null)
			{
				pos = Util.WorldToScreenPoint(this.block.go.transform.position, true);
			}
			break;
		case TargetState.Surface:
			num = 1f + 0.2f * Mathf.Sin(this.timer);
			break;
		case TargetState.PanelOffset:
			num = 80f + 20f * Mathf.Sin(this.timer);
			pos = this.panel.position + this.offset;
			break;
		case TargetState.World:
			num = 80f + 20f * Mathf.Sin(this.timer);
			pos = Util.WorldToScreenPoint(this.world, true);
			break;
		case TargetState.Screen:
			num = 80f + 20f * Mathf.Sin(this.timer);
			pos = this.screen;
			break;
		case TargetState.ArrowEnd:
			num = 80f + 20f * Mathf.Sin(this.timer);
			pos = this._arrowTarget.to;
			break;
		}
		if (this._inWorldSpace)
		{
			this._worldSpaceTargetObj.transform.localScale = new Vector3(num, num, 1f);
		}
		else
		{
			Blocksworld.UI.Overlay.SetOverlayObjectPosition(this._overlayTargetObj, pos);
			Blocksworld.UI.Overlay.SetOverlayObjectSize(this._overlayTargetObj, num);
		}
	}

	// Token: 0x0600204F RID: 8271 RVA: 0x000EDA2D File Offset: 0x000EBE2D
	public Vector3 GetScreenPosition()
	{
		if (this._inWorldSpace)
		{
			return Util.WorldToScreenPoint(this._worldSpaceTargetObj.transform.position, false);
		}
		return Blocksworld.UI.Overlay.GetOverlayObjectPosition(this._overlayTargetObj);
	}

	// Token: 0x06002050 RID: 8272 RVA: 0x000EDA66 File Offset: 0x000EBE66
	public Vector3 GetWorldPosition()
	{
		if (!this._inWorldSpace)
		{
			return Blocksworld.guiCamera.ScreenToWorldPoint(Blocksworld.UI.Overlay.GetOverlayObjectPosition(this._overlayTargetObj));
		}
		return this._worldSpaceTargetObj.transform.position;
	}

	// Token: 0x06002051 RID: 8273 RVA: 0x000EDAA3 File Offset: 0x000EBEA3
	public Vector3 GetWorldNormal()
	{
		if (!this._inWorldSpace)
		{
			return Vector3.up;
		}
		return this._worldSpaceTargetObj.transform.forward;
	}

	// Token: 0x06002052 RID: 8274 RVA: 0x000EDAC6 File Offset: 0x000EBEC6
	private void SetToWorldSpace()
	{
		this._inWorldSpace = true;
		this.Show(this._visible);
	}

	// Token: 0x06002053 RID: 8275 RVA: 0x000EDADB File Offset: 0x000EBEDB
	private void SetToOverlay()
	{
		this._inWorldSpace = false;
		this.Show(this._visible);
	}

	// Token: 0x06002054 RID: 8276 RVA: 0x000EDAF0 File Offset: 0x000EBEF0
	public void Show(bool show)
	{
		bool active = show && !this._inWorldSpace;
		bool active2 = show && this._inWorldSpace;
		this._worldSpaceTargetObj.SetActive(active2);
		this._overlayTargetObj.SetActive(active);
		this._visible = show;
		this.Update();
	}

	// Token: 0x06002055 RID: 8277 RVA: 0x000EDB45 File Offset: 0x000EBF45
	public bool InWorldSpace()
	{
		return this._inWorldSpace;
	}

	// Token: 0x06002056 RID: 8278 RVA: 0x000EDB4D File Offset: 0x000EBF4D
	public bool IsShowing()
	{
		return this._visible;
	}

	// Token: 0x04001B89 RID: 7049
	private float timer;

	// Token: 0x04001B8A RID: 7050
	public TargetState state;

	// Token: 0x04001B8B RID: 7051
	public TileObject tile;

	// Token: 0x04001B8C RID: 7052
	public Block block;

	// Token: 0x04001B8D RID: 7053
	public Panel panel;

	// Token: 0x04001B8E RID: 7054
	public Vector3 offset;

	// Token: 0x04001B8F RID: 7055
	public Vector3 world;

	// Token: 0x04001B90 RID: 7056
	public Vector3 screen;

	// Token: 0x04001B91 RID: 7057
	private bool _visible;

	// Token: 0x04001B92 RID: 7058
	private bool _inWorldSpace;

	// Token: 0x04001B93 RID: 7059
	private GameObject _worldSpaceTargetObj;

	// Token: 0x04001B94 RID: 7060
	private GameObject _overlayTargetObj;

	// Token: 0x04001B95 RID: 7061
	private Arrow _arrowTarget;
}
