using System.Text.RegularExpressions;

namespace PhysioLink.AdminPanel.Helpers;

// Exercise videos are either files the API serves (e.g. /videos/<id>.mp4) or
// YouTube links. Every served file has a poster JPG beside it with the same name
// (Backend/Scripts/generate_exercise_thumbnails.ps1), so the thumbnail URL is the
// video URL with its extension swapped; YouTube videos use YouTube's own still.
// Server-relative paths live on the API host, not this panel, so they are
// resolved against the API's public base URL — the browser fetches them.
public static class ExerciseMedia
{
    private static readonly Regex DirectFile = new(
        @"\.(mp4|mov|webm|ogg|m4v)(\?|#|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex YouTubeId = new(
        @"(?:youtu\.be/|youtube\.com/(?:embed/|watch\?v=|shorts/))([^?&#/]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Must agree with isDirectVideo() in Views/Shared/_ExerciseDetailModal.cshtml.
    private static bool IsDirectFile(string videoUrl) =>
        videoUrl.StartsWith('/') || DirectFile.IsMatch(videoUrl);

    public static string? ResolveUrl(string? url, string mediaBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
        return $"{mediaBaseUrl.TrimEnd('/')}{url}";
    }

    public static string? ThumbnailUrl(string? videoUrl, string mediaBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(videoUrl)) return null;

        if (IsDirectFile(videoUrl))
        {
            var path = videoUrl.Split('?', '#')[0];
            return ResolveUrl(Path.ChangeExtension(path, ".jpg"), mediaBaseUrl);
        }

        var match = YouTubeId.Match(videoUrl);
        var id = match.Success ? match.Groups[1].Value : videoUrl;
        return $"https://img.youtube.com/vi/{id}/hqdefault.jpg";
    }
}
