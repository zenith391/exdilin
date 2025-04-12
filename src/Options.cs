using System;
using UnityEngine;

// Token: 0x0200025F RID: 607
public static class Options
{
	// Token: 0x17000091 RID: 145
	// (get) Token: 0x06001B5F RID: 7007 RVA: 0x000C6D48 File Offset: 0x000C5148
	// (set) Token: 0x06001B60 RID: 7008 RVA: 0x000C6D54 File Offset: 0x000C5154
	public static bool LoadFromRemoteApiDevelop
	{
		get
		{
			return Options.GetBool("LoadFromRemoteApiDevelop");
		}
		set
		{
			if (value)
			{
				Options.SetBool("LoadFromRemoteApiDevelop", true);
				Options.SetBool("LoadFromRemoteApiStaging", false);
				Options.SetBool("LoadFromRemoteApiProduction", false);
				Options.SetBool("LoadWorldSessionFromTestConfigFile", false);
				Options.SetBool("LoadStarterIslandTemplateFile", false);
			}
			else
			{
				Options.SetBool("LoadFromRemoteApiDevelop", false);
			}
		}
	}

	// Token: 0x17000092 RID: 146
	// (get) Token: 0x06001B61 RID: 7009 RVA: 0x000C6DAE File Offset: 0x000C51AE
	public static string LoadFromRemoteApiDevelop_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x17000093 RID: 147
	// (get) Token: 0x06001B62 RID: 7010 RVA: 0x000C6DB5 File Offset: 0x000C51B5
	// (set) Token: 0x06001B63 RID: 7011 RVA: 0x000C6DC4 File Offset: 0x000C51C4
	public static bool LoadFromRemoteApiStaging
	{
		get
		{
			return Options.GetBool("LoadFromRemoteApiStaging");
		}
		set
		{
			if (value)
			{
				Options.SetBool("LoadFromRemoteApiDevelop", false);
				Options.SetBool("LoadFromRemoteApiStaging", true);
				Options.SetBool("LoadFromRemoteApiProduction", false);
				Options.SetBool("LoadWorldSessionFromTestConfigFile", false);
				Options.SetBool("LoadStarterIslandTemplateFile", false);
			}
			else
			{
				Options.SetBool("LoadFromRemoteApiStaging", false);
			}
		}
	}

	// Token: 0x17000094 RID: 148
	// (get) Token: 0x06001B64 RID: 7012 RVA: 0x000C6E1E File Offset: 0x000C521E
	public static string LoadFromRemoteApiStaging_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x17000095 RID: 149
	// (get) Token: 0x06001B65 RID: 7013 RVA: 0x000C6E25 File Offset: 0x000C5225
	// (set) Token: 0x06001B66 RID: 7014 RVA: 0x000C6E34 File Offset: 0x000C5234
	public static bool LoadFromRemoteApiProduction
	{
		get
		{
			return Options.GetBool("LoadFromRemoteApiProduction");
		}
		set
		{
			if (value)
			{
				Options.SetBool("LoadFromRemoteApiDevelop", false);
				Options.SetBool("LoadFromRemoteApiStaging", false);
				Options.SetBool("LoadFromRemoteApiProduction", true);
				Options.SetBool("LoadWorldSessionFromTestConfigFile", false);
				Options.SetBool("LoadStarterIslandTemplateFile", false);
			}
			else
			{
				Options.SetBool("LoadFromRemoteApiProduction", false);
			}
		}
	}

	// Token: 0x17000096 RID: 150
	// (get) Token: 0x06001B67 RID: 7015 RVA: 0x000C6E8E File Offset: 0x000C528E
	public static string LoadFromRemoteApiProduction_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x17000097 RID: 151
	// (get) Token: 0x06001B68 RID: 7016 RVA: 0x000C6E95 File Offset: 0x000C5295
	// (set) Token: 0x06001B69 RID: 7017 RVA: 0x000C6EA1 File Offset: 0x000C52A1
	public static bool BuildRemote
	{
		get
		{
			return Options.GetBool("BuildRemote");
		}
		set
		{
			Options.SetBool("BuildRemote", value);
		}
	}

	// Token: 0x17000098 RID: 152
	// (get) Token: 0x06001B6A RID: 7018 RVA: 0x000C6EAE File Offset: 0x000C52AE
	public static string BuildRemote_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x17000099 RID: 153
	// (get) Token: 0x06001B6B RID: 7019 RVA: 0x000C6EB5 File Offset: 0x000C52B5
	// (set) Token: 0x06001B6C RID: 7020 RVA: 0x000C6EC1 File Offset: 0x000C52C1
	public static bool LoadUserProfile
	{
		get
		{
			return Options.GetBool("LoadUserProfile");
		}
		set
		{
			Options.SetBool("LoadUserProfile", value);
		}
	}

	// Token: 0x1700009A RID: 154
	// (get) Token: 0x06001B6D RID: 7021 RVA: 0x000C6ECE File Offset: 0x000C52CE
	public static string LoadUserProfile_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x1700009B RID: 155
	// (get) Token: 0x06001B6E RID: 7022 RVA: 0x000C6ED5 File Offset: 0x000C52D5
	// (set) Token: 0x06001B6F RID: 7023 RVA: 0x000C6EE1 File Offset: 0x000C52E1
	public static int RemoteApiWorldId
	{
		get
		{
			return Options.GetInt("RemoteApiWorldId");
		}
		set
		{
			Options.SetInt("RemoteApiWorldId", value);
		}
	}

	// Token: 0x1700009C RID: 156
	// (get) Token: 0x06001B70 RID: 7024 RVA: 0x000C6EEE File Offset: 0x000C52EE
	public static string RemoteApiWorldId_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x1700009D RID: 157
	// (get) Token: 0x06001B71 RID: 7025 RVA: 0x000C6EF5 File Offset: 0x000C52F5
	// (set) Token: 0x06001B72 RID: 7026 RVA: 0x000C6F01 File Offset: 0x000C5301
	public static string CurrentUserAuthToken
	{
		get
		{
			return Options.GetString("CurrentUserAuthToken");
		}
		set
		{
			Options.SetString("CurrentUserAuthToken", value);
		}
	}

	// Token: 0x1700009E RID: 158
	// (get) Token: 0x06001B73 RID: 7027 RVA: 0x000C6F0E File Offset: 0x000C530E
	public static string CurrentUserAuthToken_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x1700009F RID: 159
	// (get) Token: 0x06001B74 RID: 7028 RVA: 0x000C6F15 File Offset: 0x000C5315
	// (set) Token: 0x06001B75 RID: 7029 RVA: 0x000C6F21 File Offset: 0x000C5321
	public static bool LoadWorldSessionFromTestConfigFile
	{
		get
		{
			return Options.GetBool("LoadWorldSessionFromTestConfigFile");
		}
		set
		{
			if (value)
			{
				Options.SetBool("LoadFromRemoteApi", false);
				Options.SetBool("LoadWorldSessionFromTestConfigFile", value);
				Options.SetBool("LoadStarterIslandTemplateFile", false);
			}
		}
	}

	// Token: 0x170000A0 RID: 160
	// (get) Token: 0x06001B76 RID: 7030 RVA: 0x000C6F4A File Offset: 0x000C534A
	public static string LoadWorldSessionFromTestConfigFile_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x170000A1 RID: 161
	// (get) Token: 0x06001B77 RID: 7031 RVA: 0x000C6F51 File Offset: 0x000C5351
	// (set) Token: 0x06001B78 RID: 7032 RVA: 0x000C6F5D File Offset: 0x000C535D
	public static bool LoadStarterIslandTemplateFile
	{
		get
		{
			return Options.GetBool("LoadStarterIslandTemplateFile");
		}
		set
		{
			if (value)
			{
				Options.SetBool("LoadFromRemoteApi", false);
				Options.SetBool("LoadWorldSessionFromTestConfigFile", false);
				Options.SetBool("LoadStarterIslandTemplateFile", value);
			}
		}
	}

