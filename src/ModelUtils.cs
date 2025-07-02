using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Blocks;
using SimpleJSON;
using UnityEngine;

public class ModelUtils
{
	private class blockChecksumInfo : IComparable
	{
		public int x;

		public int y;

		public int z;

		public int relX;

		public int relY;

		public int relZ;

		public string blockChecksumStr;

		public Quaternion rotation;

		public Quaternion relRotation;

		public int alphaOrder;

		private ModelOrientation orientation;

		public void SetOrientation(ModelOrientation orientation)
		{
			this.orientation = orientation;
		}

		public int Ordering()
		{
			if (orientation == null)
			{
				return 0;
			}
			int[] array = orientation.TransformXYZ(x, y, z);
			return array[0] * 10000000 + array[1] * 1000 + array[2];
		}

		public int CompareTo(object obj)
		{
			if (!(obj is blockChecksumInfo))
			{
				return 0;
			}
			blockChecksumInfo blockChecksumInfo = (blockChecksumInfo)obj;
			if (orientation == null)
			{
				return blockChecksumStr.CompareTo(blockChecksumInfo.blockChecksumStr);
			}
			int[] array = orientation.TransformXYZ(x, y, z);
			int[] array2 = orientation.TransformXYZ(blockChecksumInfo.x, blockChecksumInfo.y, blockChecksumInfo.z);
			for (int i = 0; i < 3; i++)
			{
				if (array[i] > array2[i])
				{
					return 1;
				}
				if (array[i] < array2[i])
				{
					return -1;
				}
			}
			return alphaOrder.CompareTo(blockChecksumInfo.alphaOrder);
		}
	}

	public class ModelOrientation
	{
		public int tx;

		public int ty;

		public int tz;

		public bool xFlip;

		public bool yFlip;

		public bool zFlip;

		public Quaternion baseRotation;

		public ModelOrientation(int majorAxis, bool reversed, int angle)
		{
			Vector3 axis = new Vector3((majorAxis == 0) ? 1 : 0, (majorAxis == 1) ? 1 : 0, (majorAxis == 2) ? 1 : 0);
			baseRotation = Quaternion.AngleAxis((float)angle * 90f, axis);
			Vector3 v = baseRotation * Vector3.right;
			Vector3 v2 = baseRotation * Vector3.up;
			Vector3 v3 = baseRotation * Vector3.forward;
			GetDirectionsFromVector(v, ref tx, ref xFlip);
			GetDirectionsFromVector(v2, ref ty, ref yFlip);
			GetDirectionsFromVector(v3, ref tz, ref zFlip);
		}

		private void GetDirectionsFromVector(Vector3 v, ref int t, ref bool f)
		{
			if (v.x > 0.5f)
			{
				t = 0;
				f = false;
			}
			else if (v.x < -0.5f)
			{
				t = 0;
				f = true;
			}
			else if (v.y > 0.5f)
			{
				t = 1;
				f = false;
			}
			else if (v.y < -0.5f)
			{
				t = 1;
				f = true;
			}
			else if (v.z > 0.5f)
			{
				t = 2;
				f = false;
			}
			else if (v.z < -0.5f)
			{
				t = 2;
				f = true;
			}
		}

		public int[] TransformXYZ(int x, int y, int z)
		{
			int[] array = new int[3] { x, y, z };
			return new int[3]
			{
				array[tx] * ((!xFlip) ? 1 : (-1)),
				array[ty] * ((!yFlip) ? 1 : (-1)),
				array[tz] * ((!zFlip) ? 1 : (-1))
			};
		}
	}

	private static ModelOrientation[] allOrientations = new ModelOrientation[4]
	{
		new ModelOrientation(1, reversed: false, 0),
		new ModelOrientation(1, reversed: false, 1),
		new ModelOrientation(1, reversed: false, 2),
		new ModelOrientation(1, reversed: false, 3)
	};

	private static Font _labelFont;

	private const float maxWidthForShortTitle = 64f;

	private static HashSet<Block> tempBlockSet = new HashSet<Block>();

	private static HashSet<string> tempStringSet1 = new HashSet<string>();

	private static HashSet<string> tempStringSet2 = new HashSet<string>();

