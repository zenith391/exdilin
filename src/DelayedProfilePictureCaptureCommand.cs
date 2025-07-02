using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class DelayedProfilePictureCaptureCommand : Command
{
	private float flashTime = 1.2f;

	private float screenshotTime = 1.8f;

	private int timeCounter;

	private bool modifiedFov;

	private float fov;

	private float newFov;

	private float near;

	private float far;

	private float aspect;

	private bool screenshotTaken;

	private CameraFlashEffect effect;

	private Bounds modelBounds = new Bounds(Vector3.zero, Vector3.one);

	private List<Block> profileModel;

	private const float MAX_SIZE_BEFORE_FOV_CHANGE = 3f;

	private const float MAX_SIZE_BEFORE_POS_CHANGE = 2.5f;

	private Dictionary<Tile, bool> buttonsShowingBeforeCapture = new Dictionary<Tile, bool>();

	private float prevTime;

	private void CalculateProfileModelBounds()
	{
		if (profileModel != null)
		{
			return;
		}
		foreach (Block item in BWSceneManager.AllBlocks())
		{
			if (item.IsProfileCharacter())
			{
				item.UpdateConnectedCache();
				profileModel = Block.connectedCache[item];
				modelBounds = Util.ComputeBoundsDetailed(profileModel);
				break;
			}
		}
		if (profileModel == null)
		{
			BWLog.Info("Could not find the profile character");
		}
	}

	public override void Execute()
	{
		CalculateProfileModelBounds();
		Camera mainCamera = Blocksworld.mainCamera;
		BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
		if (!modifiedFov)
		{
			AdaptCameraToTargetSize(mainCamera, blocksworldCamera);
		}
		if (timeCounter == 0)
		{
			buttonsShowingBeforeCapture.Clear();
			List<Tile> list = new List<Tile>();
			foreach (Tile item in list)
			{
				if (item != null)
				{
					buttonsShowingBeforeCapture[item] = item.IsShowing();
					item.Show(show: false);
				}
			}
		}
		timeCounter++;
		float num = (float)timeCounter * Blocksworld.fixedDeltaTime;
		if (num < flashTime)
		{
			Blocksworld.UI.Overlay.ShowOnScreenMessage("Pose for the camera!");
		}
		else
		{
			Blocksworld.UI.Overlay.HideOnScreenMessage();
		}
		if (num >= flashTime && prevTime < flashTime)
		{
			Sound.PlayOneShotSound("Button Play");
			effect = Blocksworld.guiCamera.gameObject.AddComponent<CameraFlashEffect>();
		}
		else if (num >= screenshotTime)
		{
			if (!screenshotTaken)
			{
				WorldSession.current.TakeScreenshot();
				screenshotTaken = true;
			}
			else if (!Blocksworld.lockInput)
			{
				done = true;
				RestoreCamera(mainCamera, blocksworldCamera);
				CleanUp();
				Blocksworld.stopASAP = true;
			}
		}
		prevTime = num;
	}

	private void CleanUp()
	{
		if (effect != null)
		{
			Object.Destroy(effect);
			effect = null;
		}
		foreach (KeyValuePair<Tile, bool> item in buttonsShowingBeforeCapture)
		{
			item.Key.Show(item.Value);
		}
		buttonsShowingBeforeCapture.Clear();
	}

	private void RestoreCamera(Camera cam, BlocksworldCamera bwCam)
	{
		if (modifiedFov)
		{
			cam.nearClipPlane = near;
			cam.farClipPlane = far;
			cam.aspect = aspect;
			cam.fieldOfView = fov;
			cam.projectionMatrix = Matrix4x4.Perspective(fov, aspect, near, far);
		}
		bwCam.SetLookAtOffset(Vector3.zero);
		bwCam.SetMoveToOffset(Vector3.zero);
	}

	private void AdaptCameraToTargetSize(Camera cam, BlocksworldCamera bwCam)
	{
		fov = cam.fieldOfView;
		near = cam.nearClipPlane;
		far = cam.farClipPlane;
		aspect = cam.aspect;
		newFov = Mathf.Clamp(fov + (modelBounds.size.y - 3f) * 10f, 45f, 75f);
		cam.projectionMatrix = Matrix4x4.Perspective(newFov, aspect, 1.5f, far);
		float num = Mathf.Clamp((modelBounds.size.y - 2.5f) * 0.5f, 0f, 2f);
		bwCam.SetLookAtOffset(Vector3.up * num);
		bwCam.SetMoveToOffset(Vector3.up * Mathf.Max(num * 0.75f, 0f));
		modifiedFov = true;
	}

	public override void Removed()
	{
		CleanUp();
		base.Removed();
	}
}
