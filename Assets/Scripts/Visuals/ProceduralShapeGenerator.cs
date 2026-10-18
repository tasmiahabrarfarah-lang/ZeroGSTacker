using System.Collections.Generic;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Visuals
{
    public static class ProceduralShapeGenerator
    {
        private static Shader neonShader;
        private static Dictionary<ShapeType, Mesh> cachedMeshes = new Dictionary<ShapeType, Mesh>();

        public static Shader GetNeonShader()
        {
            if (neonShader == null)
            {
                neonShader = Shader.Find("ZeroGStacker/NeonRimGlow");
                if (neonShader == null)
                {
                    neonShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                }
                if (neonShader == null)
                {
                    neonShader = Shader.Find("Sprites/Default");
                }
            }
            return neonShader;
        }

        public static Material CreateNeonMaterial(Color baseColor)
        {
            var shader = GetNeonShader();
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            
            // Neon rim is a boosted, high-intensity color for bloom
            Color rimCol = Color.Lerp(baseColor, Color.white, 0.45f) * 1.8f;
            mat.SetColor("_RimColor", rimCol);
            mat.SetFloat("_RimPower", 2.2f);
            mat.SetFloat("_InnerAlpha", 0.9f);
            mat.color = baseColor;
            return mat;
        }

        public static Mesh GetOrCreateMesh(ShapeType type, float radius = 0.7f)
        {
            if (cachedMeshes.TryGetValue(type, out Mesh m) && m != null)
            {
                return m;
            }

            Mesh newMesh = null;
            switch (type)
            {
                case ShapeType.Cube:
                    newMesh = CreateQuadMesh(radius * 2f, radius * 2f);
                    break;
                case ShapeType.Sphere:
                    newMesh = CreateCircleMesh(radius, 32);
                    break;
                case ShapeType.Prism:
                    newMesh = CreateTriangleMesh(radius * 2f, radius * 1.8f);
                    break;
            }

            if (newMesh != null)
            {
                newMesh.name = $"Procedural_{type}";
                cachedMeshes[type] = newMesh;
            }
            return newMesh;
        }

        public static GameObject CreateShapeGameObject(ShapeType type, ShapeColor colorType, float radius, float mass, out Collider2D collider)
        {
            var go = new GameObject($"Shape_{type}_{colorType}");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();

            Mesh mesh = GetOrCreateMesh(type, radius);
            mf.sharedMesh = mesh;

            Color col = GameConstants.GetColor(colorType);
            Material mat = CreateNeonMaterial(col);
            mr.material = mat;

            // Attach appropriate 2D collider
            switch (type)
            {
                case ShapeType.Cube:
                    var box = go.AddComponent<BoxCollider2D>();
                    box.size = new Vector2(radius * 2f, radius * 2f);
                    collider = box;
                    break;

                case ShapeType.Sphere:
                    var circle = go.AddComponent<CircleCollider2D>();
                    circle.radius = radius;
                    collider = circle;
                    break;

                case ShapeType.Prism:
                    var poly = go.AddComponent<PolygonCollider2D>();
                    float w = radius * 2f;
                    float h = radius * 1.8f;
                    Vector2[] points = new Vector2[]
                    {
                        new Vector2(0f, h * 0.5f),
                        new Vector2(-w * 0.5f, -h * 0.5f),
                        new Vector2(w * 0.5f, -h * 0.5f)
                    };
                    poly.points = points;
                    collider = poly;
                    break;

                default:
                    collider = go.AddComponent<BoxCollider2D>();
                    break;
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.mass = mass;

            return go;
        }

        private static Mesh CreateQuadMesh(float width, float height)
        {
            Mesh mesh = new Mesh();
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            Vector3[] verts = new Vector3[4]
            {
                new Vector3(-hw, -hh, 0f),
                new Vector3(hw, -hh, 0f),
                new Vector3(-hw, hh, 0f),
                new Vector3(hw, hh, 0f)
            };

            Vector2[] uvs = new Vector2[4]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };

            int[] tris = new int[6] { 0, 2, 1, 2, 3, 1 };

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateCircleMesh(float radius, int segments)
        {
            Mesh mesh = new Mesh();
            Vector3[] verts = new Vector3[segments + 1];
            Vector2[] uvs = new Vector2[segments + 1];
            int[] tris = new int[segments * 3];

            verts[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);

            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * angleStep);
                float x = Mathf.Cos(rad) * radius;
                float y = Mathf.Sin(rad) * radius;

                verts[i + 1] = new Vector3(x, y, 0f);
                uvs[i + 1] = new Vector2((Mathf.Cos(rad) + 1f) * 0.5f, (Mathf.Sin(rad) + 1f) * 0.5f);
            }

            for (int i = 0; i < segments; i++)
            {
                int current = i + 1;
                int next = (i + 1) % segments + 1;

                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = current;
                tris[i * 3 + 2] = next;
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTriangleMesh(float width, float height)
        {
            Mesh mesh = new Mesh();
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            Vector3[] verts = new Vector3[3]
            {
                new Vector3(0f, hh, 0f),
                new Vector3(-hw, -hh, 0f),
                new Vector3(hw, -hh, 0f)
            };

            Vector2[] uvs = new Vector2[3]
            {
                new Vector2(0.5f, 1f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f)
            };

            int[] tris = new int[3] { 0, 2, 1 };

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
