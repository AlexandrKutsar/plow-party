from mathutils import Vector

from plow_art import shapes

NAME = "HayBale"
OUTPUT = "client/Assets/_Project/Art/Environment/HayBale/SM_HayBale.fbx"

RADIUS = 0.6
WIDTH = 1.2


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    bale = shapes.cylinder("Bale", RADIUS, WIDTH, (0, 0, RADIUS), collection, root, axis="X", segments=12, chamfer=0.05, cap_inset=RADIUS * 0.35)
    shapes.paint(bale, "hay")
    shapes.paint(bale, "hay_dark", shapes.facing(Vector((1, 0, 0)), 0.9))
    shapes.paint(bale, "hay_dark", shapes.facing(Vector((-1, 0, 0)), 0.9))
    shapes.paint(bale, "hay", shapes.on_axis(0))
    snow = shapes.ball("Snow", 0.5, (0, 0, RADIUS * 2 - 0.04), collection, root, scale=(1.15, 0.9, 0.22))
    shapes.paint(snow, "snow")
    return root
