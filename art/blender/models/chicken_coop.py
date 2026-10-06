from plow_art import imported, shapes

NAME = "ChickenCoop"
OUTPUT = "client/Assets/_Project/Art/Environment/ChickenCoop/SM_ChickenCoop.fbx"

COLORS = {"DarkRed": "barn_red_dark", "LightRed": "barn_red", "White": "white", "RoofBlack": "roof", "Brown": "wood"}


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    imported.load("Body", "quaternius/ChickenCoop.glb", collection, root, COLORS, size=3.0)
    return root
