from plow_art import imported, shapes

NAME = "Loot"
OUTPUT = "client/Assets/_Project/Art/Props/Loot/SM_Loot.fbx"

COLORS = {"*": ["red", "barn_red", "green", "pine_light", "white", "yellow"]}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "creativetrio/Present.glb", collection, root, COLORS, size=0.7)
    return root
