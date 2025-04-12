using System;
using UnityEngine;

namespace AmplifyOcclusion
{
	// Token: 0x0200000A RID: 10
	[Serializable]
	public class VersionInfo
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00003C9D File Offset: 0x0000209D
		private VersionInfo()
		{
			this.m_major = 1;
			this.m_minor = 2;
			this.m_release = 2;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003CBA File Offset: 0x000020BA
		private VersionInfo(byte major, byte minor, byte release)
		{
			this.m_major = (int)major;
			this.m_minor = (int)minor;
			this.m_release = (int)release;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003CD7 File Offset: 0x000020D7
		public static string StaticToString()
		{
			return string.Format("{0}.{1}.{2}", 1, 2, 2) + VersionInfo.StageSuffix;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003CFF File Offset: 0x000020FF
		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", this.m_major, this.m_minor, this.m_release) + VersionInfo.StageSuffix;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00003D36 File Offset: 0x00002136
		public int Number
		{
			get
			{
				return this.m_major * 100 + this.m_minor * 10 + this.m_release;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003D52 File Offset: 0x00002152
		public static VersionInfo Current()
		{
			return new VersionInfo(1, 2, 2);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003D5C File Offset: 0x0000215C
		public static bool Matches(VersionInfo version)
		{
			return version.m_major == 1 && version.m_minor == 2 && 2 == version.m_release;
		}

		// Token: 0x0400008E RID: 142
		public const byte Major = 1;

		// Token: 0x0400008F RID: 143
		public const byte Minor = 2;

		// Token: 0x04000090 RID: 144
		public const byte Release = 2;

		// Token: 0x04000091 RID: 145
		private static string StageSuffix = "_dev001";

		// Token: 0x04000092 RID: 146
		[SerializeField]
		private int m_major;

		// Token: 0x04000093 RID: 147
		[SerializeField]
		private int m_minor;

		// Token: 0x04000094 RID: 148
		[SerializeField]
		private int m_release;
	}
}
