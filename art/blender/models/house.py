from plow_art import imported, shapes

NAME = "House"
OUTPUT = "client/Assets/_Project/Art/Environment/House/SM_House.fbx"

COLORS = {"*": ["seat", "brown", "wood", "tan", "pine", "pine_light", "white", "glass", "black", "gray", "roof"]}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "creativetrio/CabinShed.glb", collection, root, COLORS, size=5.0)
    return root
