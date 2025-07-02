using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

public class StepByStepAutoPlayer
{
	public float autoPlaySpeedup = 10f;

	private bool autoPlaying;

	private bool batchAutoPlaying;

	private bool paused;

	public string storeRelativePath = "/../../blocksworld-store-manager/data/dynamic/sets/";

	private TutorialState prevState;

	private int prevStateSameCount;

	private int prevTutorialStep = -1;

	private float tutorialStepStartTime;

	private List<string> worldPaths = new List<string>();

	private List<HashSet<TutorialState>> equivalentStates = new List<HashSet<TutorialState>>
	{
		new HashSet<TutorialState>
		{
			TutorialState.Position,
			TutorialState.TapBlock
		}
	};

	public void Reset()
	{
		autoPlaying = false;
		batchAutoPlaying = false;
	}

	public bool IsActive()
	{
		if (!autoPlaying)
		{
			return batchAutoPlaying;
		}
		return true;
	}

	public bool IsBatchPlaying()
	{
		return batchAutoPlaying;
	}

	public bool IsPlaying()
	{
		return autoPlaying;
	}

	public bool IsPaused()
	{
		return paused;
	}

	public void StartAutoPlay()
	{
		autoPlaying = true;
		paused = false;
		worldPaths.Clear();
		Blocksworld.bw.StartCoroutine(AutoPlayUntilDoneOrFail());
	}

	public void StopAutoPlay()
	{
		autoPlaying = false;
		paused = false;
	}

	public void ResetAutoPlaySpeedup()
	{
		Tutorial.autoCameraDelay = 0.5f;
		Tutorial.autoCameraTweenTimeMultiplier = 1f;
	}

	public void SetAutoPlaySpeedup(float speedup)
	{
		Tutorial.autoCameraDelay = 1f / speedup;
		Tutorial.autoCameraTweenTimeMultiplier = 1f / speedup;
	}

