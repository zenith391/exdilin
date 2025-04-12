using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000373 RID: 883
	public class Benchmark03 : MonoBehaviour
	{
		// Token: 0x06002798 RID: 10136 RVA: 0x001221B4 File Offset: 0x001205B4
		private void Awake()
		{
		}

		// Token: 0x06002799 RID: 10137 RVA: 0x001221B8 File Offset: 0x001205B8
		private void Start()
		{
			for (int i = 0; i < this.NumberOfNPC; i++)
			{
				if (this.SpawnType == 0)
				{
					TextMeshPro textMeshPro = new GameObject
					{
						transform = 
						{
							position = new Vector3(0f, 0f, 0f)
						}
					}.AddComponent<TextMeshPro>();
					textMeshPro.alignment = TextAlignmentOptions.Center;
					textMeshPro.fontSize = 96f;
					textMeshPro.text = "@";
					textMeshPro.color = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
				}
				else
				{
					TextMesh textMesh = new GameObject
					{
						transform = 
						{
							position = new Vector3(0f, 0f, 0f)
						}
					}.AddComponent<TextMesh>();
					textMesh.GetComponent<Renderer>().sharedMaterial = this.TheFont.material;
					textMesh.font = this.TheFont;
					textMesh.anchor = TextAnchor.MiddleCenter;
					textMesh.fontSize = 96;
					textMesh.color = new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
					textMesh.text = "@";
				}
			}
		}

		// Token: 0x0400226F RID: 8815
		public int SpawnType;

		// Token: 0x04002270 RID: 8816
		public int NumberOfNPC = 12;

		// Token: 0x04002271 RID: 8817
		public Font TheFont;
	}
}
