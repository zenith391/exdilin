using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x0200037C RID: 892
	public class SimpleScript : MonoBehaviour
	{
		// Token: 0x060027B0 RID: 10160 RVA: 0x00123230 File Offset: 0x00121630
		private void Start()
		{
			this.m_textMeshPro = base.gameObject.AddComponent<TextMeshPro>();
			this.m_textMeshPro.autoSizeTextContainer = true;
			this.m_textMeshPro.fontSize = 48f;
			this.m_textMeshPro.alignment = TextAlignmentOptions.Center;
			this.m_textMeshPro.enableWordWrapping = false;
		}

		// Token: 0x060027B1 RID: 10161 RVA: 0x00123286 File Offset: 0x00121686
		private void Update()
		{
			this.m_textMeshPro.SetText("The <#0050FF>count is: </color>{0:2}", this.m_frame % 1000f);
			this.m_frame += 1f * Time.deltaTime;
		}

		// Token: 0x040022AC RID: 8876
		private TextMeshPro m_textMeshPro;

		// Token: 0x040022AD RID: 8877
		private const string label = "The <#0050FF>count is: </color>{0:2}";

		// Token: 0x040022AE RID: 8878
		private float m_frame;
	}
}
