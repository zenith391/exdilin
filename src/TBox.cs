using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

public class TBox
{
	public enum MoveMode
	{
		Raycast,
		Plane,
		Up
	}

	public enum RotateMode
	{
		Button,
		Finger
	}

	public enum ScaleMode
	{
		Plane,
		Up
	}

	public static Vector3 v1;

	public static Vector3 v2;

	private static readonly float _minButtonScale = 1f;

	private static readonly float _maxButtonScale = 1.25f;

	public static Vector3 targetSnapScale = Vector3.zero;

	public static Vector3 targetSnapScaleMaxDistance = Vector3.zero;

	public static Vector3 targetSnapPosition = Vector3.zero;

	public static Vector3 targetSnapPositionMaxDistance = Vector3.zero;

	public static TileObject tileButtonMove;

	public static TileObject tileButtonRotate;

	public static TileObject tileButtonScale;

	public static TileObject tileLockedModelIcon;

	public static TileObject tileCharacterEditIcon;

	public static TileObject tileCharacterEditExitIcon;

	public static string moveIconName = "Buttons/Move";

	public static string moveUpIconName = "Buttons/Move_Up";

	public static string rotateIconName = "Buttons/Rotate";

	public static string scaleIconName = "Buttons/Scale";

	public static string scaleUpIconName = "Buttons/Scale_Up";

	public static string lockedModelIconName = "Misc/Locked_Model_Icon";

	public static string characterEditOnIconName = "Misc/Character_Editor";

	public static string characterEditOffIconName = "Misc/Character_Editor_Exit";

	private static float screenThickness = 10f;

	private static float thickness = 0.05f;

	public static float buttonScaleOffset = -58f;

	public static float buttonMoveToOffset = 30f;

	public static float buttonRotateOffset = 58f;

	public static float buttonCopyOffset = 58f;

	public static float buttonSaveOffset = 48f;

	public static float buttonSpacingScale = 1f;

	public static float buttonTileScale = 1f;

	public static BoxCollider collider;

	public static bool noSnap = false;

	private static GameObject go;

	private static GameObject[] goEdges = new GameObject[12];

	private static GameObject[] goEdgesHidden = new GameObject[12];

	private static GameObject goOrientationArrow;

	private static Material materialWhite;

	private static Material materialWhiteTopmost;

	private static Material materialGrey;

	private static Material materialRed;

	private static Dictionary<Face, List<int>> faces = new Dictionary<Face, List<int>>();

	public static ITBox selected = null;

	private static Block selectedBlock = null;

	private static Face selectedFace = Face.None;

	private static GameObject activeHandle;

	private static Vector3 activePos;

	private static Vector3 activeAxis;

	private static Vector3 activeNormal;

	private static Quaternion blockStartRotation;

	private static Vector3 blockStartPos;

	private static Vector3 axisPosStart;

	private static float angleStart;

	private static bool startedMove;

	private static float scaleScreenToWorld;

	private static MoveMode dragBlockMode;

	private static bool dragBlockColliding;

	public static Tween dragBlockTween = new Tween();

	private static Vector3 dragBlockAxis;

	private static Vector3 dragBlockStartPos;

	private static Vector3 dragBlockSearchFreePos;

	private static Vector3 dragBlockCurrentPos;

	private static Vector3 dragBlockLastDesiredPos;

	private static Vector3 dragBlockLastHitNormal;

	private static Vector3 dragBlockLastFreePos;

	private static Vector3 dragBlockStartAxisPos;

	private static Vector3 dragBlockScale;

	private static Vector3 dragButtonStartWorldPos;

	private static Quaternion dragBlockStartRot;

	private static Quaternion dragBlockSearchFreeRot;

	private static Quaternion dragBlockLastFreeRot;

	private static float goArrowRotateYAngle;

	private static RotateMode rotateBlockMode;

	private static bool rotateBlockColliding;

	private static Tween rotateBlockMoveTween = new Tween();

	private static float rotateBlockOffsetSignFlip = 1f;

	private static Quaternion rotateBlockStartRotation;

	private static Quaternion rotateBlockLastDesiredRot;

	private static Vector3 rotateBlockStartPos;

	private static Vector3 rotateBlockSearchFreePos;

	private static Vector3 rotateBlockScale;

	private static ScaleMode scaleBlockMode;

	private static bool scaleBlockColliding;

	private static Tween scaleBlockTween = new Tween();

	private static Vector3 scaleBlockStartScale;

	private static Vector3 scaleBlockStartPos;

	private static Vector3 scaleBlockSearchFreeScale;

	private static Vector3 scaleBlockSearchFreePos;

	private static Vector3 scaleBlockLastDesiredScale;

	private static Vector3 scaleBlockCurrentScale;

	private static Vector3 scaleBlockCurrentPos;

	private static Vector3 scaleBlockLastFreeScale;

	private static Vector3 scaleBlockLastFreePos;

	private static Vector3 scaleButtonStartWorldPos;

	private static Vector3 scaleBlockWorldAxis;

	private static Vector3 scaleBlockWorldAxis2;

	private static Vector3 scaleBlockWorldAxisStartPos;

	private static Vector3 scaleBlockWorldAxisStartPos2;

	public static void Init()
	{
		go = new GameObject("TBox");
		collider = go.AddComponent<BoxCollider>();
		collider.enabled = false;
		materialWhite = Materials.materials["White"];
		materialWhiteTopmost = Resources.Load("GUI/Box White Transparent Topmost") as Material;
		materialGrey = Materials.materials["Grey"];
		materialRed = Materials.materials["Red"];
		for (int i = 0; i < 12; i++)
		{
			goEdges[i] = CreateCube(go, materialWhite);
			goEdgesHidden[i] = CreateCube(go, materialWhiteTopmost);
		}
		tileButtonMove = Blocksworld.tilePool.GetTileObjectForIcon(moveIconName, enabled: true);
		tileButtonRotate = Blocksworld.tilePool.GetTileObjectForIcon(rotateIconName, enabled: true);
		tileButtonScale = Blocksworld.tilePool.GetTileObjectForIcon(scaleIconName, enabled: true);
		tileLockedModelIcon = Blocksworld.tilePool.GetTileObjectForIcon(lockedModelIconName, enabled: true);
		tileCharacterEditIcon = Blocksworld.tilePool.GetTileObjectForIcon(characterEditOnIconName, enabled: true);
		tileCharacterEditExitIcon = Blocksworld.tilePool.GetTileObjectForIcon(characterEditOffIconName, enabled: true);
		tileCharacterEditExitIcon.MoveTo(50f, NormalizedScreen.height - 100);
		goOrientationArrow = UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Orientation Arrow") as GameObject);
		faces[Face.XPos] = new List<int> { 5, 6, 9, 10 };
		faces[Face.XNeg] = new List<int> { 4, 7, 8, 11 };
		faces[Face.YPos] = new List<int> { 1, 2, 10, 11 };
		faces[Face.YNeg] = new List<int> { 0, 3, 8, 9 };
		faces[Face.ZPos] = new List<int> { 2, 3, 6, 7 };
		faces[Face.ZNeg] = new List<int> { 0, 1, 4, 5 };
		Show(show: false);
	}

