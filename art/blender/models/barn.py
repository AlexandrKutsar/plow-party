from plow_art import imported, shapes

NAME = "Barn"
OUTPUT = "client/Assets/_Project/Art/Environment/Barn/SM_Barn.fbx"

COLORS = {"DarkRed": "barn_red_dark", "LightRed": "barn_red", "White": "white", "RoofBlack": "roof", "Brown": "wood"}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "quaternius/BigBarn.glb", collection, root, COLORS, size=7.0)
    return root
