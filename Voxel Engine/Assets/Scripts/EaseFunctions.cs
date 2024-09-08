
using System;

public static class EaseFunctions
{
        public enum EaseFunctionType
        {
                Expo,
                Quint,
                Quad,
                Quadratic
        }

        public static Func<float, float> Function(this EaseFunctionType type)
        {
                return type switch
                {
                        EaseFunctionType.Expo => EaseInExpo,
                        EaseFunctionType.Quint => EaseInQuint,
                        EaseFunctionType.Quad => EaseInQuad,
                        EaseFunctionType.Quadratic => EaseInQuadratic,
                        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
        }
        
        public static float EaseInExpo(float x)
        {
                return x == 0 ? 0 : MathF.Pow(2, 10 * x - 10);
        }


        public static float EaseInQuint(float x)
        {
                return x * x * x * x * x;
        }
        
        public static float EaseInQuad(float x)
        {
                return x * x * x * x;
        }

        public static float EaseInQuadratic(float x)
        {
                return x * x;
        }
}