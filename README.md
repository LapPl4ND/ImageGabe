<div align="center">


# ImageGabe

**Grab any cover art or thumbnail in one click, in the best quality available.**

YouTube · Spotify · SoundCloud · Deezer · Apple Music · Twitch

[![Latest version](https://img.shields.io/github/v/release/LapPl4ND/ImageGabe?style=for-the-badge&label=version&color=8B5CF6)](https://github.com/LapPl4ND/ImageGabe/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/LapPl4ND/ImageGabe/total?style=for-the-badge&label=downloads&color=EC4899)](https://github.com/LapPl4ND/ImageGabe/releases)
![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-1E1E24?style=for-the-badge)

<br>

### [⬇️ Download ImageGabe](https://github.com/LapPl4ND/ImageGabe/releases/latest/download/ImageGabe.exe)

<sub>A single .exe file · No installation · No .NET required</sub>

</div>

<br>

<!--
  📸 SCREENSHOT: add a screenshot of the app as Assets/screenshot.png,
  then delete this comment line and the one at the bottom of the block.

<div align="center">
  <img src="Assets/screenshot.png" alt="ImageGabe preview" width="420">
</div>

-->

## ✨ Features

<table>
<tr>
<td width="50%" valign="top">

### 🎯 Quick and simple
- Paste a link and the preview shows up instantly
- **Drag and drop** a link straight from your browser
- **Automatic detection** of a link copied to your clipboard
- Copy the image in one click, or save it as **JPG** or **PNG**

</td>
<td width="50%" valign="top">

### 🖼️ Maximum quality
- Always the **highest resolution** available
- Up to **3000 × 3000 px** for Apple Music
- Quality indicator 🟢 HD / 🟠 medium
- Files are automatically named after the **track title**

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 🎨 Polished design
- The interface takes on the **color of the detected platform**
- **Ambient background** with the blurred cover art
- **Dark** or **light** theme
- Interface in **English** or **French**

</td>
<td width="50%" valign="top">

### 🔄 Always up to date
- **One-click automatic updates**
- **"What's new"** section to see what changed
- Works even on a slow connection

</td>
</tr>
</table>

<br>

## 🎵 Supported platforms

| Platform | Supported links | Max quality |
|:--|:--|:--:|
| **YouTube** | Videos, Shorts, YouTube Music, `youtu.be` links | 1280 × 720 |
| **Spotify** | Tracks, albums, playlists, artists, podcasts | 640 × 640 and up |
| **SoundCloud** | Tracks, playlists, short links | original |
| **Deezer** | Tracks, albums, playlists, artists, short links | 1000 × 1000 and up |
| **Apple Music** | Songs, albums, playlists, artists | 3000 × 3000 |
| **Twitch** | Live streams, VODs, clips | 1920 × 1080 |

<sub>Actual quality depends on the image originally uploaded.</sub>

<br>

## 🚀 How to use

1. **Copy a link** from YouTube, Spotify, Deezer, etc.
2. **Paste it** into ImageGabe (or drag it into the window)
3. Click **Preview**
4. **Save** the image or **copy** it directly

### ⌨️ Shortcuts

| Shortcut | Action |
|:--|:--|
| <kbd>Enter</kbd> | Preview |
| <kbd>Ctrl</kbd> + <kbd>S</kbd> | Save the image |
| <kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>C</kbd> | Copy the image |
| <kbd>Esc</kbd> | Close settings |

<br>

## ⚙️ Settings

Open them with the ⚙️ icon at the top of the window:

- **Theme**: dark or light
- **Language**: English or French
- **Default format**: JPG or PNG
- **Updates**: automatic check on startup
- **What's new**: the history of every version

<br>

## ❓ FAQ

<details>
<summary><b>Windows says "Windows protected your PC"</b></summary>
<br>
This is normal for a small app that isn't digitally signed.
Click <b>More info</b>, then <b>Run anyway</b>.
This message only appears the first time you launch it.
</details>

<details>
<summary><b>Do I need to install anything?</b></summary>
<br>
No. ImageGabe is a single <code>.exe</code> file that contains everything it needs.
Put it wherever you like (Desktop, Documents…) and run it.
</details>

<details>
<summary><b>Automatic updates don't work</b></summary>
<br>
Avoid putting the app in <code>C:\Program Files</code>: that folder requires administrator rights
and prevents the app from replacing itself. Your Desktop or a personal folder work fine.
You can always download the latest version from the
<a href="https://github.com/LapPl4ND/ImageGabe/releases/latest">Releases</a> page.
</details>

<details>
<summary><b>A Twitch link doesn't work</b></summary>
<br>
For a channel link, ImageGabe grabs the preview of the <b>current live stream</b>, so the channel has to be live.
For a VOD or a clip, use the direct link to the video.
</details>

<details>
<summary><b>Where are my settings saved?</b></summary>
<br>
In <code>%AppData%\ImageGabe\settings.json</code>.
</details>

<br>

## 🛠️ Building the project

Requirements: [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload.

```bash
git clone https://github.com/LapPl4ND/ImageGabe.git
cd ImageGabe
```

- **Run in development**: open the project in Visual Studio and press <kbd>F5</kbd>
- **Build the final exe**: double-click `publish.bat` (the file appears in the `publish` folder)

<details>
<summary><b>Project structure</b></summary>
<br>

| File | Purpose |
|:--|:--|
| `MainWindow.xaml` / `.cs` | Window interface and logic |
| `ArtworkService.cs` | Platform detection and image fetching |
| `UpdateService.cs` | Automatic updates and release notes via GitHub |
| `Theme.cs` | Dark and light themes |
| `Loc.cs` | English / French translations |
| `AppSettings.cs` | Settings storage |
| `Assets/Logos/` | Platform logos |
| `publish.bat` | Builds the final exe |

</details>

<br>

## 📝 Release notes

Every change is listed on the [**Releases**](https://github.com/LapPl4ND/ImageGabe/releases) page,
and right inside the app under **⚙️ Settings → See what's new**.

<br>

---

<div align="center">

Made with 💜 by **[Nyssira](https://github.com/LapPl4ND)**

<sub>ImageGabe is not affiliated with any of the platforms mentioned. Cover art and thumbnails belong to their respective owners.</sub>

</div>
