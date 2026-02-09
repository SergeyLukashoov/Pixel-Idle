# Руководство по замене визуала Idle Archer

Документ описывает, **где лежит весь визуал** и **как заменить его на новый стиль**, сохраняя структуру игры (те же префабы, сцены, логика).

---

## Текущая структура визуала

### 1. **Spine-анимации (скелеты + атласы)**

Используются для персонажа, врагов, шахты, сундуков. Формат: **.skel.bytes** (или .json) + **.atlas.txt** + **.png**.

| Объект | Путь к анимации | Файлы |
|--------|-----------------|--------|
| **Игрок** | `Assets/Resources/Player/` | Unity Animator (.controller, .anim) — не Spine |
| **Враги: Common** | `Assets/Resources/Enemy/Common/Animation/` | .skel, .atlas.txt, .png |
| **Враги: Quick** | `Assets/Resources/Enemy/Quick/Animation/` | то же |
| **Враги: Range** | `Assets/Resources/Enemy/Range/Animation/` | то же |
| **Враги: Durable** | `Assets/Resources/Enemy/Durable/Animation/` | enemy 3.* |
| **Враги: Explode** | `Assets/Resources/Enemy/Explode/Animation/` | unit_04_bomb.* |
| **Враги: Boss / BossSimple** | `Assets/Resources/Enemy/Boss*/Animation/` | то же |
| **Шахта** | `Assets/Resources/Map/Mine/Animation/` | mine.* |
| **Сундук (малый)** | в Chest — проверь префаб | — |
| **Большой сундук** | `Assets/Resources/Map/BigChest/Animation/` | Treasure_BigChest.* |

**Как заменить (Spine):**
- В Spine Editor нарисовать/импортировать новый скелет в **новом стиле**, сохранить имена костей/слотов и анимаций, если не хочешь менять код.
- Экспорт: **Binary (.skel)** или JSON + **Atlas + PNG**.
- В Unity: заменить в папках `Animation/` файлы `.png`, `.atlas.txt`, `.skel.bytes` и обновить ссылки в **SkeletonData** ассете (или пересоздать SkeletonDataAsset из новых файлов).
- Материалы (`.mat`) можно оставить или заменить шейдер под новый стиль (например, outline, toon).

---

### 2. **Спрайты UI и карты (атласы)**

Источники картинок: **`Assets/Images/`**.  
Из них редакторский скрипт собирает **Sprite Atlases** в `Assets/Resources/Atlases/`.

| Атлас | Папка с исходниками | Назначение |
|-------|---------------------|------------|
| **Ui** | `Assets/Images/Ui/` | Кнопки, фоны окон, прогресс-бары, иконки HUD, окна |
| **Map** | `Assets/Images/Map/` | Террайн, вода, путь, шахта, сундуки, башня |

**Как заменить:**
- Подставить в `Assets/Images/Ui/` и `Assets/Images/Map/` новые PNG **с теми же именами файлов** (и по возможности размерами/разрезами), тогда префабы и код продолжат находить спрайты по имени.
- Либо создать новые спрайты с новыми именами и в префабах/окнах вручную перепривязать `Sprite` поля.
- После смены картинок: **Tools → Sprite Atlas → Build Atlases**, чтобы пересобрать атласы.

---

### 3. **Террайн и вода (Map Palette)**

Префабы тайлов/воды:  
`Assets/Resources/Map/MapPalette/`  
- Terrain_1 … Terrain_4  
- Water  

Они ссылаются на спрайты из **Map** атласа (или на спрайты из `Assets/Images/Map/` до сборки атласа). Замена визуала — подмена спрайтов/материалов в этих префабах или смена исходников в `Assets/Images/Map/` (в т.ч. Terrain, TerrainZone1/2, Water).

---

### 4. **Визуальные эффекты (VFX)**

- **Смерть врагов:** `Assets/Resources/Enemy/Fx/Death/` — префабы + материалы в `Materials/`.
- **Вампирский укус:** `Assets/Resources/Enemy/Fx/Vampire Bite.prefab`.
- **Игрок:** `Assets/Resources/Player/FX/` (Death, Defend).
- **Карта:** `Assets/Resources/Map/Fx/` (CallToAction_plus, CallToAction_ring).
- **Сундук:** `Assets/Resources/Map/Chest/Fx/`, `Assets/Resources/Map/BigChest/`.

Замена: подмена текстур/партиклов в префабах и материалов в указанных папках.

---

### 5. **Иконки и контент (карты, валюта, способности)**