	// Token: 0x170000A2 RID: 162
	// (get) Token: 0x06001B79 RID: 7033 RVA: 0x000C6F86 File Offset: 0x000C5386
	public static string LoadStarterIslandTemplateFile_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x170000A3 RID: 163
	// (get) Token: 0x06001B7A RID: 7034 RVA: 0x000C6F8D File Offset: 0x000C538D
	// (set) Token: 0x06001B7B RID: 7035 RVA: 0x000C6FBD File Offset: 0x000C53BD
	public static bool LoadLastLoadedWorldFromFile
	{
		get
		{
			return !Options.GetBool("LoadFromRemoteApi") && !Options.GetBool("LoadWorldSessionFromTestConfigFile") && !Options.GetBool("LoadStarterIslandTemplateFile");
		}
		set
		{
			if (value)
			{
				Options.SetBool("LoadFromRemoteApi", false);
				Options.SetBool("LoadWorldSessionFromTestConfigFile", false);
				Options.SetBool("LoadStarterIslandTemplateFile", false);
			}
		}
	}

	// Token: 0x170000A4 RID: 164
	// (get) Token: 0x06001B7C RID: 7036 RVA: 0x000C6FE6 File Offset: 0x000C53E6
	public static string LoadLastLoadedWorldFromFile_GroupName
	{
		get
		{
			return "Scene World Loader";
		}
	}

	// Token: 0x170000A5 RID: 165
	// (get) Token: 0x06001B7D RID: 7037 RVA: 0x000C6FF0 File Offset: 0x000C53F0
	// (set) Token: 0x06001B7E RID: 7038 RVA: 0x000C7009 File Offset: 0x000C5409
	public static string EditorProfileAvatar
	{
		get
		{
			return Options.GetString("EditorProfileAvatar");
		}
		set
		{
			Options.SetString("EditorProfileAvatar", value);
		}
	}

	// Token: 0x170000A6 RID: 166
	// (get) Token: 0x06001B7F RID: 7039 RVA: 0x000C7016 File Offset: 0x000C5416
	// (set) Token: 0x06001B80 RID: 7040 RVA: 0x000C7022 File Offset: 0x000C5422
	public static bool BackupWorlds
	{
		get
		{
			return Options.GetBool("BackupWorlds");
		}
		set
		{
			Options.SetBool("BackupWorlds", value);
		}
	}

	// Token: 0x170000A7 RID: 167
	// (get) Token: 0x06001B81 RID: 7041 RVA: 0x000C702F File Offset: 0x000C542F
	public static string BackupWorlds_GroupName
	{
		get
		{
			return "World Backup";
		}
	}

	// Token: 0x170000A8 RID: 168
	// (get) Token: 0x06001B82 RID: 7042 RVA: 0x000C7036 File Offset: 0x000C5436
	// (set) Token: 0x06001B83 RID: 7043 RVA: 0x000C7042 File Offset: 0x000C5442
	public static bool DebugSFX
	{
		get
		{
			return Options.GetBool("debug-sfx");
		}
		set
		{
			Options.SetBool("debug-sfx", value);
		}
	}

	// Token: 0x170000A9 RID: 169
	// (get) Token: 0x06001B84 RID: 7044 RVA: 0x000C704F File Offset: 0x000C544F
	public static string DebugSFX_GroupName
	{
		get
		{
			return "SFX and Music";
		}
	}

	// Token: 0x170000AA RID: 170
	// (get) Token: 0x06001B85 RID: 7045 RVA: 0x000C7056 File Offset: 0x000C5456
	// (set) Token: 0x06001B86 RID: 7046 RVA: 0x000C7062 File Offset: 0x000C5462
	public static bool SFXEnabled
	{
		get
		{
			return Options.GetBool("sfx-enabled");
		}
		set
		{
			Options.SetBool("sfx-enabled", value);
		}
	}

	// Token: 0x170000AB RID: 171
	// (get) Token: 0x06001B87 RID: 7047 RVA: 0x000C706F File Offset: 0x000C546F
	public static string SFXEnabled_GroupName
	{
		get
		{
			return "SFX and Music";
		}
	}

	// Token: 0x170000AC RID: 172
	// (get) Token: 0x06001B88 RID: 7048 RVA: 0x000C7076 File Offset: 0x000C5476
	// (set) Token: 0x06001B89 RID: 7049 RVA: 0x000C7082 File Offset: 0x000C5482
	public static bool BuildMusicEnabled
	{
		get
		{
			return Options.GetBool("build-music-enabled");
		}
		set
		{
			Options.SetBool("build-music-enabled", value);
		}
	}

	// Token: 0x170000AD RID: 173
	// (get) Token: 0x06001B8A RID: 7050 RVA: 0x000C708F File Offset: 0x000C548F
	public static string BuildMusicEnabled_GroupName
	{
		get
		{
			return "SFX and Music";
		}
	}

	// Token: 0x170000AE RID: 174
	// (get) Token: 0x06001B8B RID: 7051 RVA: 0x000C7096 File Offset: 0x000C5496
	// (set) Token: 0x06001B8C RID: 7052 RVA: 0x000C70A2 File Offset: 0x000C54A2
	public static bool PlayMusicEnabled
	{
		get
		{
			return Options.GetBool("play-music-enabled");
		}
		set
		{
			Options.SetBool("play-music-enabled", value);
		}
	}

	// Token: 0x170000AF RID: 175
	// (get) Token: 0x06001B8D RID: 7053 RVA: 0x000C70AF File Offset: 0x000C54AF
	public static string PlayMusicEnabled_GroupName
	{
		get
		{
			return "SFX and Music";
		}
	}

	// Token: 0x170000B0 RID: 176
	// (get) Token: 0x06001B8E RID: 7054 RVA: 0x000C70B6 File Offset: 0x000C54B6
	// (set) Token: 0x06001B8F RID: 7055 RVA: 0x000C70C2 File Offset: 0x000C54C2
	public static bool LockTileOnNewBlocks
	{
		get
		{
			return Options.GetBool("lock-tile-on-new-blocks");
		}
		set
		{
			Options.SetBool("lock-tile-on-new-blocks", value);
		}
	}

	// Token: 0x170000B1 RID: 177
	// (get) Token: 0x06001B90 RID: 7056 RVA: 0x000C70CF File Offset: 0x000C54CF
	// (set) Token: 0x06001B91 RID: 7057 RVA: 0x000C70DB File Offset: 0x000C54DB
	public static bool BlockVolumeDisplay
	{
		get
		{
			return Options.GetBool("block-volume-display");
		}
		set
		{
			Options.SetBool("block-volume-display", value);
		}
	}

	// Token: 0x170000B2 RID: 178
	// (get) Token: 0x06001B92 RID: 7058 RVA: 0x000C70E8 File Offset: 0x000C54E8
	public static string BlockVolumeDisplay_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000B3 RID: 179
	// (get) Token: 0x06001B93 RID: 7059 RVA: 0x000C70EF File Offset: 0x000C54EF
	// (set) Token: 0x06001B94 RID: 7060 RVA: 0x000C70FB File Offset: 0x000C54FB
	public static bool BlockCameraHintDisplay
	{
		get
		{
			return Options.GetBool("block-camera-hint-display");
		}
		set
		{
			Options.SetBool("block-camera-hint-display", value);
		}
	}

	// Token: 0x170000B4 RID: 180
	// (get) Token: 0x06001B95 RID: 7061 RVA: 0x000C7108 File Offset: 0x000C5508
	public static string BlockCameraHintDisplay_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000B5 RID: 181
	// (get) Token: 0x06001B96 RID: 7062 RVA: 0x000C710F File Offset: 0x000C550F
	// (set) Token: 0x06001B97 RID: 7063 RVA: 0x000C711B File Offset: 0x000C551B
	public static bool GlueVolumeDisplay
	{
		get
		{
			return Options.GetBool("glue-volume-display");
		}
		set
		{
			Options.SetBool("glue-volume-display", value);
		}
	}

	// Token: 0x170000B6 RID: 182
	// (get) Token: 0x06001B98 RID: 7064 RVA: 0x000C7128 File Offset: 0x000C5528
	public static string GlueVolumeDisplay_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000B7 RID: 183
	// (get) Token: 0x06001B99 RID: 7065 RVA: 0x000C712F File Offset: 0x000C552F
	// (set) Token: 0x06001B9A RID: 7066 RVA: 0x000C713B File Offset: 0x000C553B
	public static bool ShapeVolumeDisplay
	{
		get
		{
			return Options.GetBool("shape-volume-display");
		}
		set
		{
			Options.SetBool("shape-volume-display", value);
		}
	}

