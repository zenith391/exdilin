using System;
using System.Collections;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x020002B3 RID: 691
public class StepByStepAutoPlayer
{
	// Token: 0x06001FD5 RID: 8149 RVA: 0x000E5296 File Offset: 0x000E3696
	public void Reset()
	{
		this.autoPlaying = false;
		this.batchAutoPlaying = false;
	}

	// Token: 0x06001FD6 RID: 8150 RVA: 0x000E52A6 File Offset: 0x000E36A6
	public bool IsActive()
	{
		return this.autoPlaying || this.batchAutoPlaying;
	}

	// Token: 0x06001FD7 RID: 8151 RVA: 0x000E52BC File Offset: 0x000E36BC
	public bool IsBatchPlaying()
	{
		return this.batchAutoPlaying;
	}

	// Token: 0x06001FD8 RID: 8152 RVA: 0x000E52C4 File Offset: 0x000E36C4
	public bool IsPlaying()
	{
		return this.autoPlaying;
	}

	// Token: 0x06001FD9 RID: 8153 RVA: 0x000E52CC File Offset: 0x000E36CC
	public bool IsPaused()
	{
		return this.paused;
	}

	// Token: 0x06001FDA RID: 8154 RVA: 0x000E52D4 File Offset: 0x000E36D4
	public void StartAutoPlay()
	{
		this.autoPlaying = true;
		this.paused = false;
		this.worldPaths.Clear();
		Blocksworld.bw.StartCoroutine(this.AutoPlayUntilDoneOrFail());
	}

	// Token: 0x06001FDB RID: 8155 RVA: 0x000E5300 File Offset: 0x000E3700
	public void StopAutoPlay()
	{
		this.autoPlaying = false;
		this.paused = false;
	}

	// Token: 0x06001FDC RID: 8156 RVA: 0x000E5310 File Offset: 0x000E3710
	public void ResetAutoPlaySpeedup()
	{
		Tutorial.autoCameraDelay = 0.5f;
		Tutorial.autoCameraTweenTimeMultiplier = 1f;
	}

	// Token: 0x06001FDD RID: 8157 RVA: 0x000E5326 File Offset: 0x000E3726
	public void SetAutoPlaySpeedup(float speedup)
	{
		Tutorial.autoCameraDelay = 1f / speedup;
		Tutorial.autoCameraTweenTimeMultiplier = 1f / speedup;
	}