	private static GameObject CreateCube(GameObject parent, Material m)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
		gameObject.transform.parent = parent.transform;
		gameObject.GetComponent<Renderer>().sharedMaterial = m;
		return gameObject;
	}

	private static GameObject CreateArrow(GameObject parent, Material m)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("GUI/Prefab Arrow") as GameObject);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
		gameObject.transform.parent = parent.transform;
		gameObject.GetComponent<Renderer>().sharedMaterial = m;
		return gameObject;
	}

	public static void Attach(ITBox obj, bool silent = false)
	{
		selected = obj;
		if (obj is Block)
		{
			selectedBlock = (Block)obj;
		}
		else
		{
			selectedBlock = null;
		}
		FitToSelected();
		PaintRed(red: false);
		if (!silent)
		{
			Sound.PlaySound("Block Selected", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		}
	}

	public static void FitToSelected()
	{
		if (selected != null)
		{
			MoveTo(selected.GetPosition());
			RotateTo(selected.GetRotation());
			ScaleTo(selected.GetScale());
		}
	}

	public static void Detach(bool silent = false)
	{
		if (selected != null && !silent)
		{
			Sound.PlaySound("Block Unselected", Sound.GetOrCreateOneShotAudioSource(), oneShot: true);
		}
		selected = (selectedBlock = null);
	}

	private static bool SelectedBunchIsTankTreads()
	{
		if (selectedBlock == null && Blocksworld.selectedBunch != null && Blocksworld.SelectedBunchIsGroup())
		{
			return Blocksworld.selectedBunch.blocks[0] is BlockTankTreadsWheel;
		}
		return false;
	}

	private static bool SelectedBunchIsLockedGroup()
	{
		if (selectedBlock == null && Blocksworld.selectedBunch != null)
		{
			return Blocksworld.selectedBunch.blocks[0].HasGroup("locked-model");
		}
		return false;
	}

	private static bool SelectedBlockIsSingleBlockLockedModel()
	{
		if (selectedBlock != null && Blocksworld.selectedBunch == null)
		{
			return selectedBlock.HasGroup("locked-model");
		}
		return false;
	}

	public static void Show(bool show)
	{
		go.SetActive(show);
		bool flag = selectedBlock is BlockAbstractWheel || selectedBlock is BlockRaycastWheel;
		bool flag2 = selectedBlock is BlockAbstractStabilizer;
		bool flag3 = selectedBlock is BlockAbstractAntiGravity;
		bool flag4 = selectedBlock is BlockTankTreadsWheel || SelectedBunchIsTankTreads();
		bool flag5 = SelectedBunchIsLockedGroup() || SelectedBlockIsSingleBlockLockedModel();
		bool flag6 = selectedBlock is BlockAnimatedCharacter;
		goOrientationArrow.SetActive(show && (flag || flag2 || flag3 || flag4));
		if (SelectedBunchIsTankTreads())
		{
			goOrientationArrow.transform.rotation = Blocksworld.selectedBunch.blocks[0].goT.rotation;
		}
		bool show2 = show;
		if (selectedBlock != null && selectedBlock.DisableBuildModeMove())
		{
			show2 = false;
		}
		bool flag7 = show;
		if (selectedBlock != null && selectedBlock.DisableBuildModeScale())
		{
			flag7 = false;
		}
		UpdateCopyButtonVisibility();
		tileButtonMove.Show(show2);
		tileButtonRotate.Show(show);
		tileButtonScale.Show(flag7 && selected != null && selected.CanScale() != Vector3.zero);
		tileLockedModelIcon.Show(show && flag5);
		if (WorldSession.isProfileBuildSession())
		{
			bool show3 = (selected == null || flag6) && !CharacterEditor.Instance.InEditMode() && Blocksworld.CurrentState == State.Build && WorldSession.current.profileWorldAnimatedBlockster != null;
			tileCharacterEditIcon.Show(show3);
			tileCharacterEditExitIcon.Show(show: false);
		}
		else
		{
			bool show4 = show && flag6 && selectedBlock.goT.up.y > 0.9f;
			tileCharacterEditIcon.Show(show4);
			tileCharacterEditExitIcon.Show(show: false);
		}
	}

	public static void UpdateCopyButtonVisibility()
	{
		bool flag = IsShowingCopy();
		bool flag2 = flag && IsShowingModel();
		if (flag)
		{
			Blocksworld.UI.SidePanel.ShowCopyModelButton();
		}
		else
		{
			Blocksworld.UI.SidePanel.HideCopyModelButton();
		}
		if (flag2)
		{
			Blocksworld.UI.SidePanel.ShowSaveModelButton();
		}
		else
		{
			Blocksworld.UI.SidePanel.HideSaveModelButton();
		}
	}

	public static bool IsShowingModel()
	{
		bool flag = IsShowingCopy();
		if (flag)
		{
			if (Blocksworld.selectedBlock != null)
			{
				flag = !Blocksworld.selectedBlock.HasDefaultTiles() && !Blocksworld.selectedBlock.HasGroup("locked-model");
			}
			else if (Blocksworld.selectedBunch != null)
			{
				if (Blocksworld.selectedBunch.blocks.Count == 1)
				{
					flag = !Blocksworld.selectedBunch.blocks[0].isTerrain;
				}
				else
				{
					foreach (Block block in Blocksworld.selectedBunch.blocks)
					{
						if (block.HasGroup("locked-model"))
						{
							flag = false;
							break;
						}
					}
				}
			}
		}
		return flag;
	}

	public static bool IsShowingCopy()
	{
		bool flag = go.activeSelf;
		if (flag && (Tutorial.InTutorialOrPuzzle() || !Blocksworld.buildPanel.IsShowing() || (Blocksworld.selectedBlock == null && Blocksworld.selectedBunch == null) || (Blocksworld.selectedBlock != null && ((Blocksworld.selectedBlock.isTerrain && Blocksworld.selectedBlock.DisableBuildModeMove()) || Blocksworld.selectedBlock is BlockGrouped))))
		{
			flag = false;
		}
		return flag;
	}

	public static bool IsShowing()
	{
		return go.activeSelf;
	}

	public static void MoveTo(Vector3 pos)
	{
		go.transform.position = pos;
		goOrientationArrow.transform.position = pos;
		if (SelectedBunchIsTankTreads())
		{
			goOrientationArrow.transform.rotation = Blocksworld.selectedBunch.blocks[0].goT.rotation;
		}
	}

	public static void RotateTo(Quaternion rot)
	{
		go.transform.rotation = rot;
		goOrientationArrow.transform.rotation = rot;
		if (SelectedBunchIsTankTreads())
		{
			goOrientationArrow.transform.rotation = Blocksworld.selectedBunch.blocks[0].goT.rotation;
		}
		if (selectedBlock is BlockAbstractStabilizer)
		{
			goOrientationArrow.transform.Rotate(-90f, -90f, 0f);
		}
		if (selectedBlock is BlockAbstractAntiGravity)
		{
			goOrientationArrow.transform.rotation *= ((BlockAbstractAntiGravity)selectedBlock).rotation;
		}
	}

	public static void ScaleTo(Vector3 size)
	{
		collider.size = size;
		goOrientationArrow.transform.localScale = size;
		if (SelectedBunchIsTankTreads())
		{
			float num = Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z));
			goOrientationArrow.transform.localScale = new Vector3(num, Mathf.Abs(size.y), num);
		}
		PositionEdge(0, 0f, -0.5f, -0.5f, 1f, 0f, 0f, size);
		PositionEdge(1, 0f, 0.5f, -0.5f, 1f, 0f, 0f, size);
		PositionEdge(2, 0f, 0.5f, 0.5f, 1f, 0f, 0f, size);
		PositionEdge(3, 0f, -0.5f, 0.5f, 1f, 0f, 0f, size);
		PositionEdge(4, -0.5f, 0f, -0.5f, 0f, 1f, 0f, size);
		PositionEdge(5, -0.5f, 0f, 0.5f, 0f, 1f, 0f, size);
		PositionEdge(6, 0.5f, 0f, 0.5f, 0f, 1f, 0f, size);
		PositionEdge(7, 0.5f, 0f, -0.5f, 0f, 1f, 0f, size);
		PositionEdge(8, -0.5f, -0.5f, 0f, 0f, 0f, 1f, size);
		PositionEdge(9, 0.5f, -0.5f, 0f, 0f, 0f, 1f, size);
		PositionEdge(10, 0.5f, 0.5f, 0f, 0f, 0f, 1f, size);
		PositionEdge(11, -0.5f, 0.5f, 0f, 0f, 0f, 1f, size);
	}

	private static void PositionEdge(int i, float x, float y, float z, float xs, float ys, float zs, Vector3 size)
	{
		goEdges[i].transform.localPosition = new Vector3(x * size.x, y * size.y, z * size.z);
		goEdges[i].transform.localScale = new Vector3(xs * size.x + thickness, ys * size.y + thickness, zs * size.z + thickness);
		goEdgesHidden[i].transform.localPosition = goEdges[i].transform.localPosition;
		goEdgesHidden[i].transform.localScale = goEdges[i].transform.localScale;
	}

	public static void SetLayer(int layer)
	{
		for (int i = 0; i < 12; i++)
		{
			goEdges[i].layer = layer;
		}
		for (int j = 0; j < 12; j++)
		{
			goEdgesHidden[j].layer = layer;
		}
	}

	private static void PositionButton(GameObject goButton, GameObject goArrow, bool show)
	{
		float num = scaleScreenToWorld;
		Transform transform = goButton.transform;
		transform.position = goArrow.transform.position;
		transform.LookAt(goButton.transform.position - Blocksworld.cameraForward);
		transform.localScale = 70f * num * Vector3.one;
		goButton.SetActive(show);
	}

	public static void PaintRed(bool red)
	{
		if (red)
		{
			for (int i = 0; i < 12; i++)
			{
				goEdges[i].GetComponent<Renderer>().sharedMaterial = materialRed;
			}
			return;
		}
		if (selectedFace == Face.None)
		{
			for (int j = 0; j < 12; j++)
			{
				goEdges[j].GetComponent<Renderer>().sharedMaterial = materialWhite;
			}
			return;
		}
		List<int> list = faces[selectedFace];
		for (int k = 0; k < 12; k++)
		{
			if (list.Contains(k))
			{
				goEdges[k].GetComponent<Renderer>().sharedMaterial = materialWhite;
				continue;
			}
			goEdges[k].GetComponent<Renderer>().sharedMaterial = materialGrey;
			goEdges[k].transform.localScale = 0.999f * goEdges[k].transform.localScale;
		}
	}

	public static void Update()
	{
		noSnap = Input.GetKey(KeyCode.LeftShift);
		if (go.activeSelf && Blocksworld.tBoxGesture.gestureState != GestureState.Active && tileButtonMove.IsShowing())
		{
			float num = Vector3.Angle(Blocksworld.mainCamera.ScreenPointToRay(tileButtonMove.GetPosition() * NormalizedScreen.scale).direction, Vector3.up);
			if (selected != null)
			{
				num = Vector3.Angle((selected.GetPosition() - Blocksworld.cameraPosition).normalized, Vector3.up);
			}
			if (num < 110f)
			{
				if (tileButtonMove.IconName() != moveUpIconName)
				{
					bool enabled = tileButtonMove.IsEnabled();
					tileButtonMove.SetupForIcon(moveUpIconName, enabled);
					bool show = tileButtonScale.IsShowing();
					bool flag = tileButtonScale.IsEnabled();
					tileButtonScale.SetupForIcon(scaleUpIconName, flag);
					tileButtonScale.Show(show);
					tileButtonScale.Enable(flag);
				}
			}
			else if (tileButtonMove.IconName() != moveIconName)
			{
				bool enabled2 = tileButtonMove.IsEnabled();
				tileButtonMove.SetupForIcon(moveIconName, enabled2);
				bool show2 = tileButtonScale.IsShowing();
				bool flag2 = tileButtonScale.IsEnabled();
				tileButtonScale.SetupForIcon(scaleIconName, flag2);
				tileButtonScale.Show(show2);
				tileButtonScale.Enable(flag2);
			}
		}
		if (selected != null)
		{
			scaleScreenToWorld = Util.ScreenToWorldScale(go.transform.position);
			float num2 = Mathf.Max(3f, Util.MinComponent(collider.size));
			thickness = Mathf.Min(0.025f * num2, screenThickness * scaleScreenToWorld);
			ScaleTo(collider.size);
		}
		UpdateButtons();
	}

	public static Vector3 BestTBoxCorner(Func<Vector3, Vector3, int> cmp = null)
	{
		if (cmp == null)
		{
			cmp = (Vector3 p1, Vector3 p2) => (int)Mathf.Sign(p1.y - p2.y);
		}
		Vector3 size = collider.size;
		float[] array = new float[8]
		{
			-0.5f * size.x,
			-0.5f * size.x,
			0.5f * size.x,
			0.5f * size.x,
			-0.5f * size.x,
			-0.5f * size.x,
			0.5f * size.x,
			0.5f * size.x
		};
		float[] array2 = new float[8]
		{
			0.5f * size.y,
			0.5f * size.y,
			0.5f * size.y,
			0.5f * size.y,
			-0.5f * size.y,
			-0.5f * size.y,
			-0.5f * size.y,
			-0.5f * size.y
		};
		float[] array3 = new float[8]
		{
			-0.5f * size.z,
			0.5f * size.z,
			0.5f * size.z,
			-0.5f * size.z,
			-0.5f * size.z,
			0.5f * size.z,
			0.5f * size.z,
			-0.5f * size.z
		};
		Vector3 result = Vector3.zero;
		Vector3 arg = Vector3.zero;
		for (int num = 0; num < 8; num++)
		{
			Vector3 vector = new Vector3(array[num], array2[num], array3[num]);
			Vector3 vector2 = go.transform.position + go.transform.rotation * vector;
			Vector3 vector3 = Util.WorldToScreenPoint(vector2, z: false);
			Vector3 vector4 = 0.001f * vector3 + 0.999f * SnapTileWithinScreen(vector3);
			if (num == 0 || cmp(vector4, arg) < 0)
			{
				arg = vector4;
				result = vector2;
			}
		}
		return result;
	}

	public static Vector3 SnapTileWithinScreen(Vector3 pos)
	{
		if (pos.y < 80f)
		{
			pos.y = 80f;
		}
		if (pos.y > (float)(NormalizedScreen.height - 80))
		{
			pos.y = NormalizedScreen.height - 80;
		}
		float num = NormalizedScreen.width;
		if (Blocksworld.buildPanel.IsShowing())
		{
			num = Blocksworld.buildPanel.position.x;
		}
		if (pos.x > num - 80f)
		{
			pos.x = num - 80f;
		}
		float num2 = 0f;
		if (pos.y < 160f || pos.y > (float)(NormalizedScreen.height - 160))
		{
			num2 = 200f;
		}
		if (pos.x < num2 + 80f)
		{
			pos.x = num2 + 80f;
		}
		return pos;
	}

	public static Vector3 GetMoveButtonInWorldSpace()
	{
		Vector3 position = Blocksworld.guiCamera.WorldToScreenPoint(tileButtonMove.GetCenterPosition());
		Vector3 position2 = BestTBoxCorner();
		float z = Blocksworld.mainCamera.WorldToScreenPoint(position2).z;
		position.z = z;
		return Blocksworld.mainCamera.ScreenToWorldPoint(position);
	}

	public static Vector3 GetRotateButtonInWorldSpace()
	{
		Vector3 buttonOffset = RotateButtonLocalOffset();
		return ProjectButtonOffsetIntoTBox(buttonOffset);
	}

	public static Vector3 GetScaleButtonInWorldSpace()
	{
		Vector3 buttonOffset = ScaleButtonLocalOffset();
		return ProjectButtonOffsetIntoTBox(buttonOffset);
	}

	private static Vector3 MoveButtonLocalOffset()
	{
		return new Vector3(0f, 0f - buttonMoveToOffset, 0f) * buttonSpacingScale;
	}

	private static Vector3 RotateButtonLocalOffset()
	{
		return new Vector3(buttonRotateOffset, 0f, 0f) * buttonSpacingScale;
	}

	private static Vector3 ScaleButtonLocalOffset()
	{
		return new Vector3(buttonScaleOffset, 0f, 0f) * buttonSpacingScale;
	}

	public static bool HitMove(Vector3 pos)
	{
		return tileButtonMove.HitExtended(pos, -10f, -10f, -10f, -10f);
	}

	public static bool HitRotate(Vector3 pos)
	{
		return tileButtonRotate.HitExtended(pos, -10f, -10f, -10f, -10f);
	}

	public static bool HitScale(Vector3 pos)
	{
		return tileButtonScale.HitExtended(pos, -10f, -10f, -10f, -10f);
	}

	private static Vector3 ProjectButtonOffsetIntoTBox(Vector3 buttonOffset)
	{
		Vector3 vector = BestTBoxCorner();
		float num = Util.ScreenToWorldScale(vector);
		float num2 = buttonOffset.x * num;
		float num3 = buttonOffset.y * num;
		return vector + num2 * Blocksworld.cameraRight + num3 * Blocksworld.cameraUp;
	}

	public static void UpdateButtons()
	{
		if (Blocksworld.tBoxGesture.IsActive)
		{
			return;
		}
		Block block = null;
		if (selected != null)
		{
			block = selectedBlock;
			Vector3 pos = Util.WorldToScreenPoint(BestTBoxCorner(), z: false);
			pos = SnapTileWithinScreen(pos);
			float value = 1f / NormalizedScreen.physicalScale;
			buttonSpacingScale = Mathf.Clamp(value, _minButtonScale, _maxButtonScale);
			float pixelScale = NormalizedScreen.pixelScale;
			pixelScale = Mathf.Max(_minButtonScale, pixelScale);
			buttonTileScale = 80f * pixelScale;
			float num = 0.5f * buttonTileScale;
			Vector3 vector = new Vector3(pos.x - num, pos.y - num, 21f);
			if (tileButtonScale.IsShowing())
			{
				tileButtonScale.MoveTo(vector + ScaleButtonLocalOffset());
				tileButtonScale.SetScale(pixelScale);
			}
			if (tileButtonMove.IsShowing())
			{
				tileButtonMove.MoveTo(vector + MoveButtonLocalOffset());
				tileButtonMove.SetScale(pixelScale);
			}
			if (tileButtonRotate.IsShowing())
			{
				tileButtonRotate.MoveTo(vector + RotateButtonLocalOffset());
				tileButtonRotate.SetScale(pixelScale);
			}
			if (tileLockedModelIcon.IsShowing())
			{
				Func<Vector3, Vector3, int> cmp = (Vector3 p1, Vector3 p2) => (int)Mathf.Sign(p2.y - p1.y);
				Vector3 worldPos = BestTBoxCorner(cmp);
				Vector3 vector2 = Util.WorldToScreenPoint(worldPos, z: false);
				tileLockedModelIcon.MoveTo(new Vector3(vector2.x - 40f * pixelScale, vector2.y - 20f * pixelScale, 21f));
				tileLockedModelIcon.SetScale(pixelScale);
			}
		}
		else if (WorldSession.isProfileBuildSession() && WorldSession.current.profileWorldAnimatedBlockster != null)
		{
			block = WorldSession.current.profileWorldAnimatedBlockster;
		}
		if (block != null)
		{
			if (tileCharacterEditIcon.IsShowing())
			{
				float pixelScale2 = NormalizedScreen.pixelScale;
				Vector3 worldPos2 = block.GetPosition() + 1.55f * Vector3.up;
				Vector3 pos2 = Util.WorldToScreenPoint(worldPos2, z: false);
				pos2.x -= 40f * pixelScale2;
				pos2 = SnapTileWithinScreen(pos2);
				pos2.z = 21f;
				tileCharacterEditIcon.MoveTo(pos2);
				tileCharacterEditIcon.SetScale(pixelScale2);
			}
			if (tileCharacterEditExitIcon.IsShowing())
			{
				float pixelScale3 = NormalizedScreen.pixelScale;
				Vector3 worldPos3 = block.GetPosition() + Vector3.up;
				worldPos3 -= Blocksworld.cameraRight * 2.5f;
				Vector3 pos3 = Util.WorldToScreenPoint(worldPos3, z: false);
				pos3 = SnapTileWithinScreen(pos3);
				pos3.z = 21f;
				tileCharacterEditExitIcon.MoveTo(pos3);
				tileCharacterEditExitIcon.SetScale(pixelScale3);
			}
		}
	}

	public static Vector3 GetMoveButtonPositionWhenMovedTo(Vector3 worldSpacePos)
	{
		Vector3 vector = Util.WorldToScreenPoint(BestTBoxCorner(), z: false);
		Vector3 vector2 = Util.WorldToScreenPoint(Blocksworld.selectedBlock.GetPosition(), z: false);
		Vector3 vector3 = vector - vector2;
		Vector3 vector4 = Util.WorldToScreenPoint(worldSpacePos, z: false);
		Vector3 vector5 = vector4 + vector3;
		float value = 1f / NormalizedScreen.physicalScale;
		buttonSpacingScale = Mathf.Clamp(value, _minButtonScale, _maxButtonScale);
		float pixelScale = NormalizedScreen.pixelScale;
		pixelScale = Mathf.Max(_minButtonScale, pixelScale);
		buttonTileScale = 80f * pixelScale;
		float num = 0.5f * buttonTileScale;
		Vector3 vector6 = new Vector3(vector5.x - num, vector5.y - num, 21f);
		return vector6 + MoveButtonLocalOffset();
	}

	public static bool IsMoveUp()
	{
		return tileButtonMove.IconName() == moveUpIconName;
	}

	public static bool IsScaleUp()
	{
		return tileButtonScale.IconName() == scaleUpIconName;
	}

	public static void StartMove(Vector2 touchPos, MoveMode mode)
	{
		if (mode == MoveMode.Plane && tileButtonMove.IconName() == moveUpIconName)
		{
			mode = MoveMode.Up;
		}
		dragBlockMode = mode;
		dragBlockStartRot = selected.GetRotation();
		dragBlockLastFreeRot = dragBlockStartRot;
		dragBlockStartPos = selected.GetPosition();
		dragBlockCurrentPos = dragBlockStartPos;
		dragBlockSearchFreePos = dragBlockStartPos;
		dragBlockSearchFreeRot = dragBlockStartRot;
		dragBlockLastFreePos = dragBlockStartPos;
		dragBlockLastDesiredPos = Util.nullVector3;
		dragBlockLastHitNormal = Util.nullVector3;
		dragBlockScale = selected.GetScale();
		dragButtonStartWorldPos = BestTBoxCorner();
		switch (mode)
		{
		case MoveMode.Up:
			dragBlockAxis = Blocksworld.constrainedManipulationAxis;
			dragBlockStartAxisPos = Util.ProjectScreenPointOnWorldAxis(dragButtonStartWorldPos, dragBlockAxis, touchPos);
			selected.EnableCollider(value: false);
			break;
		case MoveMode.Plane:
			dragBlockStartAxisPos = Util.ProjectScreenPointOnWorldPlane(dragButtonStartWorldPos, Vector3.up, touchPos);
			selected.EnableCollider(value: false);
			break;
		}
	}

	private static Vector3 GetBlockSnapPosition(Vector3 pos, Vector3 blockSize)
	{
		return new Vector3((Mathf.Round(blockSize.x) % 2f != 0f) ? Mathf.Round(pos.x) : (Mathf.Round(pos.x + 0.5f) - 0.5f), (Mathf.Round(blockSize.y) % 2f != 0f) ? Mathf.Round(pos.y) : (Mathf.Round(pos.y + 0.5f) - 0.5f), (Mathf.Round(blockSize.z) % 2f != 0f) ? Mathf.Round(pos.z) : (Mathf.Round(pos.z + 0.5f) - 0.5f));
	}

	private static void CheckSnapPosition(Vector3 snapPos, string msg)
	{
		Vector3 vector = snapPos * 2f;
		float num = Mathf.Round(vector.x) - vector.x;
		if ((double)Mathf.Abs(num) > 1E-12)
		{
			BWLog.Info("Failed snap check x " + num + " " + msg);
		}
		float num2 = Mathf.Round(vector.y) - vector.y;
		if ((double)Mathf.Abs(num2) > 1E-12)
		{
			BWLog.Info("Failed snap check y " + num2 + " " + msg);
		}
		float num3 = Mathf.Round(vector.z) - vector.z;
		if ((double)Mathf.Abs(num3) > 1E-12)
		{
			BWLog.Info("Failed snap check z " + num3 + " " + msg);
		}
	}

	public static Vector3 GetPosition()
	{
		return go.transform.position;
	}

	public static bool IsRaycastDragging()
	{
		return dragBlockMode == MoveMode.Raycast;
	}

	public static bool ContinueMove(Vector2 touchPos, bool forceUpdate = false)
	{
		if (Blocksworld.CurrentState == State.Play)
		{
			BWLog.Info("Trying to move tbox in play mode");
			return false;
		}
		if (selected == null)
		{
			return false;
		}
		Vector3 vector = Util.nullVector3;
		Vector3 vector2 = Util.nullVector3;
		Vector3 vector3 = Vector3.zero;
		Quaternion quaternion = dragBlockStartRot;
		bool flag = false;
		bool red = false;
		switch (dragBlockMode)
		{
		case MoveMode.Up:
			if (Mathf.Abs(Vector3.Dot(Blocksworld.cameraForward, Blocksworld.constrainedManipulationAxis)) <= 0.95f)
			{
				vector2 = Util.ProjectScreenPointOnWorldAxis(dragButtonStartWorldPos, dragBlockAxis, touchPos);
				Vector3 vector13 = vector2 - dragBlockStartAxisPos;
				vector2 = dragBlockStartPos + vector13;
				Vector3 vector14 = Util.Abs(selected.GetRotation() * dragBlockScale);
				vector = ((!noSnap) ? GetBlockSnapPosition(vector2, vector14) : vector2);
				if (!noSnap && !OkScalePositionCombination(vector14, vector))
				{
					BWLog.Info("Not OK snap position during moving upwards");
				}
				float num15 = Mathf.Sign(Vector3.Dot(vector - dragBlockStartPos, dragBlockAxis));
				vector3 = (0f - num15) * dragBlockAxis;
			}
			break;
		case MoveMode.Plane:
		{
			vector2 = Util.ProjectScreenPointOnWorldPlane(dragButtonStartWorldPos, Vector3.up, touchPos);
			Vector3 vector11 = vector2 - dragBlockStartAxisPos;
			vector2 = dragBlockStartPos + vector11;
			Vector3 vector12 = Util.Abs(selected.GetRotation() * dragBlockScale);
			vector = ((!noSnap) ? GetBlockSnapPosition(vector2, vector12) : vector2);
			if (!noSnap && !OkScalePositionCombination(vector12, vector))
			{
				BWLog.Info("Not OK snap position during plane move");
			}
			vector3 = Vector3.up;
			break;
		}
		case MoveMode.Raycast:
		{
			if (Blocksworld.worldSky != null)
			{
				Blocksworld.worldSky.go.SetLayer(Layer.IgnoreRaycast);
			}
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(touchPos * NormalizedScreen.scale);
			RaycastHit raycastHit = default(RaycastHit);
			RaycastHit raycastHit2 = default(RaycastHit);
			RaycastHit[] array = Physics.RaycastAll(ray);
			bool flag2 = false;
			if (array.Length != 0)
			{
				Array.Sort(array, new RaycastDistanceComparer(cameraPosition));
				RaycastHit[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit3 = array2[i];
					Block block = BWSceneManager.FindBlock(raycastHit3.collider.gameObject);
					if (block != selectedBlock && Tutorial.RaycastTargetBlockOK(block))
					{
						raycastHit = raycastHit3;
						flag2 = true;
						break;
					}
				}
			}
			RaycastHit[] array3 = Physics.SphereCastAll(ray, 0.75f);
			List<RaycastHit> list = new List<RaycastHit>();
			bool flag3 = false;
			if (array3.Length != 0)
			{
				RaycastHit[] array4 = array3;
				for (int j = 0; j < array4.Length; j++)
				{
					RaycastHit item = array4[j];
					Block block2 = BWSceneManager.FindBlock(item.collider.gameObject);
					if (block2 != selectedBlock && Tutorial.RaycastTargetBlockOK(block2))
					{
						list.Add(item);
						flag3 = true;
					}
				}
			}
			if (list.Count > 0)
			{
				list.Sort(new RaycastDistanceComparer(cameraPosition));
			}
			float num = Blocksworld.maxBlockDragDistance * 2f;
			if (flag2)
			{
				num = (raycastHit.point - cameraPosition).magnitude;
			}
			float num2 = Blocksworld.maxBlockDragDistance * 2f;
			if (flag3)
			{
				RaycastHit raycastHit4 = list[0];
				raycastHit2 = raycastHit4;
				if (flag2)
				{
					float num3 = Vector3.Distance(raycastHit2.point, raycastHit.point);
					for (int k = 1; k < list.Count; k++)
					{
						RaycastHit raycastHit5 = list[k];
						float num4 = Vector3.Distance(raycastHit5.point, raycastHit.point);
						float num5 = Vector3.Distance(raycastHit5.point, raycastHit4.point);
						if (num5 < 5f && num4 < num3)
						{
							raycastHit2 = raycastHit5;
							num3 = num4;
						}
					}
				}
				num2 = (raycastHit2.point - cameraPosition).magnitude;
			}
			bool flag4 = Mathf.Min(num2, num) < Blocksworld.maxBlockDragDistance && raycastHit.collider != null;
			Block block3 = ((!flag4) ? null : BWSceneManager.FindBlock(raycastHit.collider.gameObject, checkChildGos: true));
			bool flag5 = false;
			if (flag4 && block3 != null && block3.isTerrain)
			{
				if (flag2 && flag3)
				{
					float num6 = num2 / num;
					if (num6 <= 0.01f)
					{
						raycastHit = raycastHit2;
						flag5 = true;
					}
				}
				else
				{
					raycastHit = raycastHit2;
					flag5 = true;
				}
			}
			if (flag4 && !selected.IsColliderHit(raycastHit.collider) && Tutorial.DistanceOK(raycastHit.point))
			{
				selected.IgnoreRaycasts(value: true);
				Vector3 vector4 = Util.Abs(selected.GetRotation() * dragBlockScale);
				vector3 = Util.RoundDirection(raycastHit.normal);
				vector2 = ((selectedBlock == null || block3 == null || !Block.WantsToOccupySameGridCell(selectedBlock, block3)) ? (raycastHit.point + 0.5f * Vector3.Scale(vector3, vector4)) : (raycastHit.point - 0.5f * vector3));
				Bounds bounds;
				if (block3 == null)
				{
					bounds = new Bounds(vector2, Vector3.one);
					if (!(raycastHit.collider.gameObject != Tutorial.placementHelper))
					{
					}
				}
				else
				{
					bounds = block3.go.GetComponent<Collider>().bounds;
				}
				vector = ((!noSnap) ? GetBlockSnapPosition(vector2, vector4) : vector2);
				if (!noSnap && !OkScalePositionCombination(vector4, vector))
				{
					BWLog.Info("Not OK snap position during raycast move");
				}
				if (flag5)
				{
					List<Bounds> list2 = new List<Bounds>();
					if (selected is Block)
					{
						Collider[] array5 = Physics.OverlapSphere(raycastHit.point, 3f);
						Collider[] array6 = array5;
						foreach (Collider collider in array6)
						{
							Block block4 = BWSceneManager.FindBlock(collider.gameObject);
							if (block4 != block3 && block4 != selected && !(block4 is BlockTerrain) && !(block4 is BlockPosition) && !(block4 is BlockVolume) && !(block4 is BlockWater) && !bounds.Contains(collider.bounds.center))
							{
								list2.Add(collider.bounds);
							}
						}
					}
					List<Vector3> list3 = new List<Vector3>();
					for (int m = -1; m <= 1; m++)
					{
						for (int n = -1; n <= 1; n++)
						{
							for (int num7 = -1; num7 <= 1; num7++)
							{
								list3.Add(new Vector3(m, n, num7));
							}
						}
					}
					float num8 = -999999f;
					Vector3 vector5 = default(Vector3);
					Vector2 vector6 = touchPos * NormalizedScreen.scale;
					for (int num9 = 0; num9 < list3.Count; num9++)
					{
						Vector3 vector7 = list3[num9];
						Vector3 vector8 = vector + vector7;
						Vector3 vector9 = Blocksworld.mainCamera.WorldToScreenPoint(vector8);
						Vector2 vector10 = new Vector2(vector9.x, vector9.y);
						float magnitude = (vector10 - vector6).magnitude;
						bool flag6 = bounds.Contains(vector8);
						float num10 = Mathf.Sqrt(bounds.SqrDistance(vector8));
						float num11 = (flag6 ? 100 : 0);
						float num12 = 0.01f * magnitude / NormalizedScreen.scale;
						float magnitude2 = (vector8 - raycastHit.point).magnitude;
						float num13 = 0f - magnitude2 - num12 - num11;
						for (int num14 = 0; num14 < list2.Count; num14++)
						{
							Bounds bounds2 = list2[num14];
							if (bounds2.Contains(vector8))
							{
								num13 -= 100f;
							}
							else
							{
								num10 = Mathf.Min(num10, Mathf.Sqrt(bounds2.SqrDistance(vector8)));
							}
						}
						if (num10 < 0.1f)
						{
							num13 -= 100f;
						}
						else if (num10 > 0.51f)
						{
							num13 -= (num10 - 0.5f) * 1000f;
						}
						if (num13 > num8)
						{
							num8 = num13;
							vector5 = vector7;
						}
					}
					vector += vector5;
					vector = ((!noSnap) ? GetBlockSnapPosition(vector2, vector4) : vector2);
				}
			}
			if (Blocksworld.worldSky != null)
			{
				Blocksworld.worldSky.go.SetLayer(Layer.Default);
			}
			break;
		}
		}
		bool flag7 = !forceUpdate && !(selectedBlock is BlockGrouped);
		bool flag8 = false;
		if ((!Util.IsNullVector3(vector) && (vector != dragBlockLastDesiredPos || vector3 != dragBlockLastHitNormal)) || forceUpdate)
		{
			dragBlockLastDesiredPos = vector;
			dragBlockLastHitNormal = vector3;
			dragBlockSearchFreePos = vector;
			dragBlockSearchFreeRot = quaternion;
			dragBlockCurrentPos = vector;
			float duration = 0.25f;
			dragBlockTween.Start(duration);
			flag8 = true;
		}
		bool value = selected.IsColliderEnabled();
		selected.EnableCollider(value: true);
		Vector3 position = selected.GetPosition();
		selected.TBoxMoveTo(dragBlockSearchFreePos);
		dragBlockColliding = selected.IsColliding((dragBlockMode != MoveMode.Raycast) ? 0f : 0f);
		selected.TBoxMoveTo(position);
		selected.EnableCollider(value);
		if (dragBlockColliding)
		{
			if (flag7 && !dragBlockTween.IsFinished())
			{
				dragBlockSearchFreePos += Vector3.up;
			}
		}
		else
		{
			dragBlockLastFreePos = dragBlockSearchFreePos;
			dragBlockLastFreeRot = dragBlockSearchFreeRot;
		}
		bool flag9 = false;
		if (dragBlockColliding && (!flag7 || dragBlockTween.IsFinished()))
		{
			selected.TBoxMoveTo(dragBlockLastFreePos);
			MoveTo(vector2);
			PaintRed(red: true);
		}
		else if (flag7)
		{
			Vector3 vector15 = dragBlockSearchFreePos - dragBlockCurrentPos;
			Vector3 pos = dragBlockCurrentPos + dragBlockTween.Value() * vector15;
			selected.TBoxMoveTo(pos);
			MoveTo(vector2);
			flag9 = !dragBlockColliding && !(vector15 != Vector3.zero);
			PaintRed(!flag9);
		}
		else
		{
			flag9 = true;
			MoveTo(dragBlockSearchFreePos);
			selected.TBoxMoveTo(dragBlockSearchFreePos);
			PaintRed(red: false);
		}
		if (flag8)
		{
			Sound.PlaySoundMovedSelectedBlock();
		}
		if (flag)
		{
			PaintRed(red);
		}
		return flag9;
	}

	public static void StopMove()
	{
		if (Util.IsNullVector3(dragBlockLastFreePos))
		{
			BWLog.Info("TBox (couldn't place new block)");
			Blocksworld.DestroyBlock(selectedBlock);
			Detach();
			Show(show: false);
			Blocksworld.blocksworldCamera.Unfollow();
			Scarcity.UpdateInventory();
			if (Blocksworld.modelCollection != null)
			{
				Blocksworld.modelCollection.RefreshScarcity();
			}
			Blocksworld.buildPanel.Layout();
			History.RemoveState();
			return;
		}
		if (selected != null)
		{
			if (!noSnap)
			{
				for (int i = 0; i < 3; i++)
				{
					if (targetSnapPositionMaxDistance[i] > 0f)
					{
						float f = targetSnapPosition[i] - dragBlockLastFreePos[i];
						float num = Mathf.Abs(f);
						if (num > 0f && num < targetSnapPositionMaxDistance[i])
						{
							dragBlockLastFreePos[i] = targetSnapPosition[i];
						}
					}
				}
				Vector3 v = selected.GetRotation() * selected.GetScale();
				List<Vector3> list = new List<Vector3>();
				if (!OkScalePositionCombination(Util.Abs(v), dragBlockLastFreePos, list))
				{
					foreach (Vector3 item in list)
					{
						selected.TBoxMoveTo(item);
						if (!selected.IsColliding())
						{
							dragBlockLastFreePos = item;
							break;
						}
					}
				}
			}
			if (selected.IsColliding())
			{
				selected.TBoxMoveTo(dragBlockStartPos);
				selected.TBoxRotateTo(dragBlockStartRot);
				if (selected.IsColliding())
				{
					if (!(selected is BlockGrouped) && !Blocksworld.SelectedBunchIsGroup())
					{
						if (selectedBlock != null)
						{
							Blocksworld.DestroyBlock(selectedBlock);
							History.RemoveState();
						}
						else if (selected is Bunch)
						{
							Blocksworld.DestroyBunch((Bunch)selected);
							History.RemoveState();
						}
						Detach();
						Show(show: false);
						Blocksworld.blocksworldCamera.Unfollow();
						Scarcity.UpdateInventory();
						if (Blocksworld.modelCollection != null)
						{
							Blocksworld.modelCollection.RefreshScarcity();
						}
						Blocksworld.buildPanel.Layout();
						return;
					}
				}
				else
				{
					History.RemoveState();
					History.AddStateIfNecessary();
				}
			}
			else
			{
				selected.TBoxMoveTo(dragBlockLastFreePos);
				selected.TBoxRotateTo(dragBlockLastFreeRot);
				if (!noSnap)
				{
					selected.TBoxSnap();
				}
			}
			selected.EnableCollider(value: true);
			selected.IgnoreRaycasts(value: false);
		}
		if (selected != null)
		{
			ConnectednessGraph.Update(selected);
			Blocksworld.blocksworldCamera.ExpandWorldBounds(selected);
			if (selected is BlockGrouped blockGrouped)
			{
				ConnectednessGraph.Update(blockGrouped.GetMainBlockInGroup());
			}
		}
		FitToSelected();
		PaintRed(red: false);
	}

	public static void StartRotate(Vector2 touchPos, RotateMode mode)
	{
		rotateBlockMode = mode;
		rotateBlockStartRotation = ((!noSnap) ? Quaternion.Euler(Util.Round(selected.GetRotation().eulerAngles / 90f) * 90f) : selected.GetRotation());
		rotateBlockLastDesiredRot = rotateBlockStartRotation;
		rotateBlockStartPos = selected.GetPosition();
		rotateBlockSearchFreePos = rotateBlockStartPos;
		rotateBlockScale = selected.GetScale();
		if (selectedBlock != null)
		{
			selectedBlock.TBoxStartRotate();
		}
		selected.EnableCollider(value: false);
	}

	public static void ContinueRotate(Vector2 startTouchPos, Vector2 touchPos)
	{
		float num = 0f;
		Vector3 vector = Vector3.zero;
		switch (rotateBlockMode)
		{
		case RotateMode.Finger:
		{
			num = Mathf.Abs(startTouchPos.y - touchPos.y);
			goArrowRotateYAngle = Mathf.Sign(startTouchPos.y - touchPos.y) * 90f;
			int num3 = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
			Vector3 vector3 = Vector3.forward;
			switch (num3)
			{
			case 1:
				vector3 = Vector3.right;
				break;
			case 2:
				vector3 = -Vector3.forward;
				break;
			case 3:
				vector3 = -Vector3.right;
				break;
			}
			vector = Quaternion.Inverse(rotateBlockStartRotation) * (goArrowRotateYAngle * vector3);
			break;
		}
		case RotateMode.Button:
		{
			if (Mathf.Abs(startTouchPos.x - touchPos.x) >= Mathf.Abs(startTouchPos.y - touchPos.y))
			{
				num = Mathf.Abs(startTouchPos.x - touchPos.x);
				goArrowRotateYAngle = Mathf.Sign(startTouchPos.x - touchPos.x) * 90f;
				vector = Quaternion.Inverse(rotateBlockStartRotation) * new Vector3(0f, goArrowRotateYAngle, 0f);
				break;
			}
			num = Mathf.Abs(startTouchPos.y - touchPos.y);
			goArrowRotateYAngle = Mathf.Sign(startTouchPos.y - touchPos.y) * 90f;
			int num2 = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
			Vector3 vector2 = -Vector3.right;
			switch (num2)
			{
			case 1:
				vector2 = Vector3.forward;
				break;
			case 2:
				vector2 = Vector3.right;
				break;
			case 3:
				vector2 = -Vector3.forward;
				break;
			}
			vector = Quaternion.Inverse(rotateBlockStartRotation) * (goArrowRotateYAngle * vector2);
			break;
		}
		}
		Vector3 vector4 = 0.01f * num * vector;
		if (selectedBlock != null)
		{
			Vector3 vector5 = selectedBlock.AllowedBuildModeRotations();
			vector4.x *= vector5.x;
			vector4.y *= vector5.y;
			vector4.z *= vector5.z;
		}
		Vector3 euler = ((!noSnap) ? (Util.Round(vector4 / 90f) * 90f) : vector4);
		Quaternion rot = rotateBlockStartRotation * Quaternion.Euler(vector4);
		Quaternion quaternion = rotateBlockStartRotation * Quaternion.Euler(euler);
		Vector3 vector6 = Util.Abs(quaternion * rotateBlockScale) - Util.Abs(rotateBlockStartRotation * rotateBlockScale);
		Vector3 vector7 = new Vector3((Mathf.Round(vector6.x) % 2f == 0f) ? 0f : (rotateBlockOffsetSignFlip * 0.5f), (Mathf.Round(vector6.y) % 2f == 0f) ? 0f : (-0.5f), (Mathf.Round(vector6.z) % 2f == 0f) ? 0f : (rotateBlockOffsetSignFlip * 0.5f));
		if (noSnap)
		{
			vector7 = Vector3.zero;
		}
		bool flag = false;
		if (quaternion != rotateBlockLastDesiredRot)
		{
			rotateBlockLastDesiredRot = quaternion;
			rotateBlockSearchFreePos = rotateBlockStartPos + vector7;
			rotateBlockMoveTween.Start((!(selected is BlockGrouped)) ? 0.25f : 0f);
			flag = true;
		}
		selected.EnableCollider(value: true);
		Vector3 position = selected.GetPosition();
		Quaternion rotation = selected.GetRotation();
		selected.TBoxRotateTo(quaternion);
		selected.TBoxMoveTo(rotateBlockSearchFreePos);
		rotateBlockColliding = selected.IsColliding();
		selected.TBoxRotateTo(rotation);
		selected.TBoxMoveTo(position);
		selected.EnableCollider(value: false);
		if (rotateBlockColliding && !rotateBlockMoveTween.IsFinished())
		{
			rotateBlockSearchFreePos = 0.5f * Util.Round(2f * (rotateBlockSearchFreePos + Vector3.up));
		}
		if (rotateBlockColliding && rotateBlockMoveTween.IsFinished())
		{
			if (!(selected is BlockGrouped))
			{
				selected.TBoxRotateTo(rotateBlockStartRotation);
				selected.TBoxMoveTo(rotateBlockStartPos);
			}
			RotateTo(rot);
			PaintRed(red: true);
		}
		else
		{
			Vector3 vector8 = rotateBlockSearchFreePos - rotateBlockStartPos;
			Vector3 pos = rotateBlockStartPos + rotateBlockMoveTween.Value() * vector8;
			selected.TBoxRotateTo(quaternion);
			selected.TBoxMoveTo(pos);
			RotateTo(rot);
			MoveTo(rotateBlockStartPos + vector7);
			PaintRed(vector8 != vector7);
		}
		if (flag)
		{
			Sound.PlaySoundMovedSelectedBlock();
		}
	}

	public static void StopRotate()
	{
		if (rotateBlockColliding || selected.IsColliding())
		{
			selected.TBoxRotateTo(rotateBlockStartRotation);
			selected.TBoxMoveTo(rotateBlockStartPos);
		}
		else
		{
			selected.TBoxRotateTo(rotateBlockLastDesiredRot);
			selected.TBoxMoveTo(rotateBlockSearchFreePos);
		}
		if (selectedBlock != null)
		{
			selectedBlock.TBoxStopRotate();
		}
		if (!noSnap)
		{
			selected.TBoxSnap();
		}
		FitToSelected();
		PaintRed(red: false);
		rotateBlockOffsetSignFlip *= -1f;
		selected.EnableCollider(value: true);
		ConnectednessGraph.Update(selected);
		if (selected is BlockGrouped blockGrouped)
		{
			ConnectednessGraph.Update(blockGrouped.GetMainBlockInGroup());
		}
	}

	public static void StartScale(Vector2 touchPos, ScaleMode mode)
	{
		if (mode == ScaleMode.Plane && tileButtonScale.IconName() == scaleUpIconName)
		{
			mode = ScaleMode.Up;
		}
		scaleBlockMode = mode;
		scaleBlockStartScale = selectedBlock.Scale();
		scaleBlockCurrentScale = scaleBlockStartScale;
		scaleBlockSearchFreeScale = scaleBlockStartScale;
		scaleBlockLastFreeScale = scaleBlockStartScale;
		scaleBlockLastFreePos = selectedBlock.goT.position;
		scaleBlockLastDesiredScale = scaleBlockStartScale;
		scaleBlockStartPos = selectedBlock.goT.position;
		scaleBlockCurrentPos = scaleBlockStartPos;
		scaleBlockSearchFreePos = scaleBlockStartPos;
		scaleButtonStartWorldPos = BestTBoxCorner();
		switch (mode)
		{
		case ScaleMode.Up:
			scaleBlockWorldAxis = Blocksworld.constrainedManipulationAxis;
			scaleBlockWorldAxis2 = Vector3.zero;
			scaleBlockWorldAxisStartPos = Util.ProjectScreenPointOnWorldAxis(scaleButtonStartWorldPos, Util.Abs(scaleBlockWorldAxis), touchPos);
			break;
		case ScaleMode.Plane:
		{
			int num = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
			scaleBlockWorldAxis = ((num != 0 && num != 1) ? Vector3.right : (-Vector3.right));
			scaleBlockWorldAxis2 = ((num != 0 && num != 3) ? Vector3.forward : (-Vector3.forward));
			scaleBlockWorldAxisStartPos = Util.ProjectScreenPointOnWorldAxis(scaleButtonStartWorldPos, Vector3.up, Util.Abs(scaleBlockWorldAxis), touchPos);
			scaleBlockWorldAxisStartPos2 = Util.ProjectScreenPointOnWorldAxis(scaleButtonStartWorldPos, Vector3.up, Util.Abs(scaleBlockWorldAxis2), touchPos);
			break;
		}
		}
		if (selectedBlock.go != null)
		{
			selectedBlock.EnableCollider(value: false);
		}
	}

	private static float InvertNegativeScale(Vector3 axisAbs, ref Vector3 scale, ref Vector3 scaleSnap, ref Vector3 pos)
	{
		if ((!(axisAbs == Vector3.right) || scale.x >= 0.999f) && (!(axisAbs == Vector3.up) || scale.y >= 0.999f) && (!(axisAbs == Vector3.forward) || scale.z >= 0.999f))
		{
			return 0f;
		}
		if (axisAbs == Vector3.right)
		{
			float result = 1f - scale.x;
			scale.x = (scaleSnap.x = 1f);
			return result;
		}
		if (axisAbs == Vector3.up)
		{
			float result2 = 1f - scale.y;
			scale.y = (scaleSnap.y = 1f);
			return result2;
		}
		float result3 = 1f - scale.z;
		scale.z = (scaleSnap.z = 1f);
		return result3;
	}

	private static bool SafeScale(Vector3 s)
	{
		if (s.x >= 0.999f && s.y >= 0.999f)
		{
			return s.z >= 0.999f;
		}
		return false;
	}

	public static bool OkScalePositionCombination(Vector3 scale, Vector3 pos, List<Vector3> okPositions = null)
	{
		Vector3 vector = pos;
		bool flag = true;
		for (int i = 0; i < 3; i++)
		{
			float f = Mathf.Abs(scale[i]);
			float num = Mathf.Abs(pos[i]);
			bool flag2 = num - Mathf.Floor(num) > 0.01f;
			if (Mathf.RoundToInt(f) % 2 == 0)
			{
				if (!flag2)
				{
					flag = false;
					vector[i] = pos[i] + 0.5f;
				}
			}
			else if (flag2)
			{
				flag = false;
				vector[i] = Mathf.Round(pos[i]);
			}
		}
		if (!flag && okPositions != null)
		{
			okPositions.Add(vector + Vector3.right);
			okPositions.Add(vector + Vector3.left);
			okPositions.Add(vector + Vector3.forward);
			okPositions.Add(vector + Vector3.back);
		}
		return flag;
	}

	private static void ScaleUniformXZ(Vector3 forward, ref Vector3 ds, ref Vector3 dsSnap)
	{
		if (Vector3.Project(forward, scaleBlockWorldAxis).sqrMagnitude > Vector3.Project(forward, scaleBlockWorldAxis2).sqrMagnitude)
		{
			ds.z = ds.x;
			dsSnap.z = dsSnap.x;
		}
		else
		{
			ds.z = ds.y;
			dsSnap.z = dsSnap.y;
		}
	}

	public static void ContinueScale(Vector2 touchPos)
	{
		if (selectedBlock == null)
		{
			return;
		}
		Vector3 lhs = ((scaleBlockMode != ScaleMode.Up) ? (Util.ProjectScreenPointOnWorldAxis(scaleButtonStartWorldPos, Vector3.up, Util.Abs(scaleBlockWorldAxis), touchPos) - scaleBlockWorldAxisStartPos) : (Util.ProjectScreenPointOnWorldAxis(scaleButtonStartWorldPos, Util.Abs(scaleBlockWorldAxis), touchPos) - scaleBlockWorldAxisStartPos));
		Vector3 lhs2 = ((scaleBlockMode != ScaleMode.Up) ? (Util.ProjectScreenPointOnWorldAxis(scaleButtonStartWorldPos, Vector3.up, Util.Abs(scaleBlockWorldAxis2), touchPos) - scaleBlockWorldAxisStartPos2) : Vector3.zero);
		Vector3 ds = new Vector3(lhs.magnitude * Mathf.Sign(Vector3.Dot(lhs, scaleBlockWorldAxis)), lhs2.magnitude * Mathf.Sign(Vector3.Dot(lhs2, scaleBlockWorldAxis2)), 0f);
		Vector3 dsSnap = new Vector3(Mathf.Round(ds.x), Mathf.Round(ds.y), 0f);
		bool flag = false;
		Vector3[] array = new Vector3[3]
		{
			Util.Abs(scaleBlockWorldAxis),
			Util.Abs(scaleBlockWorldAxis2),
			Vector3.up
		};
		bool[] array2 = new bool[3];
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		Vector3[] scaleConstraints = selectedBlock.GetScaleConstraints();
		foreach (Vector3 vector in scaleConstraints)
		{
			if ((vector - Vector3.one).sqrMagnitude < 0.0001f)
			{
				flag = true;
			}
			Quaternion rotation = selectedBlock.goT.rotation;
			Vector3 v = rotation * vector;
			Vector3 vector2 = Util.Abs(v);
			for (int j = 0; j < array.Length; j++)
			{
				if ((array[j] - vector2).sqrMagnitude < 0.0001f)
				{
					array2[j] = true;
				}
			}
			if (!flag && Mathf.Abs(vector2.sqrMagnitude - 2f) < 0.001f)
			{
				if (Vector3.Dot(array[0], vector2) > 0.001f && Vector3.Dot(array[1], vector2) > 0.001f)
				{
					flag2 = true;
				}
				if (Vector3.Dot(array[0], vector2) > 0.001f && Vector3.Dot(array[2], vector2) > 0.001f)
				{
					flag3 = true;
				}
				if (Vector3.Dot(array[1], vector2) > 0.001f && Vector3.Dot(array[2], vector2) > 0.001f)
				{
					flag4 = true;
				}
			}
		}
		bool flag5 = selectedBlock is BlockAbstractWheel || selectedBlock is BlockRaycastWheel;
		bool flag6 = selectedBlock is BlockTankTreadsWheel;
		Transform goT = selectedBlock.goT;
		if (flag5 && Mathf.Abs(goT.up.x) < 0.001f && Mathf.Abs(goT.up.z) < 0.001f && Mathf.Abs(scaleBlockStartScale.y - scaleBlockStartScale.z) < 0.001f)
		{
			ScaleUniformXZ(goT.forward, ref ds, ref dsSnap);
		}
		else if (flag6)
		{
			ScaleUniformXZ(goT.forward, ref ds, ref dsSnap);
		}
		else if (flag)
		{
			ds.z = (ds.y = (ds.x = Util.MaxAbsWithSign(ds.y, ds.x)));
			dsSnap = new Vector3(Mathf.Round(ds.x), Mathf.Round(ds.y), Mathf.Round(ds.z));
		}
		else if (flag2)
		{
			ds.x = Util.MaxAbsWithSign(ds.x, ds.y);
			ds.y = ds.x;
			ds.z = ((!array2[2]) ? 0f : ds.z);
			dsSnap.x = Util.MaxAbsWithSign(dsSnap.x, dsSnap.y);
			dsSnap.y = dsSnap.x;
			dsSnap.z = ((!array2[2]) ? 0f : dsSnap.z);
		}
		else if (flag3)
		{
			ds.x = Util.MaxAbsWithSign(ds.x, ds.z);
			ds.y = ((!array2[1]) ? 0f : ds.y);
			ds.z = ds.x;
			dsSnap.x = Util.MaxAbsWithSign(dsSnap.x, dsSnap.z);
			dsSnap.y = ((!array2[1]) ? 0f : dsSnap.y);
			dsSnap.z = dsSnap.x;
		}
		else if (flag4)
		{
			ds.x = ((!array2[0]) ? 0f : ds.x);
			ds.y = Util.MaxAbsWithSign(ds.y, ds.z);
			ds.z = ds.y;
			dsSnap.x = ((!array2[0]) ? 0f : dsSnap.x);
			dsSnap.y = Util.MaxAbsWithSign(dsSnap.y, dsSnap.z);
			dsSnap.z = dsSnap.y;
		}
		Vector3 vector3 = Util.Abs(Quaternion.Inverse(selectedBlock.goT.rotation) * scaleBlockWorldAxis);
		Vector3 vector4 = Util.Abs(Quaternion.Inverse(selectedBlock.goT.rotation) * scaleBlockWorldAxis2);
		Vector3 vector5 = selectedBlock.CanScale();
		if ((vector3 == Vector3.right && vector5.x == 0f) || (vector3 == Vector3.up && vector5.y == 0f) || (vector3 == Vector3.forward && vector5.z == 0f))
		{
			ds.x = (dsSnap.x = 0f);
		}
		if ((vector4 == Vector3.right && vector5.x == 0f) || (vector4 == Vector3.up && vector5.y == 0f) || (vector4 == Vector3.forward && vector5.z == 0f))
		{
			ds.y = (dsSnap.y = 0f);
		}
		Vector3 scale = scaleBlockStartScale + ds.x * vector3 + ds.y * vector4 + ds.z * Vector3.up;
		Vector3 scaleSnap = scaleBlockStartScale + dsSnap.x * vector3 + dsSnap.y * vector4 + dsSnap.z * Vector3.up;
		Vector3 vector6 = ds / 2f;
		Vector3 vector7 = dsSnap / 2f;
		if (flag && scaleBlockMode == ScaleMode.Up)
		{
			vector6 = ds / 4f;
			vector7 = dsSnap / 4f;
		}
		bool flag7 = false;
		if (flag)
		{
			scale = scaleBlockStartScale + ds;
			scaleSnap = scaleBlockStartScale + dsSnap;
			float num = Util.MinComponent(scaleSnap);
			float num2 = Mathf.Max(1f, Util.MaxComponent(scaleSnap));
			float num3 = Util.MinComponent(scale);
			float num4 = Mathf.Max(1f, Util.MaxComponent(scale));
			if (num >= 1f)
			{
				scaleSnap = new Vector3(num, num, num);
			}
			else
			{
				scaleSnap = new Vector3(num2, num2, num2);
				flag7 = true;
			}
			if (num3 >= 1f)
			{
				scale = new Vector3(num3, num3, num3);
			}
			else
			{
				scale = new Vector3(num4, num4, num4);
				flag7 = true;
			}
		}
		Vector3 pos = scaleBlockStartPos + vector6.x * scaleBlockWorldAxis + vector6.y * scaleBlockWorldAxis2 + vector6.z * Vector3.up;
		Vector3 vector8 = scaleBlockStartPos + vector7.x * scaleBlockWorldAxis + vector7.y * scaleBlockWorldAxis2 + vector7.z * Vector3.up;
		if (flag7)
		{
			pos = selectedBlock.goT.position;
			vector8 = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));
		}
		float num5 = InvertNegativeScale(vector3, ref scale, ref scaleSnap, ref pos);
		float num6 = ((scaleBlockMode != ScaleMode.Up) ? InvertNegativeScale(vector4, ref scale, ref scaleSnap, ref pos) : 1f);
		float num7 = ((ds.z != 0f) ? InvertNegativeScale(Vector3.up, ref scale, ref scaleSnap, ref pos) : 0f);
		pos += 0.5f * num5 * scaleBlockWorldAxis;
		pos += 0.5f * num6 * scaleBlockWorldAxis2;
		pos += 0.5f * num7 * Vector3.up;
		vector8 += 0.5f * Mathf.Round(num5) * scaleBlockWorldAxis;
		vector8 += 0.5f * Mathf.Round(num6) * scaleBlockWorldAxis2;
		vector8 += 0.5f * Mathf.Round(num7) * Vector3.up;
		bool flag8 = false;
		if (scaleSnap != scaleBlockLastDesiredScale)
		{
			scaleBlockLastDesiredScale = scaleSnap;
			scaleBlockCurrentScale = scaleSnap;
			scaleBlockCurrentPos = vector8;
			scaleBlockSearchFreeScale = scaleSnap;
			scaleBlockSearchFreePos = vector8;
			scaleBlockTween.Start((!(selectedBlock is BlockGrouped)) ? 0.25f : 0f);
			flag8 = true;
		}
		selectedBlock.EnableCollider(value: true);
		Vector3 position = selectedBlock.goT.position;
		Vector3 scale2 = selectedBlock.Scale();
		selectedBlock.TBoxMoveTo(scaleBlockSearchFreePos);
		selectedBlock.TBoxScaleTo(scaleBlockSearchFreeScale);
		scaleBlockColliding = selectedBlock.IsColliding();
		selectedBlock.TBoxScaleTo(scale2);
		selectedBlock.TBoxMoveTo(position);
		selectedBlock.EnableCollider(value: false);
		if (!scaleBlockColliding)
		{
			scaleBlockLastFreeScale = scaleBlockSearchFreeScale;
			scaleBlockLastFreePos = scaleBlockSearchFreePos;
		}
		if (scaleBlockColliding && !scaleBlockTween.IsFinished())
		{
			if (Tutorial.state != TutorialState.None)
			{
				scaleBlockSearchFreePos = 0.5f * Util.Round(2f * (scaleBlockSearchFreePos + Vector3.up));
			}
			else
			{
				Vector3 vector9 = scaleBlockSearchFreeScale;
				Vector3 vector10 = scaleBlockSearchFreePos;
				if (!flag)
				{
					if (Vector3.Scale(scaleBlockSearchFreeScale, vector3) != Vector3.Scale(scaleBlockLastFreeScale, vector3) && SafeScale(scaleBlockSearchFreeScale - vector3))
					{
						scaleBlockSearchFreeScale -= vector3;
						scaleBlockSearchFreePos -= scaleBlockWorldAxis * num5 * 0.5f;
					}
					if (scaleBlockMode == ScaleMode.Plane && Vector3.Scale(scaleBlockSearchFreeScale, vector4) != Vector3.Scale(scaleBlockLastFreeScale, vector4) && SafeScale(scaleBlockSearchFreeScale - vector4))
					{
						scaleBlockSearchFreeScale -= vector4;
						scaleBlockSearchFreePos -= scaleBlockWorldAxis2 * num6 * 0.5f;
					}
					if (scaleBlockMode == ScaleMode.Plane && Vector3.Scale(scaleBlockSearchFreeScale, Vector3.up) != Vector3.Scale(scaleBlockLastFreeScale, Vector3.up) && SafeScale(scaleBlockSearchFreeScale - Vector3.up))
					{
						scaleBlockSearchFreeScale -= Vector3.up;
						scaleBlockSearchFreePos -= Vector3.up * num7 * 0.5f;
					}
				}
				else
				{
					scaleBlockSearchFreeScale = scaleBlockLastFreeScale;
					scaleBlockSearchFreePos = scaleBlockLastFreePos;
				}
				Vector3 scale3 = selectedBlock.goT.rotation * scaleBlockSearchFreeScale;
				if (!noSnap && !OkScalePositionCombination(scale3, scaleBlockSearchFreePos))
				{
					scaleBlockSearchFreeScale = vector9;
					scaleBlockSearchFreePos = vector10;
				}
			}
		}
		if (scaleBlockColliding && scaleBlockTween.IsFinished())
		{
			if (!(selectedBlock is BlockGrouped))
			{
				selectedBlock.TBoxScaleTo(scaleBlockStartScale, recalculateCollider: false);
				selectedBlock.TBoxMoveTo(scaleBlockStartPos);
			}
			PaintRed(red: true);
		}
		else
		{
			Vector3 vector11 = scaleBlockSearchFreeScale - scaleBlockCurrentScale;
			Vector3 vector12 = scaleBlockSearchFreePos - scaleBlockCurrentPos;
			selectedBlock.TBoxScaleTo(scaleBlockCurrentScale + scaleBlockTween.Value() * vector11, recalculateCollider: false);
			selectedBlock.TBoxMoveTo(scaleBlockCurrentPos + scaleBlockTween.Value() * vector12);
			PaintRed(vector11 != Vector3.zero);
		}
		ScaleTo(scale);
		MoveTo(pos);
		if (flag8)
		{
			Sound.PlaySoundMovedSelectedBlock();
		}
	}

	public static void StopScale()
	{
		Vector3 v = selectedBlock.goT.rotation * scaleBlockSearchFreeScale;
		List<Vector3> list = new List<Vector3>();
		bool flag = noSnap || OkScalePositionCombination(Util.Abs(v), scaleBlockSearchFreePos, list);
		if (!flag)
		{
			foreach (Vector3 item in list)
			{
				selectedBlock.TBoxMoveTo(item);
				if (!selectedBlock.IsColliding())
				{
					flag = true;
					scaleBlockSearchFreePos = item;
					break;
				}
			}
		}
		if (scaleBlockColliding || !flag)
		{
			selectedBlock.TBoxScaleTo(scaleBlockStartScale);
			selectedBlock.TBoxMoveTo(scaleBlockStartPos);
		}
		else
		{
			if (Util.MinComponent(targetSnapScale) >= 1f)
			{
				for (int i = 0; i < 3; i++)
				{
					float num = targetSnapScale[i] - scaleBlockSearchFreeScale[i];
					float num2 = Mathf.Abs(num);
					if (num2 > 0f && num2 <= targetSnapScaleMaxDistance[i])
					{
						scaleBlockSearchFreeScale[i] = targetSnapScale[i];
						Vector3 zero = Vector3.zero;
						zero[i] = num;
						zero = Quaternion.Inverse(selectedBlock.goT.rotation) * zero;
						Vector3 vector = scaleBlockSearchFreePos + zero * 0.5f;
						Vector3 vector2 = scaleBlockSearchFreePos - zero * 0.5f;
						scaleBlockSearchFreePos = vector;
						if ((scaleBlockStartPos - vector).sqrMagnitude < (scaleBlockStartPos - vector2).sqrMagnitude)
						{
							scaleBlockSearchFreePos = vector2;
						}
					}
				}
			}
			selectedBlock.TBoxScaleTo(scaleBlockSearchFreeScale);
			selectedBlock.TBoxMoveTo(scaleBlockSearchFreePos);
			if (selectedBlock is BlockGrouped && !noSnap)
			{
				selectedBlock.TBoxSnap();
			}
		}
		if (selectedBlock != null)
		{
			Blocksworld.blocksworldCamera.ExpandWorldBounds(selectedBlock);
			selectedBlock.TBoxStopScale();
		}
		FitToSelected();
		PaintRed(red: false);
		selectedBlock.EnableCollider(value: true);
		ConnectednessGraph.Update(selectedBlock);
		if (selected is BlockGrouped blockGrouped)
		{
			ConnectednessGraph.Update(blockGrouped.GetMainBlockInGroup());
		}
	}

	public static void ResetTargetSnapping()
	{
		targetSnapScale = Vector3.zero;
		targetSnapScaleMaxDistance = Vector3.zero;
		targetSnapPosition = Vector3.zero;
		targetSnapPositionMaxDistance = Vector3.zero;
	}
}