	// Token: 0x170000B8 RID: 184
	// (get) Token: 0x06001B9B RID: 7067 RVA: 0x000C7148 File Offset: 0x000C5548
	public static string ShapeVolumeDisplay_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000B9 RID: 185
	// (get) Token: 0x06001B9C RID: 7068 RVA: 0x000C714F File Offset: 0x000C554F
	// (set) Token: 0x06001B9D RID: 7069 RVA: 0x000C715B File Offset: 0x000C555B
	public static bool JointMeshDisplay
	{
		get
		{
			return Options.GetBool("joint-mesh-display");
		}
		set
		{
			Options.SetBool("joint-mesh-display", value);
		}
	}

	// Token: 0x170000BA RID: 186
	// (get) Token: 0x06001B9E RID: 7070 RVA: 0x000C7168 File Offset: 0x000C5568
	public static string JointMeshDisplay_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000BB RID: 187
	// (get) Token: 0x06001B9F RID: 7071 RVA: 0x000C716F File Offset: 0x000C556F
	// (set) Token: 0x06001BA0 RID: 7072 RVA: 0x000C717B File Offset: 0x000C557B
	public static bool HideTutorialGraphics
	{
		get
		{
			return Options.GetBool("HideTutorialGraphics");
		}
		set
		{
			Options.SetBool("HideTutorialGraphics", value);
		}
	}

	// Token: 0x170000BC RID: 188
	// (get) Token: 0x06001BA1 RID: 7073 RVA: 0x000C7188 File Offset: 0x000C5588
	public static string HideTutorialGraphics_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000BD RID: 189
	// (get) Token: 0x06001BA2 RID: 7074 RVA: 0x000C718F File Offset: 0x000C558F
	// (set) Token: 0x06001BA3 RID: 7075 RVA: 0x000C719B File Offset: 0x000C559B
	public static bool HideInGameUI
	{
		get
		{
			return Options.GetBool("HideInGameUI");
		}
		set
		{
			Options.SetBool("HideInGameUI", value);
		}
	}

	// Token: 0x170000BE RID: 190
	// (get) Token: 0x06001BA4 RID: 7076 RVA: 0x000C71A8 File Offset: 0x000C55A8
	public static string HideInGameUI_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000BF RID: 191
	// (get) Token: 0x06001BA5 RID: 7077 RVA: 0x000C71AF File Offset: 0x000C55AF
	// (set) Token: 0x06001BA6 RID: 7078 RVA: 0x000C71BB File Offset: 0x000C55BB
	public static bool HideLeaderboard
	{
		get
		{
			return Options.GetBool("HideLeaderboard");
		}
		set
		{
			Options.SetBool("HideLeaderboard", value);
		}
	}

	// Token: 0x170000C0 RID: 192
	// (get) Token: 0x06001BA7 RID: 7079 RVA: 0x000C71C8 File Offset: 0x000C55C8
	public static string HideLeaderboard_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000C1 RID: 193
	// (get) Token: 0x06001BA8 RID: 7080 RVA: 0x000C71CF File Offset: 0x000C55CF
	// (set) Token: 0x06001BA9 RID: 7081 RVA: 0x000C71DB File Offset: 0x000C55DB
	public static bool ShowForwardUpRightOnSelected
	{
		get
		{
			return Options.GetBool("ShowForwardUpRightOnSelected");
		}
		set
		{
			Options.SetBool("ShowForwardUpRightOnSelected", value);
		}
	}

	// Token: 0x170000C2 RID: 194
	// (get) Token: 0x06001BAA RID: 7082 RVA: 0x000C71E8 File Offset: 0x000C55E8
	public static string ShowForwardUpRightOnSelected_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000C3 RID: 195
	// (get) Token: 0x06001BAB RID: 7083 RVA: 0x000C71EF File Offset: 0x000C55EF
	// (set) Token: 0x06001BAC RID: 7084 RVA: 0x000C71FB File Offset: 0x000C55FB
	public static bool ShowCenterOfMasses
	{
		get
		{
			return Options.GetBool("ShowCenterOfMasses");
		}
		set
		{
			Options.SetBool("ShowCenterOfMasses", value);
		}
	}

	// Token: 0x170000C4 RID: 196
	// (get) Token: 0x06001BAD RID: 7085 RVA: 0x000C7208 File Offset: 0x000C5608
	public static string ShowCenterOfMasses_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000C5 RID: 197
	// (get) Token: 0x06001BAE RID: 7086 RVA: 0x000C720F File Offset: 0x000C560F
	// (set) Token: 0x06001BAF RID: 7087 RVA: 0x000C721B File Offset: 0x000C561B
	public static bool HideMover
	{
		get
		{
			return Options.GetBool("hideMover");
		}
		set
		{
			Options.SetBool("hideMover", value);
		}
	}

	// Token: 0x170000C6 RID: 198
	// (get) Token: 0x06001BB0 RID: 7088 RVA: 0x000C7228 File Offset: 0x000C5628
	public static string HideMover_GroupName
	{
		get
		{
			return "Display";
		}
	}

	// Token: 0x170000C7 RID: 199
	// (get) Token: 0x06001BB1 RID: 7089 RVA: 0x000C722F File Offset: 0x000C562F
	// (set) Token: 0x06001BB2 RID: 7090 RVA: 0x000C723B File Offset: 0x000C563B
	public static float ManualCameraSmoothness
	{
		get
		{
			return Options.GetFloat("ManualCameraSmoothness");
		}
		set
		{
			Options.SetFloat("ManualCameraSmoothness", value);
		}
	}

	// Token: 0x170000C8 RID: 200
	// (get) Token: 0x06001BB3 RID: 7091 RVA: 0x000C7248 File Offset: 0x000C5648
	public static string ManualCameraSmoothness_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000C9 RID: 201
	// (get) Token: 0x06001BB4 RID: 7092 RVA: 0x000C724F File Offset: 0x000C564F
	// (set) Token: 0x06001BB5 RID: 7093 RVA: 0x000C725B File Offset: 0x000C565B
	public static float WASDSmoothness
	{
		get
		{
			return Options.GetFloat("WASDSmoothness");
		}
		set
		{
			Options.SetFloat("WASDSmoothness", value);
		}
	}

	// Token: 0x170000CA RID: 202
	// (get) Token: 0x06001BB6 RID: 7094 RVA: 0x000C7268 File Offset: 0x000C5668
	public static string WASDSmoothness_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000CB RID: 203
	// (get) Token: 0x06001BB7 RID: 7095 RVA: 0x000C726F File Offset: 0x000C566F
	// (set) Token: 0x06001BB8 RID: 7096 RVA: 0x000C727B File Offset: 0x000C567B
	public static float WASDMovementSpeedup
	{
		get
		{
			return Options.GetFloat("WASDMovementSpeedup");
		}
		set
		{
			Options.SetFloat("WASDMovementSpeedup", value);
		}
	}

	// Token: 0x170000CC RID: 204
	// (get) Token: 0x06001BB9 RID: 7097 RVA: 0x000C7288 File Offset: 0x000C5688
	public static string WASDMovementSpeedup_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000CD RID: 205
	// (get) Token: 0x06001BBA RID: 7098 RVA: 0x000C728F File Offset: 0x000C568F
	// (set) Token: 0x06001BBB RID: 7099 RVA: 0x000C729B File Offset: 0x000C569B
	public static float WASDRotationSpeedup
	{
		get
		{
			return Options.GetFloat("WASDRotationSpeedup");
		}
		set
		{
			Options.SetFloat("WASDRotationSpeedup", value);
		}
	}

	// Token: 0x170000CE RID: 206
	// (get) Token: 0x06001BBC RID: 7100 RVA: 0x000C72A8 File Offset: 0x000C56A8
	public static string WASDRotationSpeedup_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000CF RID: 207
	// (get) Token: 0x06001BBD RID: 7101 RVA: 0x000C72AF File Offset: 0x000C56AF
	// (set) Token: 0x06001BBE RID: 7102 RVA: 0x000C72BB File Offset: 0x000C56BB
	public static float MouseWheelZoomSpeedup
	{
		get
		{
			return Options.GetFloat("mouse-wheel-zoom-speedup");
		}
		set
		{
			Options.SetFloat("mouse-wheel-zoom-speedup", value);
		}
	}

