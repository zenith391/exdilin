using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Blocks;
using SimpleJSON;
using UnityEngine;

// Token: 0x020001E2 RID: 482
public class ModelUtils
{
	// Token: 0x060018B3 RID: 6323 RVA: 0x000ADAA0 File Offset: 0x000ABEA0
	public static void WriteJSONForModel(JSONStreamEncoder encoder, List<List<List<Tile>>> model)
	{
		encoder.BeginArray();
		if (model == null)
		{
			BWLog.Warning("No model sent to WriteJSONForModel");
			return;
		}
		foreach (List<List<Tile>> list in model)
		{
			encoder.BeginObject();
			encoder.WriteKey("tile-rows");
			encoder.BeginArray();
			foreach (List<Tile> list2 in list)
			{
				encoder.BeginArray();
				foreach (Tile tile in list2)
				{
					tile.ToJSON(encoder, false);
				}
				encoder.EndArray();
			}
			encoder.EndArray();
			encoder.EndObject();
		}
		encoder.EndArray();
	}

	// Token: 0x060018B4 RID: 6324 RVA: 0x000ADBC8 File Offset: 0x000ABFC8
	public static List<List<List<Tile>>> GetModelTiles(List<Block> model)
	{
		List<List<List<Tile>>> list = new List<List<List<Tile>>>();
		foreach (Block block in model)
		{
			list.Add(block.tiles);
		}
		return list;
	}

	// Token: 0x060018B5 RID: 6325 RVA: 0x000ADC2C File Offset: 0x000AC02C
	public static string GetJSONForModel(List<List<List<Tile>>> model)
	{
		StringBuilder stringBuilder = new StringBuilder(32768);
		StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		JSONStreamEncoder encoder = new JSONStreamEncoder(writer, 20);
		ModelUtils.WriteJSONForModel(encoder, model);
		return stringBuilder.ToString();
	}

	// Token: 0x060018B6 RID: 6326 RVA: 0x000ADC68 File Offset: 0x000AC068
	public static List<List<List<Tile>>> ParseModelString(string str)
	{
		JObject jobject = null;
		try
		{
			jobject = JSONDecoder.Decode(str);
		}
		catch
		{
			BWLog.Error("Failed to decode model json string ");
		}
		if (jobject == null)
		{
			return null;
		}
		return ModelUtils.ParseModelJSON(jobject);
	}

