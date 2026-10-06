from plow_art import shapes

NAME = "SteamPuff"
OUTPUT = "client/Assets/_Project/Art/VFX/Steam/SM_SteamPuff.fbx"
ANCHORED = True


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    puff = shapes.ball("Puff", 0.5, (0, 0, 0), collection, root)
    shapes.paint(puff, "white")
    return root
