using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTrainER.source.debug
{
    internal static class Debug3D
    {
        // https://github.com/Ryan-Mirch/Line-and-Sphere-Drawing
        public static MeshInstance3D DrawPoint(Vector3 position, float radius = 0.05f, Color? color = null)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            SphereMesh sphereMesh = new SphereMesh();
            OrmMaterial3D material = new OrmMaterial3D();

            meshInstance.Mesh = sphereMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            meshInstance.Position = position;

            sphereMesh.Radius = radius;
            sphereMesh.Height = radius*2;
            sphereMesh.Material = material;

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color ?? Colors.WhiteSmoke;

            WorldManager.renderer.AddChild(meshInstance);
            return meshInstance;
        }

        public static MeshInstance3D DrawLine(Vector3 pos1, Vector3 pos2, Color? color = null)
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            ImmediateMesh immediateMesh = new ImmediateMesh();
            StandardMaterial3D material = new StandardMaterial3D();

            meshInstance.Mesh = immediateMesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
            immediateMesh.SurfaceAddVertex(pos1);
            immediateMesh.SurfaceAddVertex(pos2);
            immediateMesh.SurfaceEnd();

            material.ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color ?? Colors.WhiteSmoke;

            WorldManager.renderer.AddChild(meshInstance);

            return meshInstance;
        }

        public static void DrawVector(Vector3 vector, Vector3 origin, float radius = 0.5f, Color? colorStart = null, Color? colorEnd = null)
        {
            DrawLine(origin, origin + vector, colorStart);
            DrawPoint(origin, radius, colorStart);
            DrawPoint(origin + vector, radius, colorEnd);
        }
    }
}
