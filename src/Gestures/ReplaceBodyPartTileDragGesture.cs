using System.Collections.Generic;
using Blocks;
using UnityEngine;

namespace Gestures;

public class ReplaceBodyPartTileDragGesture : BaseGesture
{
	private BuildPanel _buildPanel;

	private BlockAnimatedCharacter _targetCharacter;

	private BlocksterBody.BodyPart[] _targetParts;

	private BlocksterBody.BodyPart _snappedToPart;

	private int _snappedToDragPartIndex;

	private Bounds[] _targetPartBounds;

	private GameObject[] _dragParts;

	private bool _hasMirrorPart;

	private Tile _dragTile;

	private string[] _bodyPartStrings;

	private string[] _originalBodyPartStrings;

	private bool _overPanel;

	private bool _partIsSnapped;

	private static GAF _bodyPartGAF;

	private static GAF _replacedBodyPartGAF;

	public ReplaceBodyPartTileDragGesture(BuildPanel buildPanel)
	{
		_buildPanel = buildPanel;
		touchBeginWindow = 12f;
	}

	public override void TouchesBegan(List<Touch> allTouches)
	{
		if (Blocksworld.CurrentState == State.Play || Blocksworld.InModalDialogState())
		{
			EnterState(GestureState.Failed);
		}
		else
		{
			if (base.gestureState == GestureState.Active)
			{
				return;
			}
			if (!TBox.dragBlockTween.IsFinished())
			{
				EnterState(GestureState.Failed);
				return;
			}
			if (CharacterEditor.Instance.InEditMode())
			{
				_targetCharacter = CharacterEditor.Instance.CharacterBlock();
			}
			else if (Blocksworld.selectedBlock != null || Blocksworld.selectedBlock is BlockAnimatedCharacter)
			{
				_targetCharacter = Blocksworld.selectedBlock as BlockAnimatedCharacter;
			}
			if (_targetCharacter == null)
			{
				EnterState(GestureState.Failed);
				return;
			}
			bool flag = allTouches[0].Phase == TouchPhase.Began;
			bool flag2 = (allTouches[0].Phase == TouchPhase.Moved || allTouches[0].Phase == TouchPhase.Stationary) && (float)allTouches[0].moveFrameCount < touchBeginWindow;
			if (!flag && !flag2)
			{
				EnterState(GestureState.Failed);
				return;
			}
			Blocksworld.blocksworldCamera.SetCameraStill(still: true);
			Vector2 position = allTouches[0].Position;
			if (!_buildPanel.Hit(position) || Blocksworld.scriptPanel.Hit(position))
			{
				return;
			}
			Tile tile = _buildPanel.HitTile(position);
			if (tile != null && tile.gaf.Predicate.Name == "AnimCharacter.ReplaceBodyPart")
			{
				string stringArg = Util.GetStringArg(tile.gaf.Args, 0, string.Empty);
				_bodyPartGAF = new GAF("AnimCharacter.ReplaceBodyPart", stringArg);
				_replacedBodyPartGAF = null;
				if (stringArg.StartsWith("Limb"))
				{
					_bodyPartStrings = new string[2]
					{
						stringArg + " Right",
						stringArg + " Left"
					};
					_hasMirrorPart = true;
				}
				else
				{
					_bodyPartStrings = new string[1] { stringArg };
					_hasMirrorPart = false;
				}
				int num = ((!_hasMirrorPart) ? 1 : 2);
				_dragParts = new GameObject[num];
				for (int i = 0; i < num; i++)
				{
					_dragParts[i] = BlocksterBody.CreateBodyPartObject(_bodyPartStrings[i]);
					if (_dragParts[i] == null)
					{
						BWLog.Error("Failed to load body part " + _bodyPartStrings[i]);
						EnterState(GestureState.Failed);
						return;
					}
				}
				string currentHeadColor = _targetCharacter.GetCurrentHeadColor();
				string currentBodyColor = _targetCharacter.GetCurrentBodyColor();
				GameObject[] dragParts = _dragParts;
				foreach (GameObject gameObject in dragParts)
				{
					BodyPartInfo[] componentsInChildren = gameObject.GetComponentsInChildren<BodyPartInfo>();
					BodyPartInfo[] array = componentsInChildren;
					foreach (BodyPartInfo bodyPartInfo in array)
					{
						MeshRenderer component = bodyPartInfo.gameObject.GetComponent<MeshRenderer>();
						switch (bodyPartInfo.colorGroup)
						{
						case BodyPartInfo.ColorGroup.SkinFace:
						case BodyPartInfo.ColorGroup.SkinRightArm:
						case BodyPartInfo.ColorGroup.SkinLeftArm:
						case BodyPartInfo.ColorGroup.SkinRightHand:
						case BodyPartInfo.ColorGroup.SkinLeftHand:
						case BodyPartInfo.ColorGroup.SkinRightLeg:
						case BodyPartInfo.ColorGroup.SkinLeftLeg:
							component.sharedMaterial = Materials.GetMaterial(currentHeadColor, "Plain", ShaderType.Normal);
							continue;
						case BodyPartInfo.ColorGroup.Shirt:
							component.sharedMaterial = Materials.GetMaterial(currentBodyColor, "Plain", ShaderType.Normal);
							continue;
						}
						if (!string.IsNullOrEmpty(bodyPartInfo.defaultPaint))
						{
							component.sharedMaterial = Materials.GetMaterial(bodyPartInfo.defaultPaint, "Plain", ShaderType.Normal);
						}
					}
				}
				_targetParts = new BlocksterBody.BodyPart[num];
				_targetPartBounds = new Bounds[num];
				_originalBodyPartStrings = new string[num];
				for (int l = 0; l < num; l++)
				{
					BlocksterBody.BodyPart bodyPart = BlocksterBody.BodyPartFromString(_bodyPartStrings[l]);
					_targetParts[l] = bodyPart;
					List<BlocksterBody.Bone> bonesForBodyPart = BlocksterBody.GetBonesForBodyPart(bodyPart);
					Vector3 size = 0.25f * Vector3.one;
					Bounds bounds = new Bounds(_targetCharacter.GetBoneTransform(bonesForBodyPart[0]).position, size);
					for (int m = 0; m < bonesForBodyPart.Count; m++)
					{
						Transform boneTransform = _targetCharacter.GetBoneTransform(bonesForBodyPart[m]);
						bounds.Encapsulate(new Bounds(boneTransform.position, size));
						List<GameObject> objectsForBone = _targetCharacter.bodyParts.GetObjectsForBone(bonesForBodyPart[m]);
						foreach (GameObject item in objectsForBone)
						{
							MeshRenderer component2 = item.GetComponent<MeshRenderer>();
							if (component2 != null)
							{
								bounds.Encapsulate(component2.bounds);
							}
						}
					}
					_targetPartBounds[l] = bounds;
					_originalBodyPartStrings[l] = _targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
					_targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.Glass);
				}
				_dragTile = tile.Clone();
				Drag(position);
				Sound.PlaySound("Tile Start Drag", Sound.GetOrCreateOneShotAudioSource(), oneShot: true, 0.2f);
				EnterState(GestureState.Active);
			}
			else
			{
				EnterState(GestureState.Possible);
			}
		}
	}

	public override void TouchesMoved(List<Touch> allTouches)
	{
		Vector2 position = allTouches[0].Position;
		Drag(position);
	}

	public override void TouchesEnded(List<Touch> allTouches)
	{
		Reset();
	}

	public override void Cancel()
	{
		_bodyPartGAF = null;
		_replacedBodyPartGAF = null;
		base.Cancel();
	}

	public override void Reset()
	{
		if (_targetCharacter != null && _targetCharacter.go != null && _targetParts != null)
		{
			BlocksterBody.BodyPart[] targetParts = _targetParts;
			foreach (BlocksterBody.BodyPart bodyPart in targetParts)
			{
				_targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.Normal);
			}
		}
		if (_dragParts != null)
		{
			GameObject[] dragParts = _dragParts;
			foreach (GameObject gameObject in dragParts)
			{
				if (gameObject != null)
				{
					Object.Destroy(gameObject);
				}
			}
			_dragParts = null;
		}
		if (_dragTile != null)
		{
			_dragTile.Destroy();
			_dragTile = null;
		}
		_bodyPartStrings = null;
		if (_partIsSnapped)
		{
			_targetCharacter.SaveBodyPartAssignmentToTiles(_snappedToPart);
			History.AddStateIfNecessary();
		}
		_partIsSnapped = false;
		_bodyPartGAF = null;
		_replacedBodyPartGAF = null;
		EnterState(GestureState.Possible);
	}

	private void Drag(Vector2 pos)
	{
		Vector3 pos2 = pos - NormalizedScreen.pixelScale * 0.5f * 80f * Vector2.one;
		pos2.z = -0.5f;
		_dragTile.MoveTo(pos2);
		Vector2 vector = pos * NormalizedScreen.scale;
		Ray ray = Blocksworld.mainCamera.ScreenPointToRay(vector);
		int num = 0;
		int num2 = 1;
		if (_hasMirrorPart)
		{
			num2 = 2;
			new Plane(_targetCharacter.goT.forward, _targetCharacter.goT.position).Raycast(ray, out var enter);
			Vector3 vector2 = ray.origin + ray.direction * enter;
			float num3 = Vector3.Dot(_targetCharacter.goT.right, vector2 - _targetCharacter.goT.position);
			if (num3 < 0f)
			{
				num = 1;
			}
		}
		GameObject gameObject = _dragParts[num];
		BlocksterBody.BodyPart bodyPart = _targetParts[num];
		Bounds bounds = _targetPartBounds[num];
		string partStr = _bodyPartStrings[num];
		string partStr2 = _originalBodyPartStrings[num];
		Vector3 vector3 = Blocksworld.mainCamera.transform.InverseTransformPoint(bounds.center);
		float num4 = vector3.z;
		if (new Plane(Vector3.up, _targetCharacter.goT.position - Vector3.up).Raycast(ray, out var enter2))
		{
			num4 = Mathf.Min(enter2, vector3.z);
		}
		num4 -= 0.05f;
		Vector3 position = new Vector3(vector.x, vector.y, num4);
		Vector3 position2 = Blocksworld.mainCamera.ScreenToWorldPoint(position);
		for (int i = 0; i < num2; i++)
		{
			_dragParts[i].transform.position = position2;
			_dragParts[i].transform.rotation = _targetCharacter.GetRotation();
			if (i != num)
			{
				_dragParts[i].SetActive(value: false);
			}
		}
		bool flag = bounds.IntersectRay(ray);
		bool flag2 = false;
		string bodyPartStr = string.Empty;
		if (flag)
		{
			if (!_partIsSnapped)
			{
				bodyPartStr = _targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
				_targetCharacter.SubstituteBodyPart(bodyPart, partStr, applyDefaultPaints: true);
				_partIsSnapped = true;
				_targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.PulsateGlow);
				flag2 = true;
			}
			else if (_snappedToPart != bodyPart)
			{
				bodyPartStr = _targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
				_targetCharacter.SubstituteBodyPart(bodyPart, partStr, applyDefaultPaints: true);
				_targetCharacter.SubstituteBodyPart(_snappedToPart, _originalBodyPartStrings[_snappedToDragPartIndex], applyDefaultPaints: true);
				_targetCharacter.SetShaderForBodyPart(_snappedToPart, ShaderType.Glass);
				flag2 = true;
			}
		}
		else if (!flag && _partIsSnapped)
		{
			bodyPartStr = _targetCharacter.CurrentlyAssignedPartVersion(bodyPart);
			_targetCharacter.SubstituteBodyPart(bodyPart, partStr2, applyDefaultPaints: true);
			_partIsSnapped = false;
			_targetCharacter.SetShaderForBodyPart(bodyPart, ShaderType.Glass);
			flag2 = true;
		}
		if (flag2)
		{
			_replacedBodyPartGAF = GAFForBodyPartStr(bodyPartStr);
			Scarcity.inventoryScales[_bodyPartGAF] = 1.5f;
			Scarcity.UpdateInventory();
		}
		if (_partIsSnapped)
		{
			_snappedToDragPartIndex = num;
			_snappedToPart = bodyPart;
		}
		if (_buildPanel.Hit(pos))
		{
			gameObject.SetActive(value: false);
			_dragTile.Show(!flag);
			_dragTile.MoveTo(pos2);
		}
		else
		{
			gameObject.SetActive(!flag);
			_dragTile.Show(show: false);
		}
	}

	private GAF GAFForBodyPartStr(string bodyPartStr)
	{
		if (bodyPartStr.EndsWith(" Left"))
		{
			bodyPartStr = bodyPartStr.Remove(bodyPartStr.Length - 5, 5);
		}
		else if (bodyPartStr.EndsWith(" Right"))
		{
			bodyPartStr = bodyPartStr.Remove(bodyPartStr.Length - 6, 6);
		}
		return new GAF("AnimCharacter.ReplaceBodyPart", bodyPartStr);
	}

	public static HashSet<GAF> GetScarcityHighlightGafs(HashSet<GAF> result)
	{
		if (_bodyPartGAF != null)
		{
			if (result == null)
			{
				result = new HashSet<GAF>();
			}
			result.Add(_bodyPartGAF);
			if (_replacedBodyPartGAF != null)
			{
				result.Add(_replacedBodyPartGAF);
			}
		}
		return result;
	}
}
