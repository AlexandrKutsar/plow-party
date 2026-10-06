from plow_art import critter, shapes

NAME = "Critter_Beaver"
OUTPUT = critter.output("Beaver")
KEEP_ORIGIN = True

FUR = "wood"
DARK = "brown"
LIGHT = "tan"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    critter.body(collection, root, FUR, LIGHT)
    critter.head(collection, root, FUR)
    critter.round_ears(collection, root, DARK, "seat", radius=0.05)
    snout = critter.muzzle(collection, root, LIGHT, size=(0.17, 0.09, 0.09), z=0.38)
    teeth = shapes.box("Teeth", (0.07, 0.02, 0.06), (0, snout.location.y - 0.03, 0.32), collection, root)
    shapes.paint(teeth, "white")
    critter.eyes(collection, root)
    critter.arms(collection, root, FUR, DARK)
    tail = shapes.box("Tail", (0.2, 0.3, 0.04), (0, 0.34, 0.02), collection, root, chamfer=0.015)
    shapes.paint(tail, "seat")
    return root
