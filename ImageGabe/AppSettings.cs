using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace ImageGabe
{
    /// <summary>
    /// Paramètres de l'app, sauvegardés dans %AppData%\ImageGabe\settings.json
    /// </summary>
    public class AppSettings
    {
        public bool DarkTheme { get; set; } = true;
        public string Language { get; set; } =
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? "fr" : "en";
        public string DefaultFormat { get; set; } = "jpg";   // "jpg" ou "png"
        public bool AutoCheckUpdates { get; set; } = true;

        private static readonly string FolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ImageGabe");
        private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
            }
            catch
            {
                // Fichier corrompu : on repart des valeurs par défaut
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Pas grave si la sauvegarde échoue
            }
        }
    }
}