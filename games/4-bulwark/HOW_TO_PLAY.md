# Как запустить Bulwark / How to play

**[Русский](#русский) · [English](#english)**

Проект полностью создан с использованием **Claude Fable 5.2**. / The project was created entirely with **Claude Fable 5.2**.

---

## Русский

### Запуск на Windows — один файл

1. Скачайте **[Bulwark-Windows-x64.exe](../../../../releases/download/bulwark-v1.0/Bulwark-Windows-x64.exe)**. Для игры нужен только этот файл.
2. Дважды кликните по скачанному EXE. Устанавливать Unity, скачивать исходники или вручную распаковывать архив не нужно.
3. При запуске игра автоматически распакуется во временную папку и откроется. Это может занять несколько секунд. После выхода временные файлы удаляются.

Если Windows показывает предупреждение SmartScreen, нажмите **«Подробнее» → «Выполнить в любом случае»**. Предупреждение появляется потому, что сборка не подписана цифровым сертификатом.

**Требования:** 64-разрядная Windows 10/11, видеокарта с поддержкой Direct3D 11 и примерно 300 МБ свободного места во временной папке. Рекомендуется видеокарта с 4 ГБ видеопамяти или больше.

### Управление

| Действие | Клавиши |
|---|---|
| Движение | `W A S D` или стрелки |
| Осмотр | мышь |
| Быстрый шаг | удерживать `Shift` |
| Включить или выключить кинематографическую автопрогулку | `C` |
| Пауза, продолжить, начать заново или выйти | `Esc` |
| Полноэкранный или оконный режим | `F11` или `Alt+Enter` |

Во время автопрогулки почти любой другой ввод возвращает управление игроку.

### Проверка скачанного файла

В PowerShell выполните:

```powershell
Get-FileHash .\Bulwark-Windows-x64.exe -Algorithm SHA256
```

Сравните результат со строкой `Bulwark-Windows-x64.exe` в файле **[SHA256SUMS.txt](../../../../releases/download/bulwark-v1.0/SHA256SUMS.txt)** из того же релиза.

### Запуск из исходников

1. Установите Unity Hub и редактор **Unity 6000.6.0f1**.
2. В Unity Hub выберите **Add → Add project from disk** и укажите папку `games/4-bulwark` — в ней находятся `Assets`, `Packages` и `ProjectSettings`.
3. Дождитесь первоначального импорта, откройте сцену `Assets/_Game/Scenes/Main.unity` и нажмите **Play**.

Команды полной пересборки Windows-версии и тестирования приведены в [README](README.md#rebuild-from-source).

---

## English

### Run on Windows — one file

1. Download **[Bulwark-Windows-x64.exe](../../../../releases/download/bulwark-v1.0/Bulwark-Windows-x64.exe)**. This is the only file the game needs.
2. Double-click the downloaded EXE. You do not need Unity, the source code, or manual archive extraction.
3. The game automatically extracts to a temporary folder and starts. This may take a few seconds. Temporary files are removed when you exit.

If Windows SmartScreen appears, click **More info → Run anyway**. The warning appears because the build is not digitally signed.

**Requirements:** 64-bit Windows 10/11, a Direct3D 11-capable GPU, and about 300 MB of free space in the temporary folder. A GPU with 4 GB of VRAM or more is recommended.

### Controls

| Action | Input |
|---|---|
| Move | `W A S D` or arrow keys |
| Look | mouse |
| Brisk walk | hold `Shift` |
| Toggle cinematic autowalk | `C` |
| Pause, resume, restart, or quit | `Esc` |
| Toggle fullscreen / windowed mode | `F11` or `Alt+Enter` |

Almost any other input during autowalk returns control to the player.

### Verify the download

Run this in PowerShell:

```powershell
Get-FileHash .\Bulwark-Windows-x64.exe -Algorithm SHA256
```

Compare the result with the `Bulwark-Windows-x64.exe` entry in **[SHA256SUMS.txt](../../../../releases/download/bulwark-v1.0/SHA256SUMS.txt)** from the same release.

### Run from source

1. Install Unity Hub and the **Unity 6000.6.0f1** editor.
2. In Unity Hub, select **Add → Add project from disk** and choose the `games/4-bulwark` folder, which contains `Assets`, `Packages`, and `ProjectSettings`.
3. Wait for the initial import, open `Assets/_Game/Scenes/Main.unity`, and press **Play**.

See [README](README.md#rebuild-from-source) for full Windows rebuild and testing commands.