	// Token: 0x060018B7 RID: 6327 RVA: 0x000ADCB4 File Offset: 0x000AC0B4
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
		foreach (JObject obj2 in arrayValue)
		{
			List<List<Tile>> list2 = Blocksworld.bw.LoadJSONTiles(Blocksworld.GetTileRows(obj2));
			if (list2 == null)
			{
				return null;
			}
			list.Add(list2);
		}
		return list;
	}

	// Token: 0x17000073 RID: 115
	// (get) Token: 0x060018B8 RID: 6328 RVA: 0x000ADD58 File Offset: 0x000AC158
	public static string ChecksumVersion
	{
		get
		{
			return "_v01_";
		}
	}

	// Token: 0x060018B9 RID: 6329 RVA: 0x000ADD60 File Offset: 0x000AC160
	public static string GenerateHashString(List<List<List<Tile>>> blockList)
	{
		List<ModelUtils.blockChecksumInfo> list = new List<ModelUtils.blockChecksumInfo>();
		for (int i = 0; i < blockList.Count; i++)
		{
			List<List<Tile>> list2 = blockList[i];
			List<Tile> list3 = list2[0];
			Vector3 vector = (Vector3)list3[2].gaf.Args[0];
			Quaternion rotation = Quaternion.Euler((Vector3)list3[3].gaf.Args[0]);
			Vector3 vector2 = (list3.Count <= 6) ? Vector3.one : ((Vector3)list3[6].gaf.Args[0]);
			int x = Mathf.RoundToInt(10f * vector.x);
			int y = Mathf.RoundToInt(10f * vector.y);
			int z = Mathf.RoundToInt(10f * vector.z);
			ModelUtils.blockChecksumInfo blockChecksumInfo = new ModelUtils.blockChecksumInfo();
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
		List<string> list5 = new List<string>(ModelUtils.allOrientations.Length);
		for (int n = 0; n < ModelUtils.allOrientations.Length; n++)
		{
			ModelUtils.ModelOrientation modelOrientation = ModelUtils.allOrientations[n];
			for (int num = 0; num < list.Count; num++)
			{
				list[num].SetOrientation(modelOrientation);
			}
			list.Sort();
			string text = string.Empty;
			foreach (ModelUtils.blockChecksumInfo blockChecksumInfo2 in list)
			{
				text = text + blockChecksumInfo2.alphaOrder + ", ";
			}
			ModelUtils.blockChecksumInfo blockChecksumInfo3 = list[0];
			Quaternion rotation2 = Quaternion.Inverse(modelOrientation.baseRotation) * blockChecksumInfo3.rotation;
			int[] array = modelOrientation.TransformXYZ(blockChecksumInfo3.x, blockChecksumInfo3.y, blockChecksumInfo3.z);
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				int[] array2 = modelOrientation.TransformXYZ(list[num2].x, list[num2].y, list[num2].z);
				list[num2].relX = array2[0] - array[0];
				list[num2].relY = array2[1] - array[1];
				list[num2].relZ = array2[2] - array[2];
				Quaternion rhs = Quaternion.Inverse(modelOrientation.baseRotation) * list[num2].rotation;
				Quaternion relRotation = Quaternion.Inverse(rotation2) * rhs;
				list[num2].relRotation = relRotation;
			}
			StringBuilder stringBuilder3 = new StringBuilder(4096);
			stringBuilder3.Append(text);
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				ModelUtils.blockChecksumInfo blockChecksumInfo4 = list[num3];
				stringBuilder3.Append("##" + blockChecksumInfo4.alphaOrder.ToString() + "##");
				stringBuilder3.Append("p:");
				stringBuilder3.Append(blockChecksumInfo4.relX.ToString("F1") + ",");
				stringBuilder3.Append(blockChecksumInfo4.relY.ToString("F1") + ",");
				stringBuilder3.Append(blockChecksumInfo4.relZ.ToString("F1"));
				stringBuilder3.Append(",r:");
				Vector3 eulerAngles = blockChecksumInfo4.relRotation.eulerAngles;
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
					stringBuilder3.Append(Mathf.RoundToInt((float)num5).ToString("F0") + ",");
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
		using (MD5 md = MD5.Create())
		{
			text2 = BitConverter.ToString(md.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", string.Empty);
		}
		text2 = ModelUtils.ChecksumVersion + text2;
		return text2;
	}

	// Token: 0x060018BA RID: 6330 RVA: 0x000AE3DC File Offset: 0x000AC7DC
	public static bool IsValidShortTitle(string shortTitleStr)
	{
		if (ModelUtils._labelFont == null)
		{
			ModelUtils._labelFont = Resources.Load<Font>("TileLabelFontBold SD");
		}
		return ModelUtils.IsValidShortTitleWithFont(shortTitleStr, ModelUtils._labelFont);
	}

	// Token: 0x060018BB RID: 6331 RVA: 0x000AE418 File Offset: 0x000AC818
	private static bool IsValidShortTitleWithFont(string shortTitleStr, Font font)
	{
		font.RequestCharactersInTexture(shortTitleStr);
		float num = 0f;
		foreach (char ch in shortTitleStr)
		{
			CharacterInfo characterInfo;
			if (font.GetCharacterInfo(ch, out characterInfo))
			{
				num += (float)characterInfo.advance;
			}
		}
		return num < 64f;
	}

	// Token: 0x060018BC RID: 6332 RVA: 0x000AE474 File Offset: 0x000AC874
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

	// Token: 0x060018BD RID: 6333 RVA: 0x000AE4B3 File Offset: 0x000AC8B3
	private static string GetStickNameWithoutNameClash(GAF gaf)
	{
		return "S_" + ModelUtils.GetStickName(gaf);
	}

	// Token: 0x060018BE RID: 6334 RVA: 0x000AE4C8 File Offset: 0x000AC8C8
	private static void CreateIfNullAndAdd(ref Dictionary<string, HashSet<GAF>> dict, string key, GAF gaf)
	{
		if (dict == null)
		{
			dict = new Dictionary<string, HashSet<GAF>>();
		}
		HashSet<GAF> hashSet;
		if (!dict.TryGetValue(key, out hashSet))
		{
			hashSet = new HashSet<GAF>();
			dict[key] = hashSet;
		}
		hashSet.Add(gaf);
	}

	// Token: 0x060018BF RID: 6335 RVA: 0x000AE50C File Offset: 0x000AC90C
	public static void CheckModelConflictInputGAFs(List<Block> modelBlocks, List<Block> worldBlocks)
	{
		HashSet<Block> hashSet = ModelUtils.tempBlockSet;
		hashSet.Clear();
		hashSet.UnionWith(modelBlocks);
		HashSet<Predicate> possibleModelConflictingInputPredicates = Block.GetPossibleModelConflictingInputPredicates();
		HashSet<Predicate> analogStickPredicates = Blocksworld.GetAnalogStickPredicates();
		HashSet<GAF> hashSet2 = ModelUtils.tempGafSet;
		HashSet<string> hashSet3 = ModelUtils.tempStringSet1;
		HashSet<string> hashSet4 = ModelUtils.tempStringSet2;
		bool flag = false;
		bool flag2 = false;
		hashSet3.Clear();
		hashSet4.Clear();
		Dictionary<string, HashSet<GAF>> dictionary = null;
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
							ModelUtils.CreateIfNullAndAdd(ref dictionary, text, gaf);
						}
						else if (analogStickPredicates.Contains(gaf.Predicate))
						{
							hashSet4.Add(ModelUtils.GetStickNameWithoutNameClash(gaf));
							ModelUtils.CreateIfNullAndAdd(ref dictionary, ModelUtils.GetStickNameWithoutNameClash(gaf), gaf);
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
			if (!hashSet.Contains(block2))
			{
				for (int m = 0; m < block2.tiles.Count; m++)
				{
					List<Tile> list3 = block2.tiles[m];
					for (int n = 0; n < list3.Count; n++)
					{
						GAF gaf2 = list3[n].gaf;
						if (possibleModelConflictingInputPredicates.Contains(gaf2.Predicate))
						{
							if (gaf2.Predicate == Block.predicateButton)
							{
								string text2 = (string)gaf2.Args[0];
								if (hashSet3.Contains(text2))
								{
									hashSet2.UnionWith(dictionary[text2]);
								}
							}
							else if (analogStickPredicates.Contains(gaf2.Predicate) && hashSet4.Contains(ModelUtils.GetStickNameWithoutNameClash(gaf2)))
							{
								hashSet2.UnionWith(dictionary[ModelUtils.GetStickNameWithoutNameClash(gaf2)]);
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
			}
		}
		hashSet2.Clear();
		hashSet3.Clear();
		hashSet4.Clear();
		hashSet.Clear();
	}

	// Token: 0x0400139B RID: 5019
	private static ModelUtils.ModelOrientation[] allOrientations = new ModelUtils.ModelOrientation[]
	{
		new ModelUtils.ModelOrientation(1, false, 0),
		new ModelUtils.ModelOrientation(1, false, 1),
		new ModelUtils.ModelOrientation(1, false, 2),
		new ModelUtils.ModelOrientation(1, false, 3)
	};

	// Token: 0x0400139C RID: 5020
	private static Font _labelFont;

	// Token: 0x0400139D RID: 5021
	private const float maxWidthForShortTitle = 64f;

	// Token: 0x0400139E RID: 5022
	private static HashSet<Block> tempBlockSet = new HashSet<Block>();

	// Token: 0x0400139F RID: 5023
	private static HashSet<string> tempStringSet1 = new HashSet<string>();

	// Token: 0x040013A0 RID: 5024
	private static HashSet<string> tempStringSet2 = new HashSet<string>();

	// Token: 0x040013A1 RID: 5025
	private static HashSet<GAF> tempGafSet = new HashSet<GAF>();

	// Token: 0x020001E3 RID: 483
	private class blockChecksumInfo : IComparable
	{
		// Token: 0x060018C2 RID: 6338 RVA: 0x000AE898 File Offset: 0x000ACC98
		public void SetOrientation(ModelUtils.ModelOrientation orientation)
		{
			this.orientation = orientation;
		}

		// Token: 0x060018C3 RID: 6339 RVA: 0x000AE8A4 File Offset: 0x000ACCA4
		public int Ordering()
		{
			if (this.orientation == null)
			{
				return 0;
			}
			int[] array = this.orientation.TransformXYZ(this.x, this.y, this.z);
			return array[0] * 10000000 + array[1] * 1000 + array[2];
		}

		// Token: 0x060018C4 RID: 6340 RVA: 0x000AE8F4 File Offset: 0x000ACCF4
		public int CompareTo(object obj)
		{
			if (!(obj is ModelUtils.blockChecksumInfo))
			{
				return 0;
			}
			ModelUtils.blockChecksumInfo blockChecksumInfo = (ModelUtils.blockChecksumInfo)obj;
			if (this.orientation == null)
			{
				return this.blockChecksumStr.CompareTo(blockChecksumInfo.blockChecksumStr);
			}
			int[] array = this.orientation.TransformXYZ(this.x, this.y, this.z);
			int[] array2 = this.orientation.TransformXYZ(blockChecksumInfo.x, blockChecksumInfo.y, blockChecksumInfo.z);
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
			return this.alphaOrder.CompareTo(blockChecksumInfo.alphaOrder);
		}

		// Token: 0x040013A2 RID: 5026
		public int x;

		// Token: 0x040013A3 RID: 5027
		public int y;

		// Token: 0x040013A4 RID: 5028
		public int z;

		// Token: 0x040013A5 RID: 5029
		public int relX;

		// Token: 0x040013A6 RID: 5030
		public int relY;

		// Token: 0x040013A7 RID: 5031
		public int relZ;

		// Token: 0x040013A8 RID: 5032
		public string blockChecksumStr;

		// Token: 0x040013A9 RID: 5033
		public Quaternion rotation;

		// Token: 0x040013AA RID: 5034
		public Quaternion relRotation;

		// Token: 0x040013AB RID: 5035
		public int alphaOrder;

		// Token: 0x040013AC RID: 5036
		private ModelUtils.ModelOrientation orientation;
	}

	// Token: 0x020001E4 RID: 484
	public class ModelOrientation
	{
		// Token: 0x060018C5 RID: 6341 RVA: 0x000AE9AC File Offset: 0x000ACDAC
		public ModelOrientation(int majorAxis, bool reversed, int angle)
		{
			Vector3 axis = new Vector3((float)((majorAxis != 0) ? 0 : 1), (float)((majorAxis != 1) ? 0 : 1), (float)((majorAxis != 2) ? 0 : 1));
			this.baseRotation = Quaternion.AngleAxis((float)angle * 90f, axis);
			Vector3 v = this.baseRotation * Vector3.right;
			Vector3 v2 = this.baseRotation * Vector3.up;
			Vector3 v3 = this.baseRotation * Vector3.forward;
			this.GetDirectionsFromVector(v, ref this.tx, ref this.xFlip);
			this.GetDirectionsFromVector(v2, ref this.ty, ref this.yFlip);
			this.GetDirectionsFromVector(v3, ref this.tz, ref this.zFlip);
		}

		// Token: 0x060018C6 RID: 6342 RVA: 0x000AEA74 File Offset: 0x000ACE74
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

		// Token: 0x060018C7 RID: 6343 RVA: 0x000AEB24 File Offset: 0x000ACF24
		public int[] TransformXYZ(int x, int y, int z)
		{
			int[] array = new int[]
			{
				x,
				y,
				z
			};
			return new int[]
			{
				array[this.tx] * ((!this.xFlip) ? 1 : -1),
				array[this.ty] * ((!this.yFlip) ? 1 : -1),
				array[this.tz] * ((!this.zFlip) ? 1 : -1)
			};
		}

		// Token: 0x040013AD RID: 5037
		public int tx;

		// Token: 0x040013AE RID: 5038
		public int ty;

		// Token: 0x040013AF RID: 5039
		public int tz;

		// Token: 0x040013B0 RID: 5040
		public bool xFlip;

		// Token: 0x040013B1 RID: 5041
		public bool yFlip;

		// Token: 0x040013B2 RID: 5042
		public bool zFlip;

		// Token: 0x040013B3 RID: 5043
		public Quaternion baseRotation;
	}
}
