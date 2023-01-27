using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralMesh {

    

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<G> : IJobFor
        where G : struct, IMeshGenerator
    {

        G generator;

        [WriteOnly]
		SingleStream streams;

		public void Execute (int i) => generator.Execute<SingleStream>(i, streams);

        public static JobHandle ScheduleParallel (
			Mesh mesh, Mesh.MeshData meshData, int resolution, float xDim, float zDim, JobHandle dependency
		) {
			var job = new MeshJob<G>();
			job.generator.Resolution = resolution;
			job.generator.dimZ = zDim;
			job.generator.dimX = xDim;
			job.streams.Setup(
				meshData,
				mesh.bounds = job.generator.Bounds,
				job.generator.VertexCount,
				job.generator.IndexCount
			);
			return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
		}
        
    }

	public delegate JobHandle MeshJobScheduleDelegate (
		Mesh mesh, Mesh.MeshData meshData, int resolution, int depth, JobHandle dependency
	);
}