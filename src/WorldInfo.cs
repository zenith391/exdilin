using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class WorldInfo
{
	public string id;

	public string title;

	public string description;

	public string authorUserName;

	public int authorId;

	public WorldPublicationStatus publicationStatus;

	public string thumbnailUrl;

	private string _worldSource;

	private Texture2D _worldThumbnail;

	private Texture2D _worldThumbnailForTiles;

	public event WorldThumbnailLoadedEventHandler ThumbnailLoaded;

	public WorldInfo(JObject json)
	{
		JObject jObject = json["id"];
		id = jObject.StringValue;
		if (id == null)
		{
			id = jObject.IntValue.ToString();
		}
		title = json["title"].StringValue;
		if (json.ContainsKey("author_username"))
		{
			authorUserName = json["author_username"].StringValue;
		}
		else
		{
			authorUserName = string.Empty;
		}
		if (json.ContainsKey("author_id"))
		{
			authorId = json["author_id"].IntValue;
		}
		publicationStatus = (WorldPublicationStatus)json["publication_status"].IntValue;
		if (json.ContainsKey("image_urls_for_sizes"))
		{
			Dictionary<string, JObject> objectValue = json["image_urls_for_sizes"].ObjectValue;
			thumbnailUrl = objectValue[(!Blocksworld.hd) ? "220x220" : "440x440"].StringValue;
		}
		else
		{
			thumbnailUrl = null;
		}
	}

	public bool LoadWorldSourceForTeleport()
	{
		if (!string.IsNullOrEmpty(_worldSource))
		{
			return true;
		}
		string path = $"/api/v1/worlds/{id}/source_for_teleport";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		if (WorldSession.isNormalBuildAndPlaySession() || WorldSession.current.config.jumpedFromBuildMode)
		{
			bWAPIRequestBase.AddParam("cache_max_age", 5f.ToString());
		}
		bWAPIRequestBase.onSuccess = delegate(JObject resp)
		{
			JObject jObject = resp["world"];
			_worldSource = jObject["source_json_str"].StringValue;
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
		};
		bWAPIRequestBase.Send();
		return false;
	}

	public void LoadThumbnail()
	{
		if (_worldThumbnail == null)
		{
			_worldThumbnail = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false);
			int width = (int)(TileObject.sizeTile.x * NormalizedScreen.scale);
			int height = (int)(TileObject.sizeTile.y * NormalizedScreen.scale);
			_worldThumbnailForTiles = new Texture2D(width, height, TextureFormat.RGBA32, mipmap: false);
			Blocksworld.bw.StartCoroutine(LoadThumbnailCoroutine());
		}
	}

	public Texture2D GetWorldThumbnail()
	{
		return _worldThumbnail;
	}

	public Texture2D GetWorldThumbnailForTiles()
	{
		return _worldThumbnailForTiles;
	}

	private void OnTumbnailLoaded(EventArgs e)
	{
		if (this.ThumbnailLoaded != null)
		{
			this.ThumbnailLoaded(this, e);
		}
	}

	private IEnumerator LoadThumbnailCoroutine()
	{
		if (string.IsNullOrEmpty(thumbnailUrl))
		{
			yield break;
		}
		WWW loader = new WWW(thumbnailUrl);
		while (!loader.isDone)
		{
			yield return null;
		}
		if (_worldThumbnail == null || _worldThumbnailForTiles == null)
		{
			yield break;
		}
		loader.LoadImageIntoTexture(_worldThumbnail);
		float num = (float)_worldThumbnail.height / (float)_worldThumbnailForTiles.height;
		int num2 = (int)NormalizedScreen.scale;
		int num3 = 2 * num2;
		int num4 = 4 * num2;
		int num5 = num3 + num4;
		int width = _worldThumbnailForTiles.width;
		int height = _worldThumbnailForTiles.height;
		int num6 = 15 * num2;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				float a;
				if (i < num6)
				{
					a = 0f;
				}
				else if (j > num5 && j < width - num5 && i > num5 && i < height - num5)
				{
					a = 1f;
				}
				else if (j < num3 || j > width - num3)
				{
					a = 0f;
				}
				else if (i < num3 || i > height - num3)
				{
					a = 0f;
				}
				else
				{
					float num7 = j;
					float num8 = i;
					float num9 = num7;
					float num10 = num8;
					if (j <= num5)
					{
						num9 = num5;
					}
					else if (j >= width - num5)
					{
						num9 = width - num5;
					}
					if (i <= num5)
					{
						num10 = num5;
					}
					else if (i >= height - num5)
					{
						num10 = height - num5;
					}
					float num11 = num7 - num9;
					float num12 = num8 - num10;
					float num13 = num11 * num11 + num12 * num12;
					a = 1f - num13 / (float)(num4 * num4);
				}
				int y = (int)((float)i * num);
				int x = (int)((float)j * num);
				Color pixel = _worldThumbnail.GetPixel(x, y);
				pixel.a = a;
				_worldThumbnailForTiles.SetPixel(j, i, pixel);
			}
		}
		_worldThumbnailForTiles.Apply();
		OnTumbnailLoaded(EventArgs.Empty);
	}

	public bool HasWorldSource()
	{
		return !string.IsNullOrEmpty(_worldSource);
	}

	public string WorldSource()
	{
		return _worldSource;
	}

	public string TitleForDropdown()
	{
		if (string.IsNullOrEmpty(title))
		{
			return "Untitled (id: " + id + ")";
		}
		return title;
	}

	public void DestroyThumbnailImages()
	{
		if (_worldThumbnail != null)
		{
			UnityEngine.Object.Destroy(_worldThumbnail);
		}
		if (_worldThumbnailForTiles != null)
		{
			UnityEngine.Object.Destroy(_worldThumbnailForTiles);
		}
	}

	public static void Get(int worldId, Action<WorldInfo> successCallback, Action failureCallback)
	{
		string path = $"/api/v1/worlds/{worldId}/basic_info";
		BWAPIRequestBase bWAPIRequestBase = BW.API.CreateRequest("GET", path);
		bWAPIRequestBase.onSuccess = delegate(JObject responseJson)
		{
			if (responseJson == null)
			{
				failureCallback();
			}
			else
			{
				WorldInfo obj = new WorldInfo(responseJson);
				successCallback(obj);
			}
		};
		bWAPIRequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.message);
			failureCallback();
		};
		bWAPIRequestBase.Send();
	}
}
