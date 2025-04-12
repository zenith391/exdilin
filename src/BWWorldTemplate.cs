using System;
using SimpleJSON;

// Token: 0x020003D8 RID: 984
public class BWWorldTemplate
{
	// Token: 0x06002BB1 RID: 11185 RVA: 0x0013B7D4 File Offset: 0x00139BD4
	internal BWWorldTemplate(JObject json)
	{
		this.largeImageURL = BWJsonHelpers.PropertyIfExists(this.largeImageURL, "image_urls_for_sizes", "1024x768", json);
		this.smallImageURL = BWJsonHelpers.PropertyIfExists(this.smallImageURL, "image_urls_for_sizes", "512x384", json);
		this.source = BWJsonHelpers.PropertyIfExists(this.source, "world_source", json);
		this.title = BWJsonHelpers.PropertyIfExists(this.title, "title", json);
	}

	// Token: 0x17000238 RID: 568
	// (get) Token: 0x06002BB2 RID: 11186 RVA: 0x0013B84D File Offset: 0x00139C4D
	// (set) Token: 0x06002BB3 RID: 11187 RVA: 0x0013B855 File Offset: 0x00139C55
	internal string largeImageURL { get; private set; }

	// Token: 0x17000239 RID: 569
	// (get) Token: 0x06002BB4 RID: 11188 RVA: 0x0013B85E File Offset: 0x00139C5E
	// (set) Token: 0x06002BB5 RID: 11189 RVA: 0x0013B866 File Offset: 0x00139C66
	internal string smallImageURL { get; private set; }

	// Token: 0x1700023A RID: 570
	// (get) Token: 0x06002BB6 RID: 11190 RVA: 0x0013B86F File Offset: 0x00139C6F
	// (set) Token: 0x06002BB7 RID: 11191 RVA: 0x0013B877 File Offset: 0x00139C77
	internal string source { get; private set; }

	// Token: 0x1700023B RID: 571
	// (get) Token: 0x06002BB8 RID: 11192 RVA: 0x0013B880 File Offset: 0x00139C80
	// (set) Token: 0x06002BB9 RID: 11193 RVA: 0x0013B888 File Offset: 0x00139C88
	internal string title { get; private set; }
}
