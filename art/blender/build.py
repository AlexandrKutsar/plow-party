import importlib
import os
import sys

import bpy

BLENDER_DIR = os.path.dirname(os.path.abspath(__file__))
REPO_ROOT = os.path.dirname(os.path.dirname(BLENDER_DIR))
PALETTE_OUTPUT = "client/Assets/_Project/Art/Shared/T_Palette.png"

if BLENDER_DIR not in sys.path:
    sys.path.insert(0, BLENDER_DIR)

from plow_art import export, palette, shapes
from models import vehicle

MODELS = {"vehicle": vehicle}


def reload_modules():
    for module in (palette, shapes, export, *MODELS.values()):
        importlib.reload(module)


def reset_scene():
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj)
    for mesh in list(bpy.data.meshes):
        bpy.data.meshes.remove(mesh)
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)


def build(name, write_files=True):
    model = MODELS[name]
    reset_scene()
    image = palette.build_image()
    palette.build_material(image)
    collection = bpy.data.collections.new(model.NAME)
    bpy.context.scene.collection.children.link(collection)
    root = model.build(collection)
    shapes.center_footprint(root)
    if write_files:
        palette.save_image(image, os.path.join(REPO_ROOT, PALETTE_OUTPUT))
        export.export_fbx(root, os.path.join(REPO_ROOT, model.OUTPUT))
    return root


def main(argv):
    names = argv or list(MODELS)
    for name in names:
        build(name)


if __name__ == "__main__":
    main(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
