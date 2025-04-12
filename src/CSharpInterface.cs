using System;
using System.Runtime.InteropServices;
using AOT;

// Token: 0x02000102 RID: 258
public class CSharpInterface
{
	// Token: 0x060012AE RID: 4782 RVA: 0x00082462 File Offset: 0x00080862
	public static void Register()
	{
	}

	// Token: 0x060012AF RID: 4783 RVA: 0x00082464 File Offset: 0x00080864
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void Blocksworld_DidReceiveMemoryWarning()
	{
		Blocksworld.DidReceiveMemoryWarning();
	}

	// Token: 0x060012B0 RID: 4784
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_DidReceiveMemoryWarning(CSharpInterface.MethodDelegate method);

	// Token: 0x060012B1 RID: 4785 RVA: 0x0008246B File Offset: 0x0008086B
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void Blocksworld_ForceSave()
	{
		Blocksworld.ForceSave();
	}

	// Token: 0x060012B2 RID: 4786
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_ForceSave(CSharpInterface.MethodDelegate method);

	// Token: 0x060012B3 RID: 4787 RVA: 0x00082472 File Offset: 0x00080872
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string Blocksworld_LockModelJSON(string modelJson)
	{
		return Blocksworld.LockModelJSON(modelJson);
	}

	// Token: 0x060012B4 RID: 4788
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_LockModelJSON(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012B5 RID: 4789 RVA: 0x0008247A File Offset: 0x0008087A
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string Blocksworld_UnlockModelJSON(string modelJson)
	{
		return Blocksworld.UnlockModelJSON(modelJson);
	}

	// Token: 0x060012B6 RID: 4790
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_UnlockModelJSON(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012B7 RID: 4791 RVA: 0x00082482 File Offset: 0x00080882
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void Blocksworld_LoadBlocksworldSceneAsync()
	{
		Blocksworld.LoadBlocksworldSceneAsync();
	}

	// Token: 0x060012B8 RID: 4792
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_LoadBlocksworldSceneAsync(CSharpInterface.MethodDelegate method);

	// Token: 0x060012B9 RID: 4793 RVA: 0x00082489 File Offset: 0x00080889
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void Blocksworld_LoadBlocksworldSceneSync()
	{
		Blocksworld.LoadBlocksworldSceneSync();
	}

	// Token: 0x060012BA RID: 4794
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_LoadBlocksworldSceneSync(CSharpInterface.MethodDelegate method);

	// Token: 0x060012BB RID: 4795 RVA: 0x00082490 File Offset: 0x00080890
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String_Int))]
	private static void Blocksworld_SetSpeechBubbleText(string text, int parameterIndex)
	{
		Blocksworld.SetSpeechBubbleText(text, parameterIndex);
	}

	// Token: 0x060012BC RID: 4796
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_SetSpeechBubbleText(CSharpInterface.MethodDelegate_WithArgs_String_Int method);

	// Token: 0x060012BD RID: 4797 RVA: 0x00082499 File Offset: 0x00080899
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_Int))]
	private static int Blocksworld_IsStarted()
	{
		return (!Blocksworld.IsStarted()) ? 0 : 1;
	}

	// Token: 0x060012BE RID: 4798
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_IsStarted(CSharpInterface.MethodDelegate_Int method);

	// Token: 0x060012BF RID: 4799 RVA: 0x000824AC File Offset: 0x000808AC
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_Int))]
	private static void Blocksworld_SetLoggingLevel(int loggingLevel)
	{
		BWLog.loggingLevel = loggingLevel;
	}

	// Token: 0x060012C0 RID: 4800
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_SetLoggingLevel(CSharpInterface.MethodDelegate_WithArgs_Int method);

	// Token: 0x060012C1 RID: 4801 RVA: 0x000824B4 File Offset: 0x000808B4
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String))]
	private static void Blocksworld_SendMessage(string messageStr)
	{
		Blocksworld.RecieveIOSMessage(messageStr);
	}

	// Token: 0x060012C2 RID: 4802
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_SendMessage(CSharpInterface.MethodDelegate_WithArgs_String method);

	// Token: 0x060012C3 RID: 4803 RVA: 0x000824BC File Offset: 0x000808BC
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void Blocksworld_MainMenuButtonsDidShow()
	{
		Blocksworld.MainMenuButtonsDidShow();
	}

	// Token: 0x060012C4 RID: 4804
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_MainMenuButtonsDidShow(CSharpInterface.MethodDelegate method);

	// Token: 0x060012C5 RID: 4805 RVA: 0x000824C3 File Offset: 0x000808C3
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string ModelCollection_GenerateMetadataForModelSource(string modelSourceJsonStr)
	{
		return ModelCollection.GenerateMetadataForModelSource(modelSourceJsonStr);
	}

	// Token: 0x060012C6 RID: 4806
	[DllImport("__Internal")]
	private static extern void register_ModelCollection_GenerateMetadataForModelSource(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012C7 RID: 4807 RVA: 0x000824CB File Offset: 0x000808CB
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string ModelCollection_GenerateBlocksInventoryForModelSource(string modelSourceJsonStr)
	{
		return ModelCollection.GenerateBlocksInventoryForModelSource(modelSourceJsonStr);
	}

	// Token: 0x060012C8 RID: 4808
	[DllImport("__Internal")]
	private static extern void register_ModelCollection_GenerateBlocksInventoryForModelSource(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012C9 RID: 4809 RVA: 0x000824D3 File Offset: 0x000808D3
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string ModelCollection_GenerateSourceEqualityChecksumForModelSource(string modelSourceJsonStr)
	{
		return ModelCollection.GenerateSourceEqualityChecksumForModelSource(modelSourceJsonStr);
	}

	// Token: 0x060012CA RID: 4810
	[DllImport("__Internal")]
	private static extern void register_ModelCollection_GenerateSourceEqualityChecksumForModelSource(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012CB RID: 4811 RVA: 0x000824DB File Offset: 0x000808DB
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_Int_WithArgs_String))]
	private static int ModelUtils_IsValidShortTitle(string modelShortTitleStr)
	{
		return (!ModelUtils.IsValidShortTitle(modelShortTitleStr)) ? 0 : 1;
	}

	// Token: 0x060012CC RID: 4812
	[DllImport("__Internal")]
	private static extern void register_ModelUtils_IsValidShortTitle(CSharpInterface.MethodDelegate_Int_WithArgs_String method);

	// Token: 0x060012CD RID: 4813 RVA: 0x000824EF File Offset: 0x000808EF
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String))]
	private static void WorldSession_Start(string worldSessionConfigJsonStr)
	{
		WorldSession.StartForIOS(worldSessionConfigJsonStr);
	}

	// Token: 0x060012CE RID: 4814
	[DllImport("__Internal")]
	private static extern void register_WorldSession_Start(CSharpInterface.MethodDelegate_WithArgs_String method);

	// Token: 0x060012CF RID: 4815 RVA: 0x000824F7 File Offset: 0x000808F7
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String_Int))]
	private static void WorldSession_UpdateModelCollection(string modelCollectionJsonStr, int includeOfflineModels)
	{
		WorldSession.UpdateModelCollection(modelCollectionJsonStr, includeOfflineModels != 0);
	}

	// Token: 0x060012D0 RID: 4816
	[DllImport("__Internal")]
	private static extern void register_WorldSession_UpdateModelCollection(CSharpInterface.MethodDelegate_WithArgs_String_Int method);

	// Token: 0x060012D1 RID: 4817 RVA: 0x00082506 File Offset: 0x00080906
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void WorldSession_RefreshModelIcons()
	{
		WorldSession.RefreshModelIcons();
	}

	// Token: 0x060012D2 RID: 4818
	[DllImport("__Internal")]
	private static extern void register_WorldSession_RefreshModelIcons(CSharpInterface.MethodDelegate method);

	// Token: 0x060012D3 RID: 4819 RVA: 0x0008250D File Offset: 0x0008090D
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String))]
	private static void WorldSession_BuildingSetPurchaseCallback(string status)
	{
		WorldSession.BuildingSetPurchaseCallback(status);
	}

	// Token: 0x060012D4 RID: 4820
	[DllImport("__Internal")]
	private static extern void register_WorldSession_BuildingSetPurchaseCallback(CSharpInterface.MethodDelegate_WithArgs_String method);

	// Token: 0x060012D5 RID: 4821 RVA: 0x00082515 File Offset: 0x00080915
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String))]
	private static void WorldSession_ModelPurchaseCallback(string status)
	{
		WorldSession.ModelPurchaseCallback(status);
	}

	// Token: 0x060012D6 RID: 4822
	[DllImport("__Internal")]
	private static extern void register_WorldSession_ModelPurchaseCallback(CSharpInterface.MethodDelegate_WithArgs_String method);

	// Token: 0x060012D7 RID: 4823 RVA: 0x0008251D File Offset: 0x0008091D
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void WorldSession_Pause()
	{
		WorldSession.PauseCurrentSession();
	}

	// Token: 0x060012D8 RID: 4824
	[DllImport("__Internal")]
	private static extern void register_WorldSession_Pause(CSharpInterface.MethodDelegate method);

	// Token: 0x060012D9 RID: 4825 RVA: 0x00082524 File Offset: 0x00080924
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void WorldSession_Unpause()
	{
		WorldSession.UnpauseCurrentSession();
	}

	// Token: 0x060012DA RID: 4826
	[DllImport("__Internal")]
	private static extern void register_WorldSession_Unpause(CSharpInterface.MethodDelegate method);

	// Token: 0x060012DB RID: 4827 RVA: 0x0008252B File Offset: 0x0008092B
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate))]
	private static void WorldSession_Quit()
	{
		WorldSession.Quit();
	}

	// Token: 0x060012DC RID: 4828
	[DllImport("__Internal")]
	private static extern void register_WorldSession_Quit(CSharpInterface.MethodDelegate method);

	// Token: 0x060012DD RID: 4829 RVA: 0x00082532 File Offset: 0x00080932
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String))]
	private static void WorldSession_Quit_And_Navigate(string link)
	{
		WorldSession.QuitWithDeepLink(link);
	}

	// Token: 0x060012DE RID: 4830
	[DllImport("__Internal")]
	private static extern void register_WorldSession_Quit_And_Navigate(CSharpInterface.MethodDelegate_WithArgs_String method);

	// Token: 0x060012DF RID: 4831 RVA: 0x0008253A File Offset: 0x0008093A
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_String))]
	private static void Clipboard_LoadCallback(string clipboardString)
	{
		Clipboard.LoadCallback(clipboardString);
	}

	// Token: 0x060012E0 RID: 4832
	[DllImport("__Internal")]
	private static extern void register_Clipboard_LoadCallback(CSharpInterface.MethodDelegate_WithArgs_String method);

	// Token: 0x060012E1 RID: 4833 RVA: 0x00082542 File Offset: 0x00080942
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_Int_WithArgs_String))]
	private static int GAF_IsJSONGAFListSupported(string jsonString)
	{
		return (!GAF.IsJSONGAFListSupported(jsonString)) ? 0 : 1;
	}

	// Token: 0x060012E2 RID: 4834
	[DllImport("__Internal")]
	private static extern void register_GAF_IsJSONGAFListSupported(CSharpInterface.MethodDelegate_Int_WithArgs_String method);

	// Token: 0x060012E3 RID: 4835 RVA: 0x00082556 File Offset: 0x00080956
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string Tile_IconDataForGAFString(string gafJsonString)
	{
		return TileIconManager.IconDataForGAFString(gafJsonString);
	}

	// Token: 0x060012E4 RID: 4836
	[DllImport("__Internal")]
	private static extern void register_Tile_IconDataForGAFString(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012E5 RID: 4837 RVA: 0x0008255E File Offset: 0x0008095E
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string Tile_IconDataForGAFArray(string gafJsonString)
	{
		return TileIconManager.IconDataForGAFArray(gafJsonString);
	}

	// Token: 0x060012E6 RID: 4838
	[DllImport("__Internal")]
	private static extern void register_Tile_IconDataForGAFArray(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x060012E7 RID: 4839 RVA: 0x00082566 File Offset: 0x00080966
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_Int_String))]
	private static void BWAPIRequest_RequestSucceeded(int requestId, string responseJsonString)
	{
		BWAPIRequest_IOS.RequestSucceeded(requestId, responseJsonString);
	}

	// Token: 0x060012E8 RID: 4840
	[DllImport("__Internal")]
	private static extern void register_BWAPIRequest_RequestSucceeded(CSharpInterface.MethodDelegate_WithArgs_Int_String method);

	// Token: 0x060012E9 RID: 4841 RVA: 0x0008256F File Offset: 0x0008096F
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_WithArgs_Int_String))]
	private static void BWAPIRequest_RequestFailed(int requestId, string errorString)
	{
		BWAPIRequest_IOS.RequestFailed(requestId, errorString);
	}

	// Token: 0x060012EA RID: 4842
	[DllImport("__Internal")]
	private static extern void register_BWAPIRequest_RequestFailed(CSharpInterface.MethodDelegate_WithArgs_Int_String method);

	// Token: 0x060012EB RID: 4843 RVA: 0x00082578 File Offset: 0x00080978
	[MonoPInvokeCallback(typeof(CSharpInterface.MethodDelegate_String_WithArgs_String))]
	private static string Blocksworld_GetWorldGAFUsageAsBlocksInventory(string worldSource)
	{
		return Blocksworld.GetWorldGAFUsageAsBlocksInventory(worldSource);
	}

	// Token: 0x060012EC RID: 4844
	[DllImport("__Internal")]
	private static extern void register_Blocksworld_GetWorldGAFUsageAsBlocksInventory(CSharpInterface.MethodDelegate_String_WithArgs_String method);

	// Token: 0x02000103 RID: 259
	// (Invoke) Token: 0x060012EE RID: 4846
	private delegate void MethodDelegate();

	// Token: 0x02000104 RID: 260
	// (Invoke) Token: 0x060012F2 RID: 4850
	private delegate int MethodDelegate_Int();

	// Token: 0x02000105 RID: 261
	// (Invoke) Token: 0x060012F6 RID: 4854
	private delegate void MethodDelegate_WithArgs_String(string arg);

	// Token: 0x02000106 RID: 262
	// (Invoke) Token: 0x060012FA RID: 4858
	private delegate string MethodDelegate_String_WithArgs_String(string arg);

	// Token: 0x02000107 RID: 263
	// (Invoke) Token: 0x060012FE RID: 4862
	private delegate void MethodDelegate_WithArgs_Int(int arg);

	// Token: 0x02000108 RID: 264
	// (Invoke) Token: 0x06001302 RID: 4866
	private delegate void MethodDelegate_WithArgs_String_Int(string argStr, int argInt);

	// Token: 0x02000109 RID: 265
	// (Invoke) Token: 0x06001306 RID: 4870
	private delegate void MethodDelegate_WithArgs_String_String(string arg0, string arg1);

	// Token: 0x0200010A RID: 266
	// (Invoke) Token: 0x0600130A RID: 4874
	private delegate void MethodDelegate_WithArgs_Int_Int(int arg0, int arg1);

	// Token: 0x0200010B RID: 267
	// (Invoke) Token: 0x0600130E RID: 4878
	private delegate void MethodDelegate_WithArgs_Int_String(int arg0, string arg1);

	// Token: 0x0200010C RID: 268
	// (Invoke) Token: 0x06001312 RID: 4882
	private delegate int MethodDelegate_Int_WithArgs_String(string arg);
}
