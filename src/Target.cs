using Blocks;
using UnityEngine;

public class Target
{
	private float timer;

	public TargetState state;

	public TileObject tile;

	public Block block;

	public Panel panel;

	public Vector3 offset;

	public Vector3 world;

	public Vector3 screen;

	private bool _visible;

	private bool _inWorldSpace;

	private GameObject _worldSpaceTargetObj;

	private GameObject _overlayTargetObj;

	private Arrow _arrowTarget;

	public Target()
	{
		_worldSpaceTargetObj = Object.Instantiate(Resources.Load("GUI/Prefab Target") as GameObject);
		_worldSpaceTargetObj.SetLayer(Layer.Default);
		_overlayTargetObj = Blocksworld.UI.Overlay.CreateTargetOverlayObject();
		Show(show: false);
	}

	public void Destroy()
	{
		Object.Destroy(_worldSpaceTargetObj);
		Blocksworld.UI.Overlay.RemoveOverlayObject(_overlayTargetObj);
	}

	public void TargetTile(TileObject tile)
	{
		SetToOverlay();
		this.tile = tile;
		timer = 0f;
		state = TargetState.Tile;
		Show(show: true);
	}

	public void TargetSurface(Vector3 point, Vector3 normal)
	{
		SetToWorldSpace();
		_worldSpaceTargetObj.transform.position = point;
		_worldSpaceTargetObj.transform.LookAt(_worldSpaceTargetObj.transform.position + normal);
		timer = 0f;
		state = TargetState.Surface;
		Show(show: true);
	}

	public void TargetBlock(Block block)
	{
		SetToOverlay();
		this.block = block;
		timer = 0f;
		state = TargetState.Block;
		Show(show: true);
	}

	public void TargetPanelOffset(Panel panel, Vector3 offset)
	{
		SetToOverlay();
		this.panel = panel;
		this.offset = offset;
		timer = 0f;
		state = TargetState.PanelOffset;
		Show(show: true);
	}

	public void TargetWorld(Vector3 world)
	{
		SetToOverlay();
		this.world = world;
		timer = 0f;
		state = TargetState.World;
		Show(show: true);
	}

	public void TargetScreen(Vector3 screen)
	{
		SetToOverlay();
		this.screen = screen;
		timer = 0f;
		state = TargetState.Screen;
		Show(show: true);
	}

	public void TargetButton(TILE_BUTTON button)
	{
		SetToOverlay();
		Transform transformForButton = Blocksworld.UI.GetTransformForButton(button);
		if (transformForButton != null)
		{
			Vector3 vector = Util.CenterOfRectTransform(transformForButton);
			TargetScreen(new Vector3(vector.x / NormalizedScreen.scale, vector.y / NormalizedScreen.scale, 1f));
		}
	}

	public void TargetControl(UIInputControl.ControlType control)
	{
		SetToOverlay();
		Transform transformForControl = Blocksworld.UI.Controls.GetTransformForControl(control);
		if (transformForControl != null)
		{
			Vector3 vector = Util.CenterOfRectTransform(transformForControl);
			TargetScreen(new Vector3(vector.x / NormalizedScreen.scale, vector.y / NormalizedScreen.scale, 1f));
		}
	}

	public void TargetMoverControl()
	{
		SetToOverlay();
		Transform transformForLeftMover = Blocksworld.UI.Controls.GetTransformForLeftMover();
		if (transformForLeftMover != null)
		{
			Vector3 vector = Util.CenterOfRectTransform(transformForLeftMover);
			TargetScreen(new Vector3(vector.x / NormalizedScreen.scale, vector.y / NormalizedScreen.scale, 1f));
		}
	}

	public void TargetArrowEnd(Arrow arrow)
	{
		SetToOverlay();
		_arrowTarget = arrow;
		state = TargetState.ArrowEnd;
	}

	public void Update()
	{
		if (!_visible)
		{
			return;
		}
		timer = 5f * Time.time;
		float num = 1f;
		Vector3 pos = Vector3.zero;
		switch (state)
		{
		case TargetState.Tile:
			num = 80f + 20f * Mathf.Sin(timer);
			if (tile.IsShowing())
			{
				Vector3 centerPosition = tile.GetCenterPosition();
				pos = new Vector3(centerPosition.x, centerPosition.y, -0.5f);
				_overlayTargetObj.SetActive(value: true);
			}
			else
			{
				_overlayTargetObj.SetActive(value: false);
			}
			break;
		case TargetState.Block:
			num = 80f + 20f * Mathf.Sin(timer);
			if (block != null && block.go != null)
			{
				pos = Util.WorldToScreenPoint(block.go.transform.position, z: true);
			}
			break;
		case TargetState.Surface:
			num = 1f + 0.2f * Mathf.Sin(timer);
			break;
		case TargetState.PanelOffset:
			num = 80f + 20f * Mathf.Sin(timer);
			pos = panel.position + offset;
			break;
		case TargetState.World:
			num = 80f + 20f * Mathf.Sin(timer);
			pos = Util.WorldToScreenPoint(world, z: true);
			break;
		case TargetState.Screen:
			num = 80f + 20f * Mathf.Sin(timer);
			pos = screen;
			break;
		case TargetState.ArrowEnd:
			num = 80f + 20f * Mathf.Sin(timer);
			pos = _arrowTarget.to;
			break;
		}
		if (_inWorldSpace)
		{
			_worldSpaceTargetObj.transform.localScale = new Vector3(num, num, 1f);
			return;
		}
		Blocksworld.UI.Overlay.SetOverlayObjectPosition(_overlayTargetObj, pos);
		Blocksworld.UI.Overlay.SetOverlayObjectSize(_overlayTargetObj, num);
	}

	public Vector3 GetScreenPosition()
	{
		if (_inWorldSpace)
		{
			return Util.WorldToScreenPoint(_worldSpaceTargetObj.transform.position, z: false);
		}
		return Blocksworld.UI.Overlay.GetOverlayObjectPosition(_overlayTargetObj);
	}

	public Vector3 GetWorldPosition()
	{
		if (!_inWorldSpace)
		{
			return Blocksworld.guiCamera.ScreenToWorldPoint(Blocksworld.UI.Overlay.GetOverlayObjectPosition(_overlayTargetObj));
		}
		return _worldSpaceTargetObj.transform.position;
	}

	public Vector3 GetWorldNormal()
	{
		if (!_inWorldSpace)
		{
			return Vector3.up;
		}
		return _worldSpaceTargetObj.transform.forward;
	}

	private void SetToWorldSpace()
	{
		_inWorldSpace = true;
		Show(_visible);
	}

	private void SetToOverlay()
	{
		_inWorldSpace = false;
		Show(_visible);
	}

	public void Show(bool show)
	{
		bool active = show && !_inWorldSpace;
		bool active2 = show && _inWorldSpace;
		_worldSpaceTargetObj.SetActive(active2);
		_overlayTargetObj.SetActive(active);
		_visible = show;
		Update();
	}

	public bool InWorldSpace()
	{
		return _inWorldSpace;
	}

	public bool IsShowing()
	{
		return _visible;
	}
}
