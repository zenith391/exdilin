using System.Runtime.InteropServices;
using AOT;

public class CSharpInterface
{
	private delegate void MethodDelegate();

	private delegate int MethodDelegate_Int();

	private delegate void MethodDelegate_WithArgs_String(string arg);

	private delegate string MethodDelegate_String_WithArgs_String(string arg);

	private delegate void MethodDelegate_WithArgs_Int(int arg);

	private delegate void MethodDelegate_WithArgs_String_Int(string argStr, int argInt);

	private delegate void MethodDelegate_WithArgs_String_String(string arg0, string arg1);

	private delegate void MethodDelegate_WithArgs_Int_Int(int arg0, int arg1);

	private delegate void MethodDelegate_WithArgs_Int_String(int arg0, string arg1);

	private delegate int MethodDelegate_Int_WithArgs_String(string arg);

	public static void Register()
	{
	}

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void Blocksworld_DidReceiveMemoryWarning()
	{
		Blocksworld.DidReceiveMemoryWarning();
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_DidReceiveMemoryWarning(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void Blocksworld_ForceSave()
	{
		Blocksworld.ForceSave();
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_ForceSave(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string Blocksworld_LockModelJSON(string modelJson)
	{
		return Blocksworld.LockModelJSON(modelJson);
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_LockModelJSON(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string Blocksworld_UnlockModelJSON(string modelJson)
	{
		return Blocksworld.UnlockModelJSON(modelJson);
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_UnlockModelJSON(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void Blocksworld_LoadBlocksworldSceneAsync()
	{
		Blocksworld.LoadBlocksworldSceneAsync();
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_LoadBlocksworldSceneAsync(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void Blocksworld_LoadBlocksworldSceneSync()
	{
		Blocksworld.LoadBlocksworldSceneSync();
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_LoadBlocksworldSceneSync(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String_Int))]
	private static void Blocksworld_SetSpeechBubbleText(string text, int parameterIndex)
	{
		Blocksworld.SetSpeechBubbleText(text, parameterIndex);
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_SetSpeechBubbleText(MethodDelegate_WithArgs_String_Int method);

	[MonoPInvokeCallback(typeof(MethodDelegate_Int))]
	private static int Blocksworld_IsStarted()
	{
		if (Blocksworld.IsStarted())
		{
			return 1;
		}
		return 0;
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_IsStarted(MethodDelegate_Int method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_Int))]
	private static void Blocksworld_SetLoggingLevel(int loggingLevel)
	{
		BWLog.loggingLevel = loggingLevel;
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_SetLoggingLevel(MethodDelegate_WithArgs_Int method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String))]
	private static void Blocksworld_SendMessage(string messageStr)
	{
		Blocksworld.RecieveIOSMessage(messageStr);
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_SendMessage(MethodDelegate_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void Blocksworld_MainMenuButtonsDidShow()
	{
		Blocksworld.MainMenuButtonsDidShow();
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_MainMenuButtonsDidShow(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string ModelCollection_GenerateMetadataForModelSource(string modelSourceJsonStr)
	{
		return ModelCollection.GenerateMetadataForModelSource(modelSourceJsonStr);
	}

	[DllImport("__Internal")]
	private static extern void register_ModelCollection_GenerateMetadataForModelSource(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string ModelCollection_GenerateBlocksInventoryForModelSource(string modelSourceJsonStr)
	{
		return ModelCollection.GenerateBlocksInventoryForModelSource(modelSourceJsonStr);
	}

	[DllImport("__Internal")]
	private static extern void register_ModelCollection_GenerateBlocksInventoryForModelSource(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string ModelCollection_GenerateSourceEqualityChecksumForModelSource(string modelSourceJsonStr)
	{
		return ModelCollection.GenerateSourceEqualityChecksumForModelSource(modelSourceJsonStr);
	}

	[DllImport("__Internal")]
	private static extern void register_ModelCollection_GenerateSourceEqualityChecksumForModelSource(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_Int_WithArgs_String))]
	private static int ModelUtils_IsValidShortTitle(string modelShortTitleStr)
	{
		if (ModelUtils.IsValidShortTitle(modelShortTitleStr))
		{
			return 1;
		}
		return 0;
	}

	[DllImport("__Internal")]
	private static extern void register_ModelUtils_IsValidShortTitle(MethodDelegate_Int_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String))]
	private static void WorldSession_Start(string worldSessionConfigJsonStr)
	{
		WorldSession.StartForIOS(worldSessionConfigJsonStr);
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_Start(MethodDelegate_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String_Int))]
	private static void WorldSession_UpdateModelCollection(string modelCollectionJsonStr, int includeOfflineModels)
	{
		WorldSession.UpdateModelCollection(modelCollectionJsonStr, includeOfflineModels != 0);
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_UpdateModelCollection(MethodDelegate_WithArgs_String_Int method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void WorldSession_RefreshModelIcons()
	{
		WorldSession.RefreshModelIcons();
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_RefreshModelIcons(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String))]
	private static void WorldSession_BuildingSetPurchaseCallback(string status)
	{
		WorldSession.BuildingSetPurchaseCallback(status);
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_BuildingSetPurchaseCallback(MethodDelegate_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String))]
	private static void WorldSession_ModelPurchaseCallback(string status)
	{
		WorldSession.ModelPurchaseCallback(status);
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_ModelPurchaseCallback(MethodDelegate_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void WorldSession_Pause()
	{
		WorldSession.PauseCurrentSession();
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_Pause(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void WorldSession_Unpause()
	{
		WorldSession.UnpauseCurrentSession();
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_Unpause(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate))]
	private static void WorldSession_Quit()
	{
		WorldSession.Quit();
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_Quit(MethodDelegate method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String))]
	private static void WorldSession_Quit_And_Navigate(string link)
	{
		WorldSession.QuitWithDeepLink(link);
	}

	[DllImport("__Internal")]
	private static extern void register_WorldSession_Quit_And_Navigate(MethodDelegate_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_String))]
	private static void Clipboard_LoadCallback(string clipboardString)
	{
		Clipboard.LoadCallback(clipboardString);
	}

	[DllImport("__Internal")]
	private static extern void register_Clipboard_LoadCallback(MethodDelegate_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_Int_WithArgs_String))]
	private static int GAF_IsJSONGAFListSupported(string jsonString)
	{
		if (GAF.IsJSONGAFListSupported(jsonString))
		{
			return 1;
		}
		return 0;
	}

	[DllImport("__Internal")]
	private static extern void register_GAF_IsJSONGAFListSupported(MethodDelegate_Int_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string Tile_IconDataForGAFString(string gafJsonString)
	{
		return TileIconManager.IconDataForGAFString(gafJsonString);
	}

	[DllImport("__Internal")]
	private static extern void register_Tile_IconDataForGAFString(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string Tile_IconDataForGAFArray(string gafJsonString)
	{
		return TileIconManager.IconDataForGAFArray(gafJsonString);
	}

	[DllImport("__Internal")]
	private static extern void register_Tile_IconDataForGAFArray(MethodDelegate_String_WithArgs_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_Int_String))]
	private static void BWAPIRequest_RequestSucceeded(int requestId, string responseJsonString)
	{
		BWAPIRequest_IOS.RequestSucceeded(requestId, responseJsonString);
	}

	[DllImport("__Internal")]
	private static extern void register_BWAPIRequest_RequestSucceeded(MethodDelegate_WithArgs_Int_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_WithArgs_Int_String))]
	private static void BWAPIRequest_RequestFailed(int requestId, string errorString)
	{
		BWAPIRequest_IOS.RequestFailed(requestId, errorString);
	}

	[DllImport("__Internal")]
	private static extern void register_BWAPIRequest_RequestFailed(MethodDelegate_WithArgs_Int_String method);

	[MonoPInvokeCallback(typeof(MethodDelegate_String_WithArgs_String))]
	private static string Blocksworld_GetWorldGAFUsageAsBlocksInventory(string worldSource)
	{
		return Blocksworld.GetWorldGAFUsageAsBlocksInventory(worldSource);
	}

	[DllImport("__Internal")]
	private static extern void register_Blocksworld_GetWorldGAFUsageAsBlocksInventory(MethodDelegate_String_WithArgs_String method);
}
