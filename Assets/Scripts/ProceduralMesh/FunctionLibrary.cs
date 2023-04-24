using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMesh{

    public static class FunctionLibrary{

        public delegate int Function (int val);

        public enum FunctionName { Rational }

        static Function[] functions = { Rational };

        public static Function GetFunction (FunctionName name) 
        {
            return functions[(int)name];
        }

        public static int Rational( int val)
        {
            return (int) round(1f / (float) val);
        }

        public static int Exponential( int val )
        {
            return (int) exp2( -val );
        }

    }
        
    
}