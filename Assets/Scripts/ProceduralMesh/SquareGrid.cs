using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMesh
{

    public struct SquareGrid : IMeshGenerator
    {
        public int VertexCount => (NumX + 1) * (NumZ + 1); 

        public int IndexCount => 6 * NumZ * NumX;

        public int JobLength => NumZ + 1 ;

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(dimX, 0f, dimZ ));

        public int Resolution { get; set; }

        public float dimZ { get; set; }
        
        public float dimX { get; set; }

        // number of triangle pairs in x direction
        int NumX => (int) round( Resolution * dimX );

        // number of triangle pairs in z direction is higher, since its height is smaller than its width
        int NumZ => (int) round( Resolution * dimZ );

        public void Execute<S>(int z, SingleStream stream) 
        {

            float triangleWidth = dimX / NumX;
            float triangleHeigth = dimZ / NumZ;

            int vi = (NumX + 1) * z, ti = 2 * NumX * (z - 1);

            float xOffset = - dimX/2;
            
            int iA = -NumX - 2, iB = -NumX - 1, iC = -1, iD = 0;
			var tA = int3(iA, iC, iD);
			var tB = int3(iA, iD, iB);

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);

            vertex.position.x = xOffset;
			vertex.position.z = z * triangleHeigth;

            vertex.texCoord0.x = 0;
            vertex.texCoord0.y = vertex.position.z / dimZ;

			stream.SetVertex(vi, vertex);
            vi += 1;

            for (int x = 1; x <= NumX; x++, vi++, ti += 2) {
                vertex.position.x = (float)x * triangleWidth + xOffset;
                
                // Texture gets stretched over entire grid no matter the dimensions
				vertex.texCoord0.x = x / (NumX + 0.5f);
				stream.SetVertex(vi, vertex);

                if (z > 0) {
                    stream.SetTriangle(
                        ti + 0, vi + tA 
                    );
                    stream.SetTriangle(
                        ti + 1, vi + tB 
                    );
                }
            }

        }
    }
}