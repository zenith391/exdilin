using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000389 RID: 905
	[ExecuteInEditMode]
	public class TMP_TextInfoDebugTool : MonoBehaviour
	{
		// Token: 0x040022D6 RID: 8918
		public bool ShowCharacters;

		// Token: 0x040022D7 RID: 8919
		public bool ShowWords;

		// Token: 0x040022D8 RID: 8920
		public bool ShowLinks;

		// Token: 0x040022D9 RID: 8921
		public bool ShowLines;

		// Token: 0x040022DA RID: 8922
		public bool ShowMeshBounds;

		// Token: 0x040022DB RID: 8923
		public bool ShowTextBounds;

		// Token: 0x040022DC RID: 8924
		[Space(10f)]
		[TextArea(2, 2)]
		public string ObjectStats;

		// Token: 0x040022DD RID: 8925
		private TMP_Text m_TextComponent;

		// Token: 0x040022DE RID: 8926
		private Transform m_Transform;
	}
}