	private IEnumerator AutoPlayUntilDoneOrFail()
	{
		OnScreenLog.Clear();
		yield return new WaitForSeconds(0.1f);
		if (BW.isUnityEditor)
		{
			Application.runInBackground = true;
		}
		bool runOnce = true;
		if (worldPaths.Count > 0)
		{
			runOnce = false;
		}
		int worldIndex = 0;
		int puzzleWorldCount = 0;
		int failCount = 0;
		int hardFailCount = 0;
		SetAutoPlaySpeedup(autoPlaySpeedup);
		while ((autoPlaying || batchAutoPlaying) && (runOnce || worldIndex < worldPaths.Count))
		{
			if (Tutorial.state == TutorialState.None)
			{
				if (Blocksworld.CurrentState == State.Play)
				{
					Blocksworld.stopASAP = true;
					yield return new WaitForSeconds(0.5f);
				}
				Tutorial.Start();
			}
			yield return new WaitForSeconds(0.5f);
			Blocksworld.UI.SidePanel.Show();
			prevTutorialStep = -2;
			tutorialStepStartTime = Time.time;
			bool done = false;
			bool failed = false;
			while (!done && !failed && (autoPlaying || batchAutoPlaying))
			{
				if (paused)
				{
					yield return new WaitForSeconds(0.5f);
					tutorialStepStartTime = Time.time;
				}
				else if (Blocksworld.recognizer.gestureCommands.Count <= 0)
				{
					Arrow arrow = Tutorial.arrow1;
					Arrow arrow2 = Tutorial.arrow2;
					float num = Time.time - tutorialStepStartTime;
					float num2 = 30f;
					if ((Tutorial.state == TutorialState.Texture || Tutorial.state == TutorialState.Color) && arrow.state == TrackingState.Tile2Screen)
					{
						num2 = 60f;
					}
					if (num > num2)
					{
						Tutorial.cheatNextStep = true;
						Tutorial.stepOnNextUpdate = true;
						OnScreenLog.AddLogItem(string.Concat("A step was taking to long. Tutorial state: ", Tutorial.state, " Step: ", Tutorial.step), 180f, log: true);
						tutorialStepStartTime = Time.time;
						failCount++;
						yield return new WaitForSeconds(Tutorial.autoCameraDelay * 3f);
					}
					switch (Tutorial.state)
					{
					case TutorialState.GetNextBlock:
					case TutorialState.DetermineInstructions:
					case TutorialState.Orbit:
					case TutorialState.Waiting:
						yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
						break;
					case TutorialState.Scroll:
						if (prevStateSameCount > 20 && Tutorial.scrollToTile != null)
						{
							OnScreenLog.AddLogItem("Failed to scroll to tile with GAF " + Tutorial.scrollToTile.gaf, 10f, log: true);
							Tutorial.cheatNextStep = true;
							Tutorial.stepOnNextUpdate = true;
							tutorialStepStartTime = Time.time;
							failCount++;
							prevStateSameCount = 0;
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 3f);
						}
						else if (Tutorial.scrollToTile == null || arrow.state != TrackingState.Screen2Screen)
						{
							Tutorial.stepOnNextUpdate = true;
							OnScreenLog.AddLogItem("In Scroll state but failing. Scroll tile null: " + (Tutorial.scrollToTile == null) + ", " + arrow.state, 10f, log: true);
							yield return new WaitForSeconds(0.5f);
						}
						else
						{
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2, 0.02f * (float)prevStateSameCount));
							if (prevStateSameCount > 7)
							{
								Tutorial.stepOnNextUpdate = true;
								yield return new WaitForSeconds(0.5f);
							}
							else if (prevStateSameCount > 4)
							{
								yield return new WaitForSeconds(0.5f);
							}
						}
						break;
					case TutorialState.DragBlockPanel:
						Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2, 0.05f));
						break;
					case TutorialState.TapBlock:
					case TutorialState.SelectBunch:
						if (prevStateSameCount > 10)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
						}
						else
						{
							Bounds colliderBounds;
							Vector3 blockCenter = GetBlockCenter(Tutorial.target1.block, out colliderBounds);
							Vector3 vector4 = Util.WorldToScreenPoint(blockCenter, z: false);
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(vector4, vector4));
						}
						break;
					case TutorialState.TapTile:
					{
						TileObject tile = Tutorial.target1.tile;
						Vector3 tapPos = tile.GetPosition() + new Vector3(40f, 40f, 0f);
						yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.5f * (float)prevStateSameCount);
						Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(tapPos, tapPos, 0.02f * (float)prevStateSameCount));
						if (prevStateSameCount > 5)
						{
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.5f * (float)prevStateSameCount);
						}
						if (prevStateSameCount > 10)
						{
							OnScreenLog.AddLogItem("Failed tap tile. Stepping tutorial manually.", 5f, log: true);
							Tutorial.stepOnNextUpdate = true;
							prevStateSameCount = 0;
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
						}
						break;
					}
					case TutorialState.TapTab:
					{
						Vector3 screen = Tutorial.target1.screen;
						Blocksworld.UI.TabBar.SelectTabAtScreenPos(screen);
						yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
						Tutorial.stepOnNextUpdate = true;
						break;
					}
					case TutorialState.CreateBlock:
						if (prevStateSameCount > 5)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
						}
						else
						{
							float num3 = 80f;
							Vector3 vector6 = new Vector3(Random.Range(0f - num3, num3), Random.Range(0f - num3, num3), 0f);
							Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(arrow.world, z: false) + vector6, 0.1f * (float)prevStateSameCount, Mathf.Max(0.3f - (float)prevStateSameCount * 0.03f, 0.02f)));
						}
						break;
					case TutorialState.DestroyBlock:
						if (prevStateSameCount > 5)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
						}
						else
						{
							Bounds colliderBounds2;
							Vector3 blockCenter2 = GetBlockCenter(arrow.block, out colliderBounds2);
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(Util.WorldToScreenPoint(blockCenter2, z: false), arrow.screen));
						}
						break;
					case TutorialState.Rotation:
						if (prevStateSameCount > 10)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
						}
						else
						{
							Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.screen, 0.1f * (float)prevStateSameCount));
						}
						break;
					case TutorialState.Scale:
						if (prevStateSameCount > 10)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
							break;
						}
						Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(arrow.world, z: false), 0.1f * (float)prevStateSameCount));
						if (arrow2.IsShowing())
						{
							float startDelay = 0.03f;
							yield return new WaitForSeconds(startDelay);
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow2.screen, arrow2.screen2, 0.05f * (float)prevStateSameCount - startDelay));
						}
						break;
					case TutorialState.Position:
						if (prevStateSameCount > 10)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
							break;
						}
						switch (arrow.state)
						{
						default:
							Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(arrow.world, z: false), 0.05f * (float)prevStateSameCount));
							if (arrow2.IsShowing())
							{
								float startDelay = 0.03f;
								yield return new WaitForSeconds(startDelay);
								Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow2.screen, arrow2.screen2, 0.05f * (float)prevStateSameCount - startDelay));
							}
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.1f * (float)prevStateSameCount);
							break;
						case TrackingState.BlockOffsetWorld:
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(Util.WorldToScreenPoint(arrow.block.GetPosition(), z: false), Util.WorldToScreenPoint(arrow.world, z: false), 0.01f * (float)prevStateSameCount));
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.2f * (float)prevStateSameCount);
							break;
						case TrackingState.MoveButtonHelper:
						{
							Vector3 centerPosition = TBox.tileButtonMove.GetCenterPosition();
							Vector3 vector3 = Util.WorldToScreenPointSafe(arrow.world) + arrow.offset;
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(centerPosition, vector3, 0.05f * (float)prevStateSameCount));
							break;
						}
						case TrackingState.Block2World:
						{
							Vector3 vector = Util.WorldToScreenPoint(arrow.block.GetPosition(), z: false);
							Vector3 vector2 = Util.WorldToScreenPoint(arrow.world, z: false);
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(vector, vector2, 0.05f * (float)prevStateSameCount));
							break;
						}
						}
						break;
					case TutorialState.Color:
						if (prevStateSameCount > 3)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
						}
						else
						{
							AddPaintOrTextureCommand();
						}
						break;
					case TutorialState.Texture:
						if (prevStateSameCount > 3)
						{
							AddOrbitCommand();
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							prevStateSameCount = 0;
						}
						else
						{
							AddPaintOrTextureCommand();
						}
						break;
					case TutorialState.RemoveTile:
						Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.screen, 0.05f));
						if (prevStateSameCount > 5)
						{
							OnScreenLog.AddLogItem("Failed remove tile. Stepping tutorial manually.", 5f, log: true);
							prevStateSameCount = 0;
							Tutorial.stepOnNextUpdate = true;
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
						}
						break;
					case TutorialState.SwapTiles:
					{
						Vector3 vector5 = arrow.tile.GetPosition() + new Vector3(40f, 40f, 0f);
						Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(vector5, vector5 + arrow.offset));
						break;
					}
					case TutorialState.AddTile:
						if (BW.isUnityEditor && prevStateSameCount > 0 && Blocksworld.CurrentState == State.EditTile)
						{
							EditableTileParameter parameter2 = Blocksworld.bw.tileParameterEditor.parameter;
							if (parameter2 is StringTileParameter)
							{
								StringTileParameter stringTileParameter2 = (StringTileParameter)parameter2;
								stringTileParameter2.ForceQuit();
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							}
						}
						if (arrow.state == TrackingState.ScreenPanelOffset)
						{
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.panel.position + arrow.offset + 0.5f * Vector3.right * 75f, Tutorial.autoCameraDelay * 0.2f * (float)prevStateSameCount));
						}
						else
						{
							Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.panel.position + arrow.offset + 0.5f * Vector3.right * 75f, Tutorial.autoCameraDelay * 0.2f * (float)prevStateSameCount));
						}
						if (prevStateSameCount > 5)
						{
							OnScreenLog.AddLogItem("Failed add tile. Stepping tutorial manually.", 5f, log: true);
							prevStateSameCount = 0;
							Tutorial.stepOnNextUpdate = true;
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
						}
						break;
					case TutorialState.SetParameter:
					{
						Tile selectedTile = Blocksworld.bw.tileParameterEditor.selectedTile;
						if (prevStateSameCount > 1 && selectedTile != null && !selectedTile.doubleWidth)
						{
							yield return new WaitForSeconds(0.5f);
							EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
							if (parameter is StringTileParameter)
							{
								StringTileParameter stringTileParameter = (StringTileParameter)parameter;
								stringTileParameter.ForceQuit();
							}
							break;
						}
						Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2));
						if (prevStateSameCount > 10)
						{
							OnScreenLog.AddLogItem("Failed set parameter. Stepping tutorial manually.", 5f, log: true);
							Tutorial.stepOnNextUpdate = true;
							prevStateSameCount = 0;
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
						}
						break;
					}
					case TutorialState.BuildingCompleted:
						done = true;
						if (runOnce)
						{
							BWLog.Info("Building completed.");
						}
						else
						{
							BWLog.Info("Building completed. World " + (worldIndex + 1) + " out of " + worldPaths.Count + ". Skipped " + puzzleWorldCount + " puzzle worlds so far.");
						}
						yield return new WaitForSeconds(0.1f);
						break;
					case TutorialState.Play:
						BWLog.Info("World entered play mode. Skipping.");
						done = true;
						break;
					default:
						failed = true;
						hardFailCount++;
						OnScreenLog.AddLogItem("Failed build: Can not cheat in state " + Tutorial.state.ToString() + " yet", 180f, log: true);
						yield return new WaitForSeconds(0.1f);
						break;
					}
					if (TutorialActions.AnyActionBlocksProgress())
					{
						TutorialActions.StopFirstBlockingAction();
					}
					if (CountAsSameState(prevState, Tutorial.state))
					{
						prevStateSameCount++;
						if (prevStateSameCount > 2)
						{
							yield return new WaitForSeconds((float)prevStateSameCount * 0.02f);
						}
					}
					else
					{
						prevStateSameCount = 0;
					}
					if (Tutorial.step != prevTutorialStep)
					{
						tutorialStepStartTime = Time.time;
					}
					prevTutorialStep = Tutorial.step;
					prevState = Tutorial.state;
				}
				else
				{
					yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.1f);
				}
			}
			if (runOnce)
			{
				break;
			}
		}
		if (BW.isUnityEditor)
		{
			Application.runInBackground = false;
		}
		ResetAutoPlaySpeedup();
		autoPlaying = false;
		batchAutoPlaying = false;
	}

	private bool CountAsSameState(TutorialState s1, TutorialState s2)
	{
		if (s1 != s2)
		{
			return equivalentStates.Exists((HashSet<TutorialState> hashSet) => hashSet.Contains(s1) && hashSet.Contains(s2));
		}
		return true;
	}

	private Vector3 GetBlockCenter(Block block, out Bounds colliderBounds)
	{
		if (block.go == null)
		{
			Transform cameraTransform = Blocksworld.cameraTransform;
			Vector3 vector = cameraTransform.position + cameraTransform.forward * 15f;
			colliderBounds = new Bounds(vector, Vector3.one);
			return vector;
		}
		Transform transform = block.go.transform;
		Collider component = block.go.GetComponent<Collider>();
		colliderBounds = component.bounds;
		Vector3 result = colliderBounds.center;
		MeshFilter component2 = block.go.GetComponent<MeshFilter>();
		if (component2 != null)
		{
			Mesh sharedMesh = component2.sharedMesh;
			result = GetMeshCenter(transform, sharedMesh);
		}
		else if (component is MeshCollider)
		{
			MeshCollider meshCollider = (MeshCollider)component;
			Mesh sharedMesh2 = meshCollider.sharedMesh;
			result = GetMeshCenter(transform, sharedMesh2);
		}
		return result;
	}

	private void AddOrbitCommand(float length = 0.15f, float speed = 0.2f, float endDelay = 0.1f)
	{
		Vector2 vector = new Vector2((float)NormalizedScreen.width * 0.4f, 50f);
		Vector2 startPos = vector;
		Vector2 vector2 = vector;
		DragGestureCommand dragGestureCommand = new DragGestureCommand(startPos, vector2 + new Vector2(1f, Random.Range(-0.5f, 0.5f)).normalized * NormalizedScreen.width * length, endDelay);
		dragGestureCommand.speed = speed;
		Blocksworld.recognizer.gestureCommands.Add(dragGestureCommand);
	}

	private void AddPaintOrTextureCommand()
	{
		Arrow arrow = Tutorial.arrow1;
		ScriptPanel scriptPanel = Blocksworld.scriptPanel;
		if (scriptPanel.IsShowing())
		{
			Vector3 position = scriptPanel.position;
			if (position.x + scriptPanel.width > 100f)
			{
				Vector2 vector = new Vector2(position.x + scriptPanel.width - 40f, position.y + scriptPanel.height * 0.5f);
				Vector2 targetPos = vector - Vector2.right * (position.x + scriptPanel.width - 80f);
				Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(vector, targetPos, 0.05f));
				return;
			}
		}
		switch (arrow.state)
		{
		case TrackingState.Screen2World:
			Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, Util.WorldToScreenPoint(arrow.world, z: false) + Random.insideUnitSphere * 60f, 0.1f));
			break;
		case TrackingState.Screen2Screen:
			Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2 + Random.insideUnitSphere * 60f, 0.7f));
			break;
		case TrackingState.Screen2Block:
		{
			Vector3 randomBlockPos2 = GetRandomBlockPos(arrow.block);
			Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, Util.WorldToScreenPoint(randomBlockPos2, z: false), 0.1f));
			break;
		}
		default:
			BWLog.Info("Don't know how to deal with state " + arrow.state.ToString() + " when painting and texturing");
			break;
		case TrackingState.Tile2Screen:
			Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.screen, 0.7f));
			break;
		case TrackingState.TileWorld:
		{
			Vector3 world = arrow.world;
			for (int i = 0; i < 3; i++)
			{
				world[i] += Random.Range(-0.5f, 0.5f);
			}
			Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(world, z: false), 0.1f));
			break;
		}
		case TrackingState.TileBlock:
		{
			Vector3 randomBlockPos = GetRandomBlockPos(arrow.block);
			Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(randomBlockPos, z: false), 0.1f));
			break;
		}
		}
	}

	private Vector3 GetRandomBlockPos(Block block)
	{
		Bounds colliderBounds;
		Vector3 blockCenter = GetBlockCenter(block, out colliderBounds);
		Vector3 extents = colliderBounds.extents;
		for (int i = 0; i < 3; i++)
		{
			float num = Mathf.Clamp(extents[i], 0.2f, 0.5f);
			blockCenter[i] += Random.Range(0f - num, num);
		}
		return blockCenter;
	}

	private Vector3 GetMeshCenter(Transform t, Mesh mesh)
	{
		Vector3 zero = Vector3.zero;
		Vector3[] vertices = mesh.vertices;
		if (vertices.Length != 0)
		{
			zero = Vector3.zero;
			Vector3[] array = vertices;
			foreach (Vector3 position in array)
			{
				zero += t.TransformPoint(position) / vertices.Length;
			}
		}
		return zero;
	}
}
