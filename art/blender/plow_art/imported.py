import os
import re

import bpy
from mathutils import Matrix, Vector

from . import palette, shapes

SOURCES_DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), "sources")


def load(name, source, collection, parent, colors, size):
    meshes = _import(source, collection)
    for obj in meshes:
        _paint_faces(obj, colors)
    obj = shapes.join(meshes, name)
    _normalize(obj, size)
    obj.parent = parent
    return obj


def _import(source, collection):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=os.path.join(SOURCES_DIR, source))
    created = [obj for obj in bpy.data.objects if obj not in before]
    bpy.context.view_layer.update()
    meshes = []
    for obj in created:
        if obj.type == "MESH":
            world = obj.matrix_world.copy()
            obj.parent = None
            obj.data = obj.data.copy()
            obj.data.transform(world)
            obj.matrix_world = Matrix.Identity(4)
            for owner in list(obj.users_collection):
                owner.objects.unlink(obj)
            collection.objects.link(obj)
            meshes.append(obj)
    for obj in created:
        if obj.type != "MESH":
            bpy.data.objects.remove(obj)
    return meshes


def _paint_faces(obj, colors):
    mesh = obj.data
    source_uv = mesh.uv_layers.active.data if mesh.uv_layers else None
    pixels = {}
    picked = [_face_color(mesh, polygon, source_uv, colors, pixels) for polygon in mesh.polygons]
    for layer in list(mesh.uv_layers):
        mesh.uv_layers.remove(layer)
    layer = mesh.uv_layers.new(name="UVMap").data
    for polygon, color in zip(mesh.polygons, picked):
        uv = palette.cell_uv(color)
        for loop_index in polygon.loop_indices:
            layer[loop_index].uv = uv
        polygon.material_index = 0
    mesh.materials.clear()
    mesh.materials.append(bpy.data.materials[palette.MATERIAL_NAME])


def _face_color(mesh, polygon, source_uv, colors, pixels):
    material = mesh.materials[polygon.material_index] if mesh.materials else None
    key = re.sub(r"\.\d{3}$", "", material.name) if material else None
    rule = colors.get(key, colors.get("*"))
    if rule is None:
        raise KeyError(f"No palette color for source material {key!r}")
    if isinstance(rule, str):
        return rule
    return _nearest(_source_color(material, polygon, source_uv, pixels), rule)


def _source_color(material, polygon, source_uv, pixels):
    nodes = material.node_tree.nodes
    texture = next((n for n in nodes if n.type == "TEX_IMAGE" and n.image), None)
    if texture is None or source_uv is None:
        shader = next(n for n in nodes if n.type == "BSDF_PRINCIPLED")
        return tuple(_to_srgb(c) for c in shader.inputs["Base Color"].default_value[:3])
    u = sum(source_uv[i].uv.x for i in polygon.loop_indices) / polygon.loop_total
    v = sum(source_uv[i].uv.y for i in polygon.loop_indices) / polygon.loop_total
    image = texture.image
    width, height = image.size
    x = min(width - 1, max(0, int((u % 1.0) * width)))
    y = min(height - 1, max(0, int((v % 1.0) * height)))
    offset = (y * width + x) * 4
    if image.name not in pixels:
        pixels[image.name] = list(image.pixels)
    return tuple(pixels[image.name][offset:offset + 3])



def _nearest(color, candidates):
    return min(candidates, key=lambda name: sum((a - b) ** 2 for a, b in zip(color, palette.rgb(palette.COLORS[name]))))


def _to_srgb(value):
    return 12.92 * value if value <= 0.0031308 else 1.055 * value ** (1 / 2.4) - 0.055


def _normalize(obj, size):
    mesh = obj.data
    points = [v.co for v in mesh.vertices]
    low = Vector([min(p[i] for p in points) for i in range(3)])
    high = Vector([max(p[i] for p in points) for i in range(3)])
    factor = size / max(high.x - low.x, high.y - low.y)
    center = Vector(((low.x + high.x) / 2, (low.y + high.y) / 2, low.z))
    mesh.transform(Matrix.Scale(factor, 4) @ Matrix.Translation(-center))
    for polygon in mesh.polygons:
        polygon.use_smooth = False
