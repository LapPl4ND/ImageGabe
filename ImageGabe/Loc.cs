using System.Collections.Generic;
using System.Windows;

namespace ImageGabe
{
    /// <summary>
    /// Traductions FR / EN.
    /// Dans le XAML : Text="{DynamicResource S.Cle}"   —   Dans le code : Loc.T("Cle")
    /// </summary>
    public static class Loc
    {
        public static string Language { get; private set; } = "fr";

        private static readonly Dictionary<string, string> Fr = new()
        {
            // Interface
            ["Minimize"] = "Réduire",
            ["Close"] = "Fermer",
            ["Settings"] = "Paramètres",
            ["Heading"] = "Récupère n'importe quelle pochette",
            ["Subheading"] = "YouTube, Spotify, SoundCloud, Deezer, Apple Music et Twitch, dans la meilleure qualité dispo.",
            ["Placeholder"] = "Colle un lien…",
            ["Paste"] = "Coller",
            ["PasteTip"] = "Coller depuis le presse-papiers",
            ["Preview"] = "Aperçu",
            ["EmptyTitle"] = "Aucun aperçu pour l'instant",
            ["EmptyHint"] = "Colle un lien, ou glisse-le directement dans la fenêtre",
            ["Loading"] = "Récupération de l'image…",
            ["Save"] = "Enregistrer l'image",
            ["CopyTip"] = "Copier l'image (Ctrl + Maj + C)",

            // Paramètres
            ["SettingsTitle"] = "Paramètres",
            ["Theme"] = "Thème",
            ["ThemeDark"] = "Sombre",
            ["ThemeLight"] = "Clair",
            ["Language"] = "Langue",
            ["Format"] = "Format d'enregistrement par défaut",
            ["Updates"] = "Mises à jour",
            ["AutoCheck"] = "Vérifier au démarrage",
            ["CheckNow"] = "Vérifier maintenant",
            ["Version"] = "Version",

            // Messages
            ["ErrEmptyLink"] = "Colle d'abord un lien.",
            ["ErrUnknownLink"] = "Lien non reconnu. Plateformes gérées : YouTube, Spotify, SoundCloud, Deezer, Apple Music, Twitch.",
            ["ErrNoImage"] = "Aucune image trouvée pour ce lien.",
            ["ErrUnsupportedLink"] = "Ce type de lien n'est pas pris en charge.",
            ["ErrTwitchOffline"] = "Cette chaîne n'est pas en live en ce moment.",
            ["ErrNetwork"] = "Connexion impossible. Vérifie ta connexion internet.",
            ["ErrSave"] = "Erreur lors de l'enregistrement : ",
            ["ErrClipboard"] = "Impossible d'accéder au presse-papiers.",
            ["ErrUnexpected"] = "Une erreur inattendue s'est produite :",
            ["ClipboardEmpty"] = "Le presse-papiers est vide.",
            ["NeedPreview"] = "Génère d'abord un aperçu.",
            ["Saved"] = "Image enregistrée !",
            ["OpenFolder"] = "Ouvrir le dossier",
            ["Copied"] = "Image copiée dans le presse-papiers.",
            ["LinkDetected"] = "Lien détecté dans le presse-papiers.",
            ["SaveDialogTitle"] = "Enregistrer la pochette",
            ["DefaultFileName"] = "pochette",
            ["UpdateAvailable"] = "Nouvelle version {0} disponible !",
            ["UpdateAction"] = "Mettre à jour",
            ["UpToDate"] = "Tu as déjà la dernière version.",
            ["UpdateFailed"] = "Impossible de vérifier les mises à jour.",
            ["UpdateDownloading"] = "Téléchargement de la mise à jour…",
            ["UpdateNotConfigured"] = "Mises à jour pas encore configurées (voir UpdateService.cs).",
        };

        private static readonly Dictionary<string, string> En = new()
        {
            ["Minimize"] = "Minimize",
            ["Close"] = "Close",
            ["Settings"] = "Settings",
            ["Heading"] = "Grab any cover art",
            ["Subheading"] = "YouTube, Spotify, SoundCloud, Deezer, Apple Music and Twitch, in the best quality available.",
            ["Placeholder"] = "Paste a link…",
            ["Paste"] = "Paste",
            ["PasteTip"] = "Paste from clipboard",
            ["Preview"] = "Preview",
            ["EmptyTitle"] = "Nothing to preview yet",
            ["EmptyHint"] = "Paste a link, or drag it straight into the window",
            ["Loading"] = "Fetching image…",
            ["Save"] = "Save image",
            ["CopyTip"] = "Copy image (Ctrl + Shift + C)",

            ["SettingsTitle"] = "Settings",
            ["Theme"] = "Theme",
            ["ThemeDark"] = "Dark",
            ["ThemeLight"] = "Light",
            ["Language"] = "Language",
            ["Format"] = "Default save format",
            ["Updates"] = "Updates",
            ["AutoCheck"] = "Check on startup",
            ["CheckNow"] = "Check now",
            ["Version"] = "Version",

            ["ErrEmptyLink"] = "Paste a link first.",
            ["ErrUnknownLink"] = "Link not recognized. Supported: YouTube, Spotify, SoundCloud, Deezer, Apple Music, Twitch.",
            ["ErrNoImage"] = "No image found for this link.",
            ["ErrUnsupportedLink"] = "This kind of link isn't supported.",
            ["ErrTwitchOffline"] = "This channel isn't live right now.",
            ["ErrNetwork"] = "Can't connect. Check your internet connection.",
            ["ErrSave"] = "Couldn't save the image: ",
            ["ErrClipboard"] = "Can't access the clipboard.",
            ["ErrUnexpected"] = "An unexpected error occurred:",
            ["ClipboardEmpty"] = "The clipboard is empty.",
            ["NeedPreview"] = "Generate a preview first.",
            ["Saved"] = "Image saved!",
            ["OpenFolder"] = "Open folder",
            ["Copied"] = "Image copied to clipboard.",
            ["LinkDetected"] = "Link found in your clipboard.",
            ["SaveDialogTitle"] = "Save cover art",
            ["DefaultFileName"] = "cover",
            ["UpdateAvailable"] = "Version {0} is available!",
            ["UpdateAction"] = "Update",
            ["UpToDate"] = "You're on the latest version.",
            ["UpdateFailed"] = "Couldn't check for updates.",
            ["UpdateDownloading"] = "Downloading update…",
            ["UpdateNotConfigured"] = "Updates aren't set up yet (see UpdateService.cs).",
        };

        /// <summary>Change la langue de toute l'interface.</summary>
        public static void Apply(string language)
        {
            Language = language == "en" ? "en" : "fr";
            var dict = Language == "en" ? En : Fr;
            foreach (var pair in dict)
                Application.Current.Resources["S." + pair.Key] = pair.Value;
        }

        /// <summary>Texte traduit pour le code C#.</summary>
        public static string T(string key)
        {
            var dict = Language == "en" ? En : Fr;
            return dict.TryGetValue(key, out var value) ? value : key;
        }
    }
}