<div align="center">

<img src="Assets/logo.png" alt="Logo ImageGabe" width="160">

# ImageGabe

**Récupère n'importe quelle pochette ou miniature en un clic, dans la meilleure qualité dispo.**

YouTube · Spotify · SoundCloud · Deezer · Apple Music · Twitch

[![Dernière version](https://img.shields.io/github/v/release/LapPl4ND/ImageGabe?style=for-the-badge&label=version&color=8B5CF6)](https://github.com/LapPl4ND/ImageGabe/releases/latest)
[![Téléchargements](https://img.shields.io/github/downloads/LapPl4ND/ImageGabe/total?style=for-the-badge&label=t%C3%A9l%C3%A9chargements&color=EC4899)](https://github.com/LapPl4ND/ImageGabe/releases)
![Windows](https://img.shields.io/badge/Windows-10%20%7C%2011-1E1E24?style=for-the-badge)

<br>

### [⬇️ Télécharger ImageGabe](https://github.com/LapPl4ND/ImageGabe/releases/latest/download/ImageGabe.exe)

<sub>Un seul fichier .exe · Aucune installation · Pas besoin de .NET</sub>

</div>

<br>

<!--
  📸 CAPTURE D'ÉCRAN : ajoute une capture de l'app dans Assets/screenshot.png,
  puis supprime cette ligne de commentaire et celle tout en bas du bloc.

<div align="center">
  <img src="Assets/screenshot.png" alt="Aperçu d'ImageGabe" width="420">
</div>

-->

## ✨ Fonctionnalités

<table>
<tr>
<td width="50%" valign="top">

### 🎯 Simple et rapide
- Colle un lien et l'aperçu s'affiche instantanément
- **Glisser-déposer** un lien depuis ton navigateur
- **Détection automatique** d'un lien copié dans le presse-papiers
- Copie l'image en un clic, ou enregistre-la en **JPG** ou **PNG**

</td>
<td width="50%" valign="top">

### 🖼️ Qualité maximale
- Toujours la **plus haute résolution** disponible
- Jusqu'à **3000 × 3000 px** pour Apple Music
- Indicateur de qualité 🟢 HD / 🟠 moyenne
- Fichier nommé automatiquement avec le **titre du morceau**

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 🎨 Design soigné
- L'interface prend la **couleur de la plateforme** détectée
- **Fond ambiant** flouté avec la pochette
- Thème **sombre** ou **clair**
- Interface en **français** ou en **anglais**

</td>
<td width="50%" valign="top">

### 🔄 Toujours à jour
- **Mises à jour automatiques** en un clic
- Section **« Nouveautés »** pour voir ce qui change
- Fonctionne même avec une connexion lente

</td>
</tr>
</table>

<br>

## 🎵 Plateformes prises en charge

| Plateforme | Liens acceptés | Qualité max |
|:--|:--|:--:|
| **YouTube** | Vidéos, Shorts, YouTube Music, liens `youtu.be` | 1280 × 720 |
| **Spotify** | Titres, albums, playlists, artistes, podcasts | 640 × 640 et + |
| **SoundCloud** | Titres, playlists, liens courts | originale |
| **Deezer** | Titres, albums, playlists, artistes, liens courts | 1000 × 1000 et + |
| **Apple Music** | Titres, albums, playlists, artistes | 3000 × 3000 |
| **Twitch** | Live en cours, VOD, clips | 1920 × 1080 |

<sub>La qualité réelle dépend de l'image mise en ligne à l'origine.</sub>

<br>

## 🚀 Utilisation

1. **Copie un lien** depuis YouTube, Spotify, Deezer, etc.
2. **Colle-le** dans ImageGabe (ou glisse-le dans la fenêtre)
3. Clique sur **Aperçu**
4. **Enregistre** l'image ou **copie-la** directement

### ⌨️ Raccourcis

| Raccourci | Action |
|:--|:--|
| <kbd>Entrée</kbd> | Lancer l'aperçu |
| <kbd>Ctrl</kbd> + <kbd>S</kbd> | Enregistrer l'image |
| <kbd>Ctrl</kbd> + <kbd>Maj</kbd> + <kbd>C</kbd> | Copier l'image |
| <kbd>Échap</kbd> | Fermer les paramètres |

<br>

## ⚙️ Paramètres

Accessibles via l'icône ⚙️ en haut de la fenêtre :

- **Thème** : sombre ou clair
- **Langue** : français ou anglais
- **Format par défaut** : JPG ou PNG
- **Mises à jour** : vérification automatique au démarrage
- **Nouveautés** : l'historique de toutes les versions

<br>

## ❓ FAQ

<details>
<summary><b>Windows affiche « Windows a protégé votre ordinateur »</b></summary>
<br>
C'est normal pour une petite application qui n'est pas signée numériquement.
Clique sur <b>Informations complémentaires</b>, puis sur <b>Exécuter quand même</b>.
Ce message n'apparaît qu'au premier lancement.
</details>

<details>
<summary><b>Est-ce que je dois installer quelque chose ?</b></summary>
<br>
Non. ImageGabe est un seul fichier <code>.exe</code> qui contient tout ce qu'il faut.
Place-le où tu veux (Bureau, Documents…) et lance-le.
</details>

<details>
<summary><b>La mise à jour automatique ne fonctionne pas</b></summary>
<br>
Évite de placer l'application dans <code>C:\Program Files</code> : ce dossier demande les droits administrateur
et empêche l'app de se remplacer. Le Bureau ou un dossier personnel fonctionnent sans souci.
Sinon, tu peux toujours télécharger la dernière version depuis la page
<a href="https://github.com/LapPl4ND/ImageGabe/releases/latest">Releases</a>.
</details>

<details>
<summary><b>Un lien Twitch ne fonctionne pas</b></summary>
<br>
Pour un lien de chaîne, ImageGabe récupère l'aperçu du <b>live en cours</b> : la chaîne doit donc être en direct.
Pour une VOD ou un clip, utilise le lien direct de la vidéo.
</details>

<details>
<summary><b>Où sont enregistrés mes paramètres ?</b></summary>
<br>
Dans <code>%AppData%\ImageGabe\settings.json</code>.
</details>

<br>

## 🛠️ Compiler le projet

Prérequis : [Visual Studio 2022](https://visualstudio.microsoft.com/) avec la charge de travail **Développement .NET Desktop**.

```bash
git clone https://github.com/LapPl4ND/ImageGabe.git
cd ImageGabe
```

- **Lancer en développement** : ouvre le projet dans Visual Studio et appuie sur <kbd>F5</kbd>
- **Créer l'exe final** : double-clique sur `publish.bat` (le fichier apparaît dans le dossier `publish`)

<details>
<summary><b>Structure du projet</b></summary>
<br>

| Fichier | Rôle |
|:--|:--|
| `MainWindow.xaml` / `.cs` | Interface et logique de la fenêtre |
| `ArtworkService.cs` | Détection des plateformes et récupération des images |
| `UpdateService.cs` | Mises à jour automatiques et notes de version via GitHub |
| `Theme.cs` | Thèmes sombre et clair |
| `Loc.cs` | Traductions français / anglais |
| `AppSettings.cs` | Sauvegarde des paramètres |
| `Assets/Logos/` | Logos des plateformes |
| `publish.bat` | Génère l'exe final |

</details>

<br>

## 📝 Notes de version

Toutes les nouveautés sont listées sur la page [**Releases**](https://github.com/LapPl4ND/ImageGabe/releases),
et directement dans l'application via **⚙️ Paramètres → Voir les nouveautés**.

<br>

---

<div align="center">

Fait avec 💜 par **[Nyssira](https://github.com/LapPl4ND)**

<sub>ImageGabe n'est affilié à aucune des plateformes citées. Les pochettes et miniatures appartiennent à leurs auteurs respectifs.</sub>

</div>
