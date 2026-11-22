# Bulwark asset notices / Уведомления об ассетах Bulwark

**[Русский](#русский) · [English](#english)**

---

## Русский

### Источники Tripo3D

| Файл | Происхождение |
|---|---|
| `3D Models/SM_Creature_Octopus.glb` | Предоставленная пользователем модель, созданная с помощью Tripo3D |
| `3D Models/SM_Creature_MonsterTentacles.glb` | Предоставленная пользователем модель, созданная с помощью Tripo3D |

Эти GLB являются источниками для генерации мешей, LOD, карт материалов и других производных в `Assets/_Game/Creature/Kraken/`, включая содержимое `Resources/Kraken/`. Процедурный код проекта описан в `Tools/kraken/` и `Assets/_Game/Creature/README.md`.

В метаданных обеих GLB указан генератор `Khronos glTF Blender I/O v5.0.21`; поле `copyright` отсутствует. Само по себе отсутствие этого поля не означает CC0 или отсутствие прав.

### Лицензионный статус

- Корневая [CC BY 4.0 для собственных ассетов](../../LICENSE-ASSETS.md) **не распространяется** на исходные GLB Tripo3D и производные, содержащие их геометрию или текстуры.
- [MIT репозитория](../../LICENSE) покрывает исходный код, но не предоставляет прав на эти модели.
- Настоящий репозиторий **не предоставляет дополнительной лицензии** на исходные модели и их производные. Их наличие документирует воспроизводимость проекта, а не передачу прав.
- Перед копированием, распространением или коммерческим использованием необходимо проверить права, связанные с аккаунтом и тарифом Tripo3D, на которых модели были созданы, а также права на исходные промпты/изображения. Условия сервиса могут меняться: https://www.tripo3d.ai/terms

Если требуется полностью открытая версия Bulwark, замените обе исходные модели собственными или надлежащим образом лицензированными моделями и заново создайте производные ассеты. Не переименовывайте и не перелицензируйте существующие файлы без подтверждения прав.

---

## English

### Tripo3D sources

| File | Provenance |
|---|---|
| `3D Models/SM_Creature_Octopus.glb` | User-supplied model generated with Tripo3D |
| `3D Models/SM_Creature_MonsterTentacles.glb` | User-supplied model generated with Tripo3D |

These GLBs are the sources for generated meshes, LODs, material maps and other derivatives under `Assets/_Game/Creature/Kraken/`, including `Resources/Kraken/`. The project's procedural pipeline is documented in `Tools/kraken/` and `Assets/_Game/Creature/README.md`.

Both GLB metadata records identify `Khronos glTF Blender I/O v5.0.21` as the generator and contain no `copyright` field. The absence of that field alone does not mean CC0 or absence of rights.

### License status

- The repository-wide [CC BY 4.0 asset license](../../LICENSE-ASSETS.md) **does not cover** the Tripo3D source GLBs or derivatives containing their geometry or textures.
- The repository's [MIT license](../../LICENSE) covers source code but grants no rights to these models.
- This repository **grants no additional license** to the source models or their derivatives. Their presence documents project reproducibility, not a transfer of rights.
- Before copying, redistribution or commercial use, verify the rights associated with the Tripo3D account and plan used to generate them and with the source prompts/images. Service terms may change: https://www.tripo3d.ai/terms

If a fully open-licensed Bulwark is required, replace both source models with original or properly licensed replacements and regenerate the derivative assets. Do not rename or relicense the existing files without confirmed rights.
