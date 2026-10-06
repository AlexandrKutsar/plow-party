import math

from mathutils import Vector

from plow_art import shapes

NAME = "Vehicle"
OUTPUT = "client/Assets/_Project/Art/Vehicles/Vehicle/SM_Vehicle.fbx"

BODY_COLOR = "red"

FRONT = Vector((0, -1, 0))
UP = Vector((0, 0, 1))
LEFT = Vector((1, 0, 0))
RIGHT = Vector((-1, 0, 0))

REAR_WHEEL = {"radius": 0.3, "width": 0.22, "x": 0.4, "y": 0.28}
FRONT_WHEEL = {"radius": 0.17, "width": 0.14, "x": 0.3, "y": -0.4}

HOOD_FRONT_Y = -0.56
BUCKET_HINGE = (0, -0.68, 0.32)
BUCKET_WIDTH = 1.0
BUCKET_SCALE = 0.85
BUCKET_PROFILE = [
    (-0.34, -0.28), (-0.12, -0.30), (0.02, -0.18), (0.06, 0.02), (0.02, 0.22), (-0.08, 0.32),
    (-0.10, 0.27), (-0.04, 0.20), (0.00, 0.02), (-0.04, -0.15), (-0.14, -0.24), (-0.34, -0.22),
]

SEAT_TOP = (0, 0.3, 0.68)


def build(collection):
    root = shapes.empty(NAME, (0, 0, 0), collection)
    _build_hood(collection, root)
    _build_rear(collection, root)
    _build_wheels(collection, root)
    _build_bucket(collection, root)
    shapes.empty("CritterSeat", SEAT_TOP, collection, root)
    return root


def _build_hood(collection, root):
    hood = shapes.box("Hood", (0.46, 0.62, 0.34), (0, -0.25, 0.5), collection, root, chamfer=0.06)
    shapes.paint(hood, BODY_COLOR)
    shapes.paint(hood, "tire", shapes.facing(LEFT, 0.9))
    shapes.paint(hood, "tire", shapes.facing(RIGHT, 0.9))

    grille_frame = shapes.box("GrilleFrame", (0.42, 0.05, 0.32), (0, HOOD_FRONT_Y, 0.5), collection, root, chamfer=0.02)
    shapes.paint(grille_frame, "metal_light")
    grille = shapes.box("Grille", (0.32, 0.06, 0.24), (0, HOOD_FRONT_Y - 0.01, 0.5), collection, root)
    shapes.paint(grille, "black")

    for side, x in (("L", 0.25), ("R", -0.25)):
        lamp = shapes.cylinder(f"Headlight{side}", 0.055, 0.06, (x, HOOD_FRONT_Y + 0.04, 0.6), collection, root, axis="Y", segments=8)
        shapes.paint(lamp, "metal_light")
        shapes.paint(lamp, "white", shapes.facing(FRONT, 0.9))

    for name, x, y, height in (("ExhaustTall", 0.12, -0.38, 0.3), ("ExhaustShort", -0.1, -0.44, 0.18)):
        pipe = shapes.cylinder(name, 0.04, height, (x, y, 0.67 + height / 2), collection, root, axis="Z", segments=6)
        shapes.paint(pipe, "black")

    for side, x in (("L", 0.2), ("R", -0.2)):
        arm = shapes.box(f"BucketArm{side}", (0.06, 0.12, 0.06), (x, -0.58, 0.32), collection, root)
        shapes.paint(arm, "metal")

    axle = shapes.box("FrontAxle", (0.5, 0.08, 0.08), (0, FRONT_WHEEL["y"], FRONT_WHEEL["radius"]), collection, root)
    shapes.paint(axle, "tire")


def _build_rear(collection, root):
    body = shapes.box("RearBody", (0.44, 0.5, 0.3), (0, 0.22, 0.45), collection, root, chamfer=0.05)
    shapes.paint(body, BODY_COLOR)

    for side, x in (("L", REAR_WHEEL["x"]), ("R", -REAR_WHEEL["x"])):
        fender = shapes.extruded_profile(
            f"Fender{side}", _arc(0.33, 0.37, 10, 170, 8), REAR_WHEEL["width"] + 0.06,
            (x, REAR_WHEEL["y"], REAR_WHEEL["radius"]), collection, root)
        shapes.paint(fender, BODY_COLOR)

    cushion = shapes.box("SeatCushion", (0.3, 0.26, 0.08), (0, 0.3, 0.64), collection, root, chamfer=0.03)
    shapes.paint(cushion, "seat")
    backrest = shapes.box("SeatBack", (0.3, 0.06, 0.2), (0, 0.45, 0.76), collection, root, chamfer=0.025)
    shapes.paint(backrest, "seat")

    column = shapes.cylinder("SteeringColumn", 0.02, 0.3, (0, 0.04, 0.72), collection, root, axis="Z", segments=6)
    column.rotation_euler = (math.radians(-35), 0, 0)
    shapes.paint(column, "black")
    wheel = shapes.cylinder("SteeringWheel", 0.1, 0.025, (0, 0.12, 0.85), collection, root, axis="Z", segments=10)
    wheel.rotation_euler = (math.radians(-35), 0, 0)
    shapes.paint(wheel, "black")


def _build_wheels(collection, root):
    for spec, prefix, segments in ((REAR_WHEEL, "WheelR", 12), (FRONT_WHEEL, "WheelF", 10)):
        for side, sign in (("L", 1), ("R", -1)):
            wheel = shapes.cylinder(
                f"{prefix}{side}", spec["radius"], spec["width"], (sign * spec["x"], spec["y"], spec["radius"]),
                collection, root, segments=segments, chamfer=spec["radius"] * 0.12, cap_inset=spec["radius"] * 0.3)
            shapes.paint(wheel, "tire")
            shapes.paint(wheel, "yellow", shapes.on_axis(0))


def _build_bucket(collection, root):
    profile = [(y * BUCKET_SCALE, z * BUCKET_SCALE) for y, z in BUCKET_PROFILE]
    bucket = shapes.extruded_profile("Bucket", profile, BUCKET_WIDTH, BUCKET_HINGE, collection, root)
    shapes.paint(bucket, "yellow")
    shapes.paint(bucket, "metal", lambda polygon: polygon.center.z < -0.22 and polygon.center.y < -0.15)


def _arc(inner_radius, outer_radius, start_degrees, end_degrees, segments):
    angles = [math.radians(start_degrees + (end_degrees - start_degrees) * i / segments) for i in range(segments + 1)]
    outer = [(-math.cos(a) * outer_radius, math.sin(a) * outer_radius) for a in angles]
    inner = [(-math.cos(a) * inner_radius, math.sin(a) * inner_radius) for a in reversed(angles)]
    return outer + inner
