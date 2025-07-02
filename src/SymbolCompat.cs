using System;
using System.Collections.Generic;
using Blocks;
using SimpleJSON;
using UnityEngine;

public static class SymbolCompat
{
	public const float OLD_WAIT_TIME = 0.24f;

	private const string READ_ONLY = "_read_only";

	private static Dictionary<string, string> predicateRenamings;

	private static Dictionary<string, string> blockRenamings;

	private static Dictionary<string, string> textureRenamings;

	private static Dictionary<string, string> paintRenamings;

	public static Dictionary<string, string> predicateInvRenamings = new Dictionary<string, string>();

	public static Dictionary<string, string> blockInvRenamings = new Dictionary<string, string>();

	public static Dictionary<string, string> textureInvRenamings = new Dictionary<string, string>();

	public static Dictionary<string, string> paintInvRenamings = new Dictionary<string, string>();

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

	public static Dictionary<Vector3, int> compactVectorsInv = new Dictionary<Vector3, int>();

	public static GAF ToGaf(JObject obj)
	{
		OldSymbol oldSymbol = (OldSymbol)Enum.Parse(typeof(OldSymbol), (string)obj["sym"]);
		switch (oldSymbol)
		{
		case OldSymbol.CreateCube1x1x1:
			return new GAF("Block.Create", "Cube");
		case OldSymbol.CreateWedge1x1x1:
			return new GAF("Block.Create", "Wedge");
		case OldSymbol.CreateRocket1x1x1:
			return new GAF("Block.Create", "Rocket");
		case OldSymbol.CreateMotor1x1x1:
			return new GAF("Block.Create", "Motor");
		case OldSymbol.CreateBody1x1x1:
			return new GAF("Block.Create", "Body");
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
			return new GAF("Block.PaintTo", oldSymbol.ToString().Substring(5));
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
			return new GAF("Block.TextureTo", oldSymbol.ToString().Substring(7), obj.Vector3Value());
		case OldSymbol.TextureFaceHappy:
			return new GAF("Block.TextureTo", "Happy", obj.Vector3Value());
		case OldSymbol.TextureFaceSurprised:
			return new GAF("Block.TextureTo", "Surprised", obj.Vector3Value());
		case OldSymbol.TextureEyes:
			return new GAF("Block.TextureTo", "Eye", obj.Vector3Value());
		case OldSymbol.TextureWood:
			return new GAF("Block.TextureTo", "Bricks", obj.Vector3Value());
		case OldSymbol.Stop:
			return new GAF("Meta.Stop");
		case OldSymbol.Play:
			return new GAF("Meta.Then");
		case OldSymbol.MoveTo:
			return new GAF("Block.MoveTo", obj.Vector3Value());
		case OldSymbol.RotateTo:
			return new GAF("Block.RotateTo", obj.Vector3Value());
		case OldSymbol.FireRocket:
			return new GAF("Rocket.Fire");
		case OldSymbol.Drive:
			return new GAF("Wheel.Drive", 20f);
		case OldSymbol.Break:
			return new GAF("Wheel.Drive", -20f);
		case OldSymbol.SteerLeft:
			return new GAF("Wheel.Turn", -30f);
		case OldSymbol.SteerRight:
			return new GAF("Wheel.Turn", 30f);
		case OldSymbol.Walk:
			return new GAF("Legs.Walk", 0.5f);
		case OldSymbol.Back:
			return new GAF("Legs.Walk", -0.5f);
		case OldSymbol.TurnLeft:
			return new GAF("Legs.Turn", -0.4f);
		case OldSymbol.TurnRight:
			return new GAF("Legs.Turn", 0.4f);
		case OldSymbol.Jump:
			return new GAF("Legs.Jump", 5f);
		case OldSymbol.Turn:
			return new GAF("Motor.Turn", -1f);
		case OldSymbol.Reverse:
			return new GAF("Motor.Turn", 1f);
		case OldSymbol.Fixed:
			return new GAF("Block.Fixed");
		case OldSymbol.Explode:
			return new GAF("Block.Explode");
		case OldSymbol.InputR1:
		case OldSymbol.InputL1:
		case OldSymbol.InputL2:
		case OldSymbol.InputR2:
		case OldSymbol.InputL3:
		case OldSymbol.InputR3:
		case OldSymbol.InputL4:
		case OldSymbol.InputR4:
			return new GAF("Block.ButtonInput", oldSymbol.ToString().Substring(5));
		case OldSymbol.TiltLeft:
			return new GAF("Block.DeviceTilt", -1f);
		case OldSymbol.TiltRight:
			return new GAF("Block.DeviceTilt", 1f);
		case OldSymbol.GameStart:
			return new GAF("Block.GameStart");
		case OldSymbol.BumpObject:
			return new GAF("Block.Bump", "object");
		case OldSymbol.BumpGround:
			return new GAF("Block.Bump", "ground");
		case OldSymbol.AngleLeft:
			return new GAF("Motor.Step", -1f);
		case OldSymbol.AngleRight:
			return new GAF("Motor.Step", 1f);
		case OldSymbol.Wait:
			return new GAF("Meta.Wait");
		case OldSymbol.Locked:
			return new GAF("Block.Locked");
		case OldSymbol.Inventory:
			return new GAF("Block.Inventory");
		case OldSymbol.Win:
			return new GAF("Block.Win");
		case OldSymbol.Smoke:
			return new GAF("Rocket.Smoke");
		case OldSymbol.FreeSpin:
			return new GAF("Motor.FreeSpin");
		case OldSymbol.Target:
			return new GAF("Block.Target");
		case OldSymbol.BumpTarget:
			return new GAF("Block.BumpTarget");
		case OldSymbol.CreateWheel1x1x1:
			return new GAF("Block.Create", "Wheel");
		case OldSymbol.CameraFollow:
			return new GAF("Block.CameraFollow");
		case OldSymbol.CreatePie1x1x1:
			return new GAF("Block.Create", "Slice");
		case OldSymbol.Speak:
			return new GAF("Block.Speak", (string)obj["string"]);
		case OldSymbol.CreateCylinder1x1x1:
			return new GAF("Block.Create", "Cylinder");
		case OldSymbol.CreateHead1x1x1:
			return new GAF("Block.Create", "Head");
		case OldSymbol.CreateCoreCube1x1x1:
			return new GAF("Block.Create", "Terrain Cube");
		case OldSymbol.CreateCoreWedge1x1x1:
			return new GAF("Block.Create", "Terrain Wedge");
		case OldSymbol.ScaleTo:
			return new GAF("Block.ScaleTo", obj.Vector3Value());
		case OldSymbol.CreateSky:
			return new GAF("Block.Create", "Sky");
		case OldSymbol.BreakOff:
			return new GAF("Block.BreakOff");
		case OldSymbol.CreateLegs1x1x1:
			return new GAF("Block.Create", "Legs");
		case OldSymbol.TapBlock:
			return new GAF("Block.Tap");
		case OldSymbol.SendA:
		case OldSymbol.SendB:
		case OldSymbol.SendC:
		case OldSymbol.SendD:
		case OldSymbol.SendE:
		case OldSymbol.SendF:
		case OldSymbol.SendG:
		case OldSymbol.SendH:
		case OldSymbol.SendI:
			return new GAF("Block.SendSignal", oldSymbol.ToString()[4] - 65);
		case OldSymbol.CreateLaser1x1x1:
			return new GAF("Block.Create", "Laser");
		case OldSymbol.LaserPulse:
			return new GAF("Laser.Pulse");
		case OldSymbol.LaserBeam:
			return new GAF("Laser.Beam");
		case OldSymbol.HitByLaserPulse:
			return new GAF("Laser.HitByPulse");
		case OldSymbol.HitByLaserBeam:
			return new GAF("Laser.HitByBeam");
		case OldSymbol.CreateCat1x1x1:
			return new GAF("Block.Create", "Cat");
		case OldSymbol.ExecuteAnimalBehavior:
			return new GAF("Animal.ExecuteBehavior");
		case OldSymbol.CreateCorner:
			return new GAF("Block.Create", "Corner");
		case OldSymbol.CreateRamp:
			return new GAF("Block.Create", "Slice Inverse");
		case OldSymbol.CreateWedgeInnerCorner:
			return new GAF("Block.Create", "Corner Inverse");
		case OldSymbol.CreateWedgeInnerEdge:
			return new GAF("Block.Create", "Wedge Inner Edge");
		case OldSymbol.CreateEmitter:
			return new GAF("Block.Create", "Emitter");
		case OldSymbol.EmitterEmit:
			return new GAF("Emitter.Emit");
		case OldSymbol.EmitterEmitSmoke:
			return new GAF("Emitter.EmitSmoke");
		case OldSymbol.EmitterEmitFast:
			return new GAF("Emitter.EmitFast");
		case OldSymbol.EmitterParticleHit:
			return new GAF("Emitter.HitByParticle");
		case OldSymbol.CreateStabilizer:
			return new GAF("Block.Create", "Stabilizer");
		case OldSymbol.StabilizerStop:
			return new GAF("Stabilizer.Stop");
		case OldSymbol.StabilizerStart:
			return new GAF("Stabilizer.Start");
		case OldSymbol.CreateWater:
			return new GAF("Block.Create", "Water");
		case OldSymbol.WaterWithin:
			return new GAF("Water.IsWithin");
		case OldSymbol.WaterStreamNorth:
			return new GAF("Water.StreamNorth");
		case OldSymbol.WaterStreamSouth:
			return new GAF("Water.StreamSouth");
		case OldSymbol.WaterStreamWest:
			return new GAF("Water.StreamWest");
		case OldSymbol.WaterStreamEast:
			return new GAF("Water.StreamEast");
		default:
			BWLog.Error("Could not convert symbol " + oldSymbol.ToString() + " to GAF");
			return null;
		}
	}

