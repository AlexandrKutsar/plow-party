from plow_art import critter, shapes

NAME = "Critter_Bear"
OUTPUT = critter.output("Bear")
KEEP_ORIGIN = True

FUR = "brown"
LIGHT = "tan"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    critter.body(collection, root, FUR, LIGHT)
    critter.head(collection, root, FUR)
    critter.round_ears(collection, root, FUR, LIGHT)
    critter.muzzle(collection, root, LIGHT, size=(0.16, 0.09, 0.1), z=0.38)
    critter.eyes(collection, root)
    critter.arms(collection, root, FUR, radius=0.045)
    tail = shapes.ball("Tail", 0.05, (0, 0.13, 0.07), collection, root)
    shapes.paint(tail, FUR)
    return root
