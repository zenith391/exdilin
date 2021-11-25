using System;
using System.Collections.Generic;
using Blocks;
using Gestures;
using UnityEngine;

// Token: 0x020002B7 RID: 695
using System.Runtime.CompilerServices;
public class TBox
{
	// Token: 0x06001FEF RID: 8175 RVA: 0x000E823C File Offset: 0x000E663C
	public static void Init()
	{
		TBox.go = new GameObject("TBox");
		TBox.collider = TBox.go.AddComponent<BoxCollider>();
		TBox.collider.enabled = false;
		TBox.materialWhite = Materials.materials["White"];
		TBox.materialWhiteTopmost = (Resources.Load("GUI/Box White Transparent Topmost") as Material);
		TBox.materialGrey = Materials.materials["Grey"];
		TBox.materialRed = Materials.materials["Red"];
		for (int i = 0; i < 12; i++)
		{
			TBox.goEdges[i] = TBox.CreateCube(TBox.go, TBox.materialWhite);
			TBox.goEdgesHidden[i] = TBox.CreateCube(TBox.go, TBox.materialWhiteTopmost);
		}
		TBox.tileButtonMove = Blocksworld.tilePool.GetTileObjectForIcon(TBox.moveIconName, true);
		TBox.tileButtonRotate = Blocksworld.tilePool.GetTileObjectForIcon(TBox.rotateIconName, true);
		TBox.tileButtonScale = Blocksworld.tilePool.GetTileObjectForIcon(TBox.scaleIconName, true);
		TBox.tileLockedModelIcon = Blocksworld.tilePool.GetTileObjectForIcon(TBox.lockedModelIconName, true);
		TBox.tileCharacterEditIcon = Blocksworld.tilePool.GetTileObjectForIcon(TBox.characterEditOnIconName, true);
		TBox.tileCharacterEditExitIcon = Blocksworld.tilePool.GetTileObjectForIcon(TBox.characterEditOffIconName, true);
		TBox.tileCharacterEditExitIcon.MoveTo(50f, (float)(NormalizedScreen.height - 100));
		TBox.goOrientationArrow = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("GUI/Prefab Orientation Arrow") as GameObject);
		TBox.faces[Face.XPos] = new List<int>
		{
			5,
			6,
			9,
			10
		};
		TBox.faces[Face.XNeg] = new List<int>
		{
			4,
			7,
			8,
			11
		};
		TBox.faces[Face.YPos] = new List<int>
		{
			1,
			2,
			10,
			11
		};
		TBox.faces[Face.YNeg] = new List<int>
		{
			0,
			3,
			8,
			9
		};
		TBox.faces[Face.ZPos] = new List<int>
		{
			2,
			3,
			6,
			7
		};
		TBox.faces[Face.ZNeg] = new List<int>
		{
			0,
			1,
			4,
			5
		};
		TBox.Show(false);
	}

	// Token: 0x06001FF0 RID: 8176 RVA: 0x000E84D0 File Offset: 0x000E68D0
	private static GameObject CreateCube(GameObject parent, Material m)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
		gameObject.transform.parent = parent.transform;
		gameObject.GetComponent<Renderer>().sharedMaterial = m;
		return gameObject;
	}

	// Token: 0x06001FF1 RID: 8177 RVA: 0x000E8510 File Offset: 0x000E6910
	private static GameObject CreateArrow(GameObject parent, Material m)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("GUI/Prefab Arrow") as GameObject);
		UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
		gameObject.transform.parent = parent.transform;
		gameObject.GetComponent<Renderer>().sharedMaterial = m;
		return gameObject;
	}

	// Token: 0x06001FF2 RID: 8178 RVA: 0x000E855C File Offset: 0x000E695C
	public static void Attach(ITBox obj, bool silent = false)
	{
		TBox.selected = obj;
		if (obj is Block)
		{
			TBox.selectedBlock = (Block)obj;
		}
		else
		{
			TBox.selectedBlock = null;
		}
		TBox.FitToSelected();
		TBox.PaintRed(false);
		if (!silent)
		{
			Sound.PlaySound("Block Selected", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
		}
	}

	// Token: 0x06001FF3 RID: 8179 RVA: 0x000E85BC File Offset: 0x000E69BC
	public static void FitToSelected()
	{
		if (TBox.selected != null)
		{
			TBox.MoveTo(TBox.selected.GetPosition());
			TBox.RotateTo(TBox.selected.GetRotation());
			TBox.ScaleTo(TBox.selected.GetScale());
		}
	}

	// Token: 0x06001FF4 RID: 8180 RVA: 0x000E85F5 File Offset: 0x000E69F5
	public static void Detach(bool silent = false)
	{
		if (TBox.selected != null && !silent)
		{
			Sound.PlaySound("Block Unselected", Sound.GetOrCreateOneShotAudioSource(), true, 1f, 1f, false);
		}
		TBox.selected = (TBox.selectedBlock = null);
	}

	// Token: 0x06001FF5 RID: 8181 RVA: 0x000E862E File Offset: 0x000E6A2E
	private static bool SelectedBunchIsTankTreads()
	{
		return TBox.selectedBlock == null && Blocksworld.selectedBunch != null && Blocksworld.SelectedBunchIsGroup() && Blocksworld.selectedBunch.blocks[0] is BlockTankTreadsWheel;
	}

	// Token: 0x06001FF6 RID: 8182 RVA: 0x000E8668 File Offset: 0x000E6A68
	private static bool SelectedBunchIsLockedGroup()
	{
		return TBox.selectedBlock == null && Blocksworld.selectedBunch != null && Blocksworld.selectedBunch.blocks[0].HasGroup("locked-model");
	}

	// Token: 0x06001FF7 RID: 8183 RVA: 0x000E869A File Offset: 0x000E6A9A
	private static bool SelectedBlockIsSingleBlockLockedModel()
	{
		return TBox.selectedBlock != null && Blocksworld.selectedBunch == null && TBox.selectedBlock.HasGroup("locked-model");
	}

	// Token: 0x06001FF8 RID: 8184 RVA: 0x000E86C4 File Offset: 0x000E6AC4
	public static void Show(bool show)
	{
		TBox.go.SetActive(show);
		bool flag = TBox.selectedBlock is BlockAbstractWheel || TBox.selectedBlock is BlockRaycastWheel;
		bool flag2 = TBox.selectedBlock is BlockAbstractStabilizer;
		bool flag3 = TBox.selectedBlock is BlockAbstractAntiGravity;
		bool flag4 = TBox.selectedBlock is BlockTankTreadsWheel || TBox.SelectedBunchIsTankTreads();
		bool flag5 = TBox.SelectedBunchIsLockedGroup() || TBox.SelectedBlockIsSingleBlockLockedModel();
		bool flag6 = TBox.selectedBlock is BlockAnimatedCharacter;
		TBox.goOrientationArrow.SetActive(show && (flag || flag2 || flag3 || flag4));
		if (TBox.SelectedBunchIsTankTreads())
		{
			TBox.goOrientationArrow.transform.rotation = Blocksworld.selectedBunch.blocks[0].goT.rotation;
		}
		bool show2 = show;
		if (TBox.selectedBlock != null && TBox.selectedBlock.DisableBuildModeMove())
		{
			show2 = false;
		}
		bool flag7 = show;
		if (TBox.selectedBlock != null && TBox.selectedBlock.DisableBuildModeScale())
		{
			flag7 = false;
		}
		TBox.UpdateCopyButtonVisibility();
		TBox.tileButtonMove.Show(show2);
		TBox.tileButtonRotate.Show(show);
		TBox.tileButtonScale.Show(flag7 && TBox.selected != null && TBox.selected.CanScale() != Vector3.zero);
		TBox.tileLockedModelIcon.Show(show && flag5);
		if (WorldSession.isProfileBuildSession())
		{
			bool show3 = (TBox.selected == null || flag6) && !CharacterEditor.Instance.InEditMode() && Blocksworld.CurrentState == State.Build && WorldSession.current.profileWorldAnimatedBlockster != null;
			TBox.tileCharacterEditIcon.Show(show3);
			TBox.tileCharacterEditExitIcon.Show(false);
		}
		else
		{
			bool show4 = show && flag6 && TBox.selectedBlock.goT.up.y > 0.9f;
			TBox.tileCharacterEditIcon.Show(show4);
			TBox.tileCharacterEditExitIcon.Show(false);
		}
	}

	// Token: 0x06001FF9 RID: 8185 RVA: 0x000E8900 File Offset: 0x000E6D00
	public static void UpdateCopyButtonVisibility()
	{
		bool flag = TBox.IsShowingCopy();
		bool flag2 = flag && TBox.IsShowingModel();
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

	// Token: 0x06001FFA RID: 8186 RVA: 0x000E8974 File Offset: 0x000E6D74
	public static bool IsShowingModel()
	{
		bool flag = TBox.IsShowingCopy();
		if (flag)
		{
			if (Blocksworld.selectedBlock != null)
			{
				flag = (!Blocksworld.selectedBlock.HasDefaultTiles() && !Blocksworld.selectedBlock.HasGroup("locked-model"));
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

	// Token: 0x06001FFB RID: 8187 RVA: 0x000E8A64 File Offset: 0x000E6E64
	public static bool IsShowingCopy()
	{
		bool flag = TBox.go.activeSelf;
		if (flag && (Tutorial.InTutorialOrPuzzle() || !Blocksworld.buildPanel.IsShowing() || (Blocksworld.selectedBlock == null && Blocksworld.selectedBunch == null) || (Blocksworld.selectedBlock != null && ((Blocksworld.selectedBlock.isTerrain && Blocksworld.selectedBlock.DisableBuildModeMove()) || Blocksworld.selectedBlock is BlockGrouped))))
		{
			flag = false;
		}
		return flag;
	}

	// Token: 0x06001FFC RID: 8188 RVA: 0x000E8AE9 File Offset: 0x000E6EE9
	public static bool IsShowing()
	{
		return TBox.go.activeSelf;
	}

	// Token: 0x06001FFD RID: 8189 RVA: 0x000E8AF8 File Offset: 0x000E6EF8
	public static void MoveTo(Vector3 pos)
	{
		TBox.go.transform.position = pos;
		TBox.goOrientationArrow.transform.position = pos;
		if (TBox.SelectedBunchIsTankTreads())
		{
			TBox.goOrientationArrow.transform.rotation = Blocksworld.selectedBunch.blocks[0].goT.rotation;
		}
	}

	// Token: 0x06001FFE RID: 8190 RVA: 0x000E8B58 File Offset: 0x000E6F58
	public static void RotateTo(Quaternion rot)
	{
		TBox.go.transform.rotation = rot;
		TBox.goOrientationArrow.transform.rotation = rot;
		if (TBox.SelectedBunchIsTankTreads())
		{
			TBox.goOrientationArrow.transform.rotation = Blocksworld.selectedBunch.blocks[0].goT.rotation;
		}
		bool flag = TBox.selectedBlock is BlockAbstractStabilizer;
		if (flag)
		{
			TBox.goOrientationArrow.transform.Rotate(-90f, -90f, 0f);
		}
		if (TBox.selectedBlock is BlockAbstractAntiGravity)
		{
			TBox.goOrientationArrow.transform.rotation *= ((BlockAbstractAntiGravity)TBox.selectedBlock).rotation;
		}
	}

	// Token: 0x06001FFF RID: 8191 RVA: 0x000E8C24 File Offset: 0x000E7024
	public static void ScaleTo(Vector3 size)
	{
		TBox.collider.size = size;
		TBox.goOrientationArrow.transform.localScale = size;
		if (TBox.SelectedBunchIsTankTreads())
		{
			float num = Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z));
			TBox.goOrientationArrow.transform.localScale = new Vector3(num, Mathf.Abs(size.y), num);
		}
		TBox.PositionEdge(0, 0f, -0.5f, -0.5f, 1f, 0f, 0f, size);
		TBox.PositionEdge(1, 0f, 0.5f, -0.5f, 1f, 0f, 0f, size);
		TBox.PositionEdge(2, 0f, 0.5f, 0.5f, 1f, 0f, 0f, size);
		TBox.PositionEdge(3, 0f, -0.5f, 0.5f, 1f, 0f, 0f, size);
		TBox.PositionEdge(4, -0.5f, 0f, -0.5f, 0f, 1f, 0f, size);
		TBox.PositionEdge(5, -0.5f, 0f, 0.5f, 0f, 1f, 0f, size);
		TBox.PositionEdge(6, 0.5f, 0f, 0.5f, 0f, 1f, 0f, size);
		TBox.PositionEdge(7, 0.5f, 0f, -0.5f, 0f, 1f, 0f, size);
		TBox.PositionEdge(8, -0.5f, -0.5f, 0f, 0f, 0f, 1f, size);
		TBox.PositionEdge(9, 0.5f, -0.5f, 0f, 0f, 0f, 1f, size);
		TBox.PositionEdge(10, 0.5f, 0.5f, 0f, 0f, 0f, 1f, size);
		TBox.PositionEdge(11, -0.5f, 0.5f, 0f, 0f, 0f, 1f, size);
	}

	// Token: 0x06002000 RID: 8192 RVA: 0x000E8E58 File Offset: 0x000E7258
	private static void PositionEdge(int i, float x, float y, float z, float xs, float ys, float zs, Vector3 size)
	{
		TBox.goEdges[i].transform.localPosition = new Vector3(x * size.x, y * size.y, z * size.z);
		TBox.goEdges[i].transform.localScale = new Vector3(xs * size.x + TBox.thickness, ys * size.y + TBox.thickness, zs * size.z + TBox.thickness);
		TBox.goEdgesHidden[i].transform.localPosition = TBox.goEdges[i].transform.localPosition;
		TBox.goEdgesHidden[i].transform.localScale = TBox.goEdges[i].transform.localScale;
	}

	// Token: 0x06002001 RID: 8193 RVA: 0x000E8F20 File Offset: 0x000E7320
	public static void SetLayer(int layer)
	{
		for (int i = 0; i < 12; i++)
		{
			TBox.goEdges[i].layer = layer;
		}
		for (int j = 0; j < 12; j++)
		{
			TBox.goEdgesHidden[j].layer = layer;
		}
	}

	// Token: 0x06002002 RID: 8194 RVA: 0x000E8F70 File Offset: 0x000E7370
	private static void PositionButton(GameObject goButton, GameObject goArrow, bool show)
	{
		float num = TBox.scaleScreenToWorld;
		Transform transform = goButton.transform;
		transform.position = goArrow.transform.position;
		transform.LookAt(goButton.transform.position - Blocksworld.cameraForward);
		transform.localScale = 70f * num * Vector3.one;
		goButton.SetActive(show);
	}

	// Token: 0x06002003 RID: 8195 RVA: 0x000E8FD4 File Offset: 0x000E73D4
	public static void PaintRed(bool red)
	{
		if (red)
		{
			for (int i = 0; i < 12; i++)
			{
				TBox.goEdges[i].GetComponent<Renderer>().sharedMaterial = TBox.materialRed;
			}
		}
		else if (TBox.selectedFace == Face.None)
		{
			for (int j = 0; j < 12; j++)
			{
				TBox.goEdges[j].GetComponent<Renderer>().sharedMaterial = TBox.materialWhite;
			}
		}
		else
		{
			List<int> list = TBox.faces[TBox.selectedFace];
			for (int k = 0; k < 12; k++)
			{
				if (list.Contains(k))
				{
					TBox.goEdges[k].GetComponent<Renderer>().sharedMaterial = TBox.materialWhite;
				}
				else
				{
					TBox.goEdges[k].GetComponent<Renderer>().sharedMaterial = TBox.materialGrey;
					TBox.goEdges[k].transform.localScale = 0.999f * TBox.goEdges[k].transform.localScale;
				}
			}
		}
	}

	// Token: 0x06002004 RID: 8196 RVA: 0x000E90DC File Offset: 0x000E74DC
	public static void Update()
	{
		TBox.noSnap = Input.GetKey(KeyCode.LeftShift); // ADDED BY EXDILIN

		if (TBox.go.activeSelf && Blocksworld.tBoxGesture.gestureState != GestureState.Active && TBox.tileButtonMove.IsShowing())
		{
			float num = Vector3.Angle(Blocksworld.mainCamera.ScreenPointToRay(TBox.tileButtonMove.GetPosition() * NormalizedScreen.scale).direction, Vector3.up);
			if (TBox.selected != null)
			{
				num = Vector3.Angle((TBox.selected.GetPosition() - Blocksworld.cameraPosition).normalized, Vector3.up);
			}
			if (num < 110f)
			{
				if (TBox.tileButtonMove.IconName() != TBox.moveUpIconName)
				{
					bool enabled = TBox.tileButtonMove.IsEnabled();
					TBox.tileButtonMove.SetupForIcon(TBox.moveUpIconName, enabled);
					bool show = TBox.tileButtonScale.IsShowing();
					bool flag = TBox.tileButtonScale.IsEnabled();
					TBox.tileButtonScale.SetupForIcon(TBox.scaleUpIconName, flag);
					TBox.tileButtonScale.Show(show);
					TBox.tileButtonScale.Enable(flag);
				}
			}
			else if (TBox.tileButtonMove.IconName() != TBox.moveIconName)
			{
				bool enabled2 = TBox.tileButtonMove.IsEnabled();
				TBox.tileButtonMove.SetupForIcon(TBox.moveIconName, enabled2);
				bool show2 = TBox.tileButtonScale.IsShowing();
				bool flag2 = TBox.tileButtonScale.IsEnabled();
				TBox.tileButtonScale.SetupForIcon(TBox.scaleIconName, flag2);
				TBox.tileButtonScale.Show(show2);
				TBox.tileButtonScale.Enable(flag2);
			}
		}
		if (TBox.selected != null)
		{
			TBox.scaleScreenToWorld = Util.ScreenToWorldScale(TBox.go.transform.position);
			float num2 = Mathf.Max(3f, Util.MinComponent(TBox.collider.size));
			TBox.thickness = Mathf.Min(0.025f * num2, TBox.screenThickness * TBox.scaleScreenToWorld);
			TBox.ScaleTo(TBox.collider.size);
		}
		TBox.UpdateButtons();
	}

	// Token: 0x06002005 RID: 8197 RVA: 0x000E92E4 File Offset: 0x000E76E4
	public static Vector3 BestTBoxCorner(Func<Vector3, Vector3, int> cmp = null)
	{
		if (cmp == null)
		{
			cmp = ((Vector3 p1, Vector3 p2) => (int)Mathf.Sign(p1.y - p2.y));
		}
		Vector3 size = TBox.collider.size;
		float[] array = new float[]
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
		float[] array2 = new float[]
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
		float[] array3 = new float[]
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
		for (int i = 0; i < 8; i++)
		{
			Vector3 point = new Vector3(array[i], array2[i], array3[i]);
			Vector3 vector = TBox.go.transform.position + TBox.go.transform.rotation * point;
			Vector3 vector2 = Util.WorldToScreenPoint(vector, false);
			Vector3 vector3 = 0.001f * vector2 + 0.999f * TBox.SnapTileWithinScreen(vector2);
			if (i == 0 || cmp(vector3, arg) < 0)
			{
				arg = vector3;
				result = vector;
			}
		}
		return result;
	}

	// Token: 0x06002006 RID: 8198 RVA: 0x000E9568 File Offset: 0x000E7968
	public static Vector3 SnapTileWithinScreen(Vector3 pos)
	{
		if (pos.y < 80f)
		{
			pos.y = 80f;
		}
		if (pos.y > (float)(NormalizedScreen.height - 80))
		{
			pos.y = (float)(NormalizedScreen.height - 80);
		}
		float num = (float)NormalizedScreen.width;
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

	// Token: 0x06002007 RID: 8199 RVA: 0x000E9658 File Offset: 0x000E7A58
	public static Vector3 GetMoveButtonInWorldSpace()
	{
		Vector3 position = Blocksworld.guiCamera.WorldToScreenPoint(TBox.tileButtonMove.GetCenterPosition());
		Vector3 position2 = TBox.BestTBoxCorner(null);
		float z = Blocksworld.mainCamera.WorldToScreenPoint(position2).z;
		position.z = z;
		return Blocksworld.mainCamera.ScreenToWorldPoint(position);
	}

	// Token: 0x06002008 RID: 8200 RVA: 0x000E96AC File Offset: 0x000E7AAC
	public static Vector3 GetRotateButtonInWorldSpace()
	{
		Vector3 buttonOffset = TBox.RotateButtonLocalOffset();
		return TBox.ProjectButtonOffsetIntoTBox(buttonOffset);
	}

	// Token: 0x06002009 RID: 8201 RVA: 0x000E96C8 File Offset: 0x000E7AC8
	public static Vector3 GetScaleButtonInWorldSpace()
	{
		Vector3 buttonOffset = TBox.ScaleButtonLocalOffset();
		return TBox.ProjectButtonOffsetIntoTBox(buttonOffset);
	}

	// Token: 0x0600200A RID: 8202 RVA: 0x000E96E1 File Offset: 0x000E7AE1
	private static Vector3 MoveButtonLocalOffset()
	{
		return new Vector3(0f, -TBox.buttonMoveToOffset, 0f) * TBox.buttonSpacingScale;
	}

	// Token: 0x0600200B RID: 8203 RVA: 0x000E9702 File Offset: 0x000E7B02
	private static Vector3 RotateButtonLocalOffset()
	{
		return new Vector3(TBox.buttonRotateOffset, 0f, 0f) * TBox.buttonSpacingScale;
	}

	// Token: 0x0600200C RID: 8204 RVA: 0x000E9722 File Offset: 0x000E7B22
	private static Vector3 ScaleButtonLocalOffset()
	{
		return new Vector3(TBox.buttonScaleOffset, 0f, 0f) * TBox.buttonSpacingScale;
	}

	// Token: 0x0600200D RID: 8205 RVA: 0x000E9742 File Offset: 0x000E7B42
	public static bool HitMove(Vector3 pos)
	{
		return TBox.tileButtonMove.HitExtended(pos, -10f, -10f, -10f, -10f, false);
	}

	// Token: 0x0600200E RID: 8206 RVA: 0x000E9764 File Offset: 0x000E7B64
	public static bool HitRotate(Vector3 pos)
	{
		return TBox.tileButtonRotate.HitExtended(pos, -10f, -10f, -10f, -10f, false);
	}

	// Token: 0x0600200F RID: 8207 RVA: 0x000E9786 File Offset: 0x000E7B86
	public static bool HitScale(Vector3 pos)
	{
		return TBox.tileButtonScale.HitExtended(pos, -10f, -10f, -10f, -10f, false);
	}

	// Token: 0x06002010 RID: 8208 RVA: 0x000E97A8 File Offset: 0x000E7BA8
	private static Vector3 ProjectButtonOffsetIntoTBox(Vector3 buttonOffset)
	{
		Vector3 vector = TBox.BestTBoxCorner(null);
		float num = Util.ScreenToWorldScale(vector);
		float d = buttonOffset.x * num;
		float d2 = buttonOffset.y * num;
		return vector + d * Blocksworld.cameraRight + d2 * Blocksworld.cameraUp;
	}

	// Token: 0x06002011 RID: 8209 RVA: 0x000E97FC File Offset: 0x000E7BFC
	public static void UpdateButtons()
	{
		if (Blocksworld.tBoxGesture.IsActive)
		{
			return;
		}
		Block block = null;
		if (TBox.selected != null)
		{
			block = TBox.selectedBlock;
			Vector3 pos = Util.WorldToScreenPoint(TBox.BestTBoxCorner(null), false);
			pos = TBox.SnapTileWithinScreen(pos);
			float value = 1f / NormalizedScreen.physicalScale;
			TBox.buttonSpacingScale = Mathf.Clamp(value, TBox._minButtonScale, TBox._maxButtonScale);
			float num = NormalizedScreen.pixelScale;
			num = Mathf.Max(TBox._minButtonScale, num);
			TBox.buttonTileScale = 80f * num;
			float num2 = 0.5f * TBox.buttonTileScale;
			Vector3 a = new Vector3(pos.x - num2, pos.y - num2, 21f);
			if (TBox.tileButtonScale.IsShowing())
			{
				TBox.tileButtonScale.MoveTo(a + TBox.ScaleButtonLocalOffset());
				TBox.tileButtonScale.SetScale(num);
			}
			if (TBox.tileButtonMove.IsShowing())
			{
				TBox.tileButtonMove.MoveTo(a + TBox.MoveButtonLocalOffset());
				TBox.tileButtonMove.SetScale(num);
			}
			if (TBox.tileButtonRotate.IsShowing())
			{
				TBox.tileButtonRotate.MoveTo(a + TBox.RotateButtonLocalOffset());
				TBox.tileButtonRotate.SetScale(num);
			}
			if (TBox.tileLockedModelIcon.IsShowing())
			{
				Func<Vector3, Vector3, int> cmp = (Vector3 p1, Vector3 p2) => (int)Mathf.Sign(p2.y - p1.y);
				Vector3 worldPos = TBox.BestTBoxCorner(cmp);
				Vector3 vector = Util.WorldToScreenPoint(worldPos, false);
				TBox.tileLockedModelIcon.MoveTo(new Vector3(vector.x - 40f * num, vector.y - 20f * num, 21f));
				TBox.tileLockedModelIcon.SetScale(num);
			}
		}
		else if (WorldSession.isProfileBuildSession() && WorldSession.current.profileWorldAnimatedBlockster != null)
		{
			block = WorldSession.current.profileWorldAnimatedBlockster;
		}
		if (block != null)
		{
			if (TBox.tileCharacterEditIcon.IsShowing())
			{
				float pixelScale = NormalizedScreen.pixelScale;
				Vector3 worldPos2 = block.GetPosition() + 1.55f * Vector3.up;
				Vector3 pos2 = Util.WorldToScreenPoint(worldPos2, false);
				pos2.x -= 40f * pixelScale;
				pos2 = TBox.SnapTileWithinScreen(pos2);
				pos2.z = 21f;
				TBox.tileCharacterEditIcon.MoveTo(pos2);
				TBox.tileCharacterEditIcon.SetScale(pixelScale);
			}
			if (TBox.tileCharacterEditExitIcon.IsShowing())
			{
				float pixelScale2 = NormalizedScreen.pixelScale;
				Vector3 vector2 = block.GetPosition() + Vector3.up;
				vector2 -= Blocksworld.cameraRight * 2.5f;
				Vector3 pos3 = Util.WorldToScreenPoint(vector2, false);
				pos3 = TBox.SnapTileWithinScreen(pos3);
				pos3.z = 21f;
				TBox.tileCharacterEditExitIcon.MoveTo(pos3);
				TBox.tileCharacterEditExitIcon.SetScale(pixelScale2);
			}
		}
	}

	// Token: 0x06002012 RID: 8210 RVA: 0x000E9AE0 File Offset: 0x000E7EE0
	public static Vector3 GetMoveButtonPositionWhenMovedTo(Vector3 worldSpacePos)
	{
		Vector3 a = Util.WorldToScreenPoint(TBox.BestTBoxCorner(null), false);
		Vector3 b = Util.WorldToScreenPoint(Blocksworld.selectedBlock.GetPosition(), false);
		Vector3 b2 = a - b;
		Vector3 a2 = Util.WorldToScreenPoint(worldSpacePos, false);
		Vector3 vector = a2 + b2;
		float value = 1f / NormalizedScreen.physicalScale;
		TBox.buttonSpacingScale = Mathf.Clamp(value, TBox._minButtonScale, TBox._maxButtonScale);
		float num = NormalizedScreen.pixelScale;
		num = Mathf.Max(TBox._minButtonScale, num);
		TBox.buttonTileScale = 80f * num;
		float num2 = 0.5f * TBox.buttonTileScale;
		Vector3 a3 = new Vector3(vector.x - num2, vector.y - num2, 21f);
		return a3 + TBox.MoveButtonLocalOffset();
	}

	// Token: 0x06002013 RID: 8211 RVA: 0x000E9BA2 File Offset: 0x000E7FA2
	public static bool IsMoveUp()
	{
		return TBox.tileButtonMove.IconName() == TBox.moveUpIconName;
	}

	// Token: 0x06002014 RID: 8212 RVA: 0x000E9BB8 File Offset: 0x000E7FB8
	public static bool IsScaleUp()
	{
		return TBox.tileButtonScale.IconName() == TBox.scaleUpIconName;
	}

	// Token: 0x06002015 RID: 8213 RVA: 0x000E9BD0 File Offset: 0x000E7FD0
	public static void StartMove(Vector2 touchPos, TBox.MoveMode mode)
	{
		if (mode == TBox.MoveMode.Plane && TBox.tileButtonMove.IconName() == TBox.moveUpIconName)
		{
			mode = TBox.MoveMode.Up;
		}
		TBox.dragBlockMode = mode;
		TBox.dragBlockStartRot = TBox.selected.GetRotation();
		TBox.dragBlockLastFreeRot = TBox.dragBlockStartRot;
		TBox.dragBlockStartPos = TBox.selected.GetPosition();
		TBox.dragBlockCurrentPos = TBox.dragBlockStartPos;
		TBox.dragBlockSearchFreePos = TBox.dragBlockStartPos;
		TBox.dragBlockSearchFreeRot = TBox.dragBlockStartRot;
		TBox.dragBlockLastFreePos = TBox.dragBlockStartPos;
		TBox.dragBlockLastDesiredPos = Util.nullVector3;
		TBox.dragBlockLastHitNormal = Util.nullVector3;
		TBox.dragBlockScale = TBox.selected.GetScale();
		TBox.dragButtonStartWorldPos = TBox.BestTBoxCorner(null);
		if (mode != TBox.MoveMode.Raycast)
		{
			if (mode != TBox.MoveMode.Plane)
			{
				if (mode == TBox.MoveMode.Up)
				{
					TBox.dragBlockAxis = Blocksworld.constrainedManipulationAxis;
					TBox.dragBlockStartAxisPos = Util.ProjectScreenPointOnWorldAxis(TBox.dragButtonStartWorldPos, TBox.dragBlockAxis, touchPos);
					TBox.selected.EnableCollider(false);
				}
			}
			else
			{
				TBox.dragBlockStartAxisPos = Util.ProjectScreenPointOnWorldPlane(TBox.dragButtonStartWorldPos, Vector3.up, touchPos);
				TBox.selected.EnableCollider(false);
			}
		}
	}

	// Token: 0x06002016 RID: 8214 RVA: 0x000E9D00 File Offset: 0x000E8100
	private static Vector3 GetBlockSnapPosition(Vector3 pos, Vector3 blockSize)
	{
		return new Vector3((Mathf.Round(blockSize.x) % 2f != 0f) ? Mathf.Round(pos.x) : (Mathf.Round(pos.x + 0.5f) - 0.5f), (Mathf.Round(blockSize.y) % 2f != 0f) ? Mathf.Round(pos.y) : (Mathf.Round(pos.y + 0.5f) - 0.5f), (Mathf.Round(blockSize.z) % 2f != 0f) ? Mathf.Round(pos.z) : (Mathf.Round(pos.z + 0.5f) - 0.5f));
	}

	// Token: 0x06002017 RID: 8215 RVA: 0x000E9DE4 File Offset: 0x000E81E4
	private static void CheckSnapPosition(Vector3 snapPos, string msg)
	{
		Vector3 vector = snapPos * 2f;
		float num = Mathf.Round(vector.x) - vector.x;
		if ((double)Mathf.Abs(num) > 1E-12)
		{
			BWLog.Info(string.Concat(new object[]
			{
				"Failed snap check x ",
				num,
				" ",
				msg
			}));
		}
		float num2 = Mathf.Round(vector.y) - vector.y;
		if ((double)Mathf.Abs(num2) > 1E-12)
		{
			BWLog.Info(string.Concat(new object[]
			{
				"Failed snap check y ",
				num2,
				" ",
				msg
			}));
		}
		float num3 = Mathf.Round(vector.z) - vector.z;
		if ((double)Mathf.Abs(num3) > 1E-12)
		{
			BWLog.Info(string.Concat(new object[]
			{
				"Failed snap check z ",
				num3,
				" ",
				msg
			}));
		}
	}

	// Token: 0x06002018 RID: 8216 RVA: 0x000E9F02 File Offset: 0x000E8302
	public static Vector3 GetPosition()
	{
		return TBox.go.transform.position;
	}

	// Token: 0x06002019 RID: 8217 RVA: 0x000E9F13 File Offset: 0x000E8313
	public static bool IsRaycastDragging()
	{
		return TBox.dragBlockMode == TBox.MoveMode.Raycast;
	}

	// Token: 0x0600201A RID: 8218 RVA: 0x000E9F20 File Offset: 0x000E8320
	public static bool ContinueMove(Vector2 touchPos, bool forceUpdate = false)
	{

		if (Blocksworld.CurrentState == State.Play)
		{
			BWLog.Info("Trying to move tbox in play mode");
			return false;
		}
		if (TBox.selected == null)
		{
			return false;
		}
		Vector3 vector = Util.nullVector3;
		Vector3 vector2 = Util.nullVector3;
		Vector3 vector3 = Vector3.zero;
		Quaternion quaternion = TBox.dragBlockStartRot;
		bool flag = false;
		bool red = false;
		TBox.MoveMode moveMode = TBox.dragBlockMode;
		if (moveMode != TBox.MoveMode.Raycast)
		{
			if (moveMode != TBox.MoveMode.Plane)
			{
				if (moveMode == TBox.MoveMode.Up)
				{
					if (Mathf.Abs(Vector3.Dot(Blocksworld.cameraForward, Blocksworld.constrainedManipulationAxis)) <= 0.95f)
					{
						vector2 = Util.ProjectScreenPointOnWorldAxis(TBox.dragButtonStartWorldPos, TBox.dragBlockAxis, touchPos);
						Vector3 b = vector2 - TBox.dragBlockStartAxisPos;
						vector2 = TBox.dragBlockStartPos + b;
						Vector3 vector4 = Util.Abs(TBox.selected.GetRotation() * TBox.dragBlockScale);
						vector = ((!TBox.noSnap) ? TBox.GetBlockSnapPosition(vector2, vector4) : vector2);
						if (!TBox.noSnap && !TBox.OkScalePositionCombination(vector4, vector, null))
						{
							BWLog.Info("Not OK snap position during moving upwards");
						}
						float num = Mathf.Sign(Vector3.Dot(vector - TBox.dragBlockStartPos, TBox.dragBlockAxis));
						vector3 = -num * TBox.dragBlockAxis;
					}
				}
			}
			else
			{
				vector2 = Util.ProjectScreenPointOnWorldPlane(TBox.dragButtonStartWorldPos, Vector3.up, touchPos);
				Vector3 b = vector2 - TBox.dragBlockStartAxisPos;
				vector2 = TBox.dragBlockStartPos + b;
				Vector3 vector4 = Util.Abs(TBox.selected.GetRotation() * TBox.dragBlockScale);
				vector = ((!TBox.noSnap) ? TBox.GetBlockSnapPosition(vector2, vector4) : vector2);
				if (!TBox.noSnap && !TBox.OkScalePositionCombination(vector4, vector, null))
				{
					BWLog.Info("Not OK snap position during plane move");
				}
				vector3 = Vector3.up;
			}
		}
		else
		{
			if (Blocksworld.worldSky != null)
			{
				Blocksworld.worldSky.go.SetLayer(Layer.IgnoreRaycast, false);
			}
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Ray ray = Blocksworld.mainCamera.ScreenPointToRay(touchPos * NormalizedScreen.scale);
			RaycastHit raycastHit = default(RaycastHit);
			RaycastHit raycastHit2 = default(RaycastHit);
			RaycastHit[] array = Physics.RaycastAll(ray);
			bool flag2 = false;
			if (array.Length > 0)
			{
				Array.Sort<RaycastHit>(array, new RaycastDistanceComparer(cameraPosition));
				foreach (RaycastHit raycastHit3 in array)
				{
					Block block = BWSceneManager.FindBlock(raycastHit3.collider.gameObject, false);
					if (block != TBox.selectedBlock)
					{
						if (Tutorial.RaycastTargetBlockOK(block))
						{
							raycastHit = raycastHit3;
							flag2 = true;
							break;
						}
					}
				}
			}
			RaycastHit[] array3 = Physics.SphereCastAll(ray, 0.75f);
			List<RaycastHit> list = new List<RaycastHit>();
			bool flag3 = false;
			if (array3.Length > 0)
			{
				foreach (RaycastHit item in array3)
				{
					Block block2 = BWSceneManager.FindBlock(item.collider.gameObject, false);
					if (block2 != TBox.selectedBlock)
					{
						if (Tutorial.RaycastTargetBlockOK(block2))
						{
							list.Add(item);
							flag3 = true;
						}
					}
				}
			}
			if (list.Count > 0)
			{
				list.Sort(new RaycastDistanceComparer(cameraPosition));
			}
			float num2 = Blocksworld.maxBlockDragDistance * 2f;
			if (flag2)
			{
				num2 = (raycastHit.point - cameraPosition).magnitude;
			}
			float num3 = Blocksworld.maxBlockDragDistance * 2f;
			if (flag3)
			{
				RaycastHit raycastHit4 = list[0];
				raycastHit2 = raycastHit4;
				if (flag2)
				{
					float num4 = Vector3.Distance(raycastHit2.point, raycastHit.point);
					for (int k = 1; k < list.Count; k++)
					{
						RaycastHit raycastHit5 = list[k];
						float num5 = Vector3.Distance(raycastHit5.point, raycastHit.point);
						float num6 = Vector3.Distance(raycastHit5.point, raycastHit4.point);
						if (num6 < 5f && num5 < num4)
						{
							raycastHit2 = raycastHit5;
							num4 = num5;
						}
					}
				}
				num3 = (raycastHit2.point - cameraPosition).magnitude;
			}
			bool flag4 = Mathf.Min(num3, num2) < Blocksworld.maxBlockDragDistance && raycastHit.collider != null;
			Block block3 = (!flag4) ? null : BWSceneManager.FindBlock(raycastHit.collider.gameObject, true);
			bool flag5 = false;
			if (flag4 && block3 != null && block3.isTerrain)
			{
				if (flag2 && flag3)
				{
					float num7 = num3 / num2;
					if (num7 <= 0.01f)
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
			if (flag4 && !TBox.selected.IsColliderHit(raycastHit.collider) && Tutorial.DistanceOK(raycastHit.point))
			{
				TBox.selected.IgnoreRaycasts(true);
				Vector3 vector4 = Util.Abs(TBox.selected.GetRotation() * TBox.dragBlockScale);
				vector3 = Util.RoundDirection(raycastHit.normal);
				if (TBox.selectedBlock != null && block3 != null && Block.WantsToOccupySameGridCell(TBox.selectedBlock, block3))
				{
					vector2 = raycastHit.point - 0.5f * vector3;
				}
				else
				{
					vector2 = raycastHit.point + 0.5f * Vector3.Scale(vector3, vector4);
				}
				Bounds bounds;
				if (block3 == null)
				{
					bounds = new Bounds(vector2, Vector3.one);
					if (raycastHit.collider.gameObject != Tutorial.placementHelper)
					{
					}
				}
				else
				{
					bounds = block3.go.GetComponent<Collider>().bounds;
				}
				vector = ((!TBox.noSnap) ? TBox.GetBlockSnapPosition(vector2, vector4) : vector2);
				if (!TBox.noSnap && !TBox.OkScalePositionCombination(vector4, vector, null))
				{
					BWLog.Info("Not OK snap position during raycast move");
				}
				if (flag5)
				{
					List<Bounds> list2 = new List<Bounds>();
					if (TBox.selected is Block)
					{
						Collider[] array5 = Physics.OverlapSphere(raycastHit.point, 3f);
						foreach (Collider collider in array5)
						{
							Block block4 = BWSceneManager.FindBlock(collider.gameObject, false);
							if (block4 != block3 && block4 != TBox.selected && !(block4 is BlockTerrain) && !(block4 is BlockPosition) && !(block4 is BlockVolume) && !(block4 is BlockWater))
							{
								if (!bounds.Contains(collider.bounds.center))
								{
									list2.Add(collider.bounds);
								}
							}
						}
					}
					List<Vector3> list3 = new List<Vector3>();
					for (int m = -1; m <= 1; m++)
					{
						for (int n = -1; n <= 1; n++)
						{
							for (int num8 = -1; num8 <= 1; num8++)
							{
								list3.Add(new Vector3((float)m, (float)n, (float)num8));
							}
						}
					}
					float num9 = -999999f;
					Vector3 b2 = default(Vector3);
					Vector2 b3 = touchPos * NormalizedScreen.scale;
					for (int num10 = 0; num10 < list3.Count; num10++)
					{
						Vector3 vector5 = list3[num10];
						Vector3 vector6 = vector + vector5;
						Vector3 vector7 = Blocksworld.mainCamera.WorldToScreenPoint(vector6);
						Vector2 a = new Vector2(vector7.x, vector7.y);
						float magnitude = (a - b3).magnitude;
						bool flag6 = bounds.Contains(vector6);
						float num11 = Mathf.Sqrt(bounds.SqrDistance(vector6));
						float num12 = (float)((!flag6) ? 0 : 100);
						float num13 = 0.01f * magnitude / NormalizedScreen.scale;
						float magnitude2 = (vector6 - raycastHit.point).magnitude;
						float num14 = -magnitude2 - num13 - num12;
						for (int num15 = 0; num15 < list2.Count; num15++)
						{
							Bounds bounds2 = list2[num15];
							if (bounds2.Contains(vector6))
							{
								num14 -= 100f;
							}
							else
							{
								num11 = Mathf.Min(num11, Mathf.Sqrt(bounds2.SqrDistance(vector6)));
							}
						}
						if (num11 < 0.1f)
						{
							num14 -= 100f;
						}
						else if (num11 > 0.51f)
						{
							num14 -= (num11 - 0.5f) * 1000f;
						}
						if (num14 > num9)
						{
							num9 = num14;
							b2 = vector5;
						}
					}
					vector += b2;
					vector = ((!TBox.noSnap) ? TBox.GetBlockSnapPosition(vector2, vector4) : vector2);
				}
			}
			if (Blocksworld.worldSky != null)
			{
				Blocksworld.worldSky.go.SetLayer(Layer.Default, false);
			}
		}
		bool flag7 = !forceUpdate && !(TBox.selectedBlock is BlockGrouped);
		bool flag8 = false;
		if ((!Util.IsNullVector3(vector) && (vector != TBox.dragBlockLastDesiredPos || vector3 != TBox.dragBlockLastHitNormal)) || forceUpdate)
		{
			TBox.dragBlockLastDesiredPos = vector;
			TBox.dragBlockLastHitNormal = vector3;
			TBox.dragBlockSearchFreePos = vector;
			TBox.dragBlockSearchFreeRot = quaternion;
			TBox.dragBlockCurrentPos = vector;
			float duration = 0.25f;
			TBox.dragBlockTween.Start(duration, 0f, 1f);
			flag8 = true;
		}
		bool value = TBox.selected.IsColliderEnabled();
		TBox.selected.EnableCollider(true);
		Vector3 position = TBox.selected.GetPosition();
		TBox.selected.TBoxMoveTo(TBox.dragBlockSearchFreePos, false);
		TBox.dragBlockColliding = TBox.selected.IsColliding((TBox.dragBlockMode != TBox.MoveMode.Raycast) ? 0f : 0f, null);
		TBox.selected.TBoxMoveTo(position, false);
		TBox.selected.EnableCollider(value);
		if (TBox.dragBlockColliding)
		{
			if (flag7 && !TBox.dragBlockTween.IsFinished())
			{
				TBox.dragBlockSearchFreePos += Vector3.up;
			}
		}
		else
		{
			TBox.dragBlockLastFreePos = TBox.dragBlockSearchFreePos;
			TBox.dragBlockLastFreeRot = TBox.dragBlockSearchFreeRot;
		}
		bool flag9 = false;
		if (TBox.dragBlockColliding && (!flag7 || TBox.dragBlockTween.IsFinished()))
		{
			TBox.selected.TBoxMoveTo(TBox.dragBlockLastFreePos, false);
			TBox.MoveTo(vector2);
			TBox.PaintRed(true);
		}
		else if (flag7)
		{
			Vector3 vector8 = TBox.dragBlockSearchFreePos - TBox.dragBlockCurrentPos;
			Vector3 pos = TBox.dragBlockCurrentPos + TBox.dragBlockTween.Value() * vector8;
			TBox.selected.TBoxMoveTo(pos, false);
			TBox.MoveTo(vector2);
			flag9 = (!TBox.dragBlockColliding && !(vector8 != Vector3.zero));
			TBox.PaintRed(!flag9);
		}
		else
		{
			flag9 = true;
			TBox.MoveTo(TBox.dragBlockSearchFreePos);
			TBox.selected.TBoxMoveTo(TBox.dragBlockSearchFreePos, false);
			TBox.PaintRed(false);
		}
		if (flag8)
		{
			Sound.PlaySoundMovedSelectedBlock();
		}
		if (flag)
		{
			TBox.PaintRed(red);
		}
		return flag9;
	}

	// Token: 0x0600201B RID: 8219 RVA: 0x000EAAA4 File Offset: 0x000E8EA4
	public static void StopMove()
	{
		if (Util.IsNullVector3(TBox.dragBlockLastFreePos))
		{
			BWLog.Info("TBox (couldn't place new block)");
			Blocksworld.DestroyBlock(TBox.selectedBlock);
			TBox.Detach(false);
			TBox.Show(false);
			Blocksworld.blocksworldCamera.Unfollow();
			Scarcity.UpdateInventory(true, null);
			if (Blocksworld.modelCollection != null)
			{
				Blocksworld.modelCollection.RefreshScarcity();
			}
			Blocksworld.buildPanel.Layout();
			History.RemoveState();
			return;
		}
		if (TBox.selected != null)
		{
			if (!TBox.noSnap)
			{
				for (int i = 0; i < 3; i++)
				{
					if (TBox.targetSnapPositionMaxDistance[i] > 0f)
					{
						float f = TBox.targetSnapPosition[i] - TBox.dragBlockLastFreePos[i];
						float num = Mathf.Abs(f);
						if (num > 0f && num < TBox.targetSnapPositionMaxDistance[i])
						{
							TBox.dragBlockLastFreePos[i] = TBox.targetSnapPosition[i];
						}
					}
				}
				Vector3 v = TBox.selected.GetRotation() * TBox.selected.GetScale();
				List<Vector3> list = new List<Vector3>();
				if (!TBox.OkScalePositionCombination(Util.Abs(v), TBox.dragBlockLastFreePos, list))
				{
					foreach (Vector3 pos in list)
					{
						TBox.selected.TBoxMoveTo(pos, false);
						if (!TBox.selected.IsColliding(0f, null))
						{
							TBox.dragBlockLastFreePos = pos;
							break;
						}
					}
				}
			}
			bool flag = TBox.selected.IsColliding(0f, null);
			if (flag)
			{
				TBox.selected.TBoxMoveTo(TBox.dragBlockStartPos, false);
				TBox.selected.TBoxRotateTo(TBox.dragBlockStartRot);
				if (TBox.selected.IsColliding(0f, null))
				{
					if (!(TBox.selected is BlockGrouped) && !Blocksworld.SelectedBunchIsGroup())
					{
						if (TBox.selectedBlock != null)
						{
							Blocksworld.DestroyBlock(TBox.selectedBlock);
							History.RemoveState();
						}
						else if (TBox.selected is Bunch)
						{
							Blocksworld.DestroyBunch((Bunch)TBox.selected);
							History.RemoveState();
						}
						TBox.Detach(false);
						TBox.Show(false);
						Blocksworld.blocksworldCamera.Unfollow();
						Scarcity.UpdateInventory(true, null);
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
				TBox.selected.TBoxMoveTo(TBox.dragBlockLastFreePos, false);
				TBox.selected.TBoxRotateTo(TBox.dragBlockLastFreeRot);
				if (!TBox.noSnap)
				{
					TBox.selected.TBoxSnap();
				}
			}
			TBox.selected.EnableCollider(true);
			TBox.selected.IgnoreRaycasts(false);
		}
		if (TBox.selected != null)
		{
			ConnectednessGraph.Update(TBox.selected);
			Blocksworld.blocksworldCamera.ExpandWorldBounds(TBox.selected);
			BlockGrouped blockGrouped = TBox.selected as BlockGrouped;
			if (blockGrouped != null)
			{
				ConnectednessGraph.Update(blockGrouped.GetMainBlockInGroup());
			}
		}
		TBox.FitToSelected();
		TBox.PaintRed(false);
	}

	// Token: 0x0600201C RID: 8220 RVA: 0x000EADEC File Offset: 0x000E91EC
	public static void StartRotate(Vector2 touchPos, TBox.RotateMode mode)
	{
		TBox.rotateBlockMode = mode;
		TBox.rotateBlockStartRotation = ((!TBox.noSnap) ? Quaternion.Euler(Util.Round(TBox.selected.GetRotation().eulerAngles / 90f) * 90f) : TBox.selected.GetRotation());
		TBox.rotateBlockLastDesiredRot = TBox.rotateBlockStartRotation;
		TBox.rotateBlockStartPos = TBox.selected.GetPosition();
		TBox.rotateBlockSearchFreePos = TBox.rotateBlockStartPos;
		TBox.rotateBlockScale = TBox.selected.GetScale();
		if (TBox.selectedBlock != null)
		{
			TBox.selectedBlock.TBoxStartRotate();
		}
		TBox.selected.EnableCollider(false);
	}

	// Token: 0x0600201D RID: 8221 RVA: 0x000EAEA0 File Offset: 0x000E92A0
	public static void ContinueRotate(Vector2 startTouchPos, Vector2 touchPos)
	{
		float num = 0f;
		Vector3 a = Vector3.zero;
		TBox.RotateMode rotateMode = TBox.rotateBlockMode;
		if (rotateMode != TBox.RotateMode.Button)
		{
			if (rotateMode == TBox.RotateMode.Finger)
			{
				num = Mathf.Abs(startTouchPos.y - touchPos.y);
				TBox.goArrowRotateYAngle = Mathf.Sign(startTouchPos.y - touchPos.y) * 90f;
				int num2 = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
				Vector3 a2 = Vector3.forward;
				if (num2 == 1)
				{
					a2 = Vector3.right;
				}
				else if (num2 == 2)
				{
					a2 = -Vector3.forward;
				}
				else if (num2 == 3)
				{
					a2 = -Vector3.right;
				}
				a = Quaternion.Inverse(TBox.rotateBlockStartRotation) * (TBox.goArrowRotateYAngle * a2);
			}
		}
		else if (Mathf.Abs(startTouchPos.x - touchPos.x) >= Mathf.Abs(startTouchPos.y - touchPos.y))
		{
			num = Mathf.Abs(startTouchPos.x - touchPos.x);
			TBox.goArrowRotateYAngle = Mathf.Sign(startTouchPos.x - touchPos.x) * 90f;
			a = Quaternion.Inverse(TBox.rotateBlockStartRotation) * new Vector3(0f, TBox.goArrowRotateYAngle, 0f);
		}
		else
		{
			num = Mathf.Abs(startTouchPos.y - touchPos.y);
			TBox.goArrowRotateYAngle = Mathf.Sign(startTouchPos.y - touchPos.y) * 90f;
			int num3 = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
			Vector3 a3 = -Vector3.right;
			if (num3 == 1)
			{
				a3 = Vector3.forward;
			}
			else if (num3 == 2)
			{
				a3 = Vector3.right;
			}
			else if (num3 == 3)
			{
				a3 = -Vector3.forward;
			}
			a = Quaternion.Inverse(TBox.rotateBlockStartRotation) * (TBox.goArrowRotateYAngle * a3);
		}
		Vector3 vector = 0.01f * num * a;
		if (TBox.selectedBlock != null)
		{
			Vector3 vector2 = TBox.selectedBlock.AllowedBuildModeRotations();
			vector.x *= vector2.x;
			vector.y *= vector2.y;
			vector.z *= vector2.z;
		}
		Vector3 euler = (!TBox.noSnap) ? (Util.Round(vector / 90f) * 90f) : vector;
		Quaternion rot = TBox.rotateBlockStartRotation * Quaternion.Euler(vector);
		Quaternion quaternion = TBox.rotateBlockStartRotation * Quaternion.Euler(euler);
		Vector3 vector3 = Util.Abs(quaternion * TBox.rotateBlockScale) - Util.Abs(TBox.rotateBlockStartRotation * TBox.rotateBlockScale);
		Vector3 zero = new Vector3((Mathf.Round(vector3.x) % 2f == 0f) ? 0f : (TBox.rotateBlockOffsetSignFlip * 0.5f), (Mathf.Round(vector3.y) % 2f == 0f) ? 0f : -0.5f, (Mathf.Round(vector3.z) % 2f == 0f) ? 0f : (TBox.rotateBlockOffsetSignFlip * 0.5f));
		if (TBox.noSnap)
		{
			zero = Vector3.zero;
		}
		bool flag = false;
		if (quaternion != TBox.rotateBlockLastDesiredRot)
		{
			TBox.rotateBlockLastDesiredRot = quaternion;
			TBox.rotateBlockSearchFreePos = TBox.rotateBlockStartPos + zero;
			TBox.rotateBlockMoveTween.Start((!(TBox.selected is BlockGrouped)) ? 0.25f : 0f, 0f, 1f);
			flag = true;
		}
		TBox.selected.EnableCollider(true);
		Vector3 position = TBox.selected.GetPosition();
		Quaternion rotation = TBox.selected.GetRotation();
		TBox.selected.TBoxRotateTo(quaternion);
		TBox.selected.TBoxMoveTo(TBox.rotateBlockSearchFreePos, false);
		TBox.rotateBlockColliding = TBox.selected.IsColliding(0f, null);
		TBox.selected.TBoxRotateTo(rotation);
		TBox.selected.TBoxMoveTo(position, false);
		TBox.selected.EnableCollider(false);
		if (TBox.rotateBlockColliding && !TBox.rotateBlockMoveTween.IsFinished())
		{
			TBox.rotateBlockSearchFreePos = 0.5f * Util.Round(2f * (TBox.rotateBlockSearchFreePos + Vector3.up));
		}
		if (TBox.rotateBlockColliding && TBox.rotateBlockMoveTween.IsFinished())
		{
			if (!(TBox.selected is BlockGrouped))
			{
				TBox.selected.TBoxRotateTo(TBox.rotateBlockStartRotation);
				TBox.selected.TBoxMoveTo(TBox.rotateBlockStartPos, false);
			}
			TBox.RotateTo(rot);
			TBox.PaintRed(true);
		}
		else
		{
			Vector3 vector4 = TBox.rotateBlockSearchFreePos - TBox.rotateBlockStartPos;
			Vector3 pos = TBox.rotateBlockStartPos + TBox.rotateBlockMoveTween.Value() * vector4;
			TBox.selected.TBoxRotateTo(quaternion);
			TBox.selected.TBoxMoveTo(pos, false);
			TBox.RotateTo(rot);
			TBox.MoveTo(TBox.rotateBlockStartPos + zero);
			TBox.PaintRed(vector4 != zero);
		}
		if (flag)
		{
			Sound.PlaySoundMovedSelectedBlock();
		}
	}

	// Token: 0x0600201E RID: 8222 RVA: 0x000EB458 File Offset: 0x000E9858
	public static void StopRotate()
	{
		if (TBox.rotateBlockColliding || TBox.selected.IsColliding(0f, null))
		{
			TBox.selected.TBoxRotateTo(TBox.rotateBlockStartRotation);
			TBox.selected.TBoxMoveTo(TBox.rotateBlockStartPos, false);
		}
		else
		{
			TBox.selected.TBoxRotateTo(TBox.rotateBlockLastDesiredRot);
			TBox.selected.TBoxMoveTo(TBox.rotateBlockSearchFreePos, false);
		}
		if (TBox.selectedBlock != null)
		{
			TBox.selectedBlock.TBoxStopRotate();
		}
		if (!TBox.noSnap)
		{
			TBox.selected.TBoxSnap();
		}
		TBox.FitToSelected();
		TBox.PaintRed(false);
		TBox.rotateBlockOffsetSignFlip *= -1f;
		TBox.selected.EnableCollider(true);
		ConnectednessGraph.Update(TBox.selected);
		BlockGrouped blockGrouped = TBox.selected as BlockGrouped;
		if (blockGrouped != null)
		{
			ConnectednessGraph.Update(blockGrouped.GetMainBlockInGroup());
		}
	}

	// Token: 0x0600201F RID: 8223 RVA: 0x000EB540 File Offset: 0x000E9940
	public static void StartScale(Vector2 touchPos, TBox.ScaleMode mode)
	{
		if (mode == TBox.ScaleMode.Plane && TBox.tileButtonScale.IconName() == TBox.scaleUpIconName)
		{
			mode = TBox.ScaleMode.Up;
		}
		TBox.scaleBlockMode = mode;
		TBox.scaleBlockStartScale = TBox.selectedBlock.Scale();
		TBox.scaleBlockCurrentScale = TBox.scaleBlockStartScale;
		TBox.scaleBlockSearchFreeScale = TBox.scaleBlockStartScale;
		TBox.scaleBlockLastFreeScale = TBox.scaleBlockStartScale;
		TBox.scaleBlockLastFreePos = TBox.selectedBlock.goT.position;
		TBox.scaleBlockLastDesiredScale = TBox.scaleBlockStartScale;
		TBox.scaleBlockStartPos = TBox.selectedBlock.goT.position;
		TBox.scaleBlockCurrentPos = TBox.scaleBlockStartPos;
		TBox.scaleBlockSearchFreePos = TBox.scaleBlockStartPos;
		TBox.scaleButtonStartWorldPos = TBox.BestTBoxCorner(null);
		if (mode != TBox.ScaleMode.Plane)
		{
			if (mode == TBox.ScaleMode.Up)
			{
				TBox.scaleBlockWorldAxis = Blocksworld.constrainedManipulationAxis;
				TBox.scaleBlockWorldAxis2 = Vector3.zero;
				TBox.scaleBlockWorldAxisStartPos = Util.ProjectScreenPointOnWorldAxis(TBox.scaleButtonStartWorldPos, Util.Abs(TBox.scaleBlockWorldAxis), touchPos);
			}
		}
		else
		{
			int num = (int)Mathf.Floor(Blocksworld.cameraTransform.eulerAngles.y / 90f);
			TBox.scaleBlockWorldAxis = ((num != 0 && num != 1) ? Vector3.right : (-Vector3.right));
			TBox.scaleBlockWorldAxis2 = ((num != 0 && num != 3) ? Vector3.forward : (-Vector3.forward));
			TBox.scaleBlockWorldAxisStartPos = Util.ProjectScreenPointOnWorldAxis(TBox.scaleButtonStartWorldPos, Vector3.up, Util.Abs(TBox.scaleBlockWorldAxis), touchPos);
			TBox.scaleBlockWorldAxisStartPos2 = Util.ProjectScreenPointOnWorldAxis(TBox.scaleButtonStartWorldPos, Vector3.up, Util.Abs(TBox.scaleBlockWorldAxis2), touchPos);
		}
		if (TBox.selectedBlock.go != null)
		{
			TBox.selectedBlock.EnableCollider(false);
		}
	}

	// Token: 0x06002020 RID: 8224 RVA: 0x000EB718 File Offset: 0x000E9B18
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

	// Token: 0x06002021 RID: 8225 RVA: 0x000EB813 File Offset: 0x000E9C13
	private static bool SafeScale(Vector3 s)
	{
		return s.x >= 0.999f && s.y >= 0.999f && s.z >= 0.999f;
	}

	// Token: 0x06002022 RID: 8226 RVA: 0x000EB84C File Offset: 0x000E9C4C
	public static bool OkScalePositionCombination(Vector3 scale, Vector3 pos, List<Vector3> okPositions = null)
	{
		Vector3 a = pos;
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
					a[i] = pos[i] + 0.5f;
				}
			}
			else if (flag2)
			{
				flag = false;
				a[i] = Mathf.Round(pos[i]);
			}
		}
		if (!flag && okPositions != null)
		{
			okPositions.Add(a + Vector3.right);
			okPositions.Add(a + Vector3.left);
			okPositions.Add(a + Vector3.forward);
			okPositions.Add(a + Vector3.back);
		}
		return flag;
	}

	// Token: 0x06002023 RID: 8227 RVA: 0x000EB940 File Offset: 0x000E9D40
	private static void ScaleUniformXZ(Vector3 forward, ref Vector3 ds, ref Vector3 dsSnap)
	{
		if (Vector3.Project(forward, TBox.scaleBlockWorldAxis).sqrMagnitude > Vector3.Project(forward, TBox.scaleBlockWorldAxis2).sqrMagnitude)
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

	// Token: 0x06002024 RID: 8228 RVA: 0x000EB9B0 File Offset: 0x000E9DB0
	public static void ContinueScale(Vector2 touchPos)
	{
		if (TBox.selectedBlock == null)
		{
			return;
		}
		Vector3 lhs = (TBox.scaleBlockMode != TBox.ScaleMode.Up) ? (Util.ProjectScreenPointOnWorldAxis(TBox.scaleButtonStartWorldPos, Vector3.up, Util.Abs(TBox.scaleBlockWorldAxis), touchPos) - TBox.scaleBlockWorldAxisStartPos) : (Util.ProjectScreenPointOnWorldAxis(TBox.scaleButtonStartWorldPos, Util.Abs(TBox.scaleBlockWorldAxis), touchPos) - TBox.scaleBlockWorldAxisStartPos);
		Vector3 lhs2 = (TBox.scaleBlockMode != TBox.ScaleMode.Up) ? (Util.ProjectScreenPointOnWorldAxis(TBox.scaleButtonStartWorldPos, Vector3.up, Util.Abs(TBox.scaleBlockWorldAxis2), touchPos) - TBox.scaleBlockWorldAxisStartPos2) : Vector3.zero;
		Vector3 vector = new Vector3(lhs.magnitude * Mathf.Sign(Vector3.Dot(lhs, TBox.scaleBlockWorldAxis)), lhs2.magnitude * Mathf.Sign(Vector3.Dot(lhs2, TBox.scaleBlockWorldAxis2)), 0f);
		Vector3 vector2 = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), 0f);
		bool flag = false;
		Vector3[] array = new Vector3[]
		{
			Util.Abs(TBox.scaleBlockWorldAxis),
			Util.Abs(TBox.scaleBlockWorldAxis2),
			Vector3.up
		};
		bool[] array2 = new bool[3];
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		foreach (Vector3 vector3 in TBox.selectedBlock.GetScaleConstraints())
		{
			if ((vector3 - Vector3.one).sqrMagnitude < 0.0001f)
			{
				flag = true;
			}
			Quaternion rotation = TBox.selectedBlock.goT.rotation;
			Vector3 v = rotation * vector3;
			Vector3 vector4 = Util.Abs(v);
			for (int j = 0; j < array.Length; j++)
			{
				if ((array[j] - vector4).sqrMagnitude < 0.0001f)
				{
					array2[j] = true;
				}
			}
			if (!flag && Mathf.Abs(vector4.sqrMagnitude - 2f) < 0.001f)
			{
				if (Vector3.Dot(array[0], vector4) > 0.001f && Vector3.Dot(array[1], vector4) > 0.001f)
				{
					flag2 = true;
				}
				if (Vector3.Dot(array[0], vector4) > 0.001f && Vector3.Dot(array[2], vector4) > 0.001f)
				{
					flag3 = true;
				}
				if (Vector3.Dot(array[1], vector4) > 0.001f && Vector3.Dot(array[2], vector4) > 0.001f)
				{
					flag4 = true;
				}
			}
		}
		bool flag5 = TBox.selectedBlock is BlockAbstractWheel || TBox.selectedBlock is BlockRaycastWheel;
		bool flag6 = TBox.selectedBlock is BlockTankTreadsWheel;
		Transform goT = TBox.selectedBlock.goT;
		if (flag5 && Mathf.Abs(goT.up.x) < 0.001f && Mathf.Abs(goT.up.z) < 0.001f && Mathf.Abs(TBox.scaleBlockStartScale.y - TBox.scaleBlockStartScale.z) < 0.001f)
		{
			TBox.ScaleUniformXZ(goT.forward, ref vector, ref vector2);
		}
		else if (flag6)
		{
			TBox.ScaleUniformXZ(goT.forward, ref vector, ref vector2);
		}
		else if (flag)
		{
			float num = Util.MaxAbsWithSign(vector.y, vector.x);
			vector.x = num;
			vector.y = num;
			vector.z = num;
			vector2 = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		}
		else if (flag2)
		{
			vector.x = Util.MaxAbsWithSign(vector.x, vector.y);
			vector.y = vector.x;
			vector.z = ((!array2[2]) ? 0f : vector.z);
			vector2.x = Util.MaxAbsWithSign(vector2.x, vector2.y);
			vector2.y = vector2.x;
			vector2.z = ((!array2[2]) ? 0f : vector2.z);
		}
		else if (flag3)
		{
			vector.x = Util.MaxAbsWithSign(vector.x, vector.z);
			vector.y = ((!array2[1]) ? 0f : vector.y);
			vector.z = vector.x;
			vector2.x = Util.MaxAbsWithSign(vector2.x, vector2.z);
			vector2.y = ((!array2[1]) ? 0f : vector2.y);
			vector2.z = vector2.x;
		}
		else if (flag4)
		{
			vector.x = ((!array2[0]) ? 0f : vector.x);
			vector.y = Util.MaxAbsWithSign(vector.y, vector.z);
			vector.z = vector.y;
			vector2.x = ((!array2[0]) ? 0f : vector2.x);
			vector2.y = Util.MaxAbsWithSign(vector2.y, vector2.z);
			vector2.z = vector2.y;
		}
		Vector3 vector5 = Util.Abs(Quaternion.Inverse(TBox.selectedBlock.goT.rotation) * TBox.scaleBlockWorldAxis);
		Vector3 vector6 = Util.Abs(Quaternion.Inverse(TBox.selectedBlock.goT.rotation) * TBox.scaleBlockWorldAxis2);
		Vector3 vector7 = TBox.selectedBlock.CanScale();
		if ((vector5 == Vector3.right && vector7.x == 0f) || (vector5 == Vector3.up && vector7.y == 0f) || (vector5 == Vector3.forward && vector7.z == 0f))
		{
			vector.x = (vector2.x = 0f);
		}
		if ((vector6 == Vector3.right && vector7.x == 0f) || (vector6 == Vector3.up && vector7.y == 0f) || (vector6 == Vector3.forward && vector7.z == 0f))
		{
			vector.y = (vector2.y = 0f);
		}
		Vector3 vector8 = TBox.scaleBlockStartScale + vector.x * vector5 + vector.y * vector6 + vector.z * Vector3.up;
		Vector3 vector9 = TBox.scaleBlockStartScale + vector2.x * vector5 + vector2.y * vector6 + vector2.z * Vector3.up;
		Vector3 vector10 = vector / 2f;
		Vector3 vector11 = vector2 / 2f;
		if (flag && TBox.scaleBlockMode == TBox.ScaleMode.Up)
		{
			vector10 = vector / 4f;
			vector11 = vector2 / 4f;
		}
		bool flag7 = false;
		if (flag)
		{
			vector8 = TBox.scaleBlockStartScale + vector;
			vector9 = TBox.scaleBlockStartScale + vector2;
			float num2 = Util.MinComponent(vector9);
			float num3 = Mathf.Max(1f, Util.MaxComponent(vector9));
			float num4 = Util.MinComponent(vector8);
			float num5 = Mathf.Max(1f, Util.MaxComponent(vector8));
			if (num2 >= 1f)
			{
				vector9 = new Vector3(num2, num2, num2);
			}
			else
			{
				vector9 = new Vector3(num3, num3, num3);
				flag7 = true;
			}
			if (num4 >= 1f)
			{
				vector8 = new Vector3(num4, num4, num4);
			}
			else
			{
				vector8 = new Vector3(num5, num5, num5);
				flag7 = true;
			}
		}
		Vector3 vector12 = TBox.scaleBlockStartPos + vector10.x * TBox.scaleBlockWorldAxis + vector10.y * TBox.scaleBlockWorldAxis2 + vector10.z * Vector3.up;
		Vector3 a = TBox.scaleBlockStartPos + vector11.x * TBox.scaleBlockWorldAxis + vector11.y * TBox.scaleBlockWorldAxis2 + vector11.z * Vector3.up;
		if (flag7)
		{
			vector12 = TBox.selectedBlock.goT.position;
			a = new Vector3(Mathf.Round(vector12.x), Mathf.Round(vector12.y), Mathf.Round(vector12.z));
		}
		float num6 = TBox.InvertNegativeScale(vector5, ref vector8, ref vector9, ref vector12);
		float num7 = (TBox.scaleBlockMode != TBox.ScaleMode.Up) ? TBox.InvertNegativeScale(vector6, ref vector8, ref vector9, ref vector12) : 1f;
		float num8 = (vector.z != 0f) ? TBox.InvertNegativeScale(Vector3.up, ref vector8, ref vector9, ref vector12) : 0f;
		vector12 += 0.5f * num6 * TBox.scaleBlockWorldAxis;
		vector12 += 0.5f * num7 * TBox.scaleBlockWorldAxis2;
		vector12 += 0.5f * num8 * Vector3.up;
		a += 0.5f * Mathf.Round(num6) * TBox.scaleBlockWorldAxis;
		a += 0.5f * Mathf.Round(num7) * TBox.scaleBlockWorldAxis2;
		a += 0.5f * Mathf.Round(num8) * Vector3.up;
		bool flag8 = false;
		if (vector9 != TBox.scaleBlockLastDesiredScale)
		{
			TBox.scaleBlockLastDesiredScale = vector9;
			TBox.scaleBlockCurrentScale = vector9;
			TBox.scaleBlockCurrentPos = a;
			TBox.scaleBlockSearchFreeScale = vector9;
			TBox.scaleBlockSearchFreePos = a;
			TBox.scaleBlockTween.Start((!(TBox.selectedBlock is BlockGrouped)) ? 0.25f : 0f, 0f, 1f);
			flag8 = true;
		}
		TBox.selectedBlock.EnableCollider(true);
		Vector3 position = TBox.selectedBlock.goT.position;
		Vector3 scale = TBox.selectedBlock.Scale();
		TBox.selectedBlock.TBoxMoveTo(TBox.scaleBlockSearchFreePos, false);
		TBox.selectedBlock.TBoxScaleTo(TBox.scaleBlockSearchFreeScale, true, false);
		TBox.scaleBlockColliding = TBox.selectedBlock.IsColliding(0f, null);
		TBox.selectedBlock.TBoxScaleTo(scale, true, false);
		TBox.selectedBlock.TBoxMoveTo(position, false);
		TBox.selectedBlock.EnableCollider(false);
		if (!TBox.scaleBlockColliding)
		{
			TBox.scaleBlockLastFreeScale = TBox.scaleBlockSearchFreeScale;
			TBox.scaleBlockLastFreePos = TBox.scaleBlockSearchFreePos;
		}
		if (TBox.scaleBlockColliding && !TBox.scaleBlockTween.IsFinished())
		{
			if (Tutorial.state != TutorialState.None)
			{
				TBox.scaleBlockSearchFreePos = 0.5f * Util.Round(2f * (TBox.scaleBlockSearchFreePos + Vector3.up));
			}
			else
			{
				Vector3 vector13 = TBox.scaleBlockSearchFreeScale;
				Vector3 vector14 = TBox.scaleBlockSearchFreePos;
				if (!flag)
				{
					if (Vector3.Scale(TBox.scaleBlockSearchFreeScale, vector5) != Vector3.Scale(TBox.scaleBlockLastFreeScale, vector5) && TBox.SafeScale(TBox.scaleBlockSearchFreeScale - vector5))
					{
						TBox.scaleBlockSearchFreeScale -= vector5;
						TBox.scaleBlockSearchFreePos -= TBox.scaleBlockWorldAxis * num6 * 0.5f;
					}
					if (TBox.scaleBlockMode == TBox.ScaleMode.Plane && Vector3.Scale(TBox.scaleBlockSearchFreeScale, vector6) != Vector3.Scale(TBox.scaleBlockLastFreeScale, vector6) && TBox.SafeScale(TBox.scaleBlockSearchFreeScale - vector6))
					{
						TBox.scaleBlockSearchFreeScale -= vector6;
						TBox.scaleBlockSearchFreePos -= TBox.scaleBlockWorldAxis2 * num7 * 0.5f;
					}
					if (TBox.scaleBlockMode == TBox.ScaleMode.Plane && Vector3.Scale(TBox.scaleBlockSearchFreeScale, Vector3.up) != Vector3.Scale(TBox.scaleBlockLastFreeScale, Vector3.up) && TBox.SafeScale(TBox.scaleBlockSearchFreeScale - Vector3.up))
					{
						TBox.scaleBlockSearchFreeScale -= Vector3.up;
						TBox.scaleBlockSearchFreePos -= Vector3.up * num8 * 0.5f;
					}
				}
				else
				{
					TBox.scaleBlockSearchFreeScale = TBox.scaleBlockLastFreeScale;
					TBox.scaleBlockSearchFreePos = TBox.scaleBlockLastFreePos;
				}
				Vector3 scale2 = TBox.selectedBlock.goT.rotation * TBox.scaleBlockSearchFreeScale;
				if (!TBox.noSnap && !TBox.OkScalePositionCombination(scale2, TBox.scaleBlockSearchFreePos, null))
				{
					TBox.scaleBlockSearchFreeScale = vector13;
					TBox.scaleBlockSearchFreePos = vector14;
				}
			}
		}
		if (TBox.scaleBlockColliding && TBox.scaleBlockTween.IsFinished())
		{
			if (!(TBox.selectedBlock is BlockGrouped))
			{
				TBox.selectedBlock.TBoxScaleTo(TBox.scaleBlockStartScale, false, false);
				TBox.selectedBlock.TBoxMoveTo(TBox.scaleBlockStartPos, false);
			}
			TBox.PaintRed(true);
		}
		else
		{
			Vector3 vector15 = TBox.scaleBlockSearchFreeScale - TBox.scaleBlockCurrentScale;
			Vector3 a2 = TBox.scaleBlockSearchFreePos - TBox.scaleBlockCurrentPos;
			TBox.selectedBlock.TBoxScaleTo(TBox.scaleBlockCurrentScale + TBox.scaleBlockTween.Value() * vector15, false, false);
			TBox.selectedBlock.TBoxMoveTo(TBox.scaleBlockCurrentPos + TBox.scaleBlockTween.Value() * a2, false);
			TBox.PaintRed(vector15 != Vector3.zero);
		}
		TBox.ScaleTo(vector8);
		TBox.MoveTo(vector12);
		if (flag8)
		{
			Sound.PlaySoundMovedSelectedBlock();
		}
	}

	// Token: 0x06002025 RID: 8229 RVA: 0x000EC8C8 File Offset: 0x000EACC8
	public static void StopScale()
	{
		Vector3 v = TBox.selectedBlock.goT.rotation * TBox.scaleBlockSearchFreeScale;
		List<Vector3> list = new List<Vector3>();
		bool flag = TBox.noSnap || TBox.OkScalePositionCombination(Util.Abs(v), TBox.scaleBlockSearchFreePos, list);
		if (!flag)
		{
			foreach (Vector3 pos in list)
			{
				TBox.selectedBlock.TBoxMoveTo(pos, false);
				if (!TBox.selectedBlock.IsColliding(0f, null))
				{
					flag = true;
					TBox.scaleBlockSearchFreePos = pos;
					break;
				}
			}
		}
		if (TBox.scaleBlockColliding || !flag)
		{
			TBox.selectedBlock.TBoxScaleTo(TBox.scaleBlockStartScale, true, false);
			TBox.selectedBlock.TBoxMoveTo(TBox.scaleBlockStartPos, false);
		}
		else
		{
			if (Util.MinComponent(TBox.targetSnapScale) >= 1f)
			{
				for (int i = 0; i < 3; i++)
				{
					float num = TBox.targetSnapScale[i] - TBox.scaleBlockSearchFreeScale[i];
					float num2 = Mathf.Abs(num);
					if (num2 > 0f && num2 <= TBox.targetSnapScaleMaxDistance[i])
					{
						TBox.scaleBlockSearchFreeScale[i] = TBox.targetSnapScale[i];
						Vector3 vector = Vector3.zero;
						vector[i] = num;
						vector = Quaternion.Inverse(TBox.selectedBlock.goT.rotation) * vector;
						Vector3 b = TBox.scaleBlockSearchFreePos + vector * 0.5f;
						Vector3 b2 = TBox.scaleBlockSearchFreePos - vector * 0.5f;
						TBox.scaleBlockSearchFreePos = b;
						if ((TBox.scaleBlockStartPos - b).sqrMagnitude < (TBox.scaleBlockStartPos - b2).sqrMagnitude)
						{
							TBox.scaleBlockSearchFreePos = b2;
						}
					}
				}
			}
			TBox.selectedBlock.TBoxScaleTo(TBox.scaleBlockSearchFreeScale, true, false);
			TBox.selectedBlock.TBoxMoveTo(TBox.scaleBlockSearchFreePos, false);
			if (TBox.selectedBlock is BlockGrouped && !TBox.noSnap)
			{
				TBox.selectedBlock.TBoxSnap();
			}
		}
		if (TBox.selectedBlock != null)
		{
			Blocksworld.blocksworldCamera.ExpandWorldBounds(TBox.selectedBlock);
			TBox.selectedBlock.TBoxStopScale();
		}
		TBox.FitToSelected();
		TBox.PaintRed(false);
		TBox.selectedBlock.EnableCollider(true);
		ConnectednessGraph.Update(TBox.selectedBlock);
		BlockGrouped blockGrouped = TBox.selected as BlockGrouped;
		if (blockGrouped != null)
		{
			ConnectednessGraph.Update(blockGrouped.GetMainBlockInGroup());
		}
	}

	// Token: 0x06002026 RID: 8230 RVA: 0x000ECB90 File Offset: 0x000EAF90
	public static void ResetTargetSnapping()
	{
		TBox.targetSnapScale = Vector3.zero;
		TBox.targetSnapScaleMaxDistance = Vector3.zero;
		TBox.targetSnapPosition = Vector3.zero;
		TBox.targetSnapPositionMaxDistance = Vector3.zero;
	}

	// Token: 0x04001AF7 RID: 6903
	public static Vector3 v1;

	// Token: 0x04001AF8 RID: 6904
	public static Vector3 v2;

	// Token: 0x04001AF9 RID: 6905
	private static readonly float _minButtonScale = 1f;

	// Token: 0x04001AFA RID: 6906
	private static readonly float _maxButtonScale = 1.25f;

	// Token: 0x04001AFB RID: 6907
	public static Vector3 targetSnapScale = Vector3.zero;

	// Token: 0x04001AFC RID: 6908
	public static Vector3 targetSnapScaleMaxDistance = Vector3.zero;

	// Token: 0x04001AFD RID: 6909
	public static Vector3 targetSnapPosition = Vector3.zero;

	// Token: 0x04001AFE RID: 6910
	public static Vector3 targetSnapPositionMaxDistance = Vector3.zero;

	// Token: 0x04001AFF RID: 6911
	public static TileObject tileButtonMove;

	// Token: 0x04001B00 RID: 6912
	public static TileObject tileButtonRotate;

	// Token: 0x04001B01 RID: 6913
	public static TileObject tileButtonScale;

	// Token: 0x04001B02 RID: 6914
	public static TileObject tileLockedModelIcon;

	// Token: 0x04001B03 RID: 6915
	public static TileObject tileCharacterEditIcon;

	// Token: 0x04001B04 RID: 6916
	public static TileObject tileCharacterEditExitIcon;

	// Token: 0x04001B05 RID: 6917
	public static string moveIconName = "Buttons/Move";

	// Token: 0x04001B06 RID: 6918
	public static string moveUpIconName = "Buttons/Move_Up";

	// Token: 0x04001B07 RID: 6919
	public static string rotateIconName = "Buttons/Rotate";

	// Token: 0x04001B08 RID: 6920
	public static string scaleIconName = "Buttons/Scale";

	// Token: 0x04001B09 RID: 6921
	public static string scaleUpIconName = "Buttons/Scale_Up";

	// Token: 0x04001B0A RID: 6922
	public static string lockedModelIconName = "Misc/Locked_Model_Icon";

	// Token: 0x04001B0B RID: 6923
	public static string characterEditOnIconName = "Misc/Character_Editor";

	// Token: 0x04001B0C RID: 6924
	public static string characterEditOffIconName = "Misc/Character_Editor_Exit";

	// Token: 0x04001B0D RID: 6925
	private static float screenThickness = 10f;

	// Token: 0x04001B0E RID: 6926
	private static float thickness = 0.05f;

	// Token: 0x04001B0F RID: 6927
	public static float buttonScaleOffset = -58f;

	// Token: 0x04001B10 RID: 6928
	public static float buttonMoveToOffset = 30f;

	// Token: 0x04001B11 RID: 6929
	public static float buttonRotateOffset = 58f;

	// Token: 0x04001B12 RID: 6930
	public static float buttonCopyOffset = 58f;

	// Token: 0x04001B13 RID: 6931
	public static float buttonSaveOffset = 48f;

	// Token: 0x04001B14 RID: 6932
	public static float buttonSpacingScale = 1f;

	// Token: 0x04001B15 RID: 6933
	public static float buttonTileScale = 1f;

	// Token: 0x04001B16 RID: 6934
	public static BoxCollider collider;

	// Token: 0x04001B17 RID: 6935
	public static bool noSnap = false;

	// Token: 0x04001B18 RID: 6936
	private static GameObject go;

	// Token: 0x04001B19 RID: 6937
	private static GameObject[] goEdges = new GameObject[12];

	// Token: 0x04001B1A RID: 6938
	private static GameObject[] goEdgesHidden = new GameObject[12];

	// Token: 0x04001B1B RID: 6939
	private static GameObject goOrientationArrow;

	// Token: 0x04001B1C RID: 6940
	private static Material materialWhite;

	// Token: 0x04001B1D RID: 6941
	private static Material materialWhiteTopmost;

	// Token: 0x04001B1E RID: 6942
	private static Material materialGrey;

	// Token: 0x04001B1F RID: 6943
	private static Material materialRed;

	// Token: 0x04001B20 RID: 6944
	private static Dictionary<Face, List<int>> faces = new Dictionary<Face, List<int>>();

	// Token: 0x04001B21 RID: 6945
	public static ITBox selected = null;

	// Token: 0x04001B22 RID: 6946
	private static Block selectedBlock = null;

	// Token: 0x04001B23 RID: 6947
	private static Face selectedFace = Face.None;

	// Token: 0x04001B24 RID: 6948
	private static GameObject activeHandle;

	// Token: 0x04001B25 RID: 6949
	private static Vector3 activePos;

	// Token: 0x04001B26 RID: 6950
	private static Vector3 activeAxis;

	// Token: 0x04001B27 RID: 6951
	private static Vector3 activeNormal;

	// Token: 0x04001B28 RID: 6952
	private static Quaternion blockStartRotation;

	// Token: 0x04001B29 RID: 6953
	private static Vector3 blockStartPos;

	// Token: 0x04001B2A RID: 6954
	private static Vector3 axisPosStart;

	// Token: 0x04001B2B RID: 6955
	private static float angleStart;

	// Token: 0x04001B2C RID: 6956
	private static bool startedMove;

	// Token: 0x04001B2D RID: 6957
	private static float scaleScreenToWorld;

	// Token: 0x04001B2E RID: 6958
	private static TBox.MoveMode dragBlockMode;

	// Token: 0x04001B2F RID: 6959
	private static bool dragBlockColliding;

	// Token: 0x04001B30 RID: 6960
	public static Tween dragBlockTween = new Tween();

	// Token: 0x04001B31 RID: 6961
	private static Vector3 dragBlockAxis;

	// Token: 0x04001B32 RID: 6962
	private static Vector3 dragBlockStartPos;

	// Token: 0x04001B33 RID: 6963
	private static Vector3 dragBlockSearchFreePos;

	// Token: 0x04001B34 RID: 6964
	private static Vector3 dragBlockCurrentPos;

	// Token: 0x04001B35 RID: 6965
	private static Vector3 dragBlockLastDesiredPos;

	// Token: 0x04001B36 RID: 6966
	private static Vector3 dragBlockLastHitNormal;

	// Token: 0x04001B37 RID: 6967
	private static Vector3 dragBlockLastFreePos;

	// Token: 0x04001B38 RID: 6968
	private static Vector3 dragBlockStartAxisPos;

	// Token: 0x04001B39 RID: 6969
	private static Vector3 dragBlockScale;

	// Token: 0x04001B3A RID: 6970
	private static Vector3 dragButtonStartWorldPos;

	// Token: 0x04001B3B RID: 6971
	private static Quaternion dragBlockStartRot;

	// Token: 0x04001B3C RID: 6972
	private static Quaternion dragBlockSearchFreeRot;

	// Token: 0x04001B3D RID: 6973
	private static Quaternion dragBlockLastFreeRot;

	// Token: 0x04001B3E RID: 6974
	private static float goArrowRotateYAngle;

	// Token: 0x04001B3F RID: 6975
	private static TBox.RotateMode rotateBlockMode;

	// Token: 0x04001B40 RID: 6976
	private static bool rotateBlockColliding;

	// Token: 0x04001B41 RID: 6977
	private static Tween rotateBlockMoveTween = new Tween();

	// Token: 0x04001B42 RID: 6978
	private static float rotateBlockOffsetSignFlip = 1f;

	// Token: 0x04001B43 RID: 6979
	private static Quaternion rotateBlockStartRotation;

	// Token: 0x04001B44 RID: 6980
	private static Quaternion rotateBlockLastDesiredRot;

	// Token: 0x04001B45 RID: 6981
	private static Vector3 rotateBlockStartPos;

	// Token: 0x04001B46 RID: 6982
	private static Vector3 rotateBlockSearchFreePos;

	// Token: 0x04001B47 RID: 6983
	private static Vector3 rotateBlockScale;

	// Token: 0x04001B48 RID: 6984
	private static TBox.ScaleMode scaleBlockMode;

	// Token: 0x04001B49 RID: 6985
	private static bool scaleBlockColliding;

	// Token: 0x04001B4A RID: 6986
	private static Tween scaleBlockTween = new Tween();

	// Token: 0x04001B4B RID: 6987
	private static Vector3 scaleBlockStartScale;

	// Token: 0x04001B4C RID: 6988
	private static Vector3 scaleBlockStartPos;

	// Token: 0x04001B4D RID: 6989
	private static Vector3 scaleBlockSearchFreeScale;

	// Token: 0x04001B4E RID: 6990
	private static Vector3 scaleBlockSearchFreePos;

	// Token: 0x04001B4F RID: 6991
	private static Vector3 scaleBlockLastDesiredScale;

	// Token: 0x04001B50 RID: 6992
	private static Vector3 scaleBlockCurrentScale;

	// Token: 0x04001B51 RID: 6993
	private static Vector3 scaleBlockCurrentPos;

	// Token: 0x04001B52 RID: 6994
	private static Vector3 scaleBlockLastFreeScale;

	// Token: 0x04001B53 RID: 6995
	private static Vector3 scaleBlockLastFreePos;

	// Token: 0x04001B54 RID: 6996
	private static Vector3 scaleButtonStartWorldPos;

	// Token: 0x04001B55 RID: 6997
	private static Vector3 scaleBlockWorldAxis;

	// Token: 0x04001B56 RID: 6998
	private static Vector3 scaleBlockWorldAxis2;

	// Token: 0x04001B57 RID: 6999
	private static Vector3 scaleBlockWorldAxisStartPos;

	// Token: 0x04001B58 RID: 7000
	private static Vector3 scaleBlockWorldAxisStartPos2;

	// Token: 0x020002B8 RID: 696
	public enum MoveMode
	{
		// Token: 0x04001B5C RID: 7004
		Raycast,
		// Token: 0x04001B5D RID: 7005
		Plane,
		// Token: 0x04001B5E RID: 7006
		Up
	}

	// Token: 0x020002B9 RID: 697
	public enum RotateMode
	{
		// Token: 0x04001B60 RID: 7008
		Button,
		// Token: 0x04001B61 RID: 7009
		Finger
	}

	// Token: 0x020002BA RID: 698
	public enum ScaleMode
	{
		// Token: 0x04001B63 RID: 7011
		Plane,
		// Token: 0x04001B64 RID: 7012
		Up
	}
}
