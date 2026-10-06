from plow_art import shapes

NAME = "Pine"
OUTPUT = "client/Assets/_Project/Art/Environment/Pine/SM_Pine.fbx"

TIERS = ((1.15, 0.5, 1.1), (0.9, 1.25, 1.0), (0.66, 1.95, 0.95))
TOP_SHARE = 0.32
SNOW_START = 0.55


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    trunk = shapes.cylinder("Trunk", 0.16, 0.6, (0, 0, 0.3), collection, root, axis="Z", segments=6)
    shapes.paint(trunk, "seat")
    last = len(TIERS) - 1
    for index, (radius, base, height) in enumerate(TIERS):
        top_radius = 0.0 if index == last else radius * TOP_SHARE
        needles = shapes.cone(f"Tier{index}", radius, top_radius, height, (0, 0, base), collection, root, segments=8)
        shapes.paint(needles, "pine" if index % 2 == 0 else "pine_light")
        snow_radius = radius + (top_radius - radius) * SNOW_START
        snow = shapes.cone(f"Snow{index}", snow_radius + 0.04, top_radius * 1.05, height * (1 - SNOW_START) + 0.03, (0, 0, base + height * SNOW_START), collection, root, segments=8)
        shapes.paint(snow, "snow")
    return root
