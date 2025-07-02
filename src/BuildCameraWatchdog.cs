using UnityEngine;

public class BuildCameraWatchdog
{
	private bool cameraResetVisible;

	private float timeWithinTerrain;

	private Bounds worldBounds;

	private Bounds testBounds;

	private HudMeshButton resetButton;

	private HudMeshStyle resetButtonStyle;

	private const float MAX_TIME_WITHIN_TERRAIN = 5f;

	private const float DEFAULT_WORLD_BOUNDS_SIZE = 150f;

	public BuildCameraWatchdog()
	{
		worldBounds = GetDefaultWorldBounds();
	}

	public void SetWorldBounds(Bounds b)
	{
		worldBounds = b;
		UpdateTestBounds();
	}

	private void UpdateTestBounds()
	{
		testBounds = worldBounds;
		testBounds.Expand(400f);
	}

	public void EncapsulateWorldBounds(Bounds b)
	{
		worldBounds.Encapsulate(b);
		UpdateTestBounds();
	}

	public Bounds GetWorldBounds()
	{
		return worldBounds;
	}

	public Bounds GetDefaultWorldBounds()
	{
		return new Bounds(Vector3.zero, Vector3.one * 150f);
	}

	public void Reset(bool boundsToo = true)
	{
		timeWithinTerrain = 0f;
		cameraResetVisible = false;
		if (boundsToo)
		{
			worldBounds = GetDefaultWorldBounds();
		}
	}

	public void Update()
	{
		if (Blocksworld.CurrentState == State.Build && Blocksworld.IsStarted() && WorldSession.current != null && WorldSession.current.worldLoadComplete)
		{
			Vector3 position = Blocksworld.cameraTransform.position;
			if (Util.PointWithinTerrain(position))
			{
				timeWithinTerrain += Time.deltaTime;
			}
			else
			{
				timeWithinTerrain = 0f;
			}
			cameraResetVisible = timeWithinTerrain > 5f || !testBounds.Contains(position);
		}
		else
		{
			cameraResetVisible = false;
		}
		if (!cameraResetVisible)
		{
			return;
		}
		float scale = NormalizedScreen.scale;
		float num = 200f * scale;
		float num2 = 50f * scale;
		Rect rect = new Rect((float)Screen.width * 0.5f - num * 0.5f, (float)Screen.height - num2 - 10f * scale, num, num2);
		if (resetButtonStyle == null)
		{
			resetButtonStyle = HudMeshOnGUI.dataSource.GetStyle("Build Mode Dialog Button");
		}
		if (!HudMeshOnGUI.Button(ref resetButton, rect, "Reset Camera", resetButtonStyle))
		{
			return;
		}
		BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
		Transform cameraTransform = Blocksworld.cameraTransform;
		blocksworldCamera.Unfollow();
		Bounds bounds = GetWorldBounds();
		Vector3 vector = bounds.center + bounds.size.y * 0.5f * Vector3.up;
		Vector3 vector2 = new Vector3(0f, -1f, 1f).normalized;
		foreach (NamedPose cameraPose in Blocksworld.cameraPoses)
		{
			if (cameraPose.name == "Camera Reset Pose")
			{
				vector = cameraPose.position;
				vector2 = cameraPose.direction;
				break;
			}
		}
		cameraTransform.position = vector;
		Vector3 vector3 = vector + vector2 * 15f;
		cameraTransform.LookAt(vector3);
		blocksworldCamera.SetTargetPosition(vector3);
		blocksworldCamera.SetTargetDistance(15f);
		Reset(boundsToo: false);
	}

	public bool CameraResetVisible()
	{
		return cameraResetVisible;
	}
}
