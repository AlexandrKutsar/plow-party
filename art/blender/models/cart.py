import math

from plow_art import shapes

NAME = "Cart"
OUTPUT = "client/Assets/_Project/Art/Environment/Cart/SM_Cart.fbx"

BED = (1.1, 1.7, 0.12)
BED_Z = 0.62
WALL = 0.07
WALL_HEIGHT = 0.3
WHEEL = {"radius": 0.45, "width": 0.1, "x": 0.66, "y": 0.15}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    floor = shapes.box("Bed", BED, (0, 0, BED_Z), collection, root, chamfer=0.02)
    shapes.paint(floor, "wood")
    wall_z = BED_Z + WALL_HEIGHT / 2
    for name, size, location in (
        ("WallL", (WALL, BED[1], WALL_HEIGHT), (BED[0] / 2 - WALL / 2, 0, wall_z)),
        ("WallR", (WALL, BED[1], WALL_HEIGHT), (-BED[0] / 2 + WALL / 2, 0, wall_z)),
        ("WallFront", (BED[0], WALL, WALL_HEIGHT), (0, -BED[1] / 2 + WALL / 2, wall_z)),
        ("WallBack", (BED[0], WALL, WALL_HEIGHT), (0, BED[1] / 2 - WALL / 2, wall_z)),
    ):
        wall = shapes.box(name, size, location, collection, root, chamfer=0.015)
        shapes.paint(wall, "wood")
    load = shapes.box("Hay", (BED[0] - 0.2, BED[1] - 0.2, 0.32), (0, 0, BED_Z + 0.2), collection, root, chamfer=0.08)
    shapes.paint(load, "hay")
    snow = shapes.box("Snow", (BED[0] - 0.24, BED[1] - 0.3, 0.1), (0, 0.02, BED_Z + 0.38), collection, root, chamfer=0.04)
    shapes.paint(snow, "snow")
    axle = shapes.box("Axle", (WHEEL["x"] * 2, 0.08, 0.08), (0, WHEEL["y"], WHEEL["radius"]), collection, root)
    shapes.paint(axle, "seat")
    for side, sign in (("L", 1), ("R", -1)):
        wheel = shapes.cylinder(f"Wheel{side}", WHEEL["radius"], WHEEL["width"], (sign * WHEEL["x"], WHEEL["y"], WHEEL["radius"]), collection, root, segments=10, cap_inset=WHEEL["radius"] * 0.7)
        shapes.paint(wheel, "seat")
        shapes.paint(wheel, "metal", shapes.on_axis(0))
        shaft = shapes.box(f"Shaft{side}", (0.07, 1.3, 0.07), (sign * 0.32, -BED[1] / 2 - 0.55, BED_Z - 0.12), collection, root)
        shaft.rotation_euler = (math.radians(12), 0, 0)
        shapes.paint(shaft, "wood")
    prop = shapes.box("Stand", (0.07, 0.07, 0.42), (0, -BED[1] / 2 - 0.15, 0.21), collection, root)
    shapes.paint(prop, "seat")
    return root
