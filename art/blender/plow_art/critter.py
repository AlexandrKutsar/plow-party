import math

from mathutils import Vector

from . import shapes

FRONT = Vector((0, -1, 0))

BODY_SIZE = (0.26, 0.2, 0.28)
BODY_CENTER = (0, 0.02, 0.15)
HEAD_SIZE = (0.4, 0.32, 0.3)
HEAD_CENTER = (0, -0.01, 0.44)
HEAD_FRONT_Y = HEAD_CENTER[1] - HEAD_SIZE[1] / 2
HEAD_TOP_Z = HEAD_CENTER[2] + HEAD_SIZE[2] / 2
SHOULDER = (0.14, -0.02, 0.24)
HAND = (0.08, -0.17, 0.21)
EYE_Z = 0.47
EYE_X = 0.09


def output(species):
    return f"client/Assets/_Project/Art/Critters/Critter_{species}/SM_Critter_{species}.fbx"


def body(collection, root, color, belly_color=None, foot_color=None):
    torso = shapes.box("Body", BODY_SIZE, BODY_CENTER, collection, root, chamfer=0.06)
    shapes.paint(torso, color)
    if belly_color:
        belly = shapes.box("Belly", (0.16, 0.04, 0.17), (0, BODY_CENTER[1] - 0.085, 0.14), collection, root, chamfer=0.03)
        shapes.paint(belly, belly_color)
    for side, sign in (("L", 1), ("R", -1)):
        thigh = shapes.box(f"Thigh{side}", (0.1, 0.17, 0.09), (sign * 0.07, -0.1, 0.045), collection, root, chamfer=0.03)
        shapes.paint(thigh, color)
        foot = shapes.box(f"Foot{side}", (0.09, 0.08, 0.1), (sign * 0.07, -0.19, -0.02), collection, root, chamfer=0.025)
        shapes.paint(foot, foot_color or color)
    return torso


def head(collection, root, color, size=HEAD_SIZE, center=HEAD_CENTER):
    skull = shapes.box("Head", size, center, collection, root, chamfer=0.07)
    shapes.paint(skull, color)
    return skull


def eyes(collection, root, color="black", x=EYE_X, z=EYE_Z, size=(0.045, 0.03, 0.055)):
    for side, sign in (("L", 1), ("R", -1)):
        eye = shapes.box(f"Eye{side}", size, (sign * x, HEAD_FRONT_Y - 0.005, z), collection, root, chamfer=0.01)
        shapes.paint(eye, color)


def muzzle(collection, root, color, size=(0.14, 0.08, 0.09), z=0.38, nose_color="black"):
    snout = shapes.box("Muzzle", size, (0, HEAD_FRONT_Y - size[1] / 2 + 0.02, z), collection, root, chamfer=0.03)
    shapes.paint(snout, color)
    if nose_color:
        nose = shapes.box("Nose", (0.05, 0.03, 0.04), (0, HEAD_FRONT_Y - size[1] + 0.01, z + size[2] / 2 - 0.02), collection, root, chamfer=0.012)
        shapes.paint(nose, nose_color)
    return snout


def arms(collection, root, color, hand_color=None, radius=0.04):
    for side, sign in (("L", 1), ("R", -1)):
        shoulder = Vector((sign * SHOULDER[0], SHOULDER[1], SHOULDER[2]))
        hand = Vector((sign * HAND[0], HAND[1], HAND[2]))
        limb(f"Arm{side}", shoulder, hand, radius, collection, root, color)
        paw = shapes.ball(f"Hand{side}", radius * 1.25, hand, collection, root)
        shapes.paint(paw, hand_color or color)


def limb(name, start, end, radius, collection, root, color, segments=6):
    direction = end - start
    obj = shapes.cylinder(name, radius, direction.length, (start + end) / 2, collection, root, axis="Z", segments=segments)
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    shapes.paint(obj, color)
    return obj


def pointed_ears(collection, root, color, inner_color, height=0.17, radius=0.085, x=0.13, tilt=18):
    for side, sign in (("L", 1), ("R", -1)):
        ear = shapes.cone(f"Ear{side}", radius, 0.0, height, (sign * x, 0.0, HEAD_TOP_Z - 0.03), collection, root, segments=4)
        ear.rotation_euler = (0, math.radians(sign * tilt), math.radians(45))
        shapes.paint(ear, color)
        inner = shapes.cone(f"EarInner{side}", radius * 0.55, 0.0, height * 0.7, (sign * x, -0.035, HEAD_TOP_Z - 0.01), collection, root, segments=4)
        inner.rotation_euler = (0, math.radians(sign * tilt), math.radians(45))
        shapes.paint(inner, inner_color)


def round_ears(collection, root, color, inner_color, radius=0.075, x=0.16):
    for side, sign in (("L", 1), ("R", -1)):
        location = (sign * x, 0.01, HEAD_TOP_Z - 0.01)
        ear = shapes.cylinder(f"Ear{side}", radius, 0.05, location, collection, root, axis="Y", segments=8)
        shapes.paint(ear, color)
        inner = shapes.cylinder(f"EarInner{side}", radius * 0.55, 0.02, (location[0], -0.02, location[2]), collection, root, axis="Y", segments=8)
        shapes.paint(inner, inner_color)


def long_ears(collection, root, color, inner_color, length=0.34, x=0.085, tilt=12):
    for side, sign in (("L", 1), ("R", -1)):
        location = (sign * x, 0.02, HEAD_TOP_Z - 0.04)
        ear = shapes.box(f"Ear{side}", (0.08, 0.04, length), location, collection, root, offset=(0, 0, length / 2), chamfer=0.02)
        ear.rotation_euler = (math.radians(-tilt), math.radians(sign * 12), 0)
        shapes.paint(ear, color)
        inner = shapes.box(f"EarInner{side}", (0.04, 0.01, length * 0.75), location, collection, root, offset=(0, -0.022, length * 0.5), chamfer=0.0)
        inner.rotation_euler = ear.rotation_euler
        shapes.paint(inner, inner_color)


def striped_tail(collection, root, colors, start, end, radius):
    start = Vector(start)
    step = (Vector(end) - start) / len(colors)
    for index, color in enumerate(colors):
        limb(f"Tail{index}", start + step * index, start + step * (index + 1), radius, collection, root, color, segments=8)
