import math

from plow_art import critter, shapes

NAME = "Critter_Fox"
OUTPUT = critter.output("Fox")
KEEP_ORIGIN = True

FUR = "orange"
LIGHT = "white"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    critter.body(collection, root, FUR, LIGHT)
    critter.head(collection, root, FUR)
    critter.pointed_ears(collection, root, FUR, "black")
    critter.muzzle(collection, root, LIGHT, size=(0.13, 0.12, 0.09), z=0.37)
    critter.eyes(collection, root)
    critter.arms(collection, root, FUR, "black")
    tail = shapes.ball("Tail", 0.12, (0, 0.28, 0.26), collection, root, scale=(1.0, 1.9, 1.0))
    tail.rotation_euler = (math.radians(35), 0, 0)
    shapes.paint(tail, FUR)
    tip = shapes.ball("TailTip", 0.085, (0, 0.44, 0.38), collection, root, scale=(1, 1.2, 1))
    tip.rotation_euler = tail.rotation_euler
    shapes.paint(tip, LIGHT)
    return root
