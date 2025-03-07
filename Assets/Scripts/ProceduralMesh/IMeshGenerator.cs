using UnityEngine;
						
namespace ProceduralMesh {

	public interface IMeshGenerator {

        int VertexCount { get; }
		
		int IndexCount { get; }

        int JobLength { get; }

		Bounds Bounds { get; }

		int Resolution { get; set; }

		float dimZ { get; set; }
		
		float dimX { get; set; }


		void Execute<S> (int i, 
		SingleStream streams);
	}
}
