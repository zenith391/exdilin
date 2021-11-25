using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

// Token: 0x02000368 RID: 872
public class WorldInfo
{
	// Token: 0x06002698 RID: 9880 RVA: 0x0011CBF0 File Offset: 0x0011AFF0
	public WorldInfo(JObject json)
	{
		JObject jobject = json["id"];
		this.id = jobject.StringValue;
		if (this.id == null)
		{
			this.id = jobject.IntValue.ToString();
		}
		this.title = json["title"].StringValue;
		if (json.ContainsKey("author_username"))
		{
			this.authorUserName = json["author_username"].StringValue;
		}
		else
		{
			this.authorUserName = string.Empty;
		}
		if (json.ContainsKey("author_id"))
		{
			this.authorId = json["author_id"].IntValue;
		}
		this.publicationStatus = (WorldPublicationStatus)json["publication_status"].IntValue;
		if (json.ContainsKey("image_urls_for_sizes"))
		{
			Dictionary<string, JObject> objectValue = json["image_urls_for_sizes"].ObjectValue;
			string key = (!Blocksworld.hd) ? "220x220" : "440x440";
			this.thumbnailUrl = objectValue[key].StringValue;
		}
		else
		{
			this.thumbnailUrl = null;
		}
	}

	// Token: 0x14000012 RID: 18
	// (add) Token: 0x06002699 RID: 9881 RVA: 0x0011CD24 File Offset: 0x0011B124
	// (remove) Token: 0x0600269A RID: 9882 RVA: 0x0011CD5C File Offset: 0x0011B15C
	public event WorldThumbnailLoadedEventHandler ThumbnailLoaded;

