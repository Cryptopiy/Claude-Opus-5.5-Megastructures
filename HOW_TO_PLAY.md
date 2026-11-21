# How to play / Как запустить

**[Русский](#русский) · [English](#english)**

---

## Русский

### Игры и файлы запуска

- **Abyss** (Unity 6000.6.0f1): [Abyss-Windows-x64.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Abyss-Windows-x64.zip) → запустите `Abyss.exe` · [Abyss-macOS.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Abyss-macOS.zip)
- **The Ring** (Unity 6000.6.0f1): [TheRing-Windows-x64.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/TheRing-Windows-x64.zip) → запустите `The Ring.exe` · [TheRing-macOS.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/TheRing-macOS.zip)
- **Thread** (Unity 6000.6.0f1): [Thread-Windows-x64.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Thread-Windows-x64.zip) → запустите `Thread.exe` · [Thread-macOS.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Thread-macOS.zip)
- **Bulwark** (Unity 6000.6.0f1, Claude Fable 5.2): [Bulwark-Windows-x64.exe](../../releases/download/bulwark-v1.0/Bulwark-Windows-x64.exe) → скачайте и дважды кликните один файл · [отдельный гайд](games/4-bulwark/HOW_TO_PLAY.md)

### Bulwark — один EXE без ручной распаковки

1. Скачайте `Bulwark-Windows-x64.exe` по ссылке выше. Другие файлы и исходники не нужны.
2. Дважды кликните по EXE. Игра автоматически распакуется во временную папку и запустится; после выхода временные файлы удаляются.
3. Требования: Windows 10/11 64-bit, Direct3D 11 и около 300 МБ свободного места во временной папке.
4. Подробности, управление и проверка SHA-256 — в [гайде Bulwark](games/4-bulwark/HOW_TO_PLAY.md).

### Abyss, The Ring и Thread — ZIP-сборки для Windows

1. Скачайте zip игры (ссылка выше или вкладка **Releases** справа на странице репозитория).
2. Нажмите по zip правой кнопкой → **Извлечь всё…** Не запускайте игру прямо из архива.
3. Откройте распакованную папку и дважды кликните `.exe` с названием игры. Всё — установка не нужна.
4. Если Windows показала «Windows защитила ваш компьютер» — это потому, что сборка не подписана платным сертификатом. Нажмите **Подробнее → Выполнить в любом случае**. Исходный код каждой игры лежит в этом репозитории.
5. Проверить, что файл не изменён: в PowerShell выполните `Get-FileHash .\ИмяФайла.zip` и сравните с `SHA256SUMS.txt` в релизе.

Требования: Windows 10/11 64-bit, видеокарта с поддержкой DirectX 12 / Vulkan. Для игр на Unreal Engine при ошибке про отсутствующие DLL установите Microsoft Visual C++ Redistributable (x64) с сайта Microsoft.

### macOS

Распакуйте zip/tar.gz, затем на приложении: правый клик → **Открыть** → **Открыть** (сборка не подписана Apple). Если macOS пишет, что приложение повреждено: `xattr -dr com.apple.quarantine /путь/к/Игра.app`, а если не помогло — `codesign --force --deep --sign - /путь/к/Игра.app`.

### Как открыть исходники в движке

**Unity (6000.6.0f1):** установите Unity Hub и редактор этой версии. **Hub → Add → Add project from disk** → выберите папку игры (где лежат `Assets`, `Packages`, `ProjectSettings`). Первое открытие долгое — Unity создаёт папку `Library`. Откройте основную сцену (см. README игры или `File → Build Profiles`) и нажмите **Play**. Для Bulwark основная сцена — `Assets/_Game/Scenes/Main.unity`.

### Сделать такую же игру с ИИ

Промпты лежат в папке [prompts/](prompts) (если есть), а гайд по связке Claude/Codex + движок + Blender MCP — в [BLENDER_MCP_GUIDE.md](BLENDER_MCP_GUIDE.md).

---

## English

### Games and launch files

- **Abyss** (Unity 6000.6.0f1): [Abyss-Windows-x64.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Abyss-Windows-x64.zip) → run `Abyss.exe` · [Abyss-macOS.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Abyss-macOS.zip)
- **The Ring** (Unity 6000.6.0f1): [TheRing-Windows-x64.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/TheRing-Windows-x64.zip) → run `The Ring.exe` · [TheRing-macOS.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/TheRing-macOS.zip)
- **Thread** (Unity 6000.6.0f1): [Thread-Windows-x64.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Thread-Windows-x64.zip) → run `Thread.exe` · [Thread-macOS.zip](https://github.com/Cryptopiy/Claude-Opus-5.5-Megastructures/releases/download/v1.0/Thread-macOS.zip)
- **Bulwark** (Unity 6000.6.0f1, Claude Fable 5.2): [Bulwark-Windows-x64.exe](../../releases/download/bulwark-v1.0/Bulwark-Windows-x64.exe) → download and double-click one file · [dedicated guide](games/4-bulwark/HOW_TO_PLAY.md)

### Bulwark — one EXE, no manual extraction

1. Download `Bulwark-Windows-x64.exe` from the link above. No other files or source code are needed.
2. Double-click the EXE. The game automatically extracts to a temporary folder and starts; temporary files are removed when you exit.
3. Requirements: 64-bit Windows 10/11, Direct3D 11, and about 300 MB of free space in the temporary folder.
4. See the [Bulwark guide](games/4-bulwark/HOW_TO_PLAY.md) for details, controls, and SHA-256 verification.

### Abyss, The Ring, and Thread — Windows ZIP builds

1. Download the game's zip (link above or the **Releases** tab on the right side of the repo page).
2. Right-click the zip → **Extract All…** Do not run the game from inside the archive.
3. Open the extracted folder and double-click the `.exe` named after the game. That's it — no installation.
4. If Windows shows “Windows protected your PC”, it's because the build isn't signed with a paid certificate. Click **More info → Run anyway**. The full source code of every game is in this repo.
5. To verify the download: in PowerShell run `Get-FileHash .\FileName.zip` and compare with `SHA256SUMS.txt` in the release.

Requirements: Windows 10/11 64-bit, a GPU with DirectX 12 / Vulkan support. For Unreal Engine games, if you get a missing-DLL error, install Microsoft Visual C++ Redistributable (x64) from Microsoft's website.

### macOS

Unzip, then right-click the app → **Open** → **Open** (the build is not notarized by Apple). If macOS says the app is damaged: `xattr -dr com.apple.quarantine /path/to/Game.app`; if that doesn't help — `codesign --force --deep --sign - /path/to/Game.app`.

### Open the source in the engine

**Unity (6000.6.0f1):** install Unity Hub and that editor version. **Hub → Add → Add project from disk** → pick the game folder (the one with `Assets`, `Packages`, `ProjectSettings`). The first open is slow — Unity rebuilds `Library`. Open the main scene (see the game's README or `File → Build Profiles`) and press **Play**. For Bulwark, the main scene is `Assets/_Game/Scenes/Main.unity`.

### Make a game like this with AI

The prompts are in [prompts/](prompts) (if present), and the Claude/Codex + engine + Blender MCP setup guide is in [BLENDER_MCP_GUIDE.md](BLENDER_MCP_GUIDE.md).
