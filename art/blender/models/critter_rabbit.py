from plow_art import critter, shapes

NAME = "Critter_Rabbit"
OUTPUT = critter.output("Rabbit")
ANCHORED = True

FUR = "white"
ACCENT = "pink"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    critter.body(collection, root, FUR)
    critter.head(collection, root, FUR)
    critter.long_ears(collection, root, FUR, ACCENT, tilt=55)
    critter.muzzle(collection, root, FUR, size=(0.12, 0.06, 0.07), z=0.38, nose_color=ACCENT)
    critter.eyes(collection, root)
    critter.arms(collection, root, FUR)
    tail = shapes.ball("Tail", 0.07, (0, 0.16, 0.26), collection, root)
    shapes.paint(tail, FUR)
    return root