	// Token: 0x06001FDE RID: 8158 RVA: 0x000E5340 File Offset: 0x000E3740
	private IEnumerator AutoPlayUntilDoneOrFail()
	{
		OnScreenLog.Clear();
		yield return new WaitForSeconds(0.1f);
		if (BW.isUnityEditor)
		{
			Application.runInBackground = true;
		}
		bool runOnce = true;
		if (this.worldPaths.Count > 0)
		{
			runOnce = false;
		}
		int worldIndex = 0;
		int puzzleWorldCount = 0;
		int failCount = 0;
		int hardFailCount = 0;
		this.SetAutoPlaySpeedup(this.autoPlaySpeedup);
		while ((this.autoPlaying || this.batchAutoPlaying) && (runOnce || worldIndex < this.worldPaths.Count))
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
			this.prevTutorialStep = -2;
			this.tutorialStepStartTime = Time.time;
			bool done = false;
			bool failed = false;
			while (!done && !failed && (this.autoPlaying || this.batchAutoPlaying))
			{
				if (this.paused)
				{
					yield return new WaitForSeconds(0.5f);
					this.tutorialStepStartTime = Time.time;
				}
				else
				{
					if (Blocksworld.recognizer.gestureCommands.Count <= 0)
					{
						Arrow arrow = Tutorial.arrow1;
						Arrow arrow2 = Tutorial.arrow2;
						float stepTime = Time.time - this.tutorialStepStartTime;
						float maxStepTime = 30f;
						if ((Tutorial.state == TutorialState.Texture || Tutorial.state == TutorialState.Color) && arrow.state == TrackingState.Tile2Screen)
						{
							maxStepTime = 60f;
						}
						if (stepTime > maxStepTime)
						{
							Tutorial.cheatNextStep = true;
							Tutorial.stepOnNextUpdate = true;
							OnScreenLog.AddLogItem(string.Concat(new object[]
							{
								"A step was taking to long. Tutorial state: ",
								Tutorial.state,
								" Step: ",
								Tutorial.step
							}), 180f, true);
							this.tutorialStepStartTime = Time.time;
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
						case TutorialState.SpeechButtonPressed:
							goto IL_15C5;
						case TutorialState.Scroll:
							if (this.prevStateSameCount > 20 && Tutorial.scrollToTile != null)
							{
								OnScreenLog.AddLogItem("Failed to scroll to tile with GAF " + Tutorial.scrollToTile.gaf, 10f, true);
								Tutorial.cheatNextStep = true;
								Tutorial.stepOnNextUpdate = true;
								this.tutorialStepStartTime = Time.time;
								failCount++;
								this.prevStateSameCount = 0;
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 3f);
							}
							else if (Tutorial.scrollToTile == null || arrow.state != TrackingState.Screen2Screen)
							{
								Tutorial.stepOnNextUpdate = true;
								OnScreenLog.AddLogItem(string.Concat(new object[]
								{
									"In Scroll state but failing. Scroll tile null: ",
									Tutorial.scrollToTile == null,
									", ",
									arrow.state
								}), 10f, true);
								yield return new WaitForSeconds(0.5f);
							}
							else
							{
								Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2, 0.02f * (float)this.prevStateSameCount));
								if (this.prevStateSameCount > 7)
								{
									Tutorial.stepOnNextUpdate = true;
									yield return new WaitForSeconds(0.5f);
								}
								else if (this.prevStateSameCount > 4)
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
							if (this.prevStateSameCount > 10)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								Bounds bounds;
								Vector3 blockCenter = this.GetBlockCenter(Tutorial.target1.block, out bounds);
								Vector3 v = Util.WorldToScreenPoint(blockCenter, false);
								Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(v, v, 0f));
							}
							break;
						case TutorialState.TapTile:
						{
							TileObject tapTile = Tutorial.target1.tile;
							Vector3 tapPos = tapTile.GetPosition() + new Vector3(40f, 40f, 0f);
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.5f * (float)this.prevStateSameCount);
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(tapPos, tapPos, 0.02f * (float)this.prevStateSameCount));
							if (this.prevStateSameCount > 5)
							{
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.5f * (float)this.prevStateSameCount);
							}
							if (this.prevStateSameCount > 10)
							{
								OnScreenLog.AddLogItem("Failed tap tile. Stepping tutorial manually.", 5f, true);
								Tutorial.stepOnNextUpdate = true;
								this.prevStateSameCount = 0;
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							}
							break;
						}
						case TutorialState.TapTab:
						{
							Vector3 target = Tutorial.target1.screen;
							Blocksworld.UI.TabBar.SelectTabAtScreenPos(target);
							yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							Tutorial.stepOnNextUpdate = true;
							break;
						}
						case TutorialState.CreateBlock:
							if (this.prevStateSameCount > 5)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								float num = 80f;
								Vector3 b = new Vector3(UnityEngine.Random.Range(-num, num), UnityEngine.Random.Range(-num, num), 0f);
								Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(arrow.world, false) + b, 0.1f * (float)this.prevStateSameCount, Mathf.Max(0.3f - (float)this.prevStateSameCount * 0.03f, 0.02f)));
							}
							break;
						case TutorialState.DestroyBlock:
							if (this.prevStateSameCount > 5)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								Bounds bounds2;
								Vector3 blockCenter2 = this.GetBlockCenter(arrow.block, out bounds2);
								Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(Util.WorldToScreenPoint(blockCenter2, false), arrow.screen, 0f));
							}
							break;
						case TutorialState.Rotation:
							if (this.prevStateSameCount > 10)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.screen, 0.1f * (float)this.prevStateSameCount, 0.3f));
							}
							break;
						case TutorialState.Scale:
							if (this.prevStateSameCount > 10)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(arrow.world, false), 0.1f * (float)this.prevStateSameCount, 0.3f));
								if (arrow2.IsShowing())
								{
									float startDelay = 0.03f;
									yield return new WaitForSeconds(startDelay);
									Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow2.screen, arrow2.screen2, 0.05f * (float)this.prevStateSameCount - startDelay));
								}
							}
							break;
						case TutorialState.Position:
							if (this.prevStateSameCount > 10)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								TrackingState state = arrow.state;
								if (state != TrackingState.Block2World)
								{
									if (state != TrackingState.MoveButtonHelper)
									{
										if (state != TrackingState.BlockOffsetWorld)
										{
											Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(arrow.world, false), 0.05f * (float)this.prevStateSameCount, 0.3f));
											if (arrow2.IsShowing())
											{
												float startDelay2 = 0.03f;
												yield return new WaitForSeconds(startDelay2);
												Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow2.screen, arrow2.screen2, 0.05f * (float)this.prevStateSameCount - startDelay2));
											}
											yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.1f * (float)this.prevStateSameCount);
										}
										else
										{
											Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(Util.WorldToScreenPoint(arrow.block.GetPosition(), false), Util.WorldToScreenPoint(arrow.world, false), 0.01f * (float)this.prevStateSameCount));
											yield return new WaitForSeconds(Tutorial.autoCameraDelay * 0.2f * (float)this.prevStateSameCount);
										}
									}
									else
									{
										Vector3 moveButtonPos = TBox.tileButtonMove.GetCenterPosition();
										Vector3 targetPos = Util.WorldToScreenPointSafe(arrow.world) + arrow.offset;
										Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(moveButtonPos, targetPos, 0.05f * (float)this.prevStateSameCount));
									}
								}
								else
								{
									Vector3 blockScreenPos = Util.WorldToScreenPoint(arrow.block.GetPosition(), false);
									Vector3 targetScreenPos = Util.WorldToScreenPoint(arrow.world, false);
									Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(blockScreenPos, targetScreenPos, 0.05f * (float)this.prevStateSameCount));
								}
							}
							break;
						case TutorialState.Color:
							if (this.prevStateSameCount > 3)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								this.AddPaintOrTextureCommand();
							}
							break;
						case TutorialState.Texture:
							if (this.prevStateSameCount > 3)
							{
								this.AddOrbitCommand(0.15f, 0.2f, 0.1f);
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								this.prevStateSameCount = 0;
							}
							else
							{
								this.AddPaintOrTextureCommand();
							}
							break;
						case TutorialState.RemoveTile:
							Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.screen, 0.05f, 0.3f));
							if (this.prevStateSameCount > 5)
							{
								OnScreenLog.AddLogItem("Failed remove tile. Stepping tutorial manually.", 5f, true);
								this.prevStateSameCount = 0;
								Tutorial.stepOnNextUpdate = true;
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							}
							break;
						case TutorialState.SwapTiles:
						{
							Vector3 vector = arrow.tile.GetPosition() + new Vector3(40f, 40f, 0f);
							Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(vector, vector + arrow.offset, 0f));
							break;
						}
						case TutorialState.AddTile:
							if (BW.isUnityEditor && this.prevStateSameCount > 0 && Blocksworld.CurrentState == State.EditTile)
							{
								EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
								if (parameter is StringTileParameter)
								{
									StringTileParameter stp = (StringTileParameter)parameter;
									stp.ForceQuit();
									yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								}
							}
							if (arrow.state == TrackingState.ScreenPanelOffset)
							{
								Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.panel.position + arrow.offset + 0.5f * Vector3.right * 75f, Tutorial.autoCameraDelay * 0.2f * (float)this.prevStateSameCount));
							}
							else
							{
								Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.panel.position + arrow.offset + 0.5f * Vector3.right * 75f, Tutorial.autoCameraDelay * 0.2f * (float)this.prevStateSameCount, 0.3f));
							}
							if (this.prevStateSameCount > 5)
							{
								OnScreenLog.AddLogItem("Failed add tile. Stepping tutorial manually.", 5f, true);
								this.prevStateSameCount = 0;
								Tutorial.stepOnNextUpdate = true;
								yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
							}
							break;
						case TutorialState.SetParameter:
						{
							Tile tile = Blocksworld.bw.tileParameterEditor.selectedTile;
							if (this.prevStateSameCount > 1 && tile != null && !tile.doubleWidth)
							{
								yield return new WaitForSeconds(0.5f);
								EditableTileParameter parameter2 = Blocksworld.bw.tileParameterEditor.parameter;
								if (parameter2 is StringTileParameter)
								{
									StringTileParameter stringTileParameter = (StringTileParameter)parameter2;
									stringTileParameter.ForceQuit();
								}
							}
							else
							{
								Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2, 0f));
								if (this.prevStateSameCount > 10)
								{
									OnScreenLog.AddLogItem("Failed set parameter. Stepping tutorial manually.", 5f, true);
									Tutorial.stepOnNextUpdate = true;
									this.prevStateSameCount = 0;
									yield return new WaitForSeconds(Tutorial.autoCameraDelay * 1.5f);
								}
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
								BWLog.Info(string.Concat(new object[]
								{
									"Building completed. World ",
									worldIndex + 1,
									" out of ",
									this.worldPaths.Count,
									". Skipped ",
									puzzleWorldCount,
									" puzzle worlds so far."
								}));
							}
							yield return new WaitForSeconds(0.1f);
							break;
						case TutorialState.Play:
							BWLog.Info("World entered play mode. Skipping.");
							done = true;
							break;
						default:
							goto IL_15C5;
						}
						IL_1628:
						if (TutorialActions.AnyActionBlocksProgress())
						{
							TutorialActions.StopFirstBlockingAction();
						}
						if (this.CountAsSameState(this.prevState, Tutorial.state))
						{
							this.prevStateSameCount++;
							if (this.prevStateSameCount > 2)
							{
								yield return new WaitForSeconds((float)this.prevStateSameCount * 0.02f);
							}
						}
						else
						{
							this.prevStateSameCount = 0;
						}
						if (Tutorial.step != this.prevTutorialStep)
						{
							this.tutorialStepStartTime = Time.time;
						}
						this.prevTutorialStep = Tutorial.step;
						this.prevState = Tutorial.state;
						continue;
						IL_15C5:
						failed = true;
						hardFailCount++;
						OnScreenLog.AddLogItem("Failed build: Can not cheat in state " + Tutorial.state + " yet", 180f, true);
						yield return new WaitForSeconds(0.1f);
						goto IL_1628;
					}
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
		this.ResetAutoPlaySpeedup();
		this.autoPlaying = false;
		this.batchAutoPlaying = false;
		yield break;
	}

	// Token: 0x06001FDF RID: 8159 RVA: 0x000E535C File Offset: 0x000E375C
	private bool CountAsSameState(TutorialState s1, TutorialState s2)
	{
		return s1 == s2 || this.equivalentStates.Exists((HashSet<TutorialState> s) => s.Contains(s1) && s.Contains(s2));
	}

	// Token: 0x06001FE0 RID: 8160 RVA: 0x000E53A8 File Offset: 0x000E37A8
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
			result = this.GetMeshCenter(transform, sharedMesh);
		}
		else if (component is MeshCollider)
		{
			MeshCollider meshCollider = (MeshCollider)component;
			Mesh sharedMesh2 = meshCollider.sharedMesh;
			result = this.GetMeshCenter(transform, sharedMesh2);
		}
		return result;
	}

	// Token: 0x06001FE1 RID: 8161 RVA: 0x000E5480 File Offset: 0x000E3880
	private void AddOrbitCommand(float length = 0.15f, float speed = 0.2f, float endDelay = 0.1f)
	{
		Vector2 vector = new Vector2((float)NormalizedScreen.width * 0.4f, 50f);
		Vector2 startPos = vector;
		Vector2 a = vector;
		Vector2 vector2 = new Vector2(1f, UnityEngine.Random.Range(-0.5f, 0.5f));
		DragGestureCommand dragGestureCommand = new DragGestureCommand(startPos, a + vector2.normalized * (float)NormalizedScreen.width * length, endDelay);
		dragGestureCommand.speed = speed;
		Blocksworld.recognizer.gestureCommands.Add(dragGestureCommand);
	}

	// Token: 0x06001FE2 RID: 8162 RVA: 0x000E5500 File Offset: 0x000E3900
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
		TrackingState state = arrow.state;
		switch (state)
		{
		case TrackingState.Screen2World:
			Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, Util.WorldToScreenPoint(arrow.world, false) + UnityEngine.Random.insideUnitSphere * 60f, 0.1f));
			break;
		case TrackingState.Screen2Screen:
			Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, arrow.screen2 + UnityEngine.Random.insideUnitSphere * 60f, 0.7f));
			break;
		case TrackingState.Screen2Block:
		{
			Vector3 worldPos = this.GetRandomBlockPos(arrow.block);
			Blocksworld.recognizer.gestureCommands.Add(new DragGestureCommand(arrow.screen, Util.WorldToScreenPoint(worldPos, false), 0.1f));
			break;
		}
		default:
			if (state != TrackingState.TileBlock)
			{
				if (state != TrackingState.TileWorld)
				{
					if (state != TrackingState.Tile2Screen)
					{
						BWLog.Info("Don't know how to deal with state " + arrow.state + " when painting and texturing");
					}
					else
					{
						Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, arrow.screen, 0.7f, 0.3f));
					}
				}
				else
				{
					Vector3 worldPos = arrow.world;
					for (int i = 0; i < 3; i++)
					{
						ref Vector3 ptr = ref worldPos;
						int index;
						worldPos[index = i] = ptr[index] + UnityEngine.Random.Range(-0.5f, 0.5f);
					}
					Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(worldPos, false), 0.1f, 0.3f));
				}
			}
			else
			{
				Vector3 worldPos = this.GetRandomBlockPos(arrow.block);
				Blocksworld.recognizer.gestureCommands.Add(new DragTileGestureCommand(arrow.tile, Util.WorldToScreenPoint(worldPos, false), 0.1f, 0.3f));
			}
			break;
		}
	}

	// Token: 0x06001FE3 RID: 8163 RVA: 0x000E57E4 File Offset: 0x000E3BE4
	private Vector3 GetRandomBlockPos(Block block)
	{
		Bounds bounds;
		Vector3 blockCenter = this.GetBlockCenter(block, out bounds);
		Vector3 extents = bounds.extents;
		for (int i = 0; i < 3; i++)
		{
			float num = Mathf.Clamp(extents[i], 0.2f, 0.5f);
			ref Vector3 ptr = ref blockCenter;
			int index;
			blockCenter[index = i] = ptr[index] + UnityEngine.Random.Range(-num, num);
		}
		return blockCenter;
	}

	// Token: 0x06001FE4 RID: 8164 RVA: 0x000E5854 File Offset: 0x000E3C54
	private Vector3 GetMeshCenter(Transform t, Mesh mesh)
	{
		Vector3 vector = Vector3.zero;
		Vector3[] vertices = mesh.vertices;
		if (vertices.Length > 0)
		{
			vector = Vector3.zero;
			foreach (Vector3 position in vertices)
			{
				vector += t.TransformPoint(position) / (float)vertices.Length;
			}
		}
		return vector;
	}

	// Token: 0x04001A03 RID: 6659
	public float autoPlaySpeedup = 10f;

	// Token: 0x04001A04 RID: 6660
	private bool autoPlaying;

	// Token: 0x04001A05 RID: 6661
	private bool batchAutoPlaying;

	// Token: 0x04001A06 RID: 6662
	private bool paused;

	// Token: 0x04001A07 RID: 6663
	public string storeRelativePath = "/../../blocksworld-store-manager/data/dynamic/sets/";

	// Token: 0x04001A08 RID: 6664
	private TutorialState prevState;

	// Token: 0x04001A09 RID: 6665
	private int prevStateSameCount;

	// Token: 0x04001A0A RID: 6666
	private int prevTutorialStep = -1;

	// Token: 0x04001A0B RID: 6667
	private float tutorialStepStartTime;

	// Token: 0x04001A0C RID: 6668
	private List<string> worldPaths = new List<string>();

	// Token: 0x04001A0D RID: 6669
	private List<HashSet<TutorialState>> equivalentStates = new List<HashSet<TutorialState>>
	{
		new HashSet<TutorialState>
		{
			TutorialState.Position,
			TutorialState.TapBlock
		}
	};
}
