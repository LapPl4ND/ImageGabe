using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImageGabe
{
    public enum Platform { None, YouTube, Spotify, SoundCloud, Deezer, AppleMusic, Twitch }

    public record ArtResult(byte[] Bytes, string Title, string Author, Platform Platform);

    /// <summary>Erreur avec un message déjà traduit, affichable tel quel.</summary>
    public class UserFacingException : Exception
    {
        public UserFacingException(string message) : base(message) { }
    }

    /// <summary>Nom, couleur et icône de chaque plateforme.</summary>
    public static class PlatformInfo
    {
        public static string Name(Platform p) => p switch
        {
            Platform.YouTube => "YouTube",
            Platform.Spotify => "Spotify",
            Platform.SoundCloud => "SoundCloud",
            Platform.Deezer => "Deezer",
            Platform.AppleMusic => "Apple Music",
            Platform.Twitch => "Twitch",
            _ => ""
        };

        public static Color Accent(Platform p) => p switch
        {
            Platform.YouTube => Color.FromRgb(0xFF, 0x00, 0x33),
            Platform.Spotify => Color.FromRgb(0x1D, 0xB9, 0x54),
            Platform.SoundCloud => Color.FromRgb(0xFF, 0x55, 0x00),
            Platform.Deezer => Color.FromRgb(0xA2, 0x38, 0xFF),
            // Blanc en thème sombre, quasi-noir en thème clair (sinon invisible sur fond clair)
            Platform.AppleMusic => App.Settings.DarkTheme ? Color.FromRgb(0xFF, 0xFF, 0xFF) : Color.FromRgb(0x1C, 0x1C, 0x1E),
            Platform.Twitch => Color.FromRgb(0x91, 0x46, 0xFF),
            _ => Color.FromRgb(0x7C, 0x4D, 0xFF)
        };

        // Nom du fichier logo dans Assets/Logos (ex : youtube.png)
        public static string FileKey(Platform p) => p switch
        {
            Platform.YouTube => "youtube",
            Platform.Spotify => "spotify",
            Platform.SoundCloud => "soundcloud",
            Platform.Deezer => "deezer",
            Platform.AppleMusic => "applemusic",
            Platform.Twitch => "twitch",
            _ => ""
        };

        // Glyphes de la police Segoe Fluent Icons / MDL2
        public static string Glyph(Platform p) => p switch
        {
            Platform.YouTube => "",     // vidéo
            Platform.Spotify => "",     // audio
            Platform.SoundCloud => "",  // nuage
            Platform.Deezer => "",      // casque
            Platform.AppleMusic => "",  // audio
            Platform.Twitch => "",      // manette
            _ => ""                     // lien
        };
    }

    /// <summary>Trouve et télécharge la pochette / miniature d'un lien.</summary>
    public class ArtworkService
    {
        private readonly HttpClient _http;

        // YouTube : watch?v=, youtu.be/, /shorts/, /embed/, /live/, music.youtube.com
        private static readonly Regex YouTubeIdRegex =
            new(@"(?:v=|youtu\.be/|shorts/|embed/|live/)([A-Za-z0-9_-]{11})", RegexOptions.Compiled);

        private static readonly Regex DeezerRegex =
            new(@"deezer\.com/(?:[a-z]{2}(?:-[a-z]{2})?/)?(track|album|playlist|artist)/(\d+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AppleRegex =
            new(@"apple\.com/(?:([a-z]{2})/)?(album|song|playlist|music-video|artist)/(?:[^/?#]+/)?([^/?#]+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ArtworkService(HttpClient http) => _http = http;

        // =====================================================================
        //  Détection
        // =====================================================================

        public static Platform Detect(string url)
        {
            string u = url.Trim().ToLowerInvariant();
            if (u.Contains("youtube.com") || u.Contains("youtu.be")) return Platform.YouTube;
            if (u.Contains("spotify.com") || u.StartsWith("spotify:")) return Platform.Spotify;
            if (u.Contains("soundcloud.com") || u.Contains("snd.sc")) return Platform.SoundCloud;
            if (u.Contains("deezer.com") || u.Contains("deezer.page.link") || u.Contains("dzr.page.link")) return Platform.Deezer;
            if (u.Contains("music.apple.com") || u.Contains("itunes.apple.com")) return Platform.AppleMusic;
            if (u.Contains("twitch.tv")) return Platform.Twitch;
            return Platform.None;
        }

        private static string Normalize(string input)
        {
            string url = input.Trim();

            // spotify:track:ID  ->  https://open.spotify.com/track/ID
            if (url.StartsWith("spotify:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = url.Split(':');
                if (parts.Length >= 3) return $"https://open.spotify.com/{parts[1]}/{parts[2]}";
            }

            // "youtube.com/watch?v=..." sans https://
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url;

            return url;
        }

        public async Task<ArtResult> FetchAsync(string input)
        {
            string url = Normalize(input);
            var platform = Detect(url);

            return platform switch
            {
                Platform.YouTube => await FetchYouTube(url),
                Platform.Spotify => await FetchSpotify(url),
                Platform.SoundCloud => await FetchSoundCloud(url),
                Platform.Deezer => await FetchDeezer(url),
                Platform.AppleMusic => await FetchAppleMusic(url),
                Platform.Twitch => await FetchTwitch(url),
                _ => throw new UserFacingException(Loc.T("ErrUnknownLink"))
            };
        }

        // =====================================================================
        //  YouTube
        // =====================================================================

        private async Task<ArtResult> FetchYouTube(string url)
        {
            var match = YouTubeIdRegex.Match(url);
            if (!match.Success) throw new UserFacingException(Loc.T("ErrUnsupportedLink"));

            string id = match.Groups[1].Value;
            string watchUrl = $"https://www.youtube.com/watch?v={id}";
            var metaTask = GetOEmbed($"https://www.youtube.com/oembed?format=json&url={E(watchUrl)}");

            byte[] bytes = await DownloadFirst(
                               $"https://i.ytimg.com/vi/{id}/maxresdefault.jpg",
                               $"https://i.ytimg.com/vi/{id}/sddefault.jpg",
                               $"https://i.ytimg.com/vi/{id}/hqdefault.jpg")
                           ?? throw new UserFacingException(Loc.T("ErrNoImage"));

            var meta = await metaTask;
            return new ArtResult(bytes, meta.Title ?? "YouTube", meta.Author ?? "", Platform.YouTube);
        }

        // =====================================================================
        //  Spotify
        // =====================================================================

        private async Task<ArtResult> FetchSpotify(string url)
        {
            var meta = await GetOEmbed($"https://open.spotify.com/oembed?url={E(url)}");
            string thumb = meta.Thumbnail ?? throw new UserFacingException(Loc.T("ErrNoImage"));

            // L'oEmbed renvoie une cover 300 px : on tente les versions plus grandes d'abord
            byte[] bytes = await DownloadFirst(
                               thumb.Replace("ab67616d00001e02", "ab67616d000082c1"),
                               thumb.Replace("ab67616d00001e02", "ab67616d0000b273"),
                               thumb)
                           ?? throw new UserFacingException(Loc.T("ErrNoImage"));

            return new ArtResult(bytes, meta.Title ?? "Spotify", meta.Author ?? "", Platform.Spotify);
        }

        // =====================================================================
        //  SoundCloud
        // =====================================================================

        private async Task<ArtResult> FetchSoundCloud(string url)
        {
            url = await ResolveShortLink(url); // on.soundcloud.com/xxxx

            var meta = await GetOEmbed($"https://soundcloud.com/oembed?format=json&url={E(url)}");
            string? thumb = meta.Thumbnail;
            string? title = meta.Title;

            if (thumb == null)
            {
                var og = await GetOpenGraph(url);
                thumb = og.Image;
                title ??= og.Title;
            }
            if (thumb == null) throw new UserFacingException(Loc.T("ErrNoImage"));

            // t500x500 -> version originale (souvent bien plus grande)
            byte[] bytes = await DownloadFirst(
                               thumb.Replace("-t500x500.", "-original."),
                               thumb.Replace("-t500x500.", "-t1080x1080."),
                               thumb)
                           ?? throw new UserFacingException(Loc.T("ErrNoImage"));

            return new ArtResult(bytes, title ?? "SoundCloud", meta.Author ?? "", Platform.SoundCloud);
        }

        // =====================================================================
        //  Deezer (API publique, sans clé)
        // =====================================================================

        private async Task<ArtResult> FetchDeezer(string url)
        {
            url = await ResolveShortLink(url); // deezer.page.link / link.deezer.com

            var match = DeezerRegex.Match(url);
            if (!match.Success) throw new UserFacingException(Loc.T("ErrUnsupportedLink"));

            string type = match.Groups[1].Value.ToLowerInvariant();
            string id = match.Groups[2].Value;

            using var doc = JsonDocument.Parse(await _http.GetStringAsync($"https://api.deezer.com/{type}/{id}"));
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out _)) throw new UserFacingException(Loc.T("ErrNoImage"));

            string? image, title, author;
            switch (type)
            {
                case "track":
                    image = Str(root, "album", "cover_xl");
                    title = Str(root, "title");
                    author = Str(root, "artist", "name");
                    break;
                case "album":
                    image = Str(root, "cover_xl");
                    title = Str(root, "title");
                    author = Str(root, "artist", "name");
                    break;
                case "playlist":
                    image = Str(root, "picture_xl");
                    title = Str(root, "title");
                    author = Str(root, "creator", "name");
                    break;
                default: // artist
                    image = Str(root, "picture_xl");
                    title = Str(root, "name");
                    author = "";
                    break;
            }
            if (image == null) throw new UserFacingException(Loc.T("ErrNoImage"));

            byte[] bytes = await DownloadFirst(image.Replace("1000x1000", "1400x1400"), image)
                           ?? throw new UserFacingException(Loc.T("ErrNoImage"));

            return new ArtResult(bytes, title ?? "Deezer", author ?? "", Platform.Deezer);
        }

        // =====================================================================
        //  Apple Music (API iTunes, sans clé)
        // =====================================================================

        private async Task<ArtResult> FetchAppleMusic(string url)
        {
            var match = AppleRegex.Match(url);
            if (!match.Success) throw new UserFacingException(Loc.T("ErrUnsupportedLink"));

            string country = match.Groups[1].Success ? match.Groups[1].Value : "us";
            string type = match.Groups[2].Value.ToLowerInvariant();
            string id = match.Groups[3].Value;

            // Lien vers un morceau dans un album : ...?i=123456
            var songParam = Regex.Match(url, @"[?&]i=(\d+)");
            if (songParam.Success) id = songParam.Groups[1].Value;

            string? image = null, title = null, author = null;

            if (type != "playlist" && type != "artist" && id.All(char.IsDigit))
            {
                try
                {
                    using var doc = JsonDocument.Parse(
                        await _http.GetStringAsync($"https://itunes.apple.com/lookup?id={id}&country={country}"));
                    var results = doc.RootElement.GetProperty("results");
                    if (results.GetArrayLength() > 0)
                    {
                        var item = results[0];
                        image = Str(item, "artworkUrl100");
                        title = Str(item, "trackName") ?? Str(item, "collectionName");
                        author = Str(item, "artistName");
                    }
                }
                catch (JsonException) { }
            }

            // Playlists, artistes ou échec de l'API : on lit l'image de la page
            if (image == null)
            {
                var og = await GetOpenGraph(url);
                image = og.Image;
                title ??= og.Title;
            }
            if (image == null) throw new UserFacingException(Loc.T("ErrNoImage"));

            // Apple sert l'image à la taille demandée dans l'URL : on demande la plus grande
            string big = Regex.Replace(image, @"/\d+x\d+[a-z]*\.(jpg|png|webp)$", "/3000x3000bb.jpg");
            string medium = Regex.Replace(image, @"/\d+x\d+[a-z]*\.(jpg|png|webp)$", "/1200x1200bb.jpg");

            byte[] bytes = await DownloadFirst(big, medium, image)
                           ?? throw new UserFacingException(Loc.T("ErrNoImage"));

            return new ArtResult(bytes, title ?? "Apple Music", author ?? "", Platform.AppleMusic);
        }

        // =====================================================================
        //  Twitch : live (aperçu en direct), VOD et clips
        // =====================================================================

        private async Task<ArtResult> FetchTwitch(string url)
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            bool isClip = uri.Host.StartsWith("clips.", StringComparison.OrdinalIgnoreCase)
                          || segments.Contains("clip", StringComparer.OrdinalIgnoreCase);
            bool isVod = segments.Length >= 2 && segments[0].Equals("videos", StringComparison.OrdinalIgnoreCase);

            if (isClip || isVod)
            {
                var og = await GetOpenGraph(url);
                if (og.Image == null) throw new UserFacingException(Loc.T("ErrNoImage"));

                string hd = Regex.Replace(og.Image, @"\d+x\d+(\.\w+)$", "1920x1080$1");
                byte[] vodBytes = await DownloadFirst(hd, og.Image)
                                  ?? throw new UserFacingException(Loc.T("ErrNoImage"));
                return new ArtResult(vodBytes, og.Title ?? "Twitch", "", Platform.Twitch);
            }

            // Lien de chaîne : aperçu du live en cours
            string login = segments.FirstOrDefault()?.ToLowerInvariant()
                           ?? throw new UserFacingException(Loc.T("ErrUnsupportedLink"));
            string preview = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_{login}-1920x1080.jpg?t={DateTime.UtcNow.Ticks}";

            using var response = await _http.GetAsync(preview);
            string finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? "";
            if (!response.IsSuccessStatusCode || finalUrl.Contains("404_preview"))
                throw new UserFacingException(Loc.T("ErrTwitchOffline"));

            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            return new ArtResult(bytes, $"{login} — live", "", Platform.Twitch);
        }

        // =====================================================================
        //  Outils
        // =====================================================================

        private static string E(string s) => Uri.EscapeDataString(s);

        /// <summary>Essaie chaque URL dans l'ordre et renvoie la première image valide.</summary>
        private async Task<byte[]?> DownloadFirst(params string[] urls)
        {
            foreach (string u in urls.Distinct())
            {
                try
                {
                    using var response = await _http.GetAsync(u);
                    if (!response.IsSuccessStatusCode) continue;
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    if (data.Length > 2000) return data; // ignore les images "placeholder"
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) { }
            }
            return null;
        }

        /// <summary>Suit les redirections des liens courts (on.soundcloud.com, deezer.page.link…).</summary>
        private async Task<string> ResolveShortLink(string url)
        {
            string host = new Uri(url).Host.ToLowerInvariant();
            bool isShort = host.StartsWith("on.") || host.StartsWith("link.") || host.EndsWith("page.link") || host == "snd.sc";
            if (!isShort) return url;

            try
            {
                using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                return response.RequestMessage?.RequestUri?.ToString() ?? url;
            }
            catch
            {
                return url;
            }
        }

        private async Task<(string? Title, string? Author, string? Thumbnail)> GetOEmbed(string apiUrl)
        {
            try
            {
                using var doc = JsonDocument.Parse(await _http.GetStringAsync(apiUrl));
                var root = doc.RootElement;
                return (Str(root, "title"), Str(root, "author_name"), Str(root, "thumbnail_url"));
            }
            catch
            {
                return (null, null, null);
            }
        }

        /// <summary>Lit les balises og:image / og:title d'une page web (méthode de secours).</summary>
        private async Task<(string? Image, string? Title)> GetOpenGraph(string url)
        {
            try
            {
                string html = await _http.GetStringAsync(url);
                return (Meta(html, "og:image"), Meta(html, "og:title"));
            }
            catch
            {
                return (null, null);
            }

            static string? Meta(string html, string property)
            {
                string p = Regex.Escape(property);
                var m = Regex.Match(html, $@"<meta[^>]+(?:property|name)=[""']{p}[""'][^>]*content=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                if (!m.Success)
                    m = Regex.Match(html, $@"<meta[^>]+content=[""']([^""']+)[""'][^>]*(?:property|name)=[""']{p}[""']", RegexOptions.IgnoreCase);
                return m.Success ? WebUtility.HtmlDecode(m.Groups[1].Value) : null;
            }
        }

        /// <summary>Lit une propriété texte, éventuellement imbriquée : Str(root, "album", "cover_xl").</summary>
        private static string? Str(JsonElement element, params string[] path)
        {
            foreach (string name in path)
            {
                if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out element))
                    return null;
            }
            return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        }
    }
}