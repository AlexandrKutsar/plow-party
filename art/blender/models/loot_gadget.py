from mathutils import Vector

from plow_art import imported, shapes

NAME = "Loot_Gadget"
OUTPUT = "client/Assets/_Project/Art/Props/Loot_Gadget/SM_Loot_Gadget.fbx"

COLORS = {"BrightGreen": "blue_dark", "BrightGreen2": "blue", "White": "yellow"}
SIZE = 0.7
TOP_PANEL = 0.24


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    body = imported.load("Body", "quaternius/CubeExclamation.glb", collection, root, COLORS, size=SIZE)
    shapes.paint(body, "yellow", lambda polygon: polygon.normal.dot(Vector((0, 0, 1))) > 0.9 and polygon.center.z > SIZE * 0.9 and abs(polygon.center.x) < TOP_PANEL and abs(polygon.center.y) < TOP_PANEL)
    return root
