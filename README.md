# Animated Wallpaper App

## Overview
This is a simple application that allows you to set animated GIFs or videos as your desktop wallpaper on Windows. It automatically checks for updates and installs the latest version silently in the background.

## Features

- Set animated GIFs or videos as wallpaper.
- Automatically checks for updates on startup.
- Downloads and installs updates silently.
- Runs as a background process (no UI required).
- Easy-to-use installer with automatic startup integration.

## Installation

### 1. Download the Installer
To install the **Animated Wallpaper App**, download the latest version from the [Releases](https://github.com/thelonewolf39/AnimatedWallpaperApp/releases) section.

### 2. Run the Installer
- Double-click the installer (`AnimatedWallpaperInstaller.msi`) to start the installation process.
- Follow the prompts to install the app to your desired location (default: `C:\Program Files\AnimatedWallpaperApp`).

### 3. Set Up as a Startup Program
The app will be added to your **Windows startup** automatically, so it will run in the background every time your computer starts.

## Usage

After installation, the app will automatically check for the latest version on startup. If a newer version is found, it will be downloaded and installed without any user interaction.

## Automatic Updates

The app automatically checks for updates each time it launches. If a new version is found, it will be downloaded and installed silently in the background without user input.

- The app will check the version stored in `https://example.com/version.txt`.
- If a newer version is available, it will download the installer from `https://example.com/AnimatedWallpaperInstaller.msi` and install it.
- No user interaction is needed for updates.

## Hosting Backend

For automatic updates to work, the app requires the following:

### 1. **`version.txt`** File
This file contains the latest version number of the app. It should be hosted somewhere accessible via HTTP (e.g., on a web server or GitHub).

#### Example of `version.txt`:
1.0.0

### 2. **Installer Download URL**
The new installer (e.g., `.msi` or `.exe`) should be hosted on a server or a service that allows direct download links (e.g., GitHub, your own web server).

For example, you can use:
- **GitHub**: Store the `version.txt` and installer in your repository and make them accessible via a direct URL.
- **Your own server**: Host the `version.txt` and installer on a web server.

Example download URL for the installer:
https://example.com/AnimatedWallpaperInstaller.msi

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
