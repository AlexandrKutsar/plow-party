import math

from plow_art import imported, shapes

NAME = "DropOffZone"
OUTPUT = "client/Assets/_Project/Art/Environment/DropOffZone/SM_DropOffZone.fbx"

FIRE_COLORS = {"Wood": "seat", "WoodSide": "wood", "Stone_Dark": "gray", "Stone_Light": "metal"}
CAULDRON_COLORS = {"DarkMetal": "tire", "Stone": "gray", "Soup": "glass", "Orange": "white"}
FIRE_SIZE = 3.4
CAULDRON_SIZE = 2.4
FLAMES = ((0.0, 0.0, 0.32, 0.7), (0.35, 0.2, 0.22, 0.5), (-0.3, 0.25, 0.22, 0.45), (0.1, -0.35, 0.2, 0.4))


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Bonfire", "quaternius/Bonfire.glb", collection, root, FIRE_COLORS, size=FIRE_SIZE)
    for index, (x, y, radius, height) in enumerate(FLAMES):
        flame = shapes.cone(f"Flame{index}", radius, 0.0, height, (x, y, 0.15), collection, root, segments=5)
        flame.rotation_euler = (0, 0, math.radians(index * 37))
        shapes.paint(flame, "fire")
        core = shapes.cone(f"FlameCore{index}", radius * 0.55, 0.0, height * 0.6, (x, y, 0.15), collection, root, segments=5)
        shapes.paint(core, "yellow")
    imported.load("Cauldron", "quaternius/Cauldron.glb", collection, root, CAULDRON_COLORS, size=CAULDRON_SIZE)
    shapes.empty("SteamOrigin", (0, 0, 1.6), collection, root)
    return root