	// Token: 0x0600269B RID: 9883 RVA: 0x0011CD94 File Offset: 0x0011B194
	public bool LoadWorldSourceForTeleport()
	{
		if (!string.IsNullOrEmpty(this._worldSource))
		{
			return true;
		}
		string path = string.Format("/api/v1/worlds/{0}/source_for_teleport", this.id);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		if (WorldSession.isNormalBuildAndPlaySession() || WorldSession.current.config.jumpedFromBuildMode)
		{
			bwapirequestBase.AddParam("cache_max_age", 5f.ToString());
		}
		bwapirequestBase.onSuccess = delegate(JObject resp)
		{
			JObject jobject = resp["world"];
			this._worldSource = jobject["source_json_str"].StringValue;
		};
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.ToString());
		};
		bwapirequestBase.Send();
		return false;
	}

	// Token: 0x0600269C RID: 9884 RVA: 0x0011CE50 File Offset: 0x0011B250
	public void LoadThumbnail()
	{
		if (this._worldThumbnail == null)
		{
			this._worldThumbnail = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			int width = (int)(TileObject.sizeTile.x * NormalizedScreen.scale);
			int height = (int)(TileObject.sizeTile.y * NormalizedScreen.scale);
			this._worldThumbnailForTiles = new Texture2D(width, height, TextureFormat.RGBA32, false);
			Blocksworld.bw.StartCoroutine(this.LoadThumbnailCoroutine());
		}
	}

	// Token: 0x0600269D RID: 9885 RVA: 0x0011CEC1 File Offset: 0x0011B2C1
	public Texture2D GetWorldThumbnail()
	{
		return this._worldThumbnail;
	}

	// Token: 0x0600269E RID: 9886 RVA: 0x0011CEC9 File Offset: 0x0011B2C9
	public Texture2D GetWorldThumbnailForTiles()
	{
		return this._worldThumbnailForTiles;
	}

	// Token: 0x0600269F RID: 9887 RVA: 0x0011CED1 File Offset: 0x0011B2D1
	private void OnTumbnailLoaded(EventArgs e)
	{
		if (this.ThumbnailLoaded != null)
		{
			this.ThumbnailLoaded(this, e);
		}
	}

	// Token: 0x060026A0 RID: 9888 RVA: 0x0011CEEC File Offset: 0x0011B2EC
	private IEnumerator LoadThumbnailCoroutine()
	{
		if (string.IsNullOrEmpty(this.thumbnailUrl))
		{
			yield break;
		}
		WWW loader = new WWW(this.thumbnailUrl);
		while (!loader.isDone)
		{
			yield return null;
		}
		if (this._worldThumbnail == null || this._worldThumbnailForTiles == null)
		{
			yield break;
		}
		loader.LoadImageIntoTexture(this._worldThumbnail);
		float tileScale = (float)this._worldThumbnail.height / (float)this._worldThumbnailForTiles.height;
		int screenScale = (int)NormalizedScreen.scale;
		int border = 2 * screenScale;
		int borderRadius = 4 * screenScale;
		int totalBorder = border + borderRadius;
		int width = this._worldThumbnailForTiles.width;
		int height = this._worldThumbnailForTiles.height;
		int yCrop = 15 * screenScale;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				float a;
				if (i < yCrop)
				{
					a = 0f;
				}
				else if (j > totalBorder && j < width - totalBorder && i > totalBorder && i < height - totalBorder)
				{
					a = 1f;
				}
				else if (j < border || j > width - border)
				{
					a = 0f;
				}
				else if (i < border || i > height - border)
				{
					a = 0f;
				}
				else
				{
					float num = (float)j;
					float num2 = (float)i;
					float num3 = num;
					float num4 = num2;
					if (j <= totalBorder)
					{
						num3 = (float)totalBorder;
					}
					else if (j >= width - totalBorder)
					{
						num3 = (float)(width - totalBorder);
					}
					if (i <= totalBorder)
					{
						num4 = (float)totalBorder;
					}
					else if (i >= height - totalBorder)
					{
						num4 = (float)(height - totalBorder);
					}
					float num5 = num - num3;
					float num6 = num2 - num4;
					float num7 = num5 * num5 + num6 * num6;
					a = 1f - num7 / (float)(borderRadius * borderRadius);
				}
				int y = (int)((float)i * tileScale);
				int x = (int)((float)j * tileScale);
				Color pixel = this._worldThumbnail.GetPixel(x, y);
				pixel.a = a;
				this._worldThumbnailForTiles.SetPixel(j, i, pixel);
			}
		}
		this._worldThumbnailForTiles.Apply();
		this.OnTumbnailLoaded(EventArgs.Empty);
		yield break;
	}

	// Token: 0x060026A1 RID: 9889 RVA: 0x0011CF07 File Offset: 0x0011B307
	public bool HasWorldSource()
	{
		return !string.IsNullOrEmpty(this._worldSource);
	}

	// Token: 0x060026A2 RID: 9890 RVA: 0x0011CF17 File Offset: 0x0011B317
	public string WorldSource()
	{
		return this._worldSource;
	}

	// Token: 0x060026A3 RID: 9891 RVA: 0x0011CF1F File Offset: 0x0011B31F
	public string TitleForDropdown()
	{
		if (string.IsNullOrEmpty(this.title))
		{
			return "Untitled (id: " + this.id + ")";
		}
		return this.title;
	}

	// Token: 0x060026A4 RID: 9892 RVA: 0x0011CF4D File Offset: 0x0011B34D
	public void DestroyThumbnailImages()
	{
		if (this._worldThumbnail != null)
		{
			UnityEngine.Object.Destroy(this._worldThumbnail);
		}
		if (this._worldThumbnailForTiles != null)
		{
			UnityEngine.Object.Destroy(this._worldThumbnailForTiles);
		}
	}

	// Token: 0x060026A5 RID: 9893 RVA: 0x0011CF88 File Offset: 0x0011B388
	public static void Get(int worldId, Action<WorldInfo> successCallback, Action failureCallback)
	{
		string path = string.Format("/api/v1/worlds/{0}/basic_info", worldId);
		BWAPIRequestBase bwapirequestBase = BW.API.CreateRequest("GET", path);
		bwapirequestBase.onSuccess = delegate(JObject responseJson)
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
		bwapirequestBase.onFailure = delegate(BWAPIRequestError error)
		{
			BWLog.Info(error.message);
			failureCallback();
		};
		bwapirequestBase.Send();
	}

	// Token: 0x040021D9 RID: 8665
	public string id;

	// Token: 0x040021DA RID: 8666
	public string title;

	// Token: 0x040021DB RID: 8667
	public string description;

	// Token: 0x040021DC RID: 8668
	public string authorUserName;

	// Token: 0x040021DD RID: 8669
	public int authorId;

	// Token: 0x040021DE RID: 8670
	public WorldPublicationStatus publicationStatus;

	// Token: 0x040021DF RID: 8671
	public string thumbnailUrl;

	// Token: 0x040021E1 RID: 8673
	private string _worldSource;

	// Token: 0x040021E2 RID: 8674
	private Texture2D _worldThumbnail;

	// Token: 0x040021E3 RID: 8675
	private Texture2D _worldThumbnailForTiles;
}
