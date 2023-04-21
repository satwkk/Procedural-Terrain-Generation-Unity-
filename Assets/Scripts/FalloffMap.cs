using UnityEngine;

namespace VoidTerrainGenerator {
    
    public static class FalloffMap
    {
        static float a = 3f;
        static float b = 2.2f;

        public static float[,] GenerateFallOffMap(int size)
        {
            float[,] map = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float x = i / (float)size * 2 - 1;
                    float y = j / (float)size * 2 - 1;
                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    map[i, j] = Evaluate(value);
                }
            }
            return map;
        }

        // A smoothing formula to increase the amount of fall off map to get more area for terrain
        private static float Evaluate(float value)
        {
            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }

}