	// Token: 0x170000D0 RID: 208
	// (get) Token: 0x06001BBF RID: 7103 RVA: 0x000C72C8 File Offset: 0x000C56C8
	public static string MouseWheelZoomSpeedup_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000D1 RID: 209
	// (get) Token: 0x06001BC0 RID: 7104 RVA: 0x000C72CF File Offset: 0x000C56CF
	// (set) Token: 0x06001BC1 RID: 7105 RVA: 0x000C72DB File Offset: 0x000C56DB
	public static bool RelativeZoom
	{
		get
		{
			return Options.GetBool("relative-zoom");
		}
		set
		{
			Options.SetBool("relative-zoom", value);
		}
	}

	// Token: 0x170000D2 RID: 210
	// (get) Token: 0x06001BC2 RID: 7106 RVA: 0x000C72E8 File Offset: 0x000C56E8
	public static string RelativeZoom_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000D3 RID: 211
	// (get) Token: 0x06001BC3 RID: 7107 RVA: 0x000C72EF File Offset: 0x000C56EF
	// (set) Token: 0x06001BC4 RID: 7108 RVA: 0x000C72FB File Offset: 0x000C56FB
	public static bool TutorialDisableAutoCamera
	{
		get
		{
			return Options.GetBool("TutorialDisableAutoCamera");
		}
		set
		{
			Options.SetBool("TutorialDisableAutoCamera", value);
		}
	}

	// Token: 0x170000D4 RID: 212
	// (get) Token: 0x06001BC5 RID: 7109 RVA: 0x000C7308 File Offset: 0x000C5708
	public static string TutorialDisableAutoCamera_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000D5 RID: 213
	// (get) Token: 0x06001BC6 RID: 7110 RVA: 0x000C730F File Offset: 0x000C570F
	// (set) Token: 0x06001BC7 RID: 7111 RVA: 0x000C731B File Offset: 0x000C571B
	public static bool DisableCameraSnapping
	{
		get
		{
			return Options.GetBool("DisableCameraSnapping");
		}
		set
		{
			Options.SetBool("DisableCameraSnapping", value);
		}
	}

	// Token: 0x170000D6 RID: 214
	// (get) Token: 0x06001BC8 RID: 7112 RVA: 0x000C7328 File Offset: 0x000C5728
	public static string DisableCameraSnapping_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000D7 RID: 215
	// (get) Token: 0x06001BC9 RID: 7113 RVA: 0x000C732F File Offset: 0x000C572F
	// (set) Token: 0x06001BCA RID: 7114 RVA: 0x000C733B File Offset: 0x000C573B
	public static bool DisableAutoFollow
	{
		get
		{
			return Options.GetBool("DisableAutoFollow");
		}
		set
		{
			Options.SetBool("DisableAutoFollow", value);
		}
	}

	// Token: 0x170000D8 RID: 216
	// (get) Token: 0x06001BCB RID: 7115 RVA: 0x000C7348 File Offset: 0x000C5748
	public static string DisableAutoFollow_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000D9 RID: 217
	// (get) Token: 0x06001BCC RID: 7116 RVA: 0x000C734F File Offset: 0x000C574F
	// (set) Token: 0x06001BCD RID: 7117 RVA: 0x000C735B File Offset: 0x000C575B
	public static bool DisableGameCamera
	{
		get
		{
			return Options.GetBool("DisableGameCamera");
		}
		set
		{
			Options.SetBool("DisableGameCamera", value);
		}
	}

	// Token: 0x170000DA RID: 218
	// (get) Token: 0x06001BCE RID: 7118 RVA: 0x000C7368 File Offset: 0x000C5768
	public static string DisableGameCamera_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000DB RID: 219
	// (get) Token: 0x06001BCF RID: 7119 RVA: 0x000C736F File Offset: 0x000C576F
	// (set) Token: 0x06001BD0 RID: 7120 RVA: 0x000C737B File Offset: 0x000C577B
	public static bool DisableWASD
	{
		get
		{
			return Options.GetBool("disableWASD");
		}
		set
		{
			Options.SetBool("disableWASD", value);
		}
	}

	// Token: 0x170000DC RID: 220
	// (get) Token: 0x06001BD1 RID: 7121 RVA: 0x000C7388 File Offset: 0x000C5788
	public static string DisableWASD_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000DD RID: 221
	// (get) Token: 0x06001BD2 RID: 7122 RVA: 0x000C738F File Offset: 0x000C578F
	// (set) Token: 0x06001BD3 RID: 7123 RVA: 0x000C739B File Offset: 0x000C579B
	public static bool EnableVRGoggles
	{
		get
		{
			return Options.GetBool("enableVRGoggles");
		}
		set
		{
			Options.SetBool("enableVRGoggles", value);
		}
	}

	// Token: 0x170000DE RID: 222
	// (get) Token: 0x06001BD4 RID: 7124 RVA: 0x000C73A8 File Offset: 0x000C57A8
	public static string EnableVRGoggles_GroupName
	{
		get
		{
			return "Camera";
		}
	}

	// Token: 0x170000DF RID: 223
	// (get) Token: 0x06001BD5 RID: 7125 RVA: 0x000C73AF File Offset: 0x000C57AF
	// (set) Token: 0x06001BD6 RID: 7126 RVA: 0x000C73BB File Offset: 0x000C57BB
	public static float MouseWheelScrollSpeedup
	{
		get
		{
			return Options.GetFloat("mouse-wheel-scroll-speedup");
		}
		set
		{
			Options.SetFloat("mouse-wheel-scroll-speedup", value);
		}
	}

	// Token: 0x170000E0 RID: 224
	// (get) Token: 0x06001BD7 RID: 7127 RVA: 0x000C73C8 File Offset: 0x000C57C8
	public static string MouseWheelScrollSpeedup_GroupName
	{
		get
		{
			return "Side Panel";
		}
	}

	// Token: 0x170000E1 RID: 225
	// (get) Token: 0x06001BD8 RID: 7128 RVA: 0x000C73CF File Offset: 0x000C57CF
	// (set) Token: 0x06001BD9 RID: 7129 RVA: 0x000C73DB File Offset: 0x000C57DB
	public static bool ShowDevTiles
	{
		get
		{
			return Options.GetBool("ShowDevTiles");
		}
		set
		{
			Options.SetBool("ShowDevTiles", value);
		}
	}

	// Token: 0x170000E2 RID: 226
	// (get) Token: 0x06001BDA RID: 7130 RVA: 0x000C73E8 File Offset: 0x000C57E8
	public static string ShowDevTiles_GroupName
	{
		get
		{
			return "Side Panel";
		}
	}

	// Token: 0x170000E3 RID: 227
	// (get) Token: 0x06001BDB RID: 7131 RVA: 0x000C73EF File Offset: 0x000C57EF
	// (set) Token: 0x06001BDC RID: 7132 RVA: 0x000C73FB File Offset: 0x000C57FB
	public static bool QuickKeyScroll
	{
		get
		{
			return Options.GetBool("QuickKeyScroll");
		}
		set
		{
			Options.SetBool("QuickKeyScroll", value);
		}
	}

	// Token: 0x170000E4 RID: 228
	// (get) Token: 0x06001BDD RID: 7133 RVA: 0x000C7408 File Offset: 0x000C5808
	public static string QuickKeyScroll_GroupName
	{
		get
		{
			return "Side Panel";
		}
	}

	// Token: 0x170000E5 RID: 229
	// (get) Token: 0x06001BDE RID: 7134 RVA: 0x000C740F File Offset: 0x000C580F
	// (set) Token: 0x06001BDF RID: 7135 RVA: 0x000C741B File Offset: 0x000C581B
	public static bool DisableAutoScrollToScriptTile
	{
		get
		{
			return Options.GetBool("DisableAutoScrollToScriptTile");
		}
		set
		{
			Options.SetBool("DisableAutoScrollToScriptTile", value);
		}
	}

	// Token: 0x170000E6 RID: 230
	// (get) Token: 0x06001BE0 RID: 7136 RVA: 0x000C7428 File Offset: 0x000C5828
	public static string DisableAutoScrollToScriptTile_GroupName
	{
		get
		{
			return "Side Panel";
		}
	}

