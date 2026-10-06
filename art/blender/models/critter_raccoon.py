from plow_art import critter, shapes

NAME = "Critter_Raccoon"
OUTPUT = critter.output("Raccoon")
ANCHORED = True

FUR = "gray"
DARK = "tire"
LIGHT = "white"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    critter.body(collection, root, FUR, "metal_light")
    critter.head(collection, root, FUR)
    critter.pointed_ears(collection, root, FUR, DARK, height=0.12, radius=0.085)
    mask = shapes.box("Mask", (0.42, 0.03, 0.09), (0, critter.HEAD_FRONT_Y + 0.005, critter.EYE_Z), collection, root, chamfer=0.012)
    shapes.paint(mask, DARK)
    critter.muzzle(collection, root, LIGHT, size=(0.12, 0.08, 0.08), z=0.37)
    critter.eyes(collection, root, LIGHT, size=(0.04, 0.03, 0.04))
    critter.arms(collection, root, FUR, DARK)
    critter.striped_tail(collection, root, (FUR, DARK, FUR, DARK, FUR, DARK), (0, 0.13, 0.2), (0, 0.5, 0.38), 0.07)
    return root
