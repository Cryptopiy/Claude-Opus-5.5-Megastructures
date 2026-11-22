# Third-party notices / Уведомления о сторонних компонентах

**[Русский](#русский) · [English](#english)**

Этот файл перечисляет известные сторонние компоненты и исключения из общих лицензий репозитория. Он не заменяет условия соответствующих правообладателей.

This file records known third-party components and exclusions from the repository-wide licenses. It does not replace the respective owners' terms.

---

## Русский

### 1. Unity и внешние инструменты

Все пять Unity-проектов используют **Unity 6000.6.0f1**, URP 17.6.0 и официальные пакеты из каждого `Packages/manifest.json`. Unity, встроенный шрифт `LegacyRuntime.ttf` и пакеты Unity распространяются по условиям Unity, включая применимые Unity Terms of Service и Unity Companion License. Они не перелицензируются под MIT или CC BY этим репозиторием.

Blender, Python, NumPy, Pillow, trimesh, pygltflib, pymeshlab, fast-simplification, SciPy и FFmpeg являются внешними инструментами разработки или сборки. Их исполняемые файлы и библиотеки не включены в репозиторий как самостоятельные распространяемые компоненты; при установке применяются их собственные лицензии.

### 2. Сторонний код

| Компонент | Где используется | Условия и атрибуция |
|---|---|---|
| **Hash without Sine — David Hoskins** | `games/3-thread/Assets/_Game/Shaders/Include/ThreadCommon.hlsl`, функции `ThHash12`, `ThHash22`, `ThHash32` | MIT, Copyright (c) 2014 David Hoskins. Источник: https://www.shadertoy.com/view/4djSRW. Полный текст: [MIT-David-Hoskins.txt](LICENSES/MIT-David-Hoskins.txt). |
| **PCG / PCG RXS-M-XS** | Целочисленные хеши `AbPcg`, `RingPcg`, `ThPcg`, `BwPcg`, `NocMath.Pcg` и их CPU/GPU-варианты в `games/*/Assets/_Game/` | Базовый PCG: Melissa E. O'Neill / PCG Project, Apache License 2.0, https://www.pcg-random.org/download.html. Полный текст: [Apache-2.0.txt](LICENSES/Apache-2.0.txt). Конкретный GPU-хеш также атрибутирован в исходниках как Mark Jarzynski and Marc Olano, *Hash Functions for GPU Rendering*, JCGT 9(3), 2020, https://www.jcgt.org/published/0009/03/02/. Ссылка на статью является атрибуцией исследования и не перелицензирует её. |
| **OKLab — Björn Ottosson** | `ThreadCommon.hlsl`, `ThreadSkyLib.hlsl` и связанные C#-реализации в `games/3-thread/` | Автор разрешает public domain или MIT; для сохранения атрибуции используется MIT, Copyright (c) 2020 Björn Ottosson. Источник: https://bottosson.github.io/posts/oklab/. Полный текст: [MIT-Bjorn-Ottosson.txt](LICENSES/MIT-Bjorn-Ottosson.txt). |

Файл `CODE_OF_CONDUCT.md` использует **Contributor Covenant 2.1** под CC BY 4.0; атрибуция сохранена в конце файла. Источник: https://www.contributor-covenant.org/version/2/1/code_of_conduct.html

Ссылки на исследования в `docs/RESEARCH.md` и связанных документах описывают использованные идеи и не означают, что соответствующие программы, изображения или данные включены в репозиторий.

### 3. Nocturne — ambientCG и Poly Haven

Nocturne содержит обработанные производные 29 наборов текстур **ambientCG** и **Poly Haven** в `Assets/_Game/Textures/External/`. Все они распространяются под **CC0 1.0 Universal**, а не под общей CC BY 4.0 репозитория.

- Полный реестр ассетов и ссылки: [games/6-nocturne/ASSET_NOTICES.md](games/6-nocturne/ASSET_NOTICES.md).
- Первоначальный журнал происхождения: [games/6-nocturne/docs/TEXTURES.md](games/6-nocturne/docs/TEXTURES.md).
- Текст CC0: [LICENSES/CC0-1.0.txt](LICENSES/CC0-1.0.txt).

### 4. Bulwark — модели Tripo3D

Bulwark содержит две предоставленные пользователем GLB-модели, созданные с помощью Tripo3D, и производные меши/текстуры кракена:

- `games/4-bulwark/3D Models/SM_Creature_Octopus.glb`;
- `games/4-bulwark/3D Models/SM_Creature_MonsterTentacles.glb`;
- производные в `games/4-bulwark/Assets/_Game/Creature/Kraken/`.

Их происхождение и ограничения описаны в [games/4-bulwark/ASSET_NOTICES.md](games/4-bulwark/ASSET_NOTICES.md). **Общая CC BY 4.0 репозитория на них не распространяется, и репозиторий не предоставляет на них дополнительных прав.** Перед повторным использованием проверьте условия Tripo3D для аккаунта и тарифа, на которых они были созданы: https://www.tripo3d.ai/terms

### 5. Полные тексты

Индекс всех включённых лицензий находится в [LICENSES/README.md](LICENSES/README.md).

---

## English

### 1. Unity and external tools

All five Unity projects use **Unity 6000.6.0f1**, URP 17.6.0 and the official packages listed in each `Packages/manifest.json`. Unity, the built-in `LegacyRuntime.ttf` font and Unity packages are governed by applicable Unity terms, including the Unity Terms of Service and Unity Companion License. This repository does not relicense them under MIT or CC BY.

Blender, Python, NumPy, Pillow, trimesh, pygltflib, pymeshlab, fast-simplification, SciPy and FFmpeg are external development or build tools. Their executables and libraries are not bundled as standalone redistributed components; their own licenses apply when they are installed.

### 2. Third-party code

| Component | Location | License and attribution |
|---|---|---|
| **Hash without Sine — David Hoskins** | `games/3-thread/Assets/_Game/Shaders/Include/ThreadCommon.hlsl`, functions `ThHash12`, `ThHash22`, `ThHash32` | MIT, Copyright (c) 2014 David Hoskins. Source: https://www.shadertoy.com/view/4djSRW. Full text: [MIT-David-Hoskins.txt](LICENSES/MIT-David-Hoskins.txt). |
| **PCG / PCG RXS-M-XS** | Integer hashes `AbPcg`, `RingPcg`, `ThPcg`, `BwPcg`, `NocMath.Pcg` and their CPU/GPU variants under `games/*/Assets/_Game/` | The underlying PCG work: Melissa E. O'Neill / PCG Project, Apache License 2.0, https://www.pcg-random.org/download.html. Full text: [Apache-2.0.txt](LICENSES/Apache-2.0.txt). The specific GPU hash is also attributed in source to Mark Jarzynski and Marc Olano, *Hash Functions for GPU Rendering*, JCGT 9(3), 2020, https://www.jcgt.org/published/0009/03/02/. The paper citation is research attribution, not a relicensing of the paper. |
| **OKLab — Björn Ottosson** | `ThreadCommon.hlsl`, `ThreadSkyLib.hlsl` and related C# implementations in `games/3-thread/` | The author permits public domain or MIT use; MIT is used here to preserve attribution, Copyright (c) 2020 Björn Ottosson. Source: https://bottosson.github.io/posts/oklab/. Full text: [MIT-Bjorn-Ottosson.txt](LICENSES/MIT-Bjorn-Ottosson.txt). |

`CODE_OF_CONDUCT.md` uses **Contributor Covenant 2.1** under CC BY 4.0; attribution is preserved at the end of that file. Source: https://www.contributor-covenant.org/version/2/1/code_of_conduct.html

Research citations in `docs/RESEARCH.md` and related documents identify ideas used and do not imply that the referenced software, images or datasets are bundled.

### 3. Nocturne — ambientCG and Poly Haven

Nocturne contains processed derivatives of 29 **ambientCG** and **Poly Haven** texture sets under `Assets/_Game/Textures/External/`. They remain under **CC0 1.0 Universal**, not the repository's CC BY 4.0 asset license.

- Asset registry and links: [games/6-nocturne/ASSET_NOTICES.md](games/6-nocturne/ASSET_NOTICES.md).
- Original provenance log: [games/6-nocturne/docs/TEXTURES.md](games/6-nocturne/docs/TEXTURES.md).
- CC0 text: [LICENSES/CC0-1.0.txt](LICENSES/CC0-1.0.txt).

### 4. Bulwark — Tripo3D models

Bulwark includes two user-supplied GLB models generated with Tripo3D and derivative kraken meshes/textures:

- `games/4-bulwark/3D Models/SM_Creature_Octopus.glb`;
- `games/4-bulwark/3D Models/SM_Creature_MonsterTentacles.glb`;
- derivatives under `games/4-bulwark/Assets/_Game/Creature/Kraken/`.

See [games/4-bulwark/ASSET_NOTICES.md](games/4-bulwark/ASSET_NOTICES.md). **The repository-wide CC BY 4.0 license does not cover them, and this repository grants no additional rights to them.** Before reuse, verify the Tripo3D terms for the account and plan used to generate them: https://www.tripo3d.ai/terms

### 5. Full texts

See the index in [LICENSES/README.md](LICENSES/README.md).