	// Token: 0x170000E7 RID: 231
	// (get) Token: 0x06001BE1 RID: 7137 RVA: 0x000C742F File Offset: 0x000C582F
	// (set) Token: 0x06001BE2 RID: 7138 RVA: 0x000C743B File Offset: 0x000C583B
	public static int PanelColumnCount
	{
		get
		{
			return Options.GetInt("PanelColumnCount");
		}
		set
		{
			Options.SetInt("PanelColumnCount", value);
		}
	}

	// Token: 0x170000E8 RID: 232
	// (get) Token: 0x06001BE3 RID: 7139 RVA: 0x000C7448 File Offset: 0x000C5848
	public static string PanelColumnCount_GroupName
	{
		get
		{
			return "Side Panel";
		}
	}

	// Token: 0x170000E9 RID: 233
	// (get) Token: 0x06001BE4 RID: 7140 RVA: 0x000C744F File Offset: 0x000C584F
	// (set) Token: 0x06001BE5 RID: 7141 RVA: 0x000C745B File Offset: 0x000C585B
	public static bool EnableTerrainSelection
	{
		get
		{
			return Options.GetBool("EnableTerrainSelection");
		}
		set
		{
			Options.SetBool("EnableTerrainSelection", value);
		}
	}

	// Token: 0x170000EA RID: 234
	// (get) Token: 0x06001BE6 RID: 7142 RVA: 0x000C7468 File Offset: 0x000C5868
	public static string EnableTerrainSelection_GroupName
	{
		get
		{
			return "Block Manipulation";
		}
	}

	// Token: 0x170000EB RID: 235
	// (get) Token: 0x06001BE7 RID: 7143 RVA: 0x000C746F File Offset: 0x000C586F
	// (set) Token: 0x06001BE8 RID: 7144 RVA: 0x000C747B File Offset: 0x000C587B
	public static bool ControlInvertsUpMode
	{
		get
		{
			return Options.GetBool("ControlInvertsUpMode");
		}
		set
		{
			Options.SetBool("ControlInvertsUpMode", value);
		}
	}

	// Token: 0x170000EC RID: 236
	// (get) Token: 0x06001BE9 RID: 7145 RVA: 0x000C7488 File Offset: 0x000C5888
	public static string ControlInvertsUpMode_GroupName
	{
		get
		{
			return "Block Manipulation";
		}
	}

	// Token: 0x170000ED RID: 237
	// (get) Token: 0x06001BEA RID: 7146 RVA: 0x000C748F File Offset: 0x000C588F
	// (set) Token: 0x06001BEB RID: 7147 RVA: 0x000C749B File Offset: 0x000C589B
	public static bool RaycastMoveSingletonBlocksWithoutSelection
	{
		get
		{
			return Options.GetBool("RaycastMoveSingletonBlocksWithoutSelection");
		}
		set
		{
			Options.SetBool("RaycastMoveSingletonBlocksWithoutSelection", value);
		}
	}

	// Token: 0x170000EE RID: 238
	// (get) Token: 0x06001BEC RID: 7148 RVA: 0x000C74A8 File Offset: 0x000C58A8
	public static string RaycastMoveSingletonBlocksWithoutSelection_GroupName
	{
		get
		{
			return "Block Manipulation";
		}
	}

	// Token: 0x170000EF RID: 239
	// (get) Token: 0x06001BED RID: 7149 RVA: 0x000C74AF File Offset: 0x000C58AF
	// (set) Token: 0x06001BEE RID: 7150 RVA: 0x000C74BB File Offset: 0x000C58BB
	public static bool RaycastMoveBlocksWithoutSelection
	{
		get
		{
			return Options.GetBool("RaycastMoveBlocksWithoutSelection");
		}
		set
		{
			Options.SetBool("RaycastMoveBlocksWithoutSelection", value);
		}
	}

	// Token: 0x170000F0 RID: 240
	// (get) Token: 0x06001BEF RID: 7151 RVA: 0x000C74C8 File Offset: 0x000C58C8
	public static string RaycastMoveBlocksWithoutSelection_GroupName
	{
		get
		{
			return "Block Manipulation";
		}
	}

	// Token: 0x170000F1 RID: 241
	// (get) Token: 0x06001BF0 RID: 7152 RVA: 0x000C74CF File Offset: 0x000C58CF
	// (set) Token: 0x06001BF1 RID: 7153 RVA: 0x000C74DB File Offset: 0x000C58DB
	public static bool AxisLockMoveAndScaleEnabled
	{
		get
		{
			return Options.GetBool("AxisLockMoveAndScaleEnabled");
		}
		set
		{
			Options.SetBool("AxisLockMoveAndScaleEnabled", value);
		}
	}

	// Token: 0x170000F2 RID: 242
	// (get) Token: 0x06001BF2 RID: 7154 RVA: 0x000C74E8 File Offset: 0x000C58E8
	public static string AxisLockMoveAndScaleEnabled_GroupName
	{
		get
		{
			return "Block Manipulation";
		}
	}

	// Token: 0x170000F3 RID: 243
	// (get) Token: 0x06001BF3 RID: 7155 RVA: 0x000C74EF File Offset: 0x000C58EF
	// (set) Token: 0x06001BF4 RID: 7156 RVA: 0x000C74FB File Offset: 0x000C58FB
	public static bool EnableDevUtils
	{
		get
		{
			return Options.GetBool("EnableDevUtils");
		}
		set
		{
			Options.SetBool("EnableDevUtils", value);
		}
	}

	// Token: 0x170000F4 RID: 244
	// (get) Token: 0x06001BF5 RID: 7157 RVA: 0x000C7508 File Offset: 0x000C5908
	public static string EnableDevUtils_GroupName
	{
		get
		{
			return "Block Manipulation";
		}
	}

	// Token: 0x170000F5 RID: 245
	// (get) Token: 0x06001BF6 RID: 7158 RVA: 0x000C750F File Offset: 0x000C590F
	// (set) Token: 0x06001BF7 RID: 7159 RVA: 0x000C751B File Offset: 0x000C591B
	public static bool ShowTextureToInfo
	{
		get
		{
			return Options.GetBool("ShowTextureToInfo");
		}
		set
		{
			Options.SetBool("ShowTextureToInfo", value);
		}
	}

	// Token: 0x170000F6 RID: 246
	// (get) Token: 0x06001BF8 RID: 7160 RVA: 0x000C7528 File Offset: 0x000C5928
	public static string ShowTextureToInfo_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x170000F7 RID: 247
	// (get) Token: 0x06001BF9 RID: 7161 RVA: 0x000C752F File Offset: 0x000C592F
	// (set) Token: 0x06001BFA RID: 7162 RVA: 0x000C753B File Offset: 0x000C593B
	public static bool SetRewardModelFromClipboard
	{
		get
		{
			return Options.GetBool("SetRewardModelFromClipboard");
		}
		set
		{
			Options.SetBool("SetRewardModelFromClipboard", value);
		}
	}

	// Token: 0x170000F8 RID: 248
	// (get) Token: 0x06001BFB RID: 7163 RVA: 0x000C7548 File Offset: 0x000C5948
	public static string SetRewardModelFromClipboard_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x170000F9 RID: 249
	// (get) Token: 0x06001BFC RID: 7164 RVA: 0x000C754F File Offset: 0x000C594F
	// (set) Token: 0x06001BFD RID: 7165 RVA: 0x000C755B File Offset: 0x000C595B
	public static bool ExportRewardModelFromClipboard
	{
		get
		{
			return Options.GetBool("ExportRewardModelFromClipboard");
		}
		set
		{
			Options.SetBool("ExportRewardModelFromClipboard", value);
		}
	}

	// Token: 0x170000FA RID: 250
	// (get) Token: 0x06001BFE RID: 7166 RVA: 0x000C7568 File Offset: 0x000C5968
	public static string ExportRewardModelFromClipboard_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x170000FB RID: 251
	// (get) Token: 0x06001BFF RID: 7167 RVA: 0x000C756F File Offset: 0x000C596F
	// (set) Token: 0x06001C00 RID: 7168 RVA: 0x000C7585 File Offset: 0x000C5985
	public static bool DisplayBlockNames
	{
		get
		{
			return Options.GetBoolFast("DisplayBlockNames", ref Options.DisplayBlockNames_Set, ref Options.DisplayBlockNames_Fast);
		}
		set
		{
			Options.SetBoolFast("DisplayBlockNames", value, ref Options.DisplayBlockNames_Set, ref Options.DisplayBlockNames_Fast);
		}
	}

