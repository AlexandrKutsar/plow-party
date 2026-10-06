from plow_art import shapes

NAME = "WoodPile"
OUTPUT = "client/Assets/_Project/Art/Environment/WoodPile/SM_WoodPile.fbx"

LOG_RADIUS = 0.13
LOG_LENGTH = 1.0
ROWS = (7, 6, 5)
POST = (0.09, 0.09, 0.85)


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    step = LOG_RADIUS * 2
    rise = LOG_RADIUS * 1.75
    for row, count in enumerate(ROWS):
        z = LOG_RADIUS + row * rise
        for index in range(count):
            x = (index - (count - 1) / 2) * step
            log = shapes.cylinder(f"Log{row}_{index}", LOG_RADIUS, LOG_LENGTH, (x, 0, z), collection, root, axis="Y", segments=7, cap_inset=LOG_RADIUS * 0.3)
            shapes.paint(log, "brown")
            shapes.paint(log, "tan", shapes.on_axis(1))
    half_width = ROWS[0] * LOG_RADIUS + POST[0] / 2
    for side, sign in (("L", 1), ("R", -1)):
        post = shapes.box(f"Post{side}", POST, (sign * half_width, 0, POST[2] / 2), collection, root, chamfer=0.015)
        shapes.paint(post, "seat")
    top = LOG_RADIUS + (len(ROWS) - 1) * rise + LOG_RADIUS
    snow = shapes.box("Snow", (ROWS[-1] * step + 0.06, LOG_LENGTH + 0.04, 0.09), (0, 0, top - 0.02), collection, root, chamfer=0.035)
    shapes.paint(snow, "snow")
    return root
