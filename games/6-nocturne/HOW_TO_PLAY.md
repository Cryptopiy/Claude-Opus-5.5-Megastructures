# Nocturne — how to run / как запустить

**[Русский](#русский) · [English](#english)**

---

## Русский

Nocturne — прогулка от первого лица (1 500 м, около 10 минут) по ночному мегаполису под дождём, у которого нет земли.
Над мостом проходит Ковчег — корабль длиной 5 км. Игра на Unity 6000.6.0f1, только Windows.

### Требования
- Windows 10/11 64-bit, видеокарта с Direct3D 11. Для 1080p хватает любой современной видеокарты (на тестовой
  машине около 290 fps), для 4K нужна мощная.
- Для сборки из исходников: Unity Hub и редактор **Unity 6000.6.0f1** (модуль Windows Build Support (Mono) ставится
  вместе с редактором для Windows), около 6 ГБ свободного места (проект, папка `Library`, сборка).

### Шаг 1. Скачать исходники игры
Вариант А — только папка игры (быстро, нужен Git). Адрес репозитория возьмите с зелёной кнопки **Code** на его
странице, папку клона назовите `megastructures`:
```
git clone --depth 1 --filter=blob:none --sparse <адрес репозитория> megastructures
cd megastructures
git sparse-checkout set games/6-nocturne
```
Вариант Б — без Git: на странице репозитория **Code → Download ZIP**, распакуйте архив и возьмите папку
`games/6-nocturne` (в архиве весь репозиторий, он большой).

Путь к папке лучше держать коротким и без кириллицы, например `C:\Games\Nocturne`.

### Шаг 2. Открыть проект в Unity
1. Unity Hub → **Add → Add project from disk** → выберите папку `6-nocturne` (в ней лежат `Assets`, `Packages`,
   `ProjectSettings`).
2. Откройте проект редактором 6000.6.0f1. Первое открытие долгое (10-30 минут): Unity импортирует текстуры и
   компилирует шейдеры в папку `Library`.
3. Если Unity спросит про Safe Mode или обновление API — откажитесь; проект собран именно под эту версию.

### Шаг 3а. Поиграть прямо в редакторе
Откройте сцену `Assets/_Game/Scenes/Main.unity` и нажмите **Play**. Для записи видео и честной картинки лучше
собрать плеер (шаг 3б): в редакторе производительность ниже.

### Шаг 3б. Собрать игру (рекомендуется)
- В меню редактора: **Tools → Megastructure → Build Windows Player**. Сборка появится в `Builds/Nocturne/Nocturne.exe`
  (около 465 МБ, 2-5 минут).
- Если что-то выглядит неправильно (розовые материалы, пустая сцена), сначала выполните **Tools → Megastructure →
  Rebuild All**, затем снова Build Windows Player.
- То же самое из командной строки (PowerShell, путь к Unity поправьте под свою установку):
```
& "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Games\Nocturne" -executeMethod Megastructure.EditorTools.BatchTools.BuildWindows -logFile build.log
```

### Шаг 4. Запустить
Дважды кликните `PLAY_WINDOWS.bat` в папке игры или `Builds\Nocturne\Nocturne.exe`. Папка
`Nocturne_BackUpThisFolder_ButDontShipItWithYourGame` рядом с exe — отладочные данные Unity, игре не нужна.

Если Windows покажет «Windows защитила ваш компьютер» — сборка не подписана: **Подробнее → Выполнить в любом случае**.

### Управление
| Ввод | Действие |
|---|---|
| W A S D / стрелки, левый стик | идти (1,5 м/с) |
| Shift (удерживать), шифты геймпада | быстрый шаг, 2,5 м/с |
| Мышь, правый стик | смотреть |
| C | включить / выключить кинематографический автопроход |
| Esc, Start | пауза |
| F11, Alt+Enter | полный экран / окно |

### Кинематографический режим и запись ролика
`Nocturne.exe -cinematic -res 2560x1440` — игра сама идёт по маршруту с постановочным взглядом (10 минут до финала,
плюс 17 с концовки). Не трогайте мышь и клавиатуру: любой ввод, кроме C, F11, Esc, Alt, Enter, Print Screen и Win,
возвращает управление. Записывайте в реальном времени в 1440p 60 fps (OBS, высокий битрейт). Громкость и подготовка
звука к загрузке на YouTube — в `README.md` («Upload loudness»).

Полезные флаги (по одному значению на флаг): `-res 1920x1080`, `-rain 0..1` (плотность дождя), `-nofade`,
`-beat 1..7` (начать с бита), `-quit-after <секунды>`. Полный список — в `README.md`.

---

## English

Nocturne is a first-person walk (1 500 m, about 10 minutes) through a rain-soaked night megacity that has no ground.
The Ark, a 5 km ship, passes over the bridge. Built with Unity 6000.6.0f1, Windows only.

### Requirements
- 64-bit Windows 10/11 and a Direct3D 11 GPU. Any current GPU is fine at 1080p (about 290 fps on the test machine);
  4K needs a strong one.
- To build from source: Unity Hub and the **Unity 6000.6.0f1** editor (Windows Build Support (Mono) comes with the
  Windows editor), and about 6 GB of free disk (project, `Library`, build).

### Step 1. Get the game's source
Option A — just the game folder (fast, needs Git). Copy the repository address from the green **Code** button on its
page; the clone goes into a folder named `megastructures`:
```
git clone --depth 1 --filter=blob:none --sparse <repository address> megastructures
cd megastructures
git sparse-checkout set games/6-nocturne
```
Option B — without Git: on the repository page click **Code → Download ZIP**, extract it and take the
`games/6-nocturne` folder (the archive holds the whole repository, so it is large).

Keep the path short and ASCII-only, for example `C:\Games\Nocturne`.

### Step 2. Open the project in Unity
1. Unity Hub → **Add → Add project from disk** → pick the `6-nocturne` folder (the one with `Assets`, `Packages`,
   `ProjectSettings`).
2. Open it with the 6000.6.0f1 editor. The first open is slow (10-30 minutes): Unity imports the textures and compiles
   the shaders into `Library`.
3. If Unity offers Safe Mode or an API update, decline; the project is made for this exact version.

### Step 3a. Play in the editor
Open the scene `Assets/_Game/Scenes/Main.unity` and press **Play**. For recording and the real picture, build the
player (step 3b): the editor runs slower.

### Step 3b. Build the game (recommended)
- From the editor menu: **Tools → Megastructure → Build Windows Player**. The build goes to
  `Builds/Nocturne/Nocturne.exe` (about 465 MB, 2-5 minutes).
- If something looks wrong (pink materials, an empty scene), run **Tools → Megastructure → Rebuild All** first, then
  Build Windows Player again.
- The same from the command line (PowerShell; adjust the Unity path to your install):
```
& "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Games\Nocturne" -executeMethod Megastructure.EditorTools.BatchTools.BuildWindows -logFile build.log
```

### Step 4. Run
Double-click `PLAY_WINDOWS.bat` in the game folder, or `Builds\Nocturne\Nocturne.exe`. The
`Nocturne_BackUpThisFolder_ButDontShipItWithYourGame` folder next to the exe is Unity's debug data; the game does not
need it.

If Windows shows "Windows protected your PC", the build is simply not code-signed: **More info → Run anyway**.

### Controls
| Input | Action |
|---|---|
| W A S D / arrow keys, left stick | walk (1.5 m/s) |
| Shift (hold), gamepad shoulder buttons | brisk walk, 2.5 m/s |
| Mouse, right stick | look |
| C | toggle the cinematic autowalk |
| Esc, Start | pause |
| F11, Alt+Enter | full screen / windowed |

### Cinematic mode and recording
`Nocturne.exe -cinematic -res 2560x1440` walks the route by itself with the authored gaze (10 minutes to the goal plus
a 17 s ending). Do not touch the mouse or keyboard: any input except C, F11, Esc, Alt, Enter, Print Screen and the
Windows keys hands control back. Record in real time at 1440p 60 fps (OBS, high bitrate). The upload loudness step for
YouTube is in `README.md` ("Upload loudness").

Useful flags (one value per flag): `-res 1920x1080`, `-rain 0..1` (rain density), `-nofade`, `-beat 1..7` (start at a
beat), `-quit-after <seconds>`. The full list is in `README.md`.
