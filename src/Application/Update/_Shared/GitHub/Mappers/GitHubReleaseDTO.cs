namespace Reaparr.Application;

public sealed record GitHubReleaseDTO
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = string.Empty;

    [JsonPropertyName("assets_url")]
    public string AssetsUrl { get; init; } = string.Empty;

    [JsonPropertyName("upload_url")]
    public string UploadUrl { get; init; } = string.Empty;

    [JsonPropertyName("tarball_url")]
    public string? TarballUrl { get; init; }

    [JsonPropertyName("zipball_url")]
    public string? ZipballUrl { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; init; } = string.Empty;

    [JsonPropertyName("tag_name")]
    public string TagName { get; init; } = string.Empty;

    [JsonPropertyName("target_commitish")]
    public string TargetCommitish { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("draft")]
    public bool Draft { get; init; }

    [JsonPropertyName("immutable")]
    public bool? Immutable { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("published_at")]
    public DateTime? PublishedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; init; }

    [JsonPropertyName("author")]
    public GitHubSimpleUser Author { get; init; } = new();

    [JsonPropertyName("assets")]
    public List<GitHubReleaseAsset> Assets { get; init; } = [];

    [JsonPropertyName("body_html")]
    public string? BodyHtml { get; init; }

    [JsonPropertyName("body_text")]
    public string? BodyText { get; init; }

    [JsonPropertyName("mentions_count")]
    public int? MentionsCount { get; init; }

    [JsonPropertyName("discussion_url")]
    public string? DiscussionUrl { get; init; }

    [JsonPropertyName("reactions")]
    public GitHubReactionRollup? Reactions { get; init; }
}

public sealed record GitHubSimpleUser
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("login")]
    public string Login { get; init; } = string.Empty;

    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; init; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; init; } = string.Empty;

    [JsonPropertyName("gravatar_id")]
    public string? GravatarId { get; init; }

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = string.Empty;

    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; init; } = string.Empty;

    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; init; } = string.Empty;

    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; init; } = string.Empty;

    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; init; } = string.Empty;

    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; init; } = string.Empty;

    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; init; } = string.Empty;

    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; init; } = string.Empty;

    [JsonPropertyName("events_url")]
    public string EventsUrl { get; init; } = string.Empty;

    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; init; }

    [JsonPropertyName("starred_at")]
    public string? StarredAt { get; init; }

    [JsonPropertyName("user_view_type")]
    public string? UserViewType { get; init; }
}

public sealed record GitHubReleaseAsset
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; init; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; init; }

    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string ContentType { get; init; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; init; }

    [JsonPropertyName("digest")]
    public string? Digest { get; init; }

    [JsonPropertyName("download_count")]
    public int DownloadCount { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonPropertyName("uploader")]
    public GitHubSimpleUser? Uploader { get; init; }
}

public sealed record GitHubReactionRollup
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }

    [JsonPropertyName("+1")]
    public int PlusOne { get; init; }

    [JsonPropertyName("-1")]
    public int MinusOne { get; init; }

    [JsonPropertyName("laugh")]
    public int Laugh { get; init; }

    [JsonPropertyName("confused")]
    public int Confused { get; init; }

    [JsonPropertyName("heart")]
    public int Heart { get; init; }

    [JsonPropertyName("hooray")]
    public int Hooray { get; init; }

    [JsonPropertyName("eyes")]
    public int Eyes { get; init; }

    [JsonPropertyName("rocket")]
    public int Rocket { get; init; }
}