	private static HashSet<GAF> tempGafSet = new HashSet<GAF>();

	public static string ChecksumVersion => "_v01_";

	public static void WriteJSONForModel(JSONStreamEncoder encoder, List<List<List<Tile>>> model)
	{
		encoder.BeginArray();
		if (model == null)
		{
			BWLog.Warning("No model sent to WriteJSONForModel");
			return;
		}
		foreach (List<List<Tile>> item in model)
		{
			encoder.BeginObject();
			encoder.WriteKey("tile-rows");
			encoder.BeginArray();
			foreach (List<Tile> item2 in item)
			{
				encoder.BeginArray();
				foreach (Tile item3 in item2)
				{
					item3.ToJSON(encoder);
				}
				encoder.EndArray();
			}
			encoder.EndArray();
			encoder.EndObject();
		}
		encoder.EndArray();
	}

	public static List<List<List<Tile>>> GetModelTiles(List<Block> model)
	{
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (Block item in model)
		{
			list.Add(item.tiles);
		}
		return list;
	}

	public static string GetJSONForModel(List<List<List<Tile>>> model)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder encoder = new JSONStreamEncoder(writer);
		WriteJSONForModel(encoder, model);
		return stringBuilder.ToString();
	}

	public static List<List<List<Tile>>> ParseModelString(string str)
	{
		JObject jObject = null;
		try
		{
			jObject = JSONDecoder.Decode(str);
		}
		catch
		{
			BWLog.Error("Failed to decode model json string ");
		}
		if (jObject == null)
		{
			return null;
		}
		return ParseModelJSON(jObject);
	}

	public static List<List<List<Tile>>> ParseModelJSON(JObject obj)
	{
		if (obj == null)
		{
			return null;
		}
		List<JObject> arrayValue = obj.ArrayValue;
		if (arrayValue == null)
		{
			BWLog.Error("failed to parse block list");
			return null;
		}
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (JObject item in arrayValue)
		{
			List<List<Tile>> list2 = Blocksworld.bw.LoadJSONTiles(Blocksworld.GetTileRows(item));
			if (list2 == null)
			{
				return null;
			}
			list.Add(list2);
		}
		return list;
	}

	public static string GenerateHashString(List<List<List<Tile>>> blockList)
	{
		List<blockChecksumInfo> list = new List<blockChecksumInfo>();
		for (int i = 0; i < blockList.Count; i++)
		{
			List<List<Tile>> list2 = blockList[i];
			List<Tile> list3 = list2[0];
			Vector3 vector = (Vector3)list3[2].gaf.Args[0];
			Quaternion rotation = Quaternion.Euler((Vector3)list3[3].gaf.Args[0]);
			Vector3 vector2 = ((list3.Count <= 6) ? Vector3.one : ((Vector3)list3[6].gaf.Args[0]));
			int x = Mathf.RoundToInt(10f * vector.x);
			int y = Mathf.RoundToInt(10f * vector.y);
			int z = Mathf.RoundToInt(10f * vector.z);
			blockChecksumInfo blockChecksumInfo = new blockChecksumInfo();
			blockChecksumInfo.x = x;
			blockChecksumInfo.y = y;
			blockChecksumInfo.z = z;
			blockChecksumInfo.rotation = rotation;
			StringBuilder stringBuilder = new StringBuilder(4096);
			for (int j = 0; j < list2.Count; j++)
			{
				List<Tile> list4 = list2[j];
				for (int k = 0; k < list4.Count; k++)
				{
					GAF gaf = list4[k].gaf;
					Predicate predicate = gaf.Predicate;
					if (predicate != Block.predicateMoveTo && predicate != Block.predicateScaleTo && predicate != Block.predicateRotateTo && predicate != Block.predicateGroup)
					{
						stringBuilder.Append(gaf.ToString());
					}
					else
					{
						stringBuilder.Append(predicate.Name);
					}
				}
			}
			for (int l = 0; l < 3; l++)
			{
				stringBuilder.Append(Mathf.RoundToInt(vector2[l] * 10f).ToString());
			}
			blockChecksumInfo.blockChecksumStr = stringBuilder.ToString();
			blockChecksumInfo.SetOrientation(null);
			list.Add(blockChecksumInfo);
		}
		StringBuilder stringBuilder2 = new StringBuilder(4096);
		list.Sort();
		for (int m = 0; m < list.Count; m++)
		{
			stringBuilder2.Append(list[m].blockChecksumStr);
			list[m].alphaOrder = m;
		}
		string value = stringBuilder2.ToString();
		List<string> list5 = new List<string>(allOrientations.Length);
		for (int n = 0; n < allOrientations.Length; n++)
		{
			ModelOrientation modelOrientation = allOrientations[n];
			for (int num = 0; num < list.Count; num++)
			{
				list[num].SetOrientation(modelOrientation);
			}
			list.Sort();
			string text = string.Empty;
			foreach (blockChecksumInfo item in list)
			{
				text = text + item.alphaOrder + ", ";
			}
			blockChecksumInfo blockChecksumInfo2 = list[0];
			Quaternion rotation2 = Quaternion.Inverse(modelOrientation.baseRotation) * blockChecksumInfo2.rotation;
			int[] array = modelOrientation.TransformXYZ(blockChecksumInfo2.x, blockChecksumInfo2.y, blockChecksumInfo2.z);
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				int[] array2 = modelOrientation.TransformXYZ(list[num2].x, list[num2].y, list[num2].z);
				list[num2].relX = array2[0] - array[0];
				list[num2].relY = array2[1] - array[1];
				list[num2].relZ = array2[2] - array[2];
				Quaternion quaternion = Quaternion.Inverse(modelOrientation.baseRotation) * list[num2].rotation;
				Quaternion relRotation = Quaternion.Inverse(rotation2) * quaternion;
				list[num2].relRotation = relRotation;
			}
			StringBuilder stringBuilder3 = new StringBuilder(4096);
			stringBuilder3.Append(text);
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				blockChecksumInfo blockChecksumInfo3 = list[num3];
				stringBuilder3.Append("##" + blockChecksumInfo3.alphaOrder + "##");
				stringBuilder3.Append("p:");
				stringBuilder3.Append(blockChecksumInfo3.relX.ToString("F1") + ",");
				stringBuilder3.Append(blockChecksumInfo3.relY.ToString("F1") + ",");
				stringBuilder3.Append(blockChecksumInfo3.relZ.ToString("F1"));
				stringBuilder3.Append(",r:");
				Vector3 eulerAngles = blockChecksumInfo3.relRotation.eulerAngles;
				for (int num4 = 0; num4 < 3; num4++)
				{
					int num5 = Mathf.RoundToInt(eulerAngles[num4]);
					if (num5 < 0)
					{
						num5 = 360 + num5;
					}
					else if (num5 >= 360)
					{
						num5 -= 360;
					}
					stringBuilder3.Append(Mathf.RoundToInt(num5).ToString("F0") + ",");
				}
			}
			list5.Add(stringBuilder3.ToString());
		}
		list5.Sort();
		StringBuilder stringBuilder4 = new StringBuilder(4096);
		for (int num6 = 0; num6 < list5.Count; num6++)
		{
			stringBuilder4.Append(list5[num6]);
		}
		stringBuilder4.Append(value);
		string s = stringBuilder4.ToString();
		string text2;
		using (MD5 mD = MD5.Create())
		{
			text2 = BitConverter.ToString(mD.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", string.Empty);
		}
		return ChecksumVersion + text2;
	}

	public static bool IsValidShortTitle(string shortTitleStr)
	{
		if (_labelFont == null)
		{
			_labelFont = Resources.Load<Font>("TileLabelFontBold SD");
		}
		return IsValidShortTitleWithFont(shortTitleStr, _labelFont);
	}

	private static bool IsValidShortTitleWithFont(string shortTitleStr, Font font)
	{
		font.RequestCharactersInTexture(shortTitleStr);
		float num = 0f;
		foreach (char ch in shortTitleStr)
		{
			if (font.GetCharacterInfo(ch, out var info))
			{
				num += (float)info.advance;
			}
		}
		return num < 64f;
	}

	private static string GetStickName(GAF gaf)
	{
		int index = 0;
		Predicate predicate = gaf.Predicate;
		if (predicate == Block.predicateDPadVertical || predicate == Block.predicateDPadHorizontal)
		{
			index = 1;
		}
		return Util.GetStringArg(gaf.Args, index, "L");
	}

	private static string GetStickNameWithoutNameClash(GAF gaf)
	{
		return "S_" + GetStickName(gaf);
	}

	private static void CreateIfNullAndAdd(ref Dictionary<string, HashSet<GAF>> dict, string key, GAF gaf)
	{
		if (dict == null)
		{
			dict = new Dictionary<string, HashSet<GAF>>();
		}
		if (!dict.TryGetValue(key, out var value))
		{
			value = new HashSet<GAF>();
			dict[key] = value;
		}
		value.Add(gaf);
	}

	public static void CheckModelConflictInputGAFs(List<Block> modelBlocks, List<Block> worldBlocks)
	{
		HashSet<Block> hashSet = tempBlockSet;
		hashSet.Clear();
		hashSet.UnionWith(modelBlocks);
		HashSet<Predicate> possibleModelConflictingInputPredicates = Block.GetPossibleModelConflictingInputPredicates();
		HashSet<Predicate> analogStickPredicates = Blocksworld.GetAnalogStickPredicates();
		HashSet<GAF> hashSet2 = tempGafSet;
		HashSet<string> hashSet3 = tempStringSet1;
		HashSet<string> hashSet4 = tempStringSet2;
		bool flag = false;
		bool flag2 = false;
		hashSet3.Clear();
		hashSet4.Clear();
		Dictionary<string, HashSet<GAF>> dict = null;
		for (int i = 0; i < modelBlocks.Count; i++)
		{
			Block block = modelBlocks[i];
			for (int j = 0; j < block.tiles.Count; j++)
			{
				List<Tile> list = block.tiles[j];
				for (int k = 0; k < list.Count; k++)
				{
					GAF gaf = list[k].gaf;
					if (possibleModelConflictingInputPredicates.Contains(gaf.Predicate))
					{
						if (gaf.Predicate == Block.predicateButton)
						{
							string text = (string)gaf.Args[0];
							hashSet3.Add(text);
							CreateIfNullAndAdd(ref dict, text, gaf);
						}
						else if (analogStickPredicates.Contains(gaf.Predicate))
						{
							hashSet4.Add(GetStickNameWithoutNameClash(gaf));
							CreateIfNullAndAdd(ref dict, GetStickNameWithoutNameClash(gaf), gaf);
						}
						else if (gaf.Predicate == Block.predicateTiltLeftRight)
						{
							flag = true;
						}
						else if (gaf.Predicate == Block.predicateTiltFrontBack)
						{
							flag2 = true;
						}
					}
				}
			}
		}
		List<Block> list2 = BWSceneManager.AllBlocks();
		for (int l = 0; l < list2.Count; l++)
		{
			Block block2 = list2[l];
			if (hashSet.Contains(block2))
			{
				continue;
			}
			for (int m = 0; m < block2.tiles.Count; m++)
			{
				List<Tile> list3 = block2.tiles[m];
				for (int n = 0; n < list3.Count; n++)
				{
					GAF gaf2 = list3[n].gaf;
					if (!possibleModelConflictingInputPredicates.Contains(gaf2.Predicate))
					{
						continue;
					}
					if (gaf2.Predicate == Block.predicateButton)
					{
						string text2 = (string)gaf2.Args[0];
						if (hashSet3.Contains(text2))
						{
							hashSet2.UnionWith(dict[text2]);
						}
					}
					else if (analogStickPredicates.Contains(gaf2.Predicate) && hashSet4.Contains(GetStickNameWithoutNameClash(gaf2)))
					{
						hashSet2.UnionWith(dict[GetStickNameWithoutNameClash(gaf2)]);
					}
					else if (flag && gaf2.Predicate == Block.predicateTiltLeftRight)
					{
						hashSet2.Add(gaf2);
					}
					else if (flag2 && gaf2.Predicate == Block.predicateTiltFrontBack)
					{
						hashSet2.Add(gaf2);
					}
				}
			}
		}
		hashSet2.Clear();
		hashSet3.Clear();
		hashSet4.Clear();
		hashSet.Clear();
	}
}
