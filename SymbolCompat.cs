using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x020002B5 RID: 693
public static class SymbolCompat
{
	// Token: 0x06001FE5 RID: 8165 RVA: 0x000E70F4 File Offset: 0x000E54F4
	public static GAF ToGaf(JObject obj)
	{
		OldSymbol oldSymbol = (OldSymbol)Enum.Parse(typeof(OldSymbol), (string)obj["sym"]);
		switch (oldSymbol)
		{
		case OldSymbol.CreateCube1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Cube"
			});
		case OldSymbol.CreateWedge1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Wedge"
			});
		case OldSymbol.CreateRocket1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Rocket"
			});
		case OldSymbol.CreateMotor1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Motor"
			});
		case OldSymbol.CreateBody1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Body"
			});
		case OldSymbol.PaintBrown:
		case OldSymbol.PaintOrange:
		case OldSymbol.PaintRed:
		case OldSymbol.PaintBlack:
		case OldSymbol.PaintBeige:
		case OldSymbol.PaintYellow:
		case OldSymbol.PaintWhite:
		case OldSymbol.PaintGrey:
		case OldSymbol.PaintGreen:
		case OldSymbol.PaintMagenta:
		case OldSymbol.PaintPurple:
		case OldSymbol.PaintBlue:
		case OldSymbol.PaintLime:
		case OldSymbol.PaintPink:
		case OldSymbol.PaintLavender:
		case OldSymbol.PaintCeleste:
		case OldSymbol.PaintGrass:
			return new GAF("Block.PaintTo", new object[]
			{
				oldSymbol.ToString().Substring(5)
			});
		case OldSymbol.TextureGlass:
		case OldSymbol.TextureCat:
		case OldSymbol.TexturePant:
		case OldSymbol.TextureSpace:
		case OldSymbol.TextureJacket:
		case OldSymbol.TextureSuit:
		case OldSymbol.TextureCheckered:
		case OldSymbol.TextureBolts:
		case OldSymbol.TextureGrill:
		case OldSymbol.TexturePlain:
		case OldSymbol.TextureStripes:
		case OldSymbol.TextureArrow:
		case OldSymbol.TextureTeeth:
		case OldSymbol.TextureNose:
		case OldSymbol.TextureClouds:
			return new GAF("Block.TextureTo", new object[]
			{
				oldSymbol.ToString().Substring(7),
				obj.Vector3Value()
			});
		case OldSymbol.TextureFaceHappy:
			return new GAF("Block.TextureTo", new object[]
			{
				"Happy",
				obj.Vector3Value()
			});
		case OldSymbol.TextureFaceSurprised:
			return new GAF("Block.TextureTo", new object[]
			{
				"Surprised",
				obj.Vector3Value()
			});
		case OldSymbol.TextureEyes:
			return new GAF("Block.TextureTo", new object[]
			{
				"Eye",
				obj.Vector3Value()
			});
		case OldSymbol.TextureWood:
			return new GAF("Block.TextureTo", new object[]
			{
				"Bricks",
				obj.Vector3Value()
			});
		case OldSymbol.Stop:
			return new GAF("Meta.Stop", new object[0]);
		case OldSymbol.Play:
			return new GAF("Meta.Then", new object[0]);
		case OldSymbol.MoveTo:
			return new GAF("Block.MoveTo", new object[]
			{
				obj.Vector3Value()
			});
		case OldSymbol.RotateTo:
			return new GAF("Block.RotateTo", new object[]
			{
				obj.Vector3Value()
			});
		case OldSymbol.FireRocket:
			return new GAF("Rocket.Fire", new object[0]);
		case OldSymbol.Drive:
			return new GAF("Wheel.Drive", new object[]
			{
				20f
			});
		case OldSymbol.Break:
			return new GAF("Wheel.Drive", new object[]
			{
				-20f
			});
		case OldSymbol.SteerLeft:
			return new GAF("Wheel.Turn", new object[]
			{
				-30f
			});
		case OldSymbol.SteerRight:
			return new GAF("Wheel.Turn", new object[]
			{
				30f
			});
		case OldSymbol.Walk:
			return new GAF("Legs.Walk", new object[]
			{
				0.5f
			});
		case OldSymbol.Back:
			return new GAF("Legs.Walk", new object[]
			{
				-0.5f
			});
		case OldSymbol.TurnLeft:
			return new GAF("Legs.Turn", new object[]
			{
				-0.4f
			});
		case OldSymbol.TurnRight:
			return new GAF("Legs.Turn", new object[]
			{
				0.4f
			});
		case OldSymbol.Jump:
			return new GAF("Legs.Jump", new object[]
			{
				5f
			});
		case OldSymbol.Turn:
			return new GAF("Motor.Turn", new object[]
			{
				-1f
			});
		case OldSymbol.Reverse:
			return new GAF("Motor.Turn", new object[]
			{
				1f
			});
		case OldSymbol.Fixed:
			return new GAF("Block.Fixed", new object[0]);
		case OldSymbol.Explode:
			return new GAF("Block.Explode", new object[0]);
		case OldSymbol.InputR1:
		case OldSymbol.InputL1:
		case OldSymbol.InputL2:
		case OldSymbol.InputR2:
		case OldSymbol.InputL3:
		case OldSymbol.InputR3:
		case OldSymbol.InputL4:
		case OldSymbol.InputR4:
			return new GAF("Block.ButtonInput", new object[]
			{
				oldSymbol.ToString().Substring(5)
			});
		case OldSymbol.TiltLeft:
			return new GAF("Block.DeviceTilt", new object[]
			{
				-1f
			});
		case OldSymbol.TiltRight:
			return new GAF("Block.DeviceTilt", new object[]
			{
				1f
			});
		case OldSymbol.GameStart:
			return new GAF("Block.GameStart", new object[0]);
		case OldSymbol.BumpObject:
			return new GAF("Block.Bump", new object[]
			{
				"object"
			});
		case OldSymbol.BumpGround:
			return new GAF("Block.Bump", new object[]
			{
				"ground"
			});
		case OldSymbol.AngleLeft:
			return new GAF("Motor.Step", new object[]
			{
				-1f
			});
		case OldSymbol.AngleRight:
			return new GAF("Motor.Step", new object[]
			{
				1f
			});
		case OldSymbol.Wait:
			return new GAF("Meta.Wait", new object[0]);
		case OldSymbol.Locked:
			return new GAF("Block.Locked", new object[0]);
		case OldSymbol.Inventory:
			return new GAF("Block.Inventory", new object[0]);
		case OldSymbol.Win:
			return new GAF("Block.Win", new object[0]);
		case OldSymbol.Smoke:
			return new GAF("Rocket.Smoke", new object[0]);
		case OldSymbol.FreeSpin:
			return new GAF("Motor.FreeSpin", new object[0]);
		case OldSymbol.Target:
			return new GAF("Block.Target", new object[0]);
		case OldSymbol.BumpTarget:
			return new GAF("Block.BumpTarget", new object[0]);
		case OldSymbol.CreateWheel1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Wheel"
			});
		case OldSymbol.CameraFollow:
			return new GAF("Block.CameraFollow", new object[0]);
		case OldSymbol.CreatePie1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Slice"
			});
		case OldSymbol.Speak:
			return new GAF("Block.Speak", new object[]
			{
				(string)obj["string"]
			});
		case OldSymbol.CreateCylinder1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Cylinder"
			});
		case OldSymbol.CreateHead1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Head"
			});
		case OldSymbol.CreateCoreCube1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Terrain Cube"
			});
		case OldSymbol.CreateCoreWedge1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Terrain Wedge"
			});
		case OldSymbol.ScaleTo:
			return new GAF("Block.ScaleTo", new object[]
			{
				obj.Vector3Value()
			});
		case OldSymbol.CreateSky:
			return new GAF("Block.Create", new object[]
			{
				"Sky"
			});
		case OldSymbol.BreakOff:
			return new GAF("Block.BreakOff", new object[0]);
		case OldSymbol.CreateLegs1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Legs"
			});
		case OldSymbol.TapBlock:
			return new GAF("Block.Tap", new object[0]);
		case OldSymbol.SendA:
		case OldSymbol.SendB:
		case OldSymbol.SendC:
		case OldSymbol.SendD:
		case OldSymbol.SendE:
		case OldSymbol.SendF:
		case OldSymbol.SendG:
		case OldSymbol.SendH:
		case OldSymbol.SendI:
			return new GAF("Block.SendSignal", new object[]
			{
				(int)(oldSymbol.ToString()[4] - 'A')
			});
		case OldSymbol.CreateLaser1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Laser"
			});
		case OldSymbol.LaserPulse:
			return new GAF("Laser.Pulse", new object[0]);
		case OldSymbol.LaserBeam:
			return new GAF("Laser.Beam", new object[0]);
		case OldSymbol.HitByLaserPulse:
			return new GAF("Laser.HitByPulse", new object[0]);
		case OldSymbol.HitByLaserBeam:
			return new GAF("Laser.HitByBeam", new object[0]);
		case OldSymbol.CreateCat1x1x1:
			return new GAF("Block.Create", new object[]
			{
				"Cat"
			});
		case OldSymbol.ExecuteAnimalBehavior:
			return new GAF("Animal.ExecuteBehavior", new object[0]);
		case OldSymbol.CreateCorner:
			return new GAF("Block.Create", new object[]
			{
				"Corner"
			});
		case OldSymbol.CreateRamp:
			return new GAF("Block.Create", new object[]
			{
				"Slice Inverse"
			});
		case OldSymbol.CreateWedgeInnerCorner:
			return new GAF("Block.Create", new object[]
			{
				"Corner Inverse"
			});
		case OldSymbol.CreateWedgeInnerEdge:
			return new GAF("Block.Create", new object[]
			{
				"Wedge Inner Edge"
			});
		case OldSymbol.CreateEmitter:
			return new GAF("Block.Create", new object[]
			{
				"Emitter"
			});
		case OldSymbol.EmitterEmit:
			return new GAF("Emitter.Emit", new object[0]);
		case OldSymbol.EmitterEmitSmoke:
			return new GAF("Emitter.EmitSmoke", new object[0]);
		case OldSymbol.EmitterEmitFast:
			return new GAF("Emitter.EmitFast", new object[0]);
		case OldSymbol.EmitterParticleHit:
			return new GAF("Emitter.HitByParticle", new object[0]);
		case OldSymbol.CreateStabilizer:
			return new GAF("Block.Create", new object[]
			{
				"Stabilizer"
			});
		case OldSymbol.StabilizerStop:
			return new GAF("Stabilizer.Stop", new object[0]);
		case OldSymbol.StabilizerStart:
			return new GAF("Stabilizer.Start", new object[0]);
		case OldSymbol.CreateWater:
			return new GAF("Block.Create", new object[]
			{
				"Water"
			});
		case OldSymbol.WaterWithin:
			return new GAF("Water.IsWithin", new object[0]);
		case OldSymbol.WaterStreamNorth:
			return new GAF("Water.StreamNorth", new object[0]);
		case OldSymbol.WaterStreamSouth:
			return new GAF("Water.StreamSouth", new object[0]);
		case OldSymbol.WaterStreamWest:
			return new GAF("Water.StreamWest", new object[0]);
		case OldSymbol.WaterStreamEast:
			return new GAF("Water.StreamEast", new object[0]);
		}
		BWLog.Error("Could not convert symbol " + oldSymbol + " to GAF");
		return null;
	}

	// Token: 0x06001FE6 RID: 8166 RVA: 0x000E7CB0 File Offset: 0x000E60B0
	private static Dictionary<string, string> ReadStringMapping(string filename, Dictionary<string, string> inverse = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		TextAsset textAsset = Resources.Load(filename) as TextAsset;
		if (textAsset == null)
		{
			BWLog.Info("Could not find file " + filename);
			return dictionary;
		}
		JObject jobject = JSONDecoder.Decode(textAsset.text);
		HashSet<string> hashSet = new HashSet<string>();
		Dictionary<string, JObject> objectValue = jobject.ObjectValue;
		foreach (KeyValuePair<string, JObject> keyValuePair in objectValue)
		{
			if (keyValuePair.Key == "_read_only")
			{
				foreach (JObject jobject2 in keyValuePair.Value.ArrayValue)
				{
					hashSet.Add(jobject2.StringValue);
				}
			}
			else
			{
				string stringValue = keyValuePair.Value.StringValue;
				if (stringValue != null)
				{
					dictionary[keyValuePair.Key] = stringValue;
				}
			}
		}
		if (inverse != null)
		{
			foreach (KeyValuePair<string, string> keyValuePair2 in dictionary)
			{
				string key = keyValuePair2.Key;
				if (!hashSet.Contains(keyValuePair2.Value))
				{
					int num;
					if (inverse.ContainsKey(keyValuePair2.Value))
					{
						string text = inverse[keyValuePair2.Value];
						if (key.Length < text.Length)
						{
							inverse[keyValuePair2.Value] = key;
						}
					}
					else if (key.Length == 1 || int.TryParse(key, out num))
					{
						inverse[keyValuePair2.Value] = key;
					}
				}
			}
		}
		return dictionary;
	}

	// Token: 0x06001FE7 RID: 8167 RVA: 0x000E7EC0 File Offset: 0x000E62C0
	public static string RenamePredicate(string name)
	{
		string result;
		if (SymbolCompat.predicateRenamings.TryGetValue(name, out result))
		{
			return result;
		}
		return name;
	}

	// Token: 0x06001FE8 RID: 8168 RVA: 0x000E7EE4 File Offset: 0x000E62E4
	public static GAF RenameGAF(GAF gaf)
	{
		Predicate predicate = gaf.Predicate;
		if (predicate == Block.predicateCreate)
		{
			string text = SymbolCompat.BlockRenamings((string)gaf.Args[0]);
			if (text != null)
			{
				object[] array = Util.CopyArray<object>(gaf.Args);
				array[0] = text;
				return new GAF(predicate, array);
			}
		}
		else if (predicate == Block.predicateTextureTo)
		{
			string text2 = SymbolCompat.TextureRenamings((string)gaf.Args[0]);
			if (text2 != null)
			{
				object[] array2 = Util.CopyArray<object>(gaf.Args);
				array2[0] = SymbolCompat.TextureRenamings((string)gaf.Args[0]);
				return new GAF(predicate, array2);
			}
		}
		else if (predicate == Block.predicatePaintTo)
		{
			string text3 = SymbolCompat.PaintRenamings((string)gaf.Args[0]);
			if (text3 != null)
			{
				object[] array3 = Util.CopyArray<object>(gaf.Args);
				array3[0] = SymbolCompat.PaintRenamings((string)gaf.Args[0]);
				return new GAF(predicate, array3);
			}
		}
		else if (predicate == Block.predicateWaitTime && gaf.Args.Length == 0)
		{
			object[] args = new object[]
			{
				0.24f
			};
			return new GAF(predicate, args);
		}
		return gaf;
	}

	// Token: 0x06001FE9 RID: 8169 RVA: 0x000E801C File Offset: 0x000E641C
	public static string BlockRenamings(string type)
	{
		string result;
		if (SymbolCompat.blockRenamings.TryGetValue(type, out result))
		{
			return result;
		}
		return type;
	}

	// Token: 0x06001FEA RID: 8170 RVA: 0x000E8040 File Offset: 0x000E6440
	public static string TextureRenamings(string texture)
	{
		string result;
		if (SymbolCompat.textureRenamings.TryGetValue(texture, out result))
		{
			return result;
		}
		return texture;
	}

	// Token: 0x06001FEB RID: 8171 RVA: 0x000E8064 File Offset: 0x000E6464
	public static string PaintRenamings(string paint)
	{
		string result;
		if (SymbolCompat.paintRenamings.TryGetValue(paint, out result))
		{
			return result;
		}
		return paint;
	}

	// Token: 0x06001FEC RID: 8172 RVA: 0x000E8088 File Offset: 0x000E6488
	public static void Init()
	{
		SymbolCompat.predicateRenamings = SymbolCompat.ReadStringMapping("PredicateRenamings", SymbolCompat.predicateInvRenamings);
		SymbolCompat.blockRenamings = SymbolCompat.ReadStringMapping("BlockRenamings", SymbolCompat.blockInvRenamings);
		SymbolCompat.textureRenamings = SymbolCompat.ReadStringMapping("TextureRenamings", SymbolCompat.textureInvRenamings);
		SymbolCompat.paintRenamings = SymbolCompat.ReadStringMapping("PaintRenamings", SymbolCompat.paintInvRenamings);
		foreach (KeyValuePair<int, Vector3> keyValuePair in SymbolCompat.compactVectors)
		{
			SymbolCompat.compactVectorsInv[keyValuePair.Value] = keyValuePair.Key;
		}
	}

	// Token: 0x04001AE3 RID: 6883
	public const float OLD_WAIT_TIME = 0.24f;

	// Token: 0x04001AE4 RID: 6884
	private const string READ_ONLY = "_read_only";

	// Token: 0x04001AE5 RID: 6885
	private static Dictionary<string, string> predicateRenamings;

	// Token: 0x04001AE6 RID: 6886
	private static Dictionary<string, string> blockRenamings;

	// Token: 0x04001AE7 RID: 6887
	private static Dictionary<string, string> textureRenamings;

	// Token: 0x04001AE8 RID: 6888
	private static Dictionary<string, string> paintRenamings;

	// Token: 0x04001AE9 RID: 6889
	public static Dictionary<string, string> predicateInvRenamings = new Dictionary<string, string>();

	// Token: 0x04001AEA RID: 6890
	public static Dictionary<string, string> blockInvRenamings = new Dictionary<string, string>();

	// Token: 0x04001AEB RID: 6891
	public static Dictionary<string, string> textureInvRenamings = new Dictionary<string, string>();

	// Token: 0x04001AEC RID: 6892
	public static Dictionary<string, string> paintInvRenamings = new Dictionary<string, string>();

	// Token: 0x04001AED RID: 6893
	public static Dictionary<int, Vector3> compactVectors = new Dictionary<int, Vector3>
	{
		{
			0,
			Vector3.zero
		},
		{
			1,
			Vector3.one
		},
		{
			2,
			Vector3.up
		},
		{
			3,
			Vector3.down
		},
		{
			4,
			Vector3.left
		},
		{
			5,
			Vector3.right
		},
		{
			6,
			Vector3.forward
		},
		{
			7,
			Vector3.back
		},
		{
			8,
			Vector3.up * 90f
		},
		{
			9,
			Vector3.up * 180f
		},
		{
			10,
			Vector3.up * 270f
		}
	};

	// Token: 0x04001AEE RID: 6894
	public static Dictionary<Vector3, int> compactVectorsInv = new Dictionary<Vector3, int>();
}