	// Token: 0x170000FC RID: 252
	// (get) Token: 0x06001C01 RID: 7169 RVA: 0x000C759C File Offset: 0x000C599C
	public static string DisplayBlockNames_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x170000FD RID: 253
	// (get) Token: 0x06001C02 RID: 7170 RVA: 0x000C75A3 File Offset: 0x000C59A3
	// (set) Token: 0x06001C03 RID: 7171 RVA: 0x000C75B9 File Offset: 0x000C59B9
	public static bool DisplayWheelBlockNames
	{
		get
		{
			return Options.GetBoolFast("DisplayWheelBlockNames", ref Options.DisplayWheelBlockNames_Set, ref Options.DisplayWheelBlockNames_Fast);
		}
		set
		{
			Options.SetBoolFast("DisplayWheelBlockNames", value, ref Options.DisplayWheelBlockNames_Set, ref Options.DisplayWheelBlockNames_Fast);
		}
	}

	// Token: 0x170000FE RID: 254
	// (get) Token: 0x06001C04 RID: 7172 RVA: 0x000C75D0 File Offset: 0x000C59D0
	public static string DisplayWheelBlockNames_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x170000FF RID: 255
	// (get) Token: 0x06001C05 RID: 7173 RVA: 0x000C75D7 File Offset: 0x000C59D7
	// (set) Token: 0x06001C06 RID: 7174 RVA: 0x000C75ED File Offset: 0x000C59ED
	public static bool ShowMisalignedBlocks
	{
		get
		{
			return Options.GetBoolFast("ShowMisalignedBlocks", ref Options.ShowMisalignedBlocks_Set, ref Options.ShowMisalignedBlocks_Fast);
		}
		set
		{
			Options.SetBoolFast("ShowMisalignedBlocks", value, ref Options.ShowMisalignedBlocks_Set, ref Options.ShowMisalignedBlocks_Fast);
		}
	}

	// Token: 0x17000100 RID: 256
	// (get) Token: 0x06001C07 RID: 7175 RVA: 0x000C7604 File Offset: 0x000C5A04
	public static string ShowMisalignedBlocks_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x17000101 RID: 257
	// (get) Token: 0x06001C08 RID: 7176 RVA: 0x000C760B File Offset: 0x000C5A0B
	// (set) Token: 0x06001C09 RID: 7177 RVA: 0x000C7617 File Offset: 0x000C5A17
	public static bool UseSimpleAutoPlayTrigger
	{
		get
		{
			return Options.GetBool("UseSimpleAutoPlayTrigger");
		}
		set
		{
			Options.SetBool("UseSimpleAutoPlayTrigger", value);
		}
	}

	// Token: 0x17000102 RID: 258
	// (get) Token: 0x06001C0A RID: 7178 RVA: 0x000C7624 File Offset: 0x000C5A24
	public static string UseSimpleAutoPlayTrigger_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x17000103 RID: 259
	// (get) Token: 0x06001C0B RID: 7179 RVA: 0x000C762B File Offset: 0x000C5A2B
	// (set) Token: 0x06001C0C RID: 7180 RVA: 0x000C7641 File Offset: 0x000C5A41
	public static bool OnScreenActionDebug
	{
		get
		{
			return Options.GetBoolFast("OnScreenActionDebug", ref Options.OnScreenActionDebug_Set, ref Options.OnScreenActionDebug_Fast);
		}
		set
		{
			Options.SetBoolFast("OnScreenActionDebug", value, ref Options.OnScreenActionDebug_Set, ref Options.OnScreenActionDebug_Fast);
		}
	}

	// Token: 0x17000104 RID: 260
	// (get) Token: 0x06001C0D RID: 7181 RVA: 0x000C7658 File Offset: 0x000C5A58
	public static string OnScreenActionDebug_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x17000105 RID: 261
	// (get) Token: 0x06001C0E RID: 7182 RVA: 0x000C765F File Offset: 0x000C5A5F
	// (set) Token: 0x06001C0F RID: 7183 RVA: 0x000C766B File Offset: 0x000C5A6B
	public static bool OnScreenMouseBlockInfo
	{
		get
		{
			return Options.GetBool("OnScreenMouseBlockInfo");
		}
		set
		{
			Options.SetBool("OnScreenMouseBlockInfo", value);
		}
	}

	// Token: 0x17000106 RID: 262
	// (get) Token: 0x06001C10 RID: 7184 RVA: 0x000C7678 File Offset: 0x000C5A78
	public static string OnScreenMouseBlockInfo_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x17000107 RID: 263
	// (get) Token: 0x06001C11 RID: 7185 RVA: 0x000C767F File Offset: 0x000C5A7F
	// (set) Token: 0x06001C12 RID: 7186 RVA: 0x000C768B File Offset: 0x000C5A8B
	public static bool DebugIconLoad
	{
		get
		{
			return Options.GetBool("DebugIconLoad");
		}
		set
		{
			Options.SetBool("DebugIconLoad", value);
		}
	}

	// Token: 0x17000108 RID: 264
	// (get) Token: 0x06001C13 RID: 7187 RVA: 0x000C7698 File Offset: 0x000C5A98
	public static string DebugIconLoad_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x17000109 RID: 265
	// (get) Token: 0x06001C14 RID: 7188 RVA: 0x000C769F File Offset: 0x000C5A9F
	// (set) Token: 0x06001C15 RID: 7189 RVA: 0x000C76AB File Offset: 0x000C5AAB
	public static bool AllowAllStateTransitions
	{
		get
		{
			return Options.GetBool("AllowAllStateTransitions");
		}
		set
		{
			Options.SetBool("AllowAllStateTransitions", value);
		}
	}

	// Token: 0x1700010A RID: 266
	// (get) Token: 0x06001C16 RID: 7190 RVA: 0x000C76B8 File Offset: 0x000C5AB8
	public static string AllowAllStateTransitions_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x1700010B RID: 267
	// (get) Token: 0x06001C17 RID: 7191 RVA: 0x000C76BF File Offset: 0x000C5ABF
	// (set) Token: 0x06001C18 RID: 7192 RVA: 0x000C76CB File Offset: 0x000C5ACB
	public static bool InstantStateAnimationShift
	{
		get
		{
			return Options.GetBool("InstantStateAnimationShift");
		}
		set
		{
			Options.SetBool("InstantStateAnimationShift", value);
		}
	}

	// Token: 0x1700010C RID: 268
	// (get) Token: 0x06001C19 RID: 7193 RVA: 0x000C76D8 File Offset: 0x000C5AD8
	public static string InstantStateAnimationShift_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x1700010D RID: 269
	// (get) Token: 0x06001C1A RID: 7194 RVA: 0x000C76DF File Offset: 0x000C5ADF
	// (set) Token: 0x06001C1B RID: 7195 RVA: 0x000C76EB File Offset: 0x000C5AEB
	public static bool DebugGestures
	{
		get
		{
			return Options.GetBool("DebugGestures");
		}
		set
		{
			Options.SetBool("DebugGestures", value);
		}
	}

	// Token: 0x1700010E RID: 270
	// (get) Token: 0x06001C1C RID: 7196 RVA: 0x000C76F8 File Offset: 0x000C5AF8
	public static string DebugGestures_GroupName
	{
		get
		{
			return "Debug";
		}
	}

	// Token: 0x1700010F RID: 271
	// (get) Token: 0x06001C1D RID: 7197 RVA: 0x000C76FF File Offset: 0x000C5AFF
	// (set) Token: 0x06001C1E RID: 7198 RVA: 0x000C770B File Offset: 0x000C5B0B
	public static string ScreenshotDirectory
	{
		get
		{
			return Options.GetString("ScreenshotDirectory");
		}
		set
		{
			Options.SetString("ScreenshotDirectory", value);
		}
	}

	// Token: 0x17000110 RID: 272
	// (get) Token: 0x06001C1F RID: 7199 RVA: 0x000C7718 File Offset: 0x000C5B18
	public static string ScreenshotDirectory_GroupName
	{
		get
		{
			return "Screenshot";
		}
	}

	// Token: 0x17000111 RID: 273
	// (get) Token: 0x06001C20 RID: 7200 RVA: 0x000C771F File Offset: 0x000C5B1F
	// (set) Token: 0x06001C21 RID: 7201 RVA: 0x000C772B File Offset: 0x000C5B2B
	public static float ScreenshotSizeMultiplier
	{
		get
		{
			return Options.GetFloat("ScreenshotSizeMultiplier");
		}
		set
		{
			Options.SetFloat("ScreenshotSizeMultiplier", value);
		}
	}