- Иконки карт, валют и т.д. задаются в **ScriptableObject** (конфиги в `Resources/Config/` или аналог) полями типа `Sprite` / `Texture`. Нужно в конфигах подставить новые спрайты.
- Часть кнопок (например, в `EndRunWindow`, `MapChestWindow`, `RateUsWindow`) получает спрайты через `[SerializeField] private Sprite ...` — они задаются в инспекторе префаба окна; при смене стиля их нужно заменить в префабах.

---

## Пошаговый план замены визуала «то же по структуре, другой стиль»

1. **Определиться со стилем**  
   Например: низкополи, пиксель-арт, плоский 2D, мультяшный контур и т.д. От этого зависят разрешения и тип атласов/Spine.

2. **Spine**  
   - Перерисовать/заменить скелеты и атласы для: игрока (если будет Spine), всех типов врагов, шахты, большого сундука.  
   - Сохранить имена анимаций и слотов, если не планируешь менять код.  
   - Подставить новые `.png`, `.atlas.txt`, `.skel.bytes` и при необходимости обновить SkeletonData ассеты.

3. **Спрайты UI и карты**  
   - Заменить PNG в `Assets/Images/Ui/` и `Assets/Images/Map/` (желательно те же имена).  
   - Выполнить **Tools → Sprite Atlas → Build Atlases**.

4. **Map Palette**  
   - Обновить префабы Terrain_1–4 и Water под новые спрайты/материалы.

5. **Эффекты**  
   - Обновить текстуры/материалы в префабах эффектов (смерть, защита, сундуки, CallToAction и т.д.).

6. **Конфиги и окна**  
   - Проставить новые иконки в ScriptableObject и в полях `Sprite` в префабах окон.

7. **Шрифты и цветовая схема**  
   - При смене стиля часто меняют шрифты (TextMeshPro) и общую палитру (например, через пресеты UI или пост-обработку).

Если напишешь целевой стиль (пиксель-арт, мультяшный, минималистичный и т.д.), можно расписать под него конкретные разрешения, настройки атласов и Spine.

---

## Пиксель-арт визуал (готовый набор)

Сгенерирован набор пиксель-арта в едином стиле (тёплые тона, чёткие пиксели, фэнтези-лучник).

- **Папка:** `Assets/Images_PixelArt/` — сюда копируются сгенерированные PNG (см. README внутри).
- **Импорт в проект:** **Tools → Pixel Art → Import from folder...** — указать папку, где лежат сгенерированные PNG (например папка Cursor `assets`). Скрипт разложит файлы по Ui/Map и т.д.
- **Включение в игре:** **Tools → Pixel Art → Use Pixel Art in project (replace Images)** — копирование из Images_PixelArt в Images с последующей сборкой атласов (**Tools → Sprite Atlas → Build Atlases**).

Список сгенерированных файлов (имена совпадают с оригинальными в Images):
- UI: `window_bg.png`, `circle_back.png`, `stats_hp_progress_back.png`, `stats_hp_progress_fill.png`, `wave_progress_fill.png`, `chest_icon.png`, `plus.png`, и др. (см. маппинг в `Assets/Scripts/Editor/PixelArtImport.cs`).
- Map: `Grass.png`, `Rock.png`, `Chest.png`, `red_crystal.png`, `green_crystal.png`, и др.

Spine-атласы (враги, шахта, большой сундук) нужно готовить в Spine Editor из пиксель-арт спрайтов или заменить текстуры в существующих атласах, сохраняя разметку.

---

## Изменение цветовой гаммы существующих картинок

Если нужно **не создавать новые изображения**, а **изменить цвета у уже лежащих в проекте** PNG:

1. Установить зависимости: из папки проекта выполнить  
   `pip install -r scripts/requirements.txt`
2. Указать файлы и параметры, запустить скрипт — он **перезапишет** указанные файлы.

**Примеры (из корня проекта):**

```bash
# Пресеты: warm, cool, darker, brighter, pastel, vivid
python scripts/change_image_colors.py --files Assets/Images/Ui/window_bg.png Assets/Images/Map/TerrainZone2/Grass.png --preset warm

# Ручные параметры: сдвиг оттенка (0..1), насыщенность, яркость
python scripts/change_image_colors.py --files Assets/Images/Ui/chest_icon.png --hue 0.1 --saturation 1.2 --brightness 1.0

# Список файлов из текстового файла (по одному пути на строку)
python scripts/change_image_colors.py --list scripts/images_to_recolor.txt --preset cool
```

Скрипт: `scripts/change_image_colors.py`. Пути можно указывать относительно корня проекта.
