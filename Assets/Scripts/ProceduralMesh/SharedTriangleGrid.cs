using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMesh
{

    public struct SharedTriangleGrid : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution + 1); 

        public int IndexCount => 6 * Resolution * Resolution ;

        public int JobLength => Resolution + 1 ;

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f + 0.5f / Resolution, 0f, sqrt(3f) / 2f ));

        public int Resolution { get; set; }

        public void Execute<S>(int z, SingleStream stream) 
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);

            float xOffset = -0.25f;
            float uOffset = 0f;

            int iA = -Resolution - 2, iB = -Resolution - 1, iC = -1, iD = 0;
			var tA = int3(iA, iC, iD);
			var tB = int3(iA, iD, iB);

            if ((z & 1) == 1) {
				xOffset = 0.25f;
                uOffset = 0.5f / (Resolution + 0.5f);
                tA = int3(iA, iC, iB);
				tB = int3(iB, iC, iD);
			}

			xOffset = xOffset / Resolution; // - 0.5f;

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);

            vertex.position.x = xOffset;
			vertex.position.z = ((float)z / Resolution) * sqrt(3f) / 2f;

            vertex.texCoord0.x = uOffset;
            vertex.texCoord0.y = vertex.position.z / (1f + 0.5f / Resolution) + 0.5f;

			stream.SetVertex(vi, vertex);
            vi += 1;

            for (int x = 1; x <= Resolution; x++, vi++, ti += 2) {
                vertex.position.x = (float)x / Resolution + xOffset;
				vertex.texCoord0.x = x / (Resolution + 0.5f) + uOffset;
				stream.SetVertex(vi, vertex);

                if (z > 0) {
                    stream.SetTriangle(
                        ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1)
                    );
                    stream.SetTriangle(
                        ti + 1, vi + int3(-Resolution - 1, -1, 0)
                    );
                }
            }

        }
    }
}