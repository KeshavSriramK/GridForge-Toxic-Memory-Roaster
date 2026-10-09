<div align="center">

# ⚡ GridForge

**A High-Performance, Cross-Platform 2D Game Engine Built with .NET 8**

![Desktop Build Status](https://img.shields.io/badge/Desktop-Passing-brightgreen?style=for-the-badge&logo=windows)
![Android Build Status](https://img.shields.io/badge/Android-Passing-brightgreen?style=for-the-badge&logo=android)
![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)

</div>

---

## 🚀 Overview

**GridForge** is a modular cross-platform C# game engine designed for grid-based simulation and rendering. Built with performance and portability in mind, it seamlessly compiles across **Desktop** and **Android** platforms through automated CI/CD pipelines.

---

## 🔥 Features

- 🎯 **Cross-Platform Architecture:** Clean separation of concerns between core engine logic and platform runners.
- ⚡ **A* Pathfinding Module:** High-speed pathfinding engine optimized for large-scale grid structures.
- 🤖 **Automated CI/CD:** Fully configured GitHub Actions pipeline for automated builds and APK releases.
- 📱 **Mobile Optimized:** Built for high performance on Android mobile devices and tablets.

---

## 🛠️ Tech Stack & Architecture

```text
GridForge
 ├── 🧩 GridForge.Core        # Engine architecture, AI pathfinding & grid math (.NET 8.0)
 ├── 💻 GridForge.Desktop     # Windows/Desktop target runner (.NET 8.0)
 └── 📱 GridForge.Android     # Android native mobile target runner (.NET 8.0-android)
