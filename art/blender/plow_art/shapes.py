import bmesh
import bpy
from mathutils import Matrix, Vector

from . import palette


def _object(name, mesh_builder, location, collection, parent):
    mesh = bpy.data.meshes.new(name)
    bm = bmesh.new()
    mesh_builder(bm)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = location
    collection.objects.link(obj)
    obj.parent = parent
    return obj


def _chamfer(bm, amount):
    if amount > 0:
        bmesh.ops.bevel(bm, geom=list(bm.edges), offset=amount, segments=1, affect="EDGES", profile=0.5)


def box(name, size, location, collection, parent=None, offset=(0, 0, 0), chamfer=0.0):
    def build(bm):
        bmesh.ops.create_cube(bm, size=1.0)
        bmesh.ops.scale(bm, vec=size, verts=bm.verts)
        bmesh.ops.translate(bm, vec=offset, verts=bm.verts)
        _chamfer(bm, chamfer)

    return _object(name, build, location, collection, parent)


def cylinder(name, radius, depth, location, collection, parent=None, axis="X", segments=8, chamfer=0.0, cap_inset=0.0):
    rotations = {"X": ("Y", 1.5708), "Y": ("X", 1.5708), "Z": ("Z", 0.0)}

    def build(bm):
        bmesh.ops.create_cone(bm, cap_ends=True, segments=segments, radius1=radius, radius2=radius, depth=depth)
        _chamfer(bm, chamfer)
        if cap_inset > 0:
            caps = [face for face in bm.faces if len(face.verts) == segments]
            bmesh.ops.inset_individual(bm, faces=caps, thickness=cap_inset, depth=0)
        rotation_axis, angle = rotations[axis]
        bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(angle, 3, rotation_axis))

    return _object(name, build, location, collection, parent)


def extruded_profile(name, profile_yz, width, location, collection, parent=None):
    def build(bm):
        half = width / 2
        left = [bm.verts.new((-half, y, z)) for y, z in profile_yz]
        right = [bm.verts.new((half, y, z)) for y, z in profile_yz]
        bm.faces.new(list(reversed(left)))
        bm.faces.new(right)
        count = len(profile_yz)
        for i in range(count):
            j = (i + 1) % count
            bm.faces.new((left[i], left[j], right[j], right[i]))
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)

    return _object(name, build, location, collection, parent)


def empty(name, location, collection, parent=None):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.2
    obj.location = location
    collection.objects.link(obj)
    obj.parent = parent
    return obj


def paint(obj, color, face_filter=None):
    mesh = obj.data
    if not mesh.uv_layers:
        mesh.uv_layers.new(name="UVMap")
    if mesh.materials:
        mesh.materials[0] = bpy.data.materials[palette.MATERIAL_NAME]
    else:
        mesh.materials.append(bpy.data.materials[palette.MATERIAL_NAME])
    uv = palette.cell_uv(color)
    layer = mesh.uv_layers.active.data
    for polygon in mesh.polygons:
        if face_filter is None or face_filter(polygon):
            for loop_index in polygon.loop_indices:
                layer[loop_index].uv = uv
    return obj


def facing(direction, threshold=0.7):
    return lambda polygon: polygon.normal.dot(direction) > threshold


def on_axis(axis_index, tolerance=0.001):
    return lambda polygon: all(abs(polygon.center[i]) < tolerance for i in range(3) if i != axis_index)


def center_footprint(root):
    bpy.context.view_layer.update()
    corners = [obj.matrix_world @ Vector(corner) for obj in root.children_recursive if obj.type == "MESH" for corner in obj.bound_box]
    offset = Vector((
        (min(c.x for c in corners) + max(c.x for c in corners)) / 2,
        (min(c.y for c in corners) + max(c.y for c in corners)) / 2,
        0,
    ))
    for child in root.children:
        child.location -= offset
    bpy.context.view_layer.update()
