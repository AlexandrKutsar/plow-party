from plow_art import imported, shapes

NAME = "Fence"
OUTPUT = "client/Assets/_Project/Art/Environment/Fence/SM_Fence.fbx"

COLORS = {"Wood": "wood"}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "quaternius/Fence.glb", collection, root, COLORS, size=2.2)
    return root