	// Token: 0x17000112 RID: 274
	// (get) Token: 0x06001C22 RID: 7202 RVA: 0x000C7738 File Offset: 0x000C5B38
	public static string ScreenshotSizeMultiplier_GroupName
	{
		get
		{
			return "Screenshot";
		}
	}

	// Token: 0x17000113 RID: 275
	// (get) Token: 0x06001C23 RID: 7203 RVA: 0x000C773F File Offset: 0x000C5B3F
	// (set) Token: 0x06001C24 RID: 7204 RVA: 0x000C774B File Offset: 0x000C5B4B
	public static bool AntialiasScreenshot
	{
		get
		{
			return Options.GetBool("AntialiasScreenshot");
		}
		set
		{
			Options.SetBool("AntialiasScreenshot", value);
		}
	}

	// Token: 0x17000114 RID: 276
	// (get) Token: 0x06001C25 RID: 7205 RVA: 0x000C7758 File Offset: 0x000C5B58
	public static string AntialiasScreenshot_GroupName
	{
		get
		{
			return "Screenshot";
		}
	}

	// Token: 0x17000115 RID: 277
	// (get) Token: 0x06001C26 RID: 7206 RVA: 0x000C775F File Offset: 0x000C5B5F
	// (set) Token: 0x06001C27 RID: 7207 RVA: 0x000C776B File Offset: 0x000C5B6B
	public static bool RemoveBackgroundInScreenshot
	{
		get
		{
			return Options.GetBool("RemoveBackgroundInScreenshot");
		}
		set
		{
			Options.SetBool("RemoveBackgroundInScreenshot", value);
		}
	}

	// Token: 0x17000116 RID: 278
	// (get) Token: 0x06001C28 RID: 7208 RVA: 0x000C7778 File Offset: 0x000C5B78
	public static string RemoveBackgroundInScreenshot_GroupName
	{
		get
		{
			return "Screenshot";
		}
	}

	// Token: 0x17000117 RID: 279
	// (get) Token: 0x06001C29 RID: 7209 RVA: 0x000C777F File Offset: 0x000C5B7F
	// (set) Token: 0x06001C2A RID: 7210 RVA: 0x000C778B File Offset: 0x000C5B8B
	public static bool SaveWorldsAsInIOS
	{
		get
		{
			return Options.GetBool("SaveWorldsAsInIOS");
		}
		set
		{
			Options.SetBool("SaveWorldsAsInIOS", value);
		}
	}

	// Token: 0x17000118 RID: 280
	// (get) Token: 0x06001C2B RID: 7211 RVA: 0x000C7798 File Offset: 0x000C5B98
	// (set) Token: 0x06001C2C RID: 7212 RVA: 0x000C77A4 File Offset: 0x000C5BA4
	public static bool AutoSaveEnabled
	{
		get
		{
			return Options.GetBool("AutoSaveEnabled");
		}
		set
		{
			Options.SetBool("AutoSavedEnabled", value);
		}
	}

	// Token: 0x17000119 RID: 281
	// (get) Token: 0x06001C2D RID: 7213 RVA: 0x000C77B1 File Offset: 0x000C5BB1
	// (set) Token: 0x06001C2E RID: 7214 RVA: 0x000C77BD File Offset: 0x000C5BBD
	public static bool DisableEditorMouseInput
	{
		get
		{
			return Options.GetBool("DisableEditorMouseInput");
		}
		set
		{
			Options.SetBool("DisableEditorMouseInput", value);
		}
	}

	// Token: 0x1700011A RID: 282
	// (get) Token: 0x06001C2F RID: 7215 RVA: 0x000C77CA File Offset: 0x000C5BCA
	// (set) Token: 0x06001C30 RID: 7216 RVA: 0x000C77D6 File Offset: 0x000C5BD6
	public static bool ShowIOSControlsInEditor
	{
		get
		{
			return Options.GetBool("ShowIOSControlsInEditor");
		}
		set
		{
			Options.SetBool("ShowIOSControlsInEditor", value);
		}
	}

	// Token: 0x1700011B RID: 283
	// (get) Token: 0x06001C31 RID: 7217 RVA: 0x000C77E3 File Offset: 0x000C5BE3
	// (set) Token: 0x06001C32 RID: 7218 RVA: 0x000C77EF File Offset: 0x000C5BEF
	public static bool UseCompactGafWriteRenamings
	{
		get
		{
			return Options.GetBool("UseCompactGafWriteRenamings");
		}
		set
		{
			Options.SetBool("UseCompactGafWriteRenamings", value);
		}
	}

	// Token: 0x1700011C RID: 284
	// (get) Token: 0x06001C33 RID: 7219 RVA: 0x000C77FC File Offset: 0x000C5BFC
	// (set) Token: 0x06001C34 RID: 7220 RVA: 0x000C7808 File Offset: 0x000C5C08
	public static int WorldBackupMaxCount
	{
		get
		{
			return PlayerPrefs.GetInt("WorldBackupMaxCount");
		}
		set
		{
			PlayerPrefs.SetInt("WorldBackupMaxCount", value);
		}
	}

	// Token: 0x1700011D RID: 285
	// (get) Token: 0x06001C35 RID: 7221 RVA: 0x000C7815 File Offset: 0x000C5C15
	public static string WorldBackupMaxCount_GroupName
	{
		get
		{
			return "World Backup";
		}
	}

	// Token: 0x1700011E RID: 286
	// (get) Token: 0x06001C36 RID: 7222 RVA: 0x000C781C File Offset: 0x000C5C1C
	// (set) Token: 0x06001C37 RID: 7223 RVA: 0x000C7828 File Offset: 0x000C5C28
	public static bool EnableUnityEditorScarcity
	{
		get
		{
			return Options.GetBool("EnableUnityEditorScarcity");
		}
		set
		{
			Options.SetBool("EnableUnityEditorScarcity", value);
		}
	}

	// Token: 0x1700011F RID: 287
	// (get) Token: 0x06001C38 RID: 7224 RVA: 0x000C7835 File Offset: 0x000C5C35
	// (set) Token: 0x06001C39 RID: 7225 RVA: 0x000C7841 File Offset: 0x000C5C41
	public static bool UseExampleInventory
	{
		get
		{
			return Options.GetBool("UseExampleInventory");
		}
		set
		{
			Options.SetBool("UseExampleInventory", value);
		}
	}

	// Token: 0x17000120 RID: 288
	// (get) Token: 0x06001C3A RID: 7226 RVA: 0x000C784E File Offset: 0x000C5C4E
	// (set) Token: 0x06001C3B RID: 7227 RVA: 0x000C785A File Offset: 0x000C5C5A
	public static bool EnableOpaqueParameterBackground
	{
		get
		{
			return Options.GetBool("EnableOpaqueParameterBackground");
		}
		set
		{
			Options.SetBool("EnableOpaqueParameterBackground", value);
		}
	}

	// Token: 0x17000121 RID: 289
	// (get) Token: 0x06001C3C RID: 7228 RVA: 0x000C7867 File Offset: 0x000C5C67
	// (set) Token: 0x06001C3D RID: 7229 RVA: 0x000C7873 File Offset: 0x000C5C73
	public static bool CreateErrorGafs
	{
		get
		{
			return Options.GetBool("CreateErrorGafs");
		}
		set
		{
			Options.SetBool("CreateErrorGafs", value);
		}
	}

	// Token: 0x17000122 RID: 290
	// (get) Token: 0x06001C3E RID: 7230 RVA: 0x000C7880 File Offset: 0x000C5C80
	// (set) Token: 0x06001C3F RID: 7231 RVA: 0x000C788C File Offset: 0x000C5C8C
	public static bool InterpolateRigidBodies
	{
		get
		{
			return Options.GetBool("InterpolateRigidBodies");
		}
		set
		{
			Options.SetBool("InterpolateRigidBodies", value);
		}
	}

