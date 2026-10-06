import bpy

CELL_PIXELS = 4
GRID = 4
IMAGE_NAME = "Palette"
MATERIAL_NAME = "Palette"

COLORS = {
    "orange": "F28C28",
    "orange_dark": "C8621B",
    "yellow": "F6C945",
    "tire": "2E2F33",
    "metal": "8A9099",
    "metal_light": "C9CED6",
    "white": "F4F6F8",
    "glass": "7FB7D9",
    "red": "D94848",
    "seat": "5A3E2B",
    "green": "5DAA5A",
    "blue": "3F7AD9",
    "purple": "8E5BD0",
    "black": "15161A",
    "snow": "E8F1F8",
    "wood": "A87445",
}


def rgb(hex_color):
    return tuple(int(hex_color[i:i + 2], 16) / 255 for i in (0, 2, 4))


def cell_index(name):
    return list(COLORS).index(name)


def cell_uv(name):
    index = cell_index(name)
    column = index % GRID
    row = index // GRID
    return ((column + 0.5) / GRID, 1 - (row + 0.5) / GRID)


def build_image():
    size = GRID * CELL_PIXELS
    image = bpy.data.images.get(IMAGE_NAME)
    if image is None or image.size[0] != size:
        if image is not None:
            bpy.data.images.remove(image)
        image = bpy.data.images.new(IMAGE_NAME, size, size, alpha=False)
    pixels = [0.0] * (size * size * 4)
    for name, hex_color in COLORS.items():
        r, g, b = rgb(hex_color)
        index = cell_index(name)
        column = index % GRID
        row_from_bottom = GRID - 1 - index // GRID
        for y in range(row_from_bottom * CELL_PIXELS, (row_from_bottom + 1) * CELL_PIXELS):
            for x in range(column * CELL_PIXELS, (column + 1) * CELL_PIXELS):
                offset = (y * size + x) * 4
                pixels[offset:offset + 4] = (r, g, b, 1.0)
    image.pixels.foreach_set(pixels)
    image.update()
    return image


def build_material(image):
    material = bpy.data.materials.get(MATERIAL_NAME) or bpy.data.materials.new(MATERIAL_NAME)
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    shader = next(n for n in nodes if n.type == "BSDF_PRINCIPLED")
    texture = next((n for n in nodes if n.type == "TEX_IMAGE"), None) or nodes.new("ShaderNodeTexImage")
    texture.image = image
    texture.interpolation = "Closest"
    links.new(texture.outputs["Color"], shader.inputs["Base Color"])
    shader.inputs["Roughness"].default_value = 0.8
    return material


def save_image(image, path):
    image.filepath_raw = path
    image.file_format = "PNG"
    image.save()
