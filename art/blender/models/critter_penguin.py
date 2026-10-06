import math

from plow_art import critter, shapes

NAME = "Critter_Penguin"
OUTPUT = critter.output("Penguin")
KEEP_ORIGIN = True

DARK = "tire"
LIGHT = "white"
BEAK = "yellow"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    critter.body(collection, root, DARK, LIGHT, BEAK)
    critter.head(collection, root, DARK)
    face = shapes.box("Face", (0.3, 0.03, 0.18), (0, critter.HEAD_FRONT_Y + 0.005, 0.45), collection, root, chamfer=0.012)
    shapes.paint(face, LIGHT)
    critter.eyes(collection, root, size=(0.04, 0.03, 0.05))
    beak = shapes.cone("Beak", 0.06, 0.0, 0.12, (0, critter.HEAD_FRONT_Y, 0.4), collection, root, segments=4)
    beak.rotation_euler = (math.radians(90), 0, math.radians(45))
    shapes.paint(beak, BEAK)
    critter.arms(collection, root, DARK, radius=0.035)
    return root
