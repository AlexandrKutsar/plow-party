using UnityEngine;
using UnityEngine.Rendering;

namespace PlowParty.Gameplay.Snow.View
{
    internal static class SnowSurfaceMesh
    {
        public static Mesh Build(int cellsX, int cellsY, int verticesPerCell, float maxHeight)
        {
            var columns = cellsX * verticesPerCell + 1;
            var rows = cellsY * verticesPerCell + 1;
            var vertices = new Vector3[columns * rows];
            var uvs = new Vector2[columns * rows];
            var normals = new Vector3[columns * rows];
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var uv = new Vector2((float)column / (columns - 1), (float)row / (rows - 1));
                    var vertex = row * columns + column;
                    vertices[vertex] = new Vector3(uv.x - 0.5f, 0f, uv.y - 0.5f);
                    uvs[vertex] = uv;
                    normals[vertex] = Vector3.up;
                }
            }

            var mesh = new Mesh
            {
                name = "SnowSurface",
                indexFormat = vertices.Length > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16,
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(Triangles(columns, rows), 0, false);
            mesh.bounds = new Bounds(new Vector3(0f, maxHeight * 0.5f, 0f), new Vector3(1f, maxHeight, 1f));
            mesh.UploadMeshData(true);
            return mesh;
        }

        private static int[] Triangles(int columns, int rows)
        {
            var triangles = new int[(columns - 1) * (rows - 1) * 6];
            var next = 0;
            for (var row = 0; row < rows - 1; row++)
            {
                for (var column = 0; column < columns - 1; column++)
                {
                    var corner = row * columns + column;
                    triangles[next++] = corner;
                    triangles[next++] = corner + columns;
                    triangles[next++] = corner + 1;
                    triangles[next++] = corner + 1;
                    triangles[next++] = corner + columns;
                    triangles[next++] = corner + columns + 1;
                }
            }

            return triangles;
        }
    }
}
