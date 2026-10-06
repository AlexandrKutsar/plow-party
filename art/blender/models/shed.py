from plow_art import imported, shapes

NAME = "Shed"
OUTPUT = "client/Assets/_Project/Art/Environment/Shed/SM_Shed.fbx"

COLORS = {"DarkRed": "barn_red_dark", "LightRed": "barn_red", "White": "white", "RoofBlack": "roof", "Brown": "wood"}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "quaternius/SmallBarn.glb", collection, root, COLORS, size=5.0)
    return root
