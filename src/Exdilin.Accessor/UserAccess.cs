namespace Exdilin.Accessor;

public class UserAccess
{
	public static BWUser getCurrentUser()
	{
		return BWUser.currentUser;
	}

	public static string GetAuthToken()
	{
		return BWUser.currentUser.authToken;
	}

	public static string GetProfileImageURL(BWUser user)
	{
		return user.profileImageURL;
	}

	public static string GetUsername(BWUser user)
	{
		return user.username;
	}

	public static int GetCoins(BWUser user)
	{
		return user.coins;
	}

	public static int GetUserID(BWUser user)
	{
		return user.userID;
	}

	public static void PrepareForWorldSessionLaunch(BWWorld world)
	{
		BWStandalone.Instance.PrepareForWorldSessionLaunch();
		BWStandalone.Instance.currentWorldInfo = world;
	}

	public static void ReloadUserWorlds()
	{
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", "/api/v1/current_user/worlds");
		bWAPIRequestBase.onSuccess = BWUser.UpdateCurrentUser;
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Warning(error.ToString());
		};
		bWAPIRequestBase.Send();
	}
}