	private static Dictionary<string, string> ReadStringMapping(string filename, Dictionary<string, string> inverse = null)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		TextAsset textAsset = Resources.Load(filename) as TextAsset;
		if (textAsset == null)
		{
			BWLog.Info("Could not find file " + filename);
			return dictionary;
		}
		JObject jObject = JSONDecoder.Decode(textAsset.text);
		HashSet<string> hashSet = new HashSet<string>();
		Dictionary<string, JObject> objectValue = jObject.ObjectValue;
		foreach (KeyValuePair<string, JObject> item in objectValue)
		{
			if (item.Key == "_read_only")
			{
				foreach (JObject item2 in item.Value.ArrayValue)
				{
					hashSet.Add(item2.StringValue);
				}
			}
			else
			{
				string stringValue = item.Value.StringValue;
				if (stringValue != null)
				{
					dictionary[item.Key] = stringValue;
				}
			}
		}
		if (inverse != null)
		{
			foreach (KeyValuePair<string, string> item3 in dictionary)
			{
				string key = item3.Key;
				if (hashSet.Contains(item3.Value))
				{
					continue;
				}
				int result;
				if (inverse.ContainsKey(item3.Value))
				{
					string text = inverse[item3.Value];
					if (key.Length < text.Length)
					{
						inverse[item3.Value] = key;
					}
				}
				else if (key.Length == 1 || int.TryParse(key, out result))
				{
					inverse[item3.Value] = key;
				}
			}
		}
		return dictionary;
	}

	public static string RenamePredicate(string name)
	{
		if (predicateRenamings.TryGetValue(name, out var value))
		{
			return value;
		}
		return name;
	}

	public static GAF RenameGAF(GAF gaf)
	{
		Predicate predicate = gaf.Predicate;
		if (predicate == Block.predicateCreate)
		{
			string text = BlockRenamings((string)gaf.Args[0]);
			if (text != null)
			{
				object[] array = Util.CopyArray(gaf.Args);
				array[0] = text;
				return new GAF(predicate, array);
			}
		}
		else if (predicate == Block.predicateTextureTo)
		{
			string text2 = TextureRenamings((string)gaf.Args[0]);
			if (text2 != null)
			{
				object[] array2 = Util.CopyArray(gaf.Args);
				array2[0] = TextureRenamings((string)gaf.Args[0]);
				return new GAF(predicate, array2);
			}
		}
		else if (predicate == Block.predicatePaintTo)
		{
			string text3 = PaintRenamings((string)gaf.Args[0]);
			if (text3 != null)
			{
				object[] array3 = Util.CopyArray(gaf.Args);
				array3[0] = PaintRenamings((string)gaf.Args[0]);
				return new GAF(predicate, array3);
			}
		}
		else if (predicate == Block.predicateWaitTime && gaf.Args.Length == 0)
		{
			object[] args = new object[1] { 0.24f };
			return new GAF(predicate, args);
		}
		return gaf;
	}

	public static string BlockRenamings(string type)
	{
		if (blockRenamings.TryGetValue(type, out var value))
		{
			return value;
		}
		return type;
	}

	public static string TextureRenamings(string texture)
	{
		if (textureRenamings.TryGetValue(texture, out var value))
		{
			return value;
		}
		return texture;
	}

	public static string PaintRenamings(string paint)
	{
		if (paintRenamings.TryGetValue(paint, out var value))
		{
			return value;
		}
		return paint;
	}

	public static void Init()
	{
		predicateRenamings = ReadStringMapping("PredicateRenamings", predicateInvRenamings);
		blockRenamings = ReadStringMapping("BlockRenamings", blockInvRenamings);
		textureRenamings = ReadStringMapping("TextureRenamings", textureInvRenamings);
		paintRenamings = ReadStringMapping("PaintRenamings", paintInvRenamings);
		foreach (KeyValuePair<int, Vector3> compactVector in compactVectors)
		{
			compactVectorsInv[compactVector.Value] = compactVector.Key;
		}
	}
}
