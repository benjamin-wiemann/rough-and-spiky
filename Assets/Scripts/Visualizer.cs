using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{

    public enum CalculationMethod { CPU, GPU, Burst}

    const int maxResolution = 1000;

    [SerializeField, Range(10, maxResolution)]
    int resolution = 50;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    [SerializeField]
    Transform pointPrefab;

    [SerializeField]
    CalculationMethod calculationMethod;

    ComputeBuffer positionsBuffer;

    Transform[] points;

    void OnEnable()
    {
        switch( calculationMethod )
        {
            case CalculationMethod.CPU:
            float step = 2f / resolution;
            var scale = Vector3.one * step;
            points = new Transform[resolution * resolution];
            for (int i = 0; i < points.Length; i++) {
                Transform point = points[i] = Instantiate(pointPrefab);
                point.localScale = scale;
                point.SetParent(transform, false);
            }
            points = new Transform[resolution * resolution];
            break;
            case CalculationMethod.GPU:
            positionsBuffer = new ComputeBuffer(resolution * resolution, 3 );
            break;
            case CalculationMethod.Burst:
            break;

        }
            
            
    }

    void OnDisable()
    {
        if( positionsBuffer != null)
        {
            positionsBuffer.Release();
            positionsBuffer = null;
        }
        
    }

    void OnValidate () {
		if (enabled) {
			OnDisable();
			OnEnable();
		}
	}

    // Update is called once per frame
    void Update()
    {
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f/resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    void InitPointCPU()
    {

    }

    void UpdatePointPositionCPU()
    {
        float step = 2f / resolution;
		float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
			if (x == resolution) {
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = 
        }

    }
}
