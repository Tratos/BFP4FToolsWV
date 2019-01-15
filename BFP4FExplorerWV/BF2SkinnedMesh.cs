﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;
using SharpDX;

namespace BFP4FExplorerWV
{
    public class BF2SkinnedMesh
    {
        public Helper.BF2MeshHeader header;
        public Helper.BF2MeshGeometry geometry;
        public List<Helper.BF2MeshSKMLod> lods;
        public List<Helper.BF2MeshSKMGeometryMaterial> geomat;

        public BF2SkinnedMesh(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            header = new Helper.BF2MeshHeader(m);
            geometry = new Helper.BF2MeshGeometry(m);
            lods = new List<Helper.BF2MeshSKMLod>();
            geomat = new List<Helper.BF2MeshSKMGeometryMaterial>();
            uint count = geometry.GetSumOfLODs();
            for (int i = 0; i < count; i++)
                lods.Add(new Helper.BF2MeshSKMLod(m, header.version));
            for (int i = 0; i < count; i++)
                geomat.Add(new Helper.BF2MeshSKMGeometryMaterial(m));
        }

        public List<RenderObject> ConvertForEngine(Engine3D engine)
        {
            List<RenderObject> result = new List<RenderObject>();
            Helper.BF2MeshSKMGeometryMaterial lod0 = geomat[0];
            for (int i = 0; i < lod0.numMaterials; i++)
            {
                Helper.BF2MeshSKMMaterial mat = lod0.materials[i];
                List<RenderObject.Vertex> list = new List<RenderObject.Vertex>();
                List<RawVector3> list2 = new List<RawVector3>();
                int m = geometry.vertices.Count / (int)geometry.numVertices;
                for (int j = 0; j < mat.numIndicies; j++)
                {
                    int pos = (geometry.indices[(int)mat.indiciesStartIndex + j] + (int)mat.vertexStartIndex) * m;
                    list.Add(GetVertex(pos));
                    list2.Add(GetVector(pos));
                }
                if (mat.numIndicies != 0)
                {
                    RenderObject o = new RenderObject(engine.device, RenderObject.RenderType.TriListTextured, engine.defaultTexture, engine);
                    o.verticesTextured = list.ToArray();
                    o.InitGeometry();
                    result.Add(o);
                    RenderObject o2 = new RenderObject(engine.device, RenderObject.RenderType.TriListWired, engine.defaultTexture, engine);
                    o2.vertices = list2.ToArray();
                    o2.InitGeometry();
                    result.Add(o2);
                }
            }
            return result;
        }

        public RawVector3 GetVector(int pos)
        {
            return new RawVector3(geometry.vertices[pos], geometry.vertices[pos + 1], geometry.vertices[pos + 2]);
        }

        public RenderObject.Vertex GetVertex(int pos)
        {
            return new RenderObject.Vertex(new Vector4(geometry.vertices[pos], geometry.vertices[pos + 1], geometry.vertices[pos + 2], 1), Color.White, new Vector2(geometry.vertices[pos + 8], geometry.vertices[pos + 9]));
        }
    }
}
