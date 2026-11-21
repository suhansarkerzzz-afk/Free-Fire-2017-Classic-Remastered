extends RefCounted
## Lightweight original outfit palettes for the Godot player prototype.
## These are procedural color variants, not imported or official game skins.

static func get_skin_count() -> int:
    return 4

static func get_skin_name(index: int) -> String:
    return str(get_skin(index).get("display_name", "UNKNOWN"))

static func get_skin(index: int) -> Dictionary:
    match clampi(index, 0, get_skin_count() - 1):
        0:
            return {
                "id": "classic_blue",
                "display_name": "CLASSIC BLUE",
                "torso": Color(0.12, 0.38, 0.72),
                "arms": Color(0.12, 0.28, 0.55),
                "legs": Color(0.055, 0.065, 0.08),
                "accent": Color(0.90, 0.78, 0.22)
            }
        1:
            return {
                "id": "desert_ranger",
                "display_name": "DESERT RANGER",
                "torso": Color(0.58, 0.38, 0.19),
                "arms": Color(0.72, 0.56, 0.32),
                "legs": Color(0.24, 0.19, 0.13),
                "accent": Color(0.86, 0.72, 0.42)
            }
        2:
            return {
                "id": "forest_scout",
                "display_name": "FOREST SCOUT",
                "torso": Color(0.16, 0.34, 0.22),
                "arms": Color(0.22, 0.30, 0.19),
                "legs": Color(0.09, 0.14, 0.11),
                "accent": Color(0.70, 0.75, 0.52)
            }
        _:
            return {
                "id": "crimson_strike",
                "display_name": "CRIMSON STRIKE",
                "torso": Color(0.52, 0.10, 0.12),
                "arms": Color(0.26, 0.06, 0.08),
                "legs": Color(0.08, 0.08, 0.10),
                "accent": Color(0.78, 0.76, 0.70)
            }
