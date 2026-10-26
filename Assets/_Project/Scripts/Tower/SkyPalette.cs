using UnityEngine;

namespace ZeroHour.Tower
{
    // Sky colors by tower height: day, sunset, dusk, night. Pure math, so it can be tested.
    public static class SkyPalette
    {
        private const float StarsStartHeight = 20f;
        private const float StarsFullHeight = 32f;

        private static readonly float[] Heights = { 10f, 20f, 30f, 40f };
        private static readonly Color[] Tops = { Hex(0x4A6FA5), Hex(0x2B2D6E), Hex(0x0A0F2C), Hex(0x03051A) };
        private static readonly Color[] Bottoms = { Hex(0xF4A261), Hex(0xC2557A), Hex(0x1F2A5C), Hex(0x101A40) };

        // Height 0 uses dayColor for both ends; past the last row the sky stays at the last colors.
        public static void Evaluate(float height, Color dayColor, out Color top, out Color bottom)
        {
            if (height <= 0f)
            {
                top = bottom = dayColor;
                return;
            }

            Color fromTop = dayColor;
            Color fromBottom = dayColor;
            float fromHeight = 0f;
            for (int i = 0; i < Heights.Length; i++)
            {
                if (height <= Heights[i])
                {
                    float t = (height - fromHeight) / (Heights[i] - fromHeight);
                    top = Color.Lerp(fromTop, Tops[i], t);
                    bottom = Color.Lerp(fromBottom, Bottoms[i], t);
                    return;
                }
                fromHeight = Heights[i];
                fromTop = Tops[i];
                fromBottom = Bottoms[i];
            }
            top = Tops[Tops.Length - 1];
            bottom = Bottoms[Bottoms.Length - 1];
        }

        // 0 up to height 20, rising linearly to 1 at height 32.
        public static float StarAlpha(float height) =>
            Mathf.Clamp01((height - StarsStartHeight) / (StarsFullHeight - StarsStartHeight));

        private static Color Hex(int rgb) =>
            new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);
    }
}
