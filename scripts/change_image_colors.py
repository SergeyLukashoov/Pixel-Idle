#!/usr/bin/env python3
"""
Скрипт для изменения цветовой гаммы существующих PNG в проекте.
Перезаписывает указанные файлы (не создаёт новые).

Использование:
  python change_image_colors.py --files path/to/img1.png path/to/img2.png --hue 20 --saturation 1.1 --brightness 1.0
  python change_image_colors.py --files path/to/img.png --preset warm
  python change_image_colors.py --list list.txt --preset cool   # пути в list.txt по одному на строку

Пресеты: warm, cool, darker, brighter, pastel, vivid
"""

import argparse
import colorsys
from pathlib import Path

try:
    from PIL import Image, ImageEnhance
except ImportError:
    print("Установите Pillow: pip install Pillow")
    raise


def rgb_to_hsv_tuple(r, g, b):
    r, g, b = r / 255.0, g / 255.0, b / 255.0
    h, s, v = colorsys.rgb_to_hsv(r, g, b)
    return (h, s, v)


def hsv_to_rgb_tuple(h, s, v):
    r, g, b = colorsys.hsv_to_rgb(h, s, v)
    return (int(round(r * 255)), int(round(g * 255)), int(round(b * 255)))


def apply_hue_shift(pixels, shift_float):
    """shift_float: 0..1 (например 0.1 = сдвиг на ~36°)"""
    out = []
    for r, g, b, a in pixels:
        if a == 0:
            out.append((r, g, b, a))
            continue
        h, s, v = rgb_to_hsv_tuple(r, g, b)
        h = (h + shift_float) % 1.0
        r, g, b = hsv_to_rgb_tuple(h, s, v)
        out.append((r, g, b, a))
    return out


def process_image(path: Path, hue_shift: float = 0, saturation: float = 1.0, brightness: float = 1.0) -> None:
    """Изменяет цветовую гамму изображения и сохраняет в тот же файл."""
    path = path.resolve()
    if not path.exists():
        print(f"Пропуск (нет файла): {path}")
        return
    if path.suffix.lower() not in (".png", ".jpg", ".jpeg"):
        print(f"Пропуск (не изображение): {path}")
        return

    img = Image.open(path)
    img = img.convert("RGBA")
    w, h = img.size
    pixels = list(img.getdata())

    if hue_shift != 0:
        pixels = apply_hue_shift(pixels, hue_shift)
        img.putdata(pixels)

    if saturation != 1.0:
        img = ImageEnhance.Color(img).enhance(saturation)
    if brightness != 1.0:
        img = ImageEnhance.Brightness(img).enhance(brightness)

    # Сохраняем в тот же файл
    save_kw = {}
    if path.suffix.lower() == ".png":
        save_kw["compress_level"] = 6
    img.save(path, **save_kw)
    print(f"Обновлено: {path}")


# Пресеты: hue 0=красный, ~0.08=оранжевый, ~0.33=зелёный, ~0.67=синий. Сдвиг +0.33: фиолетовое→оранжевое.
PRESETS = {
    "orange": {"hue_shift": 0.33,  "saturation": 1.15, "brightness": 1.05},   # оранжевая гамма
    "green":  {"hue_shift": 0.33,   "saturation": 1.1,  "brightness": 1.0},   # в зелёную (тёмно-зелёная гамма)
    "warm":   {"hue_shift": 0.02,  "saturation": 1.15, "brightness": 1.05},
    "cool":   {"hue_shift": 0.55,  "saturation": 1.1,  "brightness": 1.0},
    "darker": {"hue_shift": 0,     "saturation": 1.0,  "brightness": 0.85},
    "brighter": {"hue_shift": 0,   "saturation": 1.0,  "brightness": 1.15},
    "pastel": {"hue_shift": 0,     "saturation": 0.7,  "brightness": 1.1},
    "vivid":  {"hue_shift": 0,     "saturation": 1.3,  "brightness": 1.05},
}


def main():
    ap = argparse.ArgumentParser(description="Изменение цветовой гаммы PNG (файлы перезаписываются)")
    ap.add_argument("--files", nargs="+", help="Пути к PNG файлам")
    ap.add_argument("--list", type=str, help="Файл со списком путей (по одному на строку)")
    ap.add_argument("--folder", type=str, help="Папка: обработать все PNG внутри рекурсивно (например Assets/Images/Ui)")
    ap.add_argument("--exclude", type=str, action="append", default=[], metavar="PART", help="Исключить пути, содержащие PART (можно указать несколько раз)")
    ap.add_argument("--hue", type=float, default=0, help="Сдвиг оттенка 0..1 (например 0.1 ≈ 36°)")
    ap.add_argument("--saturation", type=float, default=1.0, help="Насыщенность (1.0 = без изменений)")
    ap.add_argument("--brightness", type=float, default=1.0, help="Яркость (1.0 = без изменений)")
    ap.add_argument("--preset", choices=list(PRESETS), help="Пресет: orange, green, warm, cool, darker, brighter, pastel, vivid")
    args = ap.parse_args()

    if args.preset:
        p = PRESETS[args.preset]
        hue_shift, saturation, brightness = p["hue_shift"], p["saturation"], p["brightness"]
    else:
        hue_shift, saturation, brightness = args.hue, args.saturation, args.brightness

    root = Path(__file__).resolve().parent.parent
    paths = []
    if args.files:
        paths.extend(Path(f) for f in args.files)
    if args.list:
        list_path = Path(args.list)
        if list_path.exists():
            paths.extend(Path(line.strip()) for line in list_path.read_text(encoding="utf-8").splitlines() if line.strip())
    if args.folder:
        folder = Path(args.folder)
        if not folder.is_absolute():
            folder = root / folder
        if folder.is_dir():
            for f in sorted(folder.rglob("*.png")):
                if not any(ex in str(f) for ex in args.exclude):
                    paths.append(f)
        else:
            print(f"Папка не найдена: {folder}")
            return

    if not paths:
        print("Укажите файлы: --files img1.png img2.png или --list paths.txt или --folder Assets/Images/Ui")
        return

    for p in paths:
        if not p.is_absolute():
            p = root / p
        process_image(p, hue_shift=hue_shift, saturation=saturation, brightness=brightness)


if __name__ == "__main__":
    main()