	// Token: 0x17000123 RID: 291
	// (get) Token: 0x06001C40 RID: 7232 RVA: 0x000C7899 File Offset: 0x000C5C99
	// (set) Token: 0x06001C41 RID: 7233 RVA: 0x000C78A5 File Offset: 0x000C5CA5
	public static float TimeScale
	{
		get
		{
			return Options.GetFloat("TimeScale");
		}
		set
		{
			Options.SetFloat("TimeScale", value);
		}
	}

	// Token: 0x17000124 RID: 292
	// (get) Token: 0x06001C42 RID: 7234 RVA: 0x000C78B2 File Offset: 0x000C5CB2
	// (set) Token: 0x06001C43 RID: 7235 RVA: 0x000C78BE File Offset: 0x000C5CBE
	public static bool EditorIsHD
	{
		get
		{
			return Options.GetBool("EditorIsHD");
		}
		set
		{
			Options.SetBool("EditorIsHD", value);
		}
	}

	// Token: 0x17000125 RID: 293
	// (get) Token: 0x06001C44 RID: 7236 RVA: 0x000C78CB File Offset: 0x000C5CCB
	// (set) Token: 0x06001C45 RID: 7237 RVA: 0x000C78D7 File Offset: 0x000C5CD7
	public static bool MarketingScreenshots
	{
		get
		{
			return Options.GetBool("MarketingScreenshots");
		}
		set
		{
			Options.SetBool("MarketingScreenshots", value);
		}
	}

	// Token: 0x17000126 RID: 294
	// (get) Token: 0x06001C46 RID: 7238 RVA: 0x000C78E4 File Offset: 0x000C5CE4
	// (set) Token: 0x06001C47 RID: 7239 RVA: 0x000C78F0 File Offset: 0x000C5CF0
	public static bool Cowlorded
	{
		get
		{
			return Options.GetBool("cowlorded");
		}
		set
		{
			Options.SetBool("cowlorded", value);
		}
	}

	// Token: 0x17000127 RID: 295
	// (get) Token: 0x06001C48 RID: 7240 RVA: 0x000C78FD File Offset: 0x000C5CFD
	// (set) Token: 0x06001C49 RID: 7241 RVA: 0x000C7909 File Offset: 0x000C5D09
	public static string EditorUser
	{
		get
		{
			return Options.GetString("EditorUser");
		}
		set
		{
			Options.SetString("EditorUser", value);
		}
	}

	// Token: 0x17000128 RID: 296
	// (get) Token: 0x06001C4A RID: 7242 RVA: 0x000C7916 File Offset: 0x000C5D16
	// (set) Token: 0x06001C4B RID: 7243 RVA: 0x000C7922 File Offset: 0x000C5D22
	public static bool ReloadCachedMaterialsEveryFrame
	{
		get
		{
			return Options.GetBool("ReloadCachedMaterialsEveryFrame");
		}
		set
		{
			Options.SetBool("ReloadCachedMaterialsEveryFrame", value);
		}
	}

	// Token: 0x17000129 RID: 297
	// (get) Token: 0x06001C4C RID: 7244 RVA: 0x000C792F File Offset: 0x000C5D2F
	// (set) Token: 0x06001C4D RID: 7245 RVA: 0x000C793B File Offset: 0x000C5D3B
	public static bool NoSnapSave
	{
		get
		{
			return Options.GetBool("NoSnapSave");
		}
		set
		{
			Options.SetBool("NoSnapSave", value);
		}
	}

	// Token: 0x1700012A RID: 298
	// (get) Token: 0x06001C4E RID: 7246 RVA: 0x000C7948 File Offset: 0x000C5D48
	// (set) Token: 0x06001C4F RID: 7247 RVA: 0x000C7954 File Offset: 0x000C5D54
	public static bool WriteExtendedRenamingsOnLoad
	{
		get
		{
			return Options.GetBool("WriteExtendedRenamingsOnLoad");
		}
		set
		{
			Options.SetBool("WriteExtendedRenamingsOnLoad", value);
		}
	}

	// Token: 0x1700012B RID: 299
	// (get) Token: 0x06001C50 RID: 7248 RVA: 0x000C7961 File Offset: 0x000C5D61
	public static bool HighlightTouches
	{
		get
		{
			return Options.GetBool("highlightTouches");
		}
	}

	// Token: 0x1700012C RID: 300
	// (get) Token: 0x06001C51 RID: 7249 RVA: 0x000C796D File Offset: 0x000C5D6D
	// (set) Token: 0x06001C52 RID: 7250 RVA: 0x000C7979 File Offset: 0x000C5D79
	public static bool DisableShape
	{
		get
		{
			return Options.GetBool("disableShape");
		}
		set
		{
			Options.SetBool("disableShape", value);
		}
	}

	// Token: 0x1700012D RID: 301
	// (get) Token: 0x06001C53 RID: 7251 RVA: 0x000C7986 File Offset: 0x000C5D86
	// (set) Token: 0x06001C54 RID: 7252 RVA: 0x000C7992 File Offset: 0x000C5D92
	public static bool ShowScreenRecordButtonsInEditor
	{
		get
		{
			return Options.GetBool("showScreenRecordButtonsInEditor");
		}
		set
		{
			Options.SetBool("showScreenRecordButtonsInEditor", value);
		}
	}

	// Token: 0x06001C55 RID: 7253 RVA: 0x000C799F File Offset: 0x000C5D9F
	private static float GetFloat(string prefName)
	{
		return PlayerPrefs.GetFloat(prefName);
	}

	// Token: 0x06001C56 RID: 7254 RVA: 0x000C79A7 File Offset: 0x000C5DA7
	private static void SetFloat(string prefName, float value)
	{
		PlayerPrefs.SetFloat(prefName, value);
	}

	// Token: 0x06001C57 RID: 7255 RVA: 0x000C79B0 File Offset: 0x000C5DB0
	private static int GetInt(string prefName)
	{
		return PlayerPrefs.GetInt(prefName);
	}

	// Token: 0x06001C58 RID: 7256 RVA: 0x000C79B8 File Offset: 0x000C5DB8
	private static void SetInt(string prefName, int value)
	{
		PlayerPrefs.SetInt(prefName, value);
	}

	// Token: 0x06001C59 RID: 7257 RVA: 0x000C79C1 File Offset: 0x000C5DC1
	private static string GetString(string prefName)
	{
		return PlayerPrefs.GetString(prefName);
	}

	// Token: 0x06001C5A RID: 7258 RVA: 0x000C79C9 File Offset: 0x000C5DC9
	private static void SetString(string prefName, string value)
	{
		PlayerPrefs.SetString(prefName, value);
	}

	// Token: 0x06001C5B RID: 7259 RVA: 0x000C79D2 File Offset: 0x000C5DD2
	private static bool GetBool(string prefName)
	{
		return PlayerPrefs.GetInt(prefName) != 0;
	}

	// Token: 0x06001C5C RID: 7260 RVA: 0x000C79E0 File Offset: 0x000C5DE0
	private static void SetBool(string prefName, bool value)
	{
		PlayerPrefs.SetInt(prefName, (!value) ? 0 : 1);
	}

	// Token: 0x06001C5D RID: 7261 RVA: 0x000C79F5 File Offset: 0x000C5DF5
	private static bool GetBoolFast(string prefName, ref bool set, ref bool fast)
	{
		if (!set)
		{
			fast = Options.GetBool(prefName);
			set = true;
		}
		return fast;
	}

	// Token: 0x06001C5E RID: 7262 RVA: 0x000C7A0B File Offset: 0x000C5E0B
	private static void SetBoolFast(string prefName, bool value, ref bool set, ref bool fast)
	{
		Options.SetBool(prefName, value);
		set = true;
		fast = value;
	}

	// Token: 0x0400173A RID: 5946
	private static bool DisplayBlockNames_Set;

	// Token: 0x0400173B RID: 5947
	private static bool DisplayBlockNames_Fast;

	// Token: 0x0400173C RID: 5948
	private static bool DisplayWheelBlockNames_Set;

	// Token: 0x0400173D RID: 5949
	private static bool DisplayWheelBlockNames_Fast;

	// Token: 0x0400173E RID: 5950
	private static bool ShowMisalignedBlocks_Set;

	// Token: 0x0400173F RID: 5951
	private static bool ShowMisalignedBlocks_Fast;

	// Token: 0x04001740 RID: 5952
	private static bool OnScreenActionDebug_Set;

	// Token: 0x04001741 RID: 5953
	private static bool OnScreenActionDebug_Fast;
}
