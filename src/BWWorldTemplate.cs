using SimpleJSON;

public class BWWorldTemplate
{
	internal string largeImageURL { get; private set; }

	internal string smallImageURL { get; private set; }

	internal string source { get; private set; }

	internal string title { get; private set; }

	internal BWWorldTemplate(JObject json)
	{
		largeImageURL = BWJsonHelpers.PropertyIfExists(largeImageURL, "image_urls_for_sizes", "1024x768", json);
		smallImageURL = BWJsonHelpers.PropertyIfExists(smallImageURL, "image_urls_for_sizes", "512x384", json);
		source = BWJsonHelpers.PropertyIfExists(source, "world_source", json);
		title = BWJsonHelpers.PropertyIfExists(title, "title", json);
	}
}
