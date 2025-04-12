using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
	// Token: 0x020000C9 RID: 201
	public class BlockSky : Block
	{
		// Token: 0x06000F3A RID: 3898 RVA: 0x0006663C File Offset: 0x00064A3C
		public BlockSky(string skyType, List<List<Tile>> tiles) : base(tiles)
		{
			this.skyParameters = this.GetSkyParameters();
			this.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
			for (int i = 0; i < this.tiles[0].Count; i++)
			{
				Tile tile = this.tiles[0][i];
				if (tile.gaf.Predicate == BlockSky.predicateSkyBoxTo)
				{
					this._skyBoxTile = tile;
				}
				else if (tile.gaf.Predicate == Block.predicateSetFog)
				{
					if (tile.gaf.Args.Length < 3)
					{
						tile.gaf = new GAF(Block.predicateSetFog, new object[]
						{
							tile.gaf.Args[0],
							tile.gaf.Args[1],
							"White"
						});
					}
					this._setFogTile = tile;
				}
			}
			bool flag = skyType == "Sky Space Asteroid";
			int num = (!flag) ? 0 : 9;
			if (this._skyBoxTile == null)
			{
				this._skyBoxTile = new Tile(BlockSky.predicateSkyBoxTo, new object[]
				{
					num
				});
				this.tiles[0].Add(this._skyBoxTile);
			}
			else if (flag && Util.GetIntArg(this._skyBoxTile.gaf.Args, 0, 0) == 0)
			{
				this._skyBoxTile.gaf.Args[0] = num;
			}
			WorldEnvironmentManager.ChangeSkyBoxTemporarily((int)this._skyBoxTile.gaf.Args[0]);
			this._currentSkyBoxIndex = (int)this._skyBoxTile.gaf.Args[0];
			if (skyType == "Sky Oz")
			{
				this.lockY = false;
				this.yLock = 0f;
			}
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x000668A0 File Offset: 0x00064CA0
		public new static void Register()
		{
			BlockSky.predicateEnvEffect = PredicateRegistry.Add<BlockSky>("Sky.EnvEffect", null, (Block b) => new PredicateActionDelegate(((BlockSky)b).SetEnvEffect), new Type[]
			{
				typeof(string)
			}, null, null);
			PredicateRegistry.Add<BlockSky>("Sky.AddSphereLightChanger", null, (Block b) => new PredicateActionDelegate(((BlockSky)b).AddSphereLightChanger), new Type[]
			{
				typeof(string),
				typeof(Vector3),
				typeof(Vector3),
				typeof(float),
				typeof(float)
			}, null, null);
			PredicateRegistry.Add<BlockSky>("Sky.SetGravity", null, (Block b) => new PredicateActionDelegate(((BlockSky)b).SetGravity), new Type[]
			{
				typeof(Vector3)
			}, null, null);
			PredicateRegistry.Add<BlockSky>("Sky.SetAtmosphere", null, (Block b) => new PredicateActionDelegate(((BlockSky)b).SetAtmosphere), new Type[]
			{
				typeof(float),
				typeof(float)
			}, null, null);
			BlockSky.predicateSkyBoxTo = PredicateRegistry.Add<BlockSky>("Sky.SkyBoxTo", (Block b) => new PredicateSensorDelegate(((BlockSky)b).IsSkyBox), (Block b) => new PredicateActionDelegate(((BlockSky)b).SetSkyBox), new Type[]
			{
				typeof(int)
			}, null, null);
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x00066A48 File Offset: 0x00064E48
		private Material GetRendererMaterial()
		{
			if (this.copiedRendererMaterial)
			{
				return this.go.GetComponent<Renderer>().sharedMaterial;
			}
			this.copiedRendererMaterial = true;
			return this.go.GetComponent<Renderer>().material;
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x00066A80 File Offset: 0x00064E80
		public TileResultCode AddSphereLightChanger(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string text = (args.Length <= 0) ? string.Empty : ((string)args[0]);
			Vector3 vector = (args.Length <= 1) ? Vector3.one : ((Vector3)args[1]);
			Vector3 position = (args.Length <= 2) ? Vector3.zero : ((Vector3)args[2]);
			float radius = (args.Length <= 3) ? 50f : ((float)args[3]);
			float innerRadius = (args.Length <= 4) ? 20f : ((float)args[4]);
			if (text != string.Empty)
			{
				SphereLightChanger sphereLightChanger = new SphereLightChanger();
				sphereLightChanger.color = new Color(vector.x, vector.y, vector.z, 1f);
				sphereLightChanger.position = position;
				sphereLightChanger.radius = radius;
				sphereLightChanger.innerRadius = innerRadius;
				this.lightChangers[text] = sphereLightChanger;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x00066B80 File Offset: 0x00064F80
		public TileResultCode SetGravity(ScriptRowExecutionInfo eInfo, object[] args)
		{
			Vector3 vector3Arg = Util.GetVector3Arg(args, 0, Vector3.zero);
			if (this.oldGravity != vector3Arg)
			{
				Physics.gravity = vector3Arg;
				foreach (Block block in BWSceneManager.AllBlocks())
				{
					block.SetVaryingGravity(true);
				}
				this.oldGravity = vector3Arg;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x00066C08 File Offset: 0x00065008
		public TileResultCode SetAtmosphere(ScriptRowExecutionInfo eInfo, object[] args)
		{
			float floatArg = Util.GetFloatArg(args, 0, 0f);
			float floatArg2 = Util.GetFloatArg(args, 1, 0f);
			if (this.oldDrag != floatArg)
			{
				Blocksworld.dragMultiplier = floatArg;
				Blocksworld.UpdateDrag();
				this.oldDrag = floatArg;
			}
			if (this.oldAngularDrag != floatArg2)
			{
				Blocksworld.angularDragMultiplier = floatArg2;
				Blocksworld.UpdateAngularDrag();
				this.oldAngularDrag = floatArg2;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x00066C6C File Offset: 0x0006506C
		public override bool HasDynamicalLight()
		{
			return true;
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x00066C70 File Offset: 0x00065070
		public override Color GetDynamicalLightTint()
		{
			Vector3 cameraPosition = Blocksworld.cameraPosition;
			Color color = Color.white;
			foreach (KeyValuePair<string, LightChanger> keyValuePair in this.lightChangers)
			{
				color *= keyValuePair.Value.GetLightTint(cameraPosition);
			}
			return color;
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x00066CE8 File Offset: 0x000650E8
		public TileResultCode SetEnvEffect(ScriptRowExecutionInfo eInfo, object[] args)
		{
			string envEffect = (args.Length <= 0) ? "Clear" : ((string)args[0]);
			this.SetEnvEffect(envEffect);
			return TileResultCode.True;
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x00066D19 File Offset: 0x00065119
		private void SetEnvEffect(string effect)
		{
			this.UpdateWeather(BlockSky.GetWeatherEffectByName(effect), true);
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x00066D28 File Offset: 0x00065128
		public override Color GetLightTint()
		{
			return this.lightColor;
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x00066D30 File Offset: 0x00065130
		public static List<string> GetAllEnvEffects()
		{
			List<string> list = new List<string>();
			BlockSky.CreateEffectInfos();
			foreach (BlockSky.EnvEffectInfo envEffectInfo in BlockSky.envEffectInfos)
			{
				list.Add(envEffectInfo.effectName);
			}
			return list;
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x00066D9C File Offset: 0x0006519C
		private static void CreateEffectInfos()
		{
			if (BlockSky.envEffectInfos == null)
			{
				BlockSky.envEffectInfos = new List<BlockSky.EnvEffectInfo>
				{
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.clear,
						effectName = "Clear",
						textures = new HashSet<string>
						{
							"Plain"
						},
						mainTexture = "Plain",
						changeSkyTexture = true
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.snow,
						effectName = "Snow",
						textures = new HashSet<string>
						{
							"Ice Material",
							"Ice Material_Sky"
						},
						mainTexture = "Ice Material",
						changeSkyTexture = true
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.rain,
						effectName = "Rain",
						textures = new HashSet<string>
						{
							"Texture Water Stream"
						},
						mainTexture = "Texture Water Stream"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.sandStorm,
						effectName = "Sand",
						textures = new HashSet<string>
						{
							"Sand",
							"Sand_Sky"
						},
						mainTexture = "Sand"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.greenLeaves,
						effectName = "Leaves Green",
						textures = new HashSet<string>
						{
							"Grass",
							"Grass Castle",
							"Grass_Sky",
							"Grass Castle_Sky"
						},
						mainTexture = "Grass"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.autumnLeaves,
						effectName = "Leaves",
						textures = new HashSet<string>
						{
							"Grass 2"
						},
						mainTexture = "Grass 2"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.ash,
						effectName = "Ash",
						textures = new HashSet<string>
						{
							"Texture Lava"
						},
						mainTexture = "Texture Lava"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.meteors,
						effectName = "Meteors",
						textures = new HashSet<string>
						{
							"Rock"
						},
						mainTexture = "Rock"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.meteors2,
						effectName = "Meteors 2",
						textures = new HashSet<string>
						{
							"Texture Crater"
						},
						mainTexture = "Texture Crater"
					},
					new BlockSky.EnvEffectInfo
					{
						effect = WeatherEffect.spaceDust,
						effectName = "Space Dust",
						textures = new HashSet<string>
						{
							"Dust"
						},
						mainTexture = "Dust"
					}
				};
				foreach (BlockSky.EnvEffectInfo envEffectInfo in BlockSky.envEffectInfos)
				{
					BlockSky.effectNameToEnvEffectInfos[envEffectInfo.effectName] = envEffectInfo;
					foreach (string key in envEffectInfo.textures)
					{
						BlockSky.textureToEnvEffectInfos[key] = envEffectInfo;
					}
					BlockSky.effectToEnvEffectInfos[envEffectInfo.effect] = envEffectInfo;
				}
			}
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x0006718C File Offset: 0x0006558C
		public static WeatherEffect GetWeatherEffectByName(string name)
		{
			BlockSky.CreateEffectInfos();
			BlockSky.EnvEffectInfo envEffectInfo;
			return (!BlockSky.effectNameToEnvEffectInfos.TryGetValue(name, out envEffectInfo)) ? WeatherEffect.clear : envEffectInfo.effect;
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x000671C0 File Offset: 0x000655C0
		public static WeatherEffect GetWeatherEffect(string texture)
		{
			BlockSky.CreateEffectInfos();
			BlockSky.EnvEffectInfo envEffectInfo;
			return (!BlockSky.textureToEnvEffectInfos.TryGetValue(texture, out envEffectInfo)) ? WeatherEffect.clear : envEffectInfo.effect;
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x000671F4 File Offset: 0x000655F4
		public static string EnvEffectToTexture(string effect)
		{
			BlockSky.CreateEffectInfos();
			BlockSky.EnvEffectInfo envEffectInfo;
			return (!BlockSky.effectNameToEnvEffectInfos.TryGetValue(effect, out envEffectInfo)) ? "Plain" : envEffectInfo.mainTexture;
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x00067228 File Offset: 0x00065628
		public static string SkyTextureToEnvEffect(string texture)
		{
			BlockSky.CreateEffectInfos();
			BlockSky.EnvEffectInfo envEffectInfo;
			return (!BlockSky.textureToEnvEffectInfos.TryGetValue(texture, out envEffectInfo)) ? "Clear" : envEffectInfo.effectName;
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x0006725C File Offset: 0x0006565C
		public static bool IsSkyTexture(string texture)
		{
			BlockSky.CreateEffectInfos();
			return BlockSky.textureToEnvEffectInfos.ContainsKey(texture);
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x00067270 File Offset: 0x00065670
		public static bool ChangeSkyTexture(string texture)
		{
			BlockSky.CreateEffectInfos();
			BlockSky.EnvEffectInfo envEffectInfo;
			return BlockSky.textureToEnvEffectInfos.TryGetValue(texture, out envEffectInfo) && envEffectInfo.changeSkyTexture;
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x000672A0 File Offset: 0x000656A0
		public int GetSkyBoxIndex()
		{
			return this._currentSkyBoxIndex;
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x000672A8 File Offset: 0x000656A8
		public void SetSkyBoxIndex(int sbIndex)
		{
			this._currentSkyBoxIndex = sbIndex;
			this._skyBoxTile.gaf.Args[0] = sbIndex;
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x000672CC File Offset: 0x000656CC
		public TileResultCode IsSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			return base.boolToTileResult(intArg == this._currentSkyBoxIndex);
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x000672F4 File Offset: 0x000656F4
		public TileResultCode SetSkyBox(ScriptRowExecutionInfo eInfo, object[] args)
		{
			int intArg = Util.GetIntArg(args, 0, 0);
			WorldEnvironmentManager.ChangeSkyBoxTemporarily(intArg);
			return TileResultCode.True;
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x00067311 File Offset: 0x00065711
		public float GetSkyBoxRotation()
		{
			return WorldEnvironmentManager.SkyBoxRotation();
		}

		// Token: 0x06000F52 RID: 3922 RVA: 0x00067318 File Offset: 0x00065718
		public void SetSkyBoxRotation(float angle, bool immediate)
		{
			this._targetSkyBoxRotation = angle;
			if (Blocksworld.isFirstFrame || immediate)
			{
				WorldEnvironmentManager.OverrideSkyBoxRotationTemporarily(this._targetSkyBoxRotation);
			}
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x0006733C File Offset: 0x0006573C
		public float GetSunIntensity()
		{
			return this._sunIntensity;
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x00067344 File Offset: 0x00065744
		public void SetSunIntensity(float intensity)
		{
			this._sunIntensity = intensity;
			Blocksworld.UpdateSunIntensity(this._sunIntensity);
		}

		// Token: 0x06000F55 RID: 3925 RVA: 0x00067358 File Offset: 0x00065758
		public override void Reset(bool forceRescale = false)
		{
			this.weatherLocked = true;
			base.Reset(forceRescale);
			this.weatherLocked = false;
			this.copiedRendererMaterial = false;
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x00067378 File Offset: 0x00065778
		public TileResultCode TransitionPaintTo(string paint, int meshIndex, float duration, float timer)
		{
			Material rendererMaterial = this.GetRendererMaterial();
			if (timer == 0f)
			{
				if (this.inColorTransition)
				{
					return TileResultCode.False;
				}
				this.transitionStart = new SkyColorState();
				this.transitionStart.color = rendererMaterial.GetColor("_Color");
				this.transitionStart.emissiveColor = rendererMaterial.GetColor("_Emission");
				this.transitionStart.specColor = rendererMaterial.GetColor("_SpecColor");
				this.transitionStart.lightColor = this.lightColor;
				this.transitionStart.lightIntensity = Blocksworld.directionalLight.GetComponent<Light>().intensity;
				string texture = base.GetTexture(0);
				ShaderType shaderType;
				if (!Materials.shaders.TryGetValue(texture, out shaderType))
				{
					BWLog.Info("Could not find shader for texture " + texture);
					return TileResultCode.True;
				}
				Material material = Materials.GetMaterial(paint, texture, shaderType);
				if (!(material != null))
				{
					BWLog.Info(string.Concat(new object[]
					{
						"Failed to find material for ",
						paint,
						" ",
						texture,
						" ",
						shaderType
					}));
					return TileResultCode.True;
				}
				this.transitionEnd = new SkyColorState();
				this.transitionEnd.color = material.GetColor("_Color");
				this.transitionEnd.specColor = material.GetColor("_SpecColor");
				this.transitionEnd.lightColor = Color.white;
				this.transitionEnd.lightIntensity = 1f;
				BlockSky.GetSkyColor(paint, ref this.transitionEnd.color, ref this.transitionEnd.specColor, ref this.transitionEnd.lightColor, ref this.transitionEnd.lightIntensity);
				this.transitionEnd.emissiveColor = material.GetColor("_Emission");
				WorldEnvironmentManager.AssignNewSkyPaint(paint);
				this.SetFogColor(paint);
			}
			else
			{
				if (timer >= duration)
				{
					this.inColorTransition = false;
					this.PaintTo(paint, false, meshIndex);
					return TileResultCode.True;
				}
				float num = timer / duration;
				float num2 = 1f - num;
				Color color = Color.Lerp(this.transitionStart.color, this.transitionEnd.color, num);
				Color emissiveColor = Color.Lerp(this.transitionStart.emissiveColor, this.transitionEnd.emissiveColor, num);
				Color specColor = Color.Lerp(this.transitionStart.specColor, this.transitionEnd.specColor, num);
				this.lightColor = Color.Lerp(this.transitionStart.lightColor, this.transitionEnd.lightColor, num);
				float lightIntensity = num2 * this.transitionStart.lightIntensity + num * this.transitionEnd.lightIntensity;
				WorldEnvironmentManager.AssignNewSkyPaint(paint);
				this.SetFogColor(paint);
				this.UpdateColorAndLightState(rendererMaterial, color, emissiveColor, specColor, lightIntensity);
			}
			return TileResultCode.Delayed;
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x00067640 File Offset: 0x00065A40
		public void SetFogColor(string paint)
		{
			Color[] array;
			if (Blocksworld.colorDefinitions.TryGetValue(paint, out array))
			{
				Blocksworld.UpdateFogColor(array[1]);
			}
			if (this._setFogTile != null)
			{
				this._setFogTile.gaf.Args[2] = paint;
			}
			if (!this.doNotApplyFogColorBackToEnvironmentManager)
			{
				WorldEnvironmentManager.AssignNewFogPaint(paint);
			}
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x000676A0 File Offset: 0x00065AA0
		public static void GetSkyColor(string paint, ref Color color, ref Color specColor, ref Color lightColor, ref float lightIntensity)
		{
			Color[] array;
			if (Blocksworld.colorDefinitions.TryGetValue(paint, out array))
			{
				color = array[1];
			}
			Color[] array2;
			if (Blocksworld.colorDefinitions.TryGetValue("Sky " + paint, out array2))
			{
				color = array2[0];
			}
			specColor = color;
			Color[] array3;
			if (Blocksworld.colorDefinitions.TryGetValue("Sky Spec " + paint, out array3))
			{
				specColor = array3[0];
			}
			bool flag = false;
			Color[] array4;
			if (Blocksworld.colorDefinitions.TryGetValue("Sky Light Color " + paint, out array4))
			{
				lightColor = array4[0];
				flag = true;
			}
			if (Blocksworld.IsLuminousPaint(paint))
			{
				lightIntensity = 1.1f;
				if (!flag)
				{
					lightColor = 0.5f * Color.white + 0.5f * color;
				}
			}
			else if (paint.StartsWith("Darker"))
			{
				lightIntensity = 0.4f;
			}
			else if (paint.StartsWith("Dark") || paint.StartsWith("Earth") || paint == "Black")
			{
				lightIntensity = 0.55f;
			}
			else if (paint.StartsWith("Light") || paint == "White")
			{
				lightIntensity = 0.9f;
			}
			else if (paint == "Super Black")
			{
				lightIntensity = 0.2f;
			}
			else
			{
				lightIntensity = 0.7f;
			}
			if (!Blocksworld.renderingShadows)
			{
				lightIntensity = (lightIntensity + 2f) * 0.33f;
			}
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x0006787C File Offset: 0x00065C7C
		private static Color CombineSkyColors(Color c1, Color c2)
		{
			return c1 * c2;
		}

		// Token: 0x06000F5A RID: 3930 RVA: 0x00067888 File Offset: 0x00065C88
		public override TileResultCode PaintTo(string paint, bool permanent, int meshIndex = 0)
		{
			base.PaintTo(paint, permanent, meshIndex);
			if (permanent)
			{
				for (int i = 0; i < this.tiles[0].Count; i++)
				{
					Tile tile = this.tiles[0][i];
					if (tile.gaf.Predicate.Name == "Block.SetFog" && tile.gaf.Args.Length > 2)
					{
						tile.gaf.Args[2] = paint;
						break;
					}
				}
			}
			Material rendererMaterial = this.GetRendererMaterial();
			Materials.materialCachePaint[rendererMaterial] = paint;
			this.lightColor = Color.white;
			Color color = rendererMaterial.GetColor("_Color");
			Color color2 = rendererMaterial.GetColor("_Emission");
			Color specColor = color;
			this.oldColor = color;
			float lightIntensity = 1f;
			BlockSky.GetSkyColor(paint, ref color, ref specColor, ref this.lightColor, ref lightIntensity);
			WorldEnvironmentManager.AssignNewSkyPaint(paint);
			this.SetFogColor(paint);
			this.UpdateColorAndLightState(rendererMaterial, color, color2, specColor, lightIntensity);
			return TileResultCode.True;
		}

		// Token: 0x06000F5B RID: 3931 RVA: 0x00067998 File Offset: 0x00065D98
		private bool colorDiff(Color c1, Color c2)
		{
			Vector4 vector = new Vector4(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b, c1.a - c2.a);
			return vector.magnitude > 0.01f;
		}

		// Token: 0x06000F5C RID: 3932 RVA: 0x000679F8 File Offset: 0x00065DF8
		private void UpdateColorAndLightState(Material m, Color color, Color emissiveColor, Color specColor, float lightIntensity)
		{
			if (this.colorDiff(color, this.oldColor) || this.colorDiff(emissiveColor, this.oldEmissiveColor) || this.colorDiff(specColor, this.oldSpecColor) || Mathf.Abs(lightIntensity - this.oldLightIntensity) > 0.01f)
			{
				this.oldEmissiveColor = emissiveColor;
				this.oldSpecColor = specColor;
				this.oldLightIntensity = lightIntensity;
				m.SetColor("_SpecColor", specColor);
				m.SetColor("_Color", color);
				m.SetColor("_Emission", emissiveColor);
				this._lightIntensityMultiplier = lightIntensity;
				WorldEnvironmentManager.RevertToAssignedSkyColor();
				WorldEnvironmentManager.RevertToAssignedFogPaint();
				Blocksworld.UpdateLightColor(false);
				Blocksworld.UpdateDynamicalLights(false, true);
				Blocksworld.UpdateFogColor(BlockSky.GetFogColor());
				Blocksworld.UpdateSunLight(color, emissiveColor, this._lightIntensityMultiplier, this._sunIntensity);
			}
		}

		// Token: 0x06000F5D RID: 3933 RVA: 0x00067ACC File Offset: 0x00065ECC
		public override float GetLightIntensityMultiplier()
		{
			return this._lightIntensityMultiplier;
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x00067AD4 File Offset: 0x00065ED4
		public static Color GetFogColor()
		{
			if (Blocksworld.renderingSkybox)
			{
				return RenderSettings.fogColor;
			}
			if (Blocksworld.worldSky != null && Blocksworld.worldSky.go != null)
			{
				Material sharedMaterial = Blocksworld.worldSky.renderer.sharedMaterial;
				Color result = 1.5f * sharedMaterial.GetColor("_Color");
				Color color = Blocksworld.directionalLight.GetComponent<Light>().color;
				result.r *= color.r;
				result.g *= color.g;
				result.b *= color.b;
				return result;
			}
			return Color.white;
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x00067B8C File Offset: 0x00065F8C
		public override void OnCreate()
		{
			base.OnCreate();
			this.weatherLocked = true;
			this.TextureTo(base.GetTexture(0), base.GetTextureNormal(), true, 0, false);
			this.weatherLocked = false;
			this.UpdateWeatherFromEnvEffectTile();
			this.UpdateFarplane();
		}

		// Token: 0x06000F60 RID: 3936 RVA: 0x00067BC8 File Offset: 0x00065FC8
		private void UpdateWeatherFromEnvEffectTile()
		{
			GAF simpleInitGAF = base.GetSimpleInitGAF("Sky.EnvEffect");
			if (simpleInitGAF == null)
			{
				this.UpdateWeatherCurrent();
			}
			else
			{
				this.SetEnvEffect((string)simpleInitGAF.Args[0]);
			}
		}

		// Token: 0x06000F61 RID: 3937 RVA: 0x00067C05 File Offset: 0x00066005
		public override void FixedUpdate()
		{
			if (Blocksworld.isFirstFrame)
			{
				this._sunIntensity = 1f;
				this._targetSkyBoxRotation = 0f;
			}
			base.FixedUpdate();
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x00067C30 File Offset: 0x00066030
		public override void Update()
		{
			base.Update();
			if (Blocksworld.worldOceanBlock != null && Blocksworld.worldOceanBlock.go != null && Blocksworld.worldOceanBlock.go.GetComponent<Collider>() != null)
			{
				Material rendererMaterial = this.GetRendererMaterial();
				if (rendererMaterial != null)
				{
					rendererMaterial.SetFloat("_WaterLevel", Blocksworld.worldOceanBlock.GetWaterBounds().max.y);
					rendererMaterial.SetVector("_OverlayCosSinAngles", new Vector4(Mathf.Cos(0.01f * Time.time), Mathf.Sin(0.01f * Time.time)));
				}
			}
			float skyBoxRotation = this.GetSkyBoxRotation();
			if (!Util.FloatsClose(this._targetSkyBoxRotation, skyBoxRotation))
			{
				float angle = Mathf.LerpAngle(skyBoxRotation, this._targetSkyBoxRotation, 0.1f);
				WorldEnvironmentManager.OverrideSkyBoxRotationTemporarily(angle);
			}
		}

		// Token: 0x06000F63 RID: 3939 RVA: 0x00067D16 File Offset: 0x00066116
		public override void OnReconstructed()
		{
			base.OnReconstructed();
			this.UpdateWeatherFromEnvEffectTile();
		}

		// Token: 0x06000F64 RID: 3940 RVA: 0x00067D24 File Offset: 0x00066124
		public void UpdateWeatherCurrent()
		{
			string texture = base.GetTexture(0);
			texture = Scarcity.GetNormalizedTexture(texture);
			this.UpdateWeather(texture);
		}

		// Token: 0x06000F65 RID: 3941 RVA: 0x00067D48 File Offset: 0x00066148
		private void UpdateWeather(string texture)
		{
			WeatherEffect weatherEffect = BlockSky.GetWeatherEffect(texture);
			this.UpdateWeather(weatherEffect, true);
		}

		// Token: 0x06000F66 RID: 3942 RVA: 0x00067D64 File Offset: 0x00066164
		public static string GetEnvEffectName(WeatherEffect newWeather)
		{
			string result;
			if (newWeather == WeatherEffect.snow)
			{
				result = "Snow";
			}
			else if (newWeather == WeatherEffect.ash)
			{
				result = "Ash";
			}
			else if (newWeather == WeatherEffect.sandStorm)
			{
				result = "Sand";
			}
			else if (newWeather == WeatherEffect.rain)
			{
				result = "Rain";
			}
			else if (newWeather == WeatherEffect.meteors)
			{
				result = "Meteors";
			}
			else if (newWeather == WeatherEffect.autumnLeaves)
			{
				result = "Leaves";
			}
			else if (newWeather == WeatherEffect.greenLeaves)
			{
				result = "Leaves Green";
			}
			else if (newWeather == WeatherEffect.spaceDust)
			{
				result = "Space Dust";
			}
			else
			{
				result = "Clear";
			}
			return result;
		}

		// Token: 0x06000F67 RID: 3943 RVA: 0x00067E28 File Offset: 0x00066228
		public void UpdateWeather(WeatherEffect newWeather, bool permanent = true)
		{
			if (Blocksworld.weather != newWeather)
			{
				Blocksworld.weather.Stop();
				Blocksworld.weather = newWeather;
			}
			newWeather.Start();
			newWeather.IntensityMultiplier = 1f;
			if (permanent)
			{
				base.SetSimpleInitTile(new GAF("Sky.EnvEffect", new object[]
				{
					BlockSky.GetEnvEffectName(newWeather)
				}));
			}
			Blocksworld.bw.SetFogMultiplier(newWeather.GetFogMultiplier());
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x00067E98 File Offset: 0x00066298
		public override TileResultCode TextureTo(string texture, Vector3 normal, bool permanent, int meshIndex = 0, bool force = false)
		{
			texture = Scarcity.GetNormalizedTexture(texture);
			if (!this.weatherLocked)
			{
				this.UpdateWeather(texture);
			}
			bool flag = BlockSky.IsSkyTexture(texture);
			bool flag2 = BlockSky.ChangeSkyTexture(texture);
			if (texture == "Plain")
			{
				string defaultTexture = base.GetDefaultTexture(meshIndex);
				flag = BlockSky.IsSkyTexture(defaultTexture);
				texture = defaultTexture;
				force = true;
			}
			if (flag)
			{
				if (flag2)
				{
					texture += "_Sky";
				}
				else
				{
					texture = base.GetDefaultTexture(meshIndex);
					flag2 = BlockSky.ChangeSkyTexture(texture);
					if (flag2)
					{
						texture += "_Sky";
					}
				}
			}
			if (this.skyParameters != null && !this.skyParameters.canChangeTexture)
			{
				return TileResultCode.True;
			}
			if (flag)
			{
				TileResultCode result = base.TextureTo(texture, normal, permanent, meshIndex, flag || force);
				if (this.copiedRendererMaterial)
				{
					Materials.materialCacheTexture[this.GetRendererMaterial()] = base.GetTexture(0);
				}
				return result;
			}
			return TileResultCode.True;
		}

		// Token: 0x06000F69 RID: 3945 RVA: 0x00067F9A File Offset: 0x0006639A
		public override void Play()
		{
			this.oldGravity = Vector3.up;
			base.Play();
			this.go.GetComponent<Collider>().enabled = false;
			this.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x00067FD6 File Offset: 0x000663D6
		public override void Stop(bool resetBlock = true)
		{
			base.Stop(resetBlock);
			this.go.GetComponent<Collider>().enabled = true;
			this.go.GetComponent<Renderer>().enabled = !Blocksworld.renderingSkybox;
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x00068008 File Offset: 0x00066408
		public SkyParameters GetSkyParameters()
		{
			return this.go.GetComponent<SkyParameters>();
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x00068022 File Offset: 0x00066422
		public override bool ScaleTo(Vector3 scale, bool recalculateCollider = true, bool forceRescale = false)
		{
			this.goT.localScale = scale;
			this.UpdateFarplane();
			return true;
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x00068038 File Offset: 0x00066438
		private void UpdateFarplane()
		{
			MeshRenderer component = this.go.GetComponent<MeshRenderer>();
			if (component != null)
			{
				float a = 0.51f * component.bounds.size.magnitude;
				float farClipPlane = Mathf.Max(a, 400f);
				Blocksworld.mainCamera.farClipPlane = farClipPlane;
			}
		}

		// Token: 0x06000F6E RID: 3950 RVA: 0x00068094 File Offset: 0x00066494
		public override void Destroy()
		{
			if (this.copiedRendererMaterial)
			{
				Material rendererMaterial = this.GetRendererMaterial();
				Materials.materialCachePaint.Remove(rendererMaterial);
				Materials.materialCacheTexture.Remove(rendererMaterial);
				UnityEngine.Object.Destroy(rendererMaterial);
				this.copiedRendererMaterial = false;
			}
			base.Destroy();
		}

		// Token: 0x04000BC9 RID: 3017
		private bool weatherLocked;

		// Token: 0x04000BCA RID: 3018
		public bool lockY;

		// Token: 0x04000BCB RID: 3019
		public float yLock;

		// Token: 0x04000BCC RID: 3020
		public static Predicate predicateEnvEffect;

		// Token: 0x04000BCD RID: 3021
		public static Predicate predicateSkyBoxTo;

		// Token: 0x04000BCE RID: 3022
		private Dictionary<string, LightChanger> lightChangers = new Dictionary<string, LightChanger>();

		// Token: 0x04000BCF RID: 3023
		private SkyParameters skyParameters;

		// Token: 0x04000BD0 RID: 3024
		private static Color _skyboxDefaultTint;

		// Token: 0x04000BD1 RID: 3025
		private static string _skyBoxPaint = "White";

		// Token: 0x04000BD2 RID: 3026
		private int _currentSkyBoxIndex;

		// Token: 0x04000BD3 RID: 3027
		private float _targetSkyBoxRotation;

		// Token: 0x04000BD4 RID: 3028
		private Tile _setFogTile;

		// Token: 0x04000BD5 RID: 3029
		private Tile _skyBoxTile;

		// Token: 0x04000BD6 RID: 3030
		private float _sunIntensity = 1f;

		// Token: 0x04000BD7 RID: 3031
		private bool copiedRendererMaterial;

		// Token: 0x04000BD8 RID: 3032
		private float _lightIntensityMultiplier = 1f;

		// Token: 0x04000BD9 RID: 3033
		private Color oldColor = Color.black;

		// Token: 0x04000BDA RID: 3034
		private Color oldSpecColor = Color.black;

		// Token: 0x04000BDB RID: 3035
		private Color oldEmissiveColor = Color.black;

		// Token: 0x04000BDC RID: 3036
		private float oldLightIntensity;

		// Token: 0x04000BDD RID: 3037
		private Vector3 oldGravity = Vector3.up;

		// Token: 0x04000BDE RID: 3038
		private float oldDrag = -1f;

		// Token: 0x04000BDF RID: 3039
		private float oldAngularDrag = -1f;

		// Token: 0x04000BE0 RID: 3040
		private const string CLEAR = "Clear";

		// Token: 0x04000BE1 RID: 3041
		private const string RAIN = "Rain";

		// Token: 0x04000BE2 RID: 3042
		private const string METEORS = "Meteors";

		// Token: 0x04000BE3 RID: 3043
		private const string METEORS_2 = "Meteors 2";

		// Token: 0x04000BE4 RID: 3044
		private const string SNOW = "Snow";

		// Token: 0x04000BE5 RID: 3045
		private const string ASH = "Ash";

		// Token: 0x04000BE6 RID: 3046
		private const string LEAVES = "Leaves";

		// Token: 0x04000BE7 RID: 3047
		private const string LEAVES_GREEN = "Leaves Green";

		// Token: 0x04000BE8 RID: 3048
		private const string SAND = "Sand";

		// Token: 0x04000BE9 RID: 3049
		private const string SPACE_DUST = "Space Dust";

		// Token: 0x04000BEA RID: 3050
		private Color lightColor = Color.white;

		// Token: 0x04000BEB RID: 3051
		private static List<BlockSky.EnvEffectInfo> envEffectInfos = null;

		// Token: 0x04000BEC RID: 3052
		private static Dictionary<string, BlockSky.EnvEffectInfo> effectNameToEnvEffectInfos = new Dictionary<string, BlockSky.EnvEffectInfo>();

		// Token: 0x04000BED RID: 3053
		private static Dictionary<string, BlockSky.EnvEffectInfo> textureToEnvEffectInfos = new Dictionary<string, BlockSky.EnvEffectInfo>();

		// Token: 0x04000BEE RID: 3054
		private static Dictionary<WeatherEffect, BlockSky.EnvEffectInfo> effectToEnvEffectInfos = new Dictionary<WeatherEffect, BlockSky.EnvEffectInfo>();

		// Token: 0x04000BEF RID: 3055
		private bool inColorTransition;

		// Token: 0x04000BF0 RID: 3056
		private SkyColorState transitionStart;

		// Token: 0x04000BF1 RID: 3057
		private SkyColorState transitionEnd;

		// Token: 0x04000BF2 RID: 3058
		public bool doNotApplyFogColorBackToEnvironmentManager;

		// Token: 0x020000CA RID: 202
		public class EnvEffectInfo
		{
			// Token: 0x04000BF9 RID: 3065
			public WeatherEffect effect;

			// Token: 0x04000BFA RID: 3066
			public string effectName;

			// Token: 0x04000BFB RID: 3067
			public HashSet<string> textures;

			// Token: 0x04000BFC RID: 3068
			public string mainTexture;

			// Token: 0x04000BFD RID: 3069
			public bool changeSkyTexture;
		}
	}
}
