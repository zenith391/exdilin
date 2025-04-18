﻿using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000393 RID: 915
	public class TextMeshSpawner : MonoBehaviour
	{
		// Token: 0x06002807 RID: 10247 RVA: 0x00126F98 File Offset: 0x00125398
		private void Awake()
		{
		}

		// Token: 0x06002808 RID: 10248 RVA: 0x00126F9C File Offset: 0x0012539C
		private void Start()
		{
			for (int i = 0; i < this.NumberOfNPC; i++)
			{
				if (this.SpawnType == 0)
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.position = new Vector3(UnityEngine.Random.Range(-95f, 95f), 0.5f, UnityEngine.Random.Range(-95f, 95f));
					TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
					textMeshPro.fontSize = 96f;
					textMeshPro.text = "!";
					textMeshPro.color = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
					this.floatingText_Script = gameObject.AddComponent<TextMeshProFloatingText>();
					this.floatingText_Script.SpawnType = 0;
				}
				else
				{
					GameObject gameObject2 = new GameObject();
					gameObject2.transform.position = new Vector3(UnityEngine.Random.Range(-95f, 95f), 0.5f, UnityEngine.Random.Range(-95f, 95f));
					TextMesh textMesh = gameObject2.AddComponent<TextMesh>();
					textMesh.GetComponent<Renderer>().sharedMaterial = this.TheFont.material;
					textMesh.font = this.TheFont;
					textMesh.anchor = TextAnchor.LowerCenter;
					textMesh.fontSize = 96;
					textMesh.color = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
					textMesh.text = "!";
					this.floatingText_Script = gameObject2.AddComponent<TextMeshProFloatingText>();
					this.floatingText_Script.SpawnType = 1;
				}
			}
		}

		// Token: 0x0400231B RID: 8987
		public int SpawnType;

		// Token: 0x0400231C RID: 8988
		public int NumberOfNPC = 12;

		// Token: 0x0400231D RID: 8989
		public Font TheFont;

		// Token: 0x0400231E RID: 8990
		private TextMeshProFloatingText floatingText_Script;
	}
}
