from plow_art import shapes

NAME = "Snowball"
OUTPUT = "client/Assets/_Project/Art/Props/Snowball/SM_Snowball.fbx"
KEEP_ORIGIN = True

RADIUS = 0.18


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    ball = shapes.ball("Ball", RADIUS, (0, 0, 0), collection, root, subdivisions=2)
    shapes.paint(ball, "white")
    shapes.paint(ball, "metal_light", lambda polygon: polygon.center.z < -RADIUS * 0.45)
    return root
