from models.barn import COLORS
from plow_art import imported, shapes

NAME = "Shed"
OUTPUT = "client/Assets/_Project/Art/Environment/Shed/SM_Shed.fbx"


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "quaternius/SmallBarn.glb", collection, root, COLORS, size=5.0)
    return root
