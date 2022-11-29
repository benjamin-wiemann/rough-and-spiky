using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public static class CPUPointControlHelper
{

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct SetStartPositionsJob : IJobFor {

        public int resolution;

		public NativeArray<float3> positions;

		public void Execute (int i) {
			float step = 2f / resolution;        
           
            float x = (i % resolution + 0.5f) * step - 1.0f;
            float y = 0;
            float z = (floor(i/resolution) + 0.5f) * step - 1.0f;
            positions[i] = float3(x, y, z);
      
		}
	}

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct UpdatePointPositionsJob : IJobFor {

        public int resolution;

        public int depth;

        public float heightScale;

        public int indexOffset;

        [ReadOnly]
        public NativeArray<float> frequencyBands;

		public NativeArray<float3> positions;

        [ReadOnly]
        public NativeArray<float3> prevPositions;

		public void Execute (int i) {
			float3 pos = positions[i];
            int x = i % resolution;
            int y = (int) floor( i / resolution);
            
            if (i >= resolution * (depth - 1))
            {   
                if( indexOffset > 0)
                    pos.y = heightScale * frequencyBands[i % resolution];
                else
                    pos.y = prevPositions[i].y;
            }
            else
            {
                int clampedIndex = clamp( y + indexOffset, y, depth - 1);
                pos.y = prevPositions[ x + clampedIndex * resolution].y;	
            }
            
            positions[i] = pos;      
		}
	}



}