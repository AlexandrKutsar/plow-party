from plow_art import shapes

NAME = "Drift"
OUTPUT = "client/Assets/_Project/Art/Environment/Drift/SM_Drift.fbx"

RADIUS = 1.2
TUFTS = ((0.95, 0.5), (-1.0, 0.3), (0.25, -1.05), (-0.6, -0.85), (0.5, 0.95))


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    stack = shapes.ball("Stack", RADIUS, (0, 0, RADIUS * 0.35), collection, root, scale=(1, 1, 0.75), subdivisions=2)
    shapes.paint(stack, "hay")
    snow = shapes.ball("Snow", RADIUS * 1.02, (0, 0, RADIUS * 0.55), collection, root, scale=(1, 1, 0.68), subdivisions=2)
    shapes.paint(snow, "snow")
    for index, (x, y) in enumerate(TUFTS):
        tuft = shapes.cone(f"Tuft{index}", 0.16, 0.0, 0.32, (x, y, 0.0), collection, root, segments=4)
        tuft.rotation_euler = (-y * 0.7, x * 0.7, 0)
        shapes.paint(tuft, "hay_dark")
    return root
