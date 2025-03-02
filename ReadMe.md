# Kick Ball Game

Kick Ball Game is a Unity-based project utilizing free assets. All external assets used in the game can be found in:

```
./Assets/Resources/AdditivePackages
```
At the moment the project contains developed multiplayer part with player synchronization and switching of goalkeeper - forward modes between players, as well as logic of computer goalkeeper behavior. For the realization of multiplayer were used Websockets and node.js server.

## Core Components

### Main Scripts

- `Scripts/Game/GameController` - The primary scene object that controls and manages key events, linking various components together.
- `Scripts/Scene/AdditiveUISceneController` - Loads the UI scene and links it to game objects and the main scene.
- `Scripts/UI/UserUIComponentsProducer` - Provides core game objects access to UI elements and events.
- `Scripts/UI/IngameUIController` - Manages UI elements, buttons, and panels within the game.
- `Scripts/Ball/BallController` - Attached to the ball object, handling the core ball behavior.
- `Scripts/Ball/GateMove` - Controls the logic of the ball's movement towards the goal, including shot strength, jump height, and trajectory calculations.
- `Scripts/Environment/` - Contains scripts for scene triggers and NPC character management.
- `Scripts/Environment/Players/GoalkeeperController` - Attached to the goal prefab, managing the goalkeeper's movement, ball interception calculations, trajectory predictions, jump power, and response delay settings.
- `Scripts/Web/WebGKUpdate` - Attached to the goalkeeper. Synchronizes the goalkeeper's actions between clients.
- `Scripts/Ball/WebBallUpdate` - Attached to ball. Synchronizes the ball movement between clients.
- `Scripts/Web/Messages/` - There are all templates fo websocket messages, that sending to server and getting from it.

### Prefabs

- `EnemyGoalGate` - Goalpost with preset animations, a goalkeeper, and a penalty kicker.
- `CameraPack` - A dynamic camera that follows the ball and switches modes.
- `SoccerBall` - The configured ball object containing all control logic.

## Scenes

The project consists of three scenes:

1. **Main Menu**
2. **Game Field**
3. **Dynamically Loaded UI Scene**

## WebGL Build Settings

To ensure proper WebGL functionality across all devices, the following settings should be applied:

### Canvas Settings

- Each **Canvas** should have a reference resolution of **1920x1080**.
- Textures should be in **Sprite format** and **Override for WebGL** enabled.
- Large textures should not exceed **5MB** and should be pre-compressed in an image editor.
- Texture format: **.png**

### Player Settings

```
Resolution and Presentation:
- Resolution: 1920x1080

Other:
- Color Space: Gamma
- MSAA Fallback: Downgrade
- Auto Graphics API: Disabled
- Graphics API: WebGL2 and WebGL1
- Texture Compression Format: ASTC
- Quality: Normal
- Lightmap Streaming: Enabled
- Default Chunk Size: 16MB
- Strip Engine Code: High
- Optimize Mesh Data: Enabled
- Texture MipMap Stripping: Disabled (can be tested)

Publishing:
- Compression Format: Disabled
- Data Caching: Enabled
- Initial Memory Size: 32MB
- Growth Mode: Geometric
- Power Reference: High Performance
- Maximum Memory Size: 2048MB
- Step: 0.2
- Cap: 96
```

### Quality Settings for WebGL

- **Medium**

## WebGL Index Template for Telegram Mini Apps

To ensure proper functionality within Telegram Mini Apps, replace `index.html` with the following template:

```html
<!DOCTYPE html>
<html lang="en-us">
<head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>Unity WebGL Player | Kick Ball Project</title>
    <link rel="shortcut icon" href="TemplateData/favicon.ico">
    <link rel="stylesheet" href="TemplateData/style.css">
    <link rel="manifest" href="manifest.webmanifest">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no, orientation=landscape">
    <script src="https://telegram.org/js/telegram-web-app.js"></script>
    <style>
        body { overflow: hidden; margin: 0; padding: 0; }
        #unity-container { position: fixed; width: 100vw; height: 100vh; }
        #unity-canvas { width: 100% !important; height: 100% !important; }
    </style>
</head>
<body>
    <div id="unity-container">
        <canvas id="unity-canvas" tabindex="-1"></canvas>
        <div id="unity-loading-bar">
            <div id="unity-logo"></div>
            <div id="unity-progress-bar-empty">
                <div id="unity-progress-bar-full"></div>
            </div>
        </div>
        <div id="unity-warning"></div>
    </div>
    <script>
        window.addEventListener("load", function () {
            if (window.Telegram && Telegram.WebApp) {
                Telegram.WebApp.expand();
                setTimeout(() => Telegram.WebApp.expand(), 1000);
            }
            if ("serviceWorker" in navigator) {
                navigator.serviceWorker.register("ServiceWorker.js");
            }
        });
    </script>
</body>
</html>
```

Replace `PROJECT_FOLDER_NAME` with the actual build folder name.

## Deployment

### Backend

- Located in: `/Builds` along with the latest build.
- Final deployment path: `/var/www/SoccerGame/backend/`.
- app.py отвечает за запуск игры и открытие её в telegram mini app.
- websockets.js is responsible for creating lobbies for players, assigning initial roles to them, and exchanging messages between Unity-clients.

### Unity Frontend

- Final deployment path: `/var/www/SoccerGame/`
- If paths need to be changed, update `app.py` accordingly.

## Server Requirements

To run the project on a Linux server, ensure the following are installed:

- **Python**
- **Aiogram 2.x** (Aiogram 3.x is not supported)
- **Nodejs**

---

This document serves as a structured README for better project understanding, setup, and deployment.




