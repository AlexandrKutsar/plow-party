from plow_art import imported, shapes

NAME = "Well"
OUTPUT = "client/Assets/_Project/Art/Environment/Well/SM_Well.fbx"

COLORS = {"Wood": "wood", "Bag": "tan", "RoofTiles_Red": "barn_red_dark", "Stone_Dark": "gray", "Stone_Light": "metal"}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "quaternius/Well.glb", collection, root, COLORS, size=1.6)
    return root
