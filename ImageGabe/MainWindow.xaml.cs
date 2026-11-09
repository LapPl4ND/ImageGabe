using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ImageGabe
{
    public partial class MainWindow : Window
    {
        private enum ToastKind { Info, Success, Warning, Error }

        private static readonly HttpClient Http = CreateHttpClient();
        private readonly ArtworkService _artwork = new(Http);

        private Color _accentColor = PlatformInfo.Accent(Platform.None);
        private int _accentVersion;

        private byte[]? _imageBytes;
        private BitmapSource? _currentBitmap;
        private string _currentTitle = "";
        private Platform _platform = Platform.None;
        private bool _isLoading;
        private bool _syncingSettings;
        private int _toastVersion;
        private Action? _toastAction;
        private string _lastClipboardSuggestion = "";

        private static AppSettings Settings => App.Settings;

        public MainWindow()
        {
            // Couleur d'accent utilisée partout via DynamicResource
            SetAccentBrushes(_accentColor);
            InitializeComponent();

            SyncSettingsUi();
            LoadTitleIcon();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtUrl.Focus();

            // Vérification silencieuse des mises à jour au démarrage
            if (Settings.AutoCheckUpdates && UpdateService.IsConfigured)
                await CheckForUpdates(silent: true);
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) ImageGabe/2.0");
            return client;
        }

        // =====================================================================
        //  Aperçu
        // =====================================================================

        private async Task LoadPreview()
        {
            if (_isLoading) return;

            string input = txtUrl.Text.Trim();
            if (input.Length == 0)
            {
                ShowToast(Loc.T("ErrEmptyLink"), ToastKind.Warning);
                txtUrl.Focus();
                return;
            }

            SetLoading(true);
            try
            {
                var art = await _artwork.FetchAsync(input);
                var bitmap = Decode(art.Bytes);

                _imageBytes = art.Bytes;
                _currentBitmap = bitmap;
                _currentTitle = art.Title;
                ShowPreview(bitmap, art);
            }
            catch (UserFacingException ex)
            {
                ShowToast(ex.Message, ToastKind.Error);
            }
            catch (HttpRequestException)
            {
                ShowToast(Loc.T("ErrNetwork"), ToastKind.Error);
            }
            catch (TaskCanceledException)
            {
                ShowToast(Loc.T("ErrNetwork"), ToastKind.Error);
            }
            catch (Exception ex)
            {
                ShowToast(Loc.T("ErrNoImage") + " (" + ex.Message + ")", ToastKind.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private static BitmapImage Decode(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void ShowPreview(BitmapSource bitmap, ArtResult art)
        {
            emptyState.Visibility = Visibility.Collapsed;

            picPreview.Source = bitmap;
            ambientImage.Source = bitmap;

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            picPreview.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(350)) { EasingFunction = ease });
            var scale = new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(450)) { EasingFunction = ease };
            previewScale.BeginAnimation(ScaleTransform.ScaleXProperty, scale);
            previewScale.BeginAnimation(ScaleTransform.ScaleYProperty, scale);
            ambientImage.BeginAnimation(OpacityProperty, new DoubleAnimation(0.6, TimeSpan.FromMilliseconds(900)) { EasingFunction = ease });

            txtTitle.Text = art.Title;
            string platformName = PlatformInfo.Name(art.Platform);
            txtAuthor.Text = string.IsNullOrWhiteSpace(art.Author) ? platformName : $"{art.Author}  ·  {platformName}";
            txtResolution.Text = $"{bitmap.PixelWidth} × {bitmap.PixelHeight}";
            qualityDot.Fill = bitmap.PixelWidth >= 1000
                ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))  // HD
                : new SolidColorBrush(Color.FromRgb(0xF5, 0xA5, 0x24)); // qualité moyenne
            metaPanel.Visibility = Visibility.Visible;

            btnDownload.IsEnabled = true;
            btnCopy.IsEnabled = true;
        }

        private void SetLoading(bool loading)
        {
            _isLoading = loading;
            loadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
            btnPreview.IsEnabled = !loading;
            btnDownload.IsEnabled = !loading && _imageBytes != null;
            btnCopy.IsEnabled = !loading && _currentBitmap != null;
        }

        // =====================================================================
        //  Plateforme détectée : badge + couleur d'accent
        // =====================================================================

        private void SetPlatform(Platform platform)
        {
            if (platform == _platform) return;
            _platform = platform;

            platformGlyph.Text = PlatformInfo.Glyph(platform);
            var logo = GetLogo(platform);
            platformLogo.Source = logo;
            platformLogo.Visibility = logo != null ? Visibility.Visible : Visibility.Collapsed;
            platformGlyph.Visibility = logo != null ? Visibility.Collapsed : Visibility.Visible;
            platformBadge.ToolTip = platform == Platform.None ? null : PlatformInfo.Name(platform);

            if (logo != null)
            {
                // Le logo officiel s'affiche tel quel, sans fond coloré
                platformBadge.Background = Brushes.Transparent;
            }
            else if (platform == Platform.None)
            {
                platformBadge.SetResourceReference(Border.BackgroundProperty, "SubtleBrush");
                platformGlyph.SetResourceReference(TextBlock.ForegroundProperty, "MutedBrush");
            }
            else
            {
                // Pas de fichier logo : pastille colorée avec une icône
                platformBadge.Background = new SolidColorBrush(PlatformInfo.Accent(platform));
                platformGlyph.SetResourceReference(TextBlock.ForegroundProperty, "AccentForegroundBrush");
            }

            AnimateAccent(PlatformInfo.Accent(platform));
        }

        // Logos chargés depuis Assets/Logos/<nom>.png (intégrés à l'exe), gardés en cache
        private static readonly Dictionary<Platform, ImageSource?> LogoCache = new();

        private static ImageSource? GetLogo(Platform platform)
        {
            if (platform == Platform.None) return null;
            if (LogoCache.TryGetValue(platform, out var cached)) return cached;

            ImageSource? logo = null;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"pack://application:,,,/Assets/Logos/{PlatformInfo.FileKey(platform)}.png");
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                logo = bitmap;
            }
            catch
            {
                // Fichier absent : on garde la pastille colorée
            }

            LogoCache[platform] = logo;
            return logo;
        }

        // Icône de la barre de titre : on prend la plus grande image contenue dans app.ico
        private void LoadTitleIcon()
        {
            try
            {
                var decoder = BitmapDecoder.Create(new Uri("pack://application:,,,/app.ico"),
                    BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                titleIcon.Source = decoder.Frames.OrderByDescending(f => f.PixelWidth).First();
                titleFallback.Visibility = Visibility.Collapsed;
            }
            catch
            {
                // app.ico pas intégré : on garde le logo de secours
            }
        }

        // Transition douce : on remplace le pinceau à chaque étape (animer un pinceau partagé fait planter WPF)
        private async void AnimateAccent(Color target)
        {
            int version = ++_accentVersion;
            Color from = _accentColor;
            const int steps = 20;

            for (int i = 1; i <= steps; i++)
            {
                if (version != _accentVersion) return;
                double t = 1 - Math.Pow(1 - (double)i / steps, 3);
                _accentColor = Color.FromRgb(
                    (byte)(from.R + (target.R - from.R) * t),
                    (byte)(from.G + (target.G - from.G) * t),
                    (byte)(from.B + (target.B - from.B) * t));
                SetAccentBrushes(_accentColor);
                await Task.Delay(16);
            }
        }

        private bool? _accentIsLight;

        private void SetAccentBrushes(Color accent)
        {
            Resources["AccentBrush"] = new SolidColorBrush(accent);

            // Accent clair (ex : blanc) -> texte foncé par-dessus, sinon texte blanc
            bool isLight = 0.299 * accent.R + 0.587 * accent.G + 0.114 * accent.B > 186;
            if (isLight != _accentIsLight)
            {
                _accentIsLight = isLight;
                Resources["AccentForegroundBrush"] = new SolidColorBrush(isLight ? Color.FromRgb(0x11, 0x11, 0x14) : Colors.White);
            }
        }

        // =====================================================================
        //  Actions principales
        // =====================================================================

        private async void BtnPreview_Click(object sender, RoutedEventArgs e) => await LoadPreview();

        private async void TxtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await LoadPreview();
            }
        }

        private void TxtUrl_TextChanged(object sender, TextChangedEventArgs e)
            => SetPlatform(ArtworkService.Detect(txtUrl.Text));

        private async void Paste_Click(object sender, RoutedEventArgs e)
        {
            string text = TryGetClipboardText();
            if (string.IsNullOrWhiteSpace(text))
            {
                ShowToast(Loc.T("ClipboardEmpty"), ToastKind.Warning);
                return;
            }
            txtUrl.Text = text.Trim();
            txtUrl.CaretIndex = txtUrl.Text.Length;
            await LoadPreview();
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e) => await SaveImage();

        private async Task SaveImage()
        {
            if (_imageBytes == null || _currentBitmap == null)
            {
                ShowToast(Loc.T("NeedPreview"), ToastKind.Warning);
                return;
            }

            bool pngByDefault = Settings.DefaultFormat == "png";
            var dialog = new SaveFileDialog
            {
                Title = Loc.T("SaveDialogTitle"),
                FileName = SanitizeFileName(_currentTitle),
                Filter = "JPEG (*.jpg)|*.jpg|PNG (*.png)|*.png",
                FilterIndex = pngByDefault ? 2 : 1,
                DefaultExt = pngByDefault ? ".png" : ".jpg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            if (dialog.ShowDialog(this) != true) return;

            try
            {
                string path = dialog.FileName;
                bool wantsPng = Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase);
                bool isJpeg = _imageBytes.Length > 2 && _imageBytes[0] == 0xFF && _imageBytes[1] == 0xD8;

                if (!wantsPng && isJpeg)
                {
                    // Déjà en JPEG : on écrit l'original, sans perte de qualité
                    await File.WriteAllBytesAsync(path, _imageBytes);
                }
                else
                {
                    BitmapEncoder encoder = wantsPng ? new PngBitmapEncoder() : new JpegBitmapEncoder { QualityLevel = 95 };
                    encoder.Frames.Add(BitmapFrame.Create(_currentBitmap));
                    await using var fs = File.Create(path);
                    encoder.Save(fs);
                }

                ShowToast(Loc.T("Saved"), ToastKind.Success, Loc.T("OpenFolder"),
                    () => Process.Start("explorer.exe", $"/select,\"{path}\""));
            }
            catch (Exception ex)
            {
                ShowToast(Loc.T("ErrSave") + ex.Message, ToastKind.Error);
            }
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBitmap == null) return;
            try
            {
                Clipboard.SetImage(_currentBitmap);
                ShowToast(Loc.T("Copied"), ToastKind.Success);
            }
            catch
            {
                ShowToast(Loc.T("ErrClipboard"), ToastKind.Error);
            }
        }

        private void ToastAction_Click(object sender, RoutedEventArgs e) => _toastAction?.Invoke();

        // Raccourcis : Ctrl+S = enregistrer, Ctrl+Maj+C = copier l'image, Échap = fermer les paramètres
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && settingsOverlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                HideSettings();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && btnDownload.IsEnabled)
            {
                e.Handled = true;
                await SaveImage();
            }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C && btnCopy.IsEnabled)
            {
                e.Handled = true;
                BtnCopy_Click(this, new RoutedEventArgs());
            }
        }

        // Lien dans le presse-papiers quand on revient sur l'app : on le propose
        private void Window_Activated(object? sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtUrl.Text) || _isLoading) return;

                string text = TryGetClipboardText().Trim();
                if (text.Length == 0 || text.Length > 500 || text == _lastClipboardSuggestion) return;
                if (ArtworkService.Detect(text) == Platform.None) return;

                _lastClipboardSuggestion = text;
                txtUrl.Text = text;
                txtUrl.CaretIndex = text.Length;
                ShowToast(Loc.T("LinkDetected"), ToastKind.Info, Loc.T("Preview"), async () => await LoadPreview());
            }
            catch
            {
                // Fonction bonus : elle ne doit jamais faire planter l'app
            }
        }

        // Glisser-déposer d'un lien
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.UnicodeText) || e.Data.GetDataPresent(DataFormats.Text)
                ? DragDropEffects.Link
                : DragDropEffects.None;
            e.Handled = true;
        }

        private async void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            string? text = (e.Data.GetData(DataFormats.UnicodeText) ?? e.Data.GetData(DataFormats.Text)) as string;
            e.Handled = true;
            if (string.IsNullOrWhiteSpace(text)) return;

            txtUrl.Text = text.Trim().Split('\n')[0].Trim();
            await LoadPreview();
        }

        // =====================================================================
        //  Paramètres
        // =====================================================================

        private void SyncSettingsUi()
        {
            _syncingSettings = true;
            rbDark.IsChecked = Settings.DarkTheme;
            rbLight.IsChecked = !Settings.DarkTheme;
            rbFr.IsChecked = Settings.Language != "en";
            rbEn.IsChecked = Settings.Language == "en";
            rbJpg.IsChecked = Settings.DefaultFormat != "png";
            rbPng.IsChecked = Settings.DefaultFormat == "png";
            chkAutoUpdate.IsChecked = Settings.AutoCheckUpdates;
            txtVersion.Text = $"ImageGabe v{UpdateService.CurrentVersion}";
            _syncingSettings = false;
        }

        private void Setting_Changed(object sender, RoutedEventArgs e)
        {
            if (_syncingSettings) return;

            bool dark = rbDark.IsChecked == true;
            string lang = rbEn.IsChecked == true ? "en" : "fr";

            bool themeChanged = dark != Settings.DarkTheme;
            if (themeChanged) Theme.Apply(dark);
            if (lang != Settings.Language) Loc.Apply(lang);

            Settings.DarkTheme = dark;
            // La couleur d'Apple Music dépend du thème : on la recalcule
            if (themeChanged) AnimateAccent(PlatformInfo.Accent(_platform));
            Settings.Language = lang;
            Settings.DefaultFormat = rbPng.IsChecked == true ? "png" : "jpg";
            Settings.AutoCheckUpdates = chkAutoUpdate.IsChecked == true;
            Settings.Save();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            settingsOverlay.Visibility = Visibility.Visible;
            settingsOverlay.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(180)));
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e) => HideSettings();

        private void SettingsOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clic sur le fond sombre (pas sur la carte) = fermer
            if (e.OriginalSource == settingsOverlay) HideSettings();
        }

        private void HideSettings()
        {
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
            fade.Completed += (_, _) =>
            {
                if (settingsOverlay.Opacity == 0) settingsOverlay.Visibility = Visibility.Collapsed;
            };
            settingsOverlay.BeginAnimation(OpacityProperty, fade);
        }

        // =====================================================================
        //  Mises à jour
        // =====================================================================

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e) => await CheckForUpdates(silent: false);

        private async Task CheckForUpdates(bool silent)
        {
            if (!UpdateService.IsConfigured)
            {
                if (!silent) ShowToast(Loc.T("UpdateNotConfigured"), ToastKind.Warning);
                return;
            }

            btnCheckUpdates.IsEnabled = false;
            try
            {
                var update = await UpdateService.CheckAsync(Http);
                if (update == null)
                {
                    if (!silent) ShowToast(Loc.T("UpToDate"), ToastKind.Success);
                    return;
                }

                ShowToast(string.Format(Loc.T("UpdateAvailable"), "v" + update.Version), ToastKind.Info,
                    Loc.T("UpdateAction"), async () =>
                    {
                        try
                        {
                            ShowToast(Loc.T("UpdateDownloading"), ToastKind.Info);
                            await UpdateService.InstallAsync(Http, update);
                        }
                        catch
                        {
                            ShowToast(Loc.T("UpdateFailed"), ToastKind.Error);
                        }
                    });
            }
            catch
            {
                if (!silent) ShowToast(Loc.T("UpdateFailed"), ToastKind.Error);
            }
            finally
            {
                btnCheckUpdates.IsEnabled = true;
            }
        }

        // =====================================================================
        //  Fenêtre sans bordure
        // =====================================================================

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
            => Root.Clip = new RectangleGeometry(new Rect(e.NewSize), 18, 18);

        private void PicPreview_SizeChanged(object sender, SizeChangedEventArgs e)
            => picPreview.Clip = new RectangleGeometry(new Rect(e.NewSize), 12, 12);

        // =====================================================================
        //  Toast
        // =====================================================================

        private async void ShowToast(string message, ToastKind kind, string? actionText = null, Action? action = null)
        {
            int version = ++_toastVersion;

            var (glyph, color) = kind switch
            {
                ToastKind.Success => ("", Color.FromRgb(0x22, 0xC5, 0x5E)),
                ToastKind.Warning => ("", Color.FromRgb(0xF5, 0xA5, 0x24)),
                ToastKind.Error => ("", Color.FromRgb(0xF4, 0x3F, 0x5E)),
                _ => ("", _accentColor)
            };
            toastIcon.Text = glyph;
            toastIcon.Foreground = new SolidColorBrush(color);
            toastText.Text = message;

            _toastAction = action;
            toastAction.Content = actionText;
            toastAction.Visibility = action != null ? Visibility.Visible : Visibility.Collapsed;
            toast.IsHitTestVisible = true;

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            toast.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)));
            toastShift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { EasingFunction = ease });

            await Task.Delay(action != null ? 5000 : 2800);
            if (version != _toastVersion) return; // un autre toast a pris la place

            toast.IsHitTestVisible = false;
            toast.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)));
            toastShift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(12, TimeSpan.FromMilliseconds(250)));
        }

        // =====================================================================
        //  Utilitaires
        // =====================================================================

        private static string TryGetClipboardText()
        {
            try { return Clipboard.ContainsText() ? Clipboard.GetText() : ""; }
            catch { return ""; }
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            string clean = new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
            if (clean.Length > 80) clean = clean[..80].Trim();
            return clean.Length == 0 ? Loc.T("DefaultFileName") : clean;
        }
    }
}