using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.PixelTilemap
{
    public enum BlendMode
    {
        NONE,
        MIX_COLORS,
        ALPHA_BLENDING,
        ADDITIVE,
        SUBTRACTIVE,
        MULTIPLY,
        DIVIDE,
    }

    public class ColorHSL
    {
        private float _h;
        private float _s;
        private float _l;

        public ColorHSL(float h = 0, float s = 0, float l = 0)
        {
            _h = h;
            _s = s;
            _l = l;
        }

        public float h
        {
            get { return _h; }
            set { _h = value; }
        }

        public float s
        {
            get { return _s; }
            set { _s = value; }
        }

        public float l
        {
            get { return _l; }
            set { _l = value; }
        }
    }

    public class ColorHSV
    {
        private float _h;
        private float _s;
        private float _v;

        public ColorHSV(float h = 0, float s = 0, float v = 0)
        {
            _h = h;
            _s = s;
            _v = v;
        }

        public float h
        {
            get { return _h; }
            set { _h = value; }
        }

        public float s
        {
            get { return _s; }
            set { _s = value; }
        }

        public float v
        {
            get { return _v; }
            set { _v = value; }
        }
    }

    public static class ptColorUtils
    {

        public static ColorHSL RGBtoHSL(this Color color)
        {
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);

            float h;
            float s;
            float l = (max + min) / 2f;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                float d = max - min;
                s = l > .5f ? d / (2f - max - min) : d / (max + min);

                if (max == color.r)
                {
                    h = (color.g - color.b) / d + (color.g < color.b ? 6f : 0);
                }
                else if (max == color.g)
                {
                    h = (color.b - color.r) / d + 2f;
                }
                else
                {
                    h = (color.r - color.g) / d + 4f;
                }
                h /= 6;
            }

            return new ColorHSL(h, s, l);
        }

        public static Color toRGB(this ColorHSL color)
        {

            float r;
            float g;
            float b;

            if (color.s == 0)
            {
                r = g = b = color.l;
            }
            else
            {
                float q = color.l < .5f ? color.l * (1f + color.s) : color.l + color.s - color.l * color.s;
                float p = 2f * color.l - q;

                r = toRGB(p, q, color.h + 1f / 3f);
                g = toRGB(p, q, color.h);
                b = toRGB(p, q, color.h + 1f / 3f);
            }

            return new Color(r, g, b);
        }

        private static float toRGB(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        public static ColorHSV toHSV(this Color color)
        {
            float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
            float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);

            float h;
            float s;
            float v = max;

            float d = max - min;
            s = max == 0 ? 0 : d / max;

            if (max == min)
            {
                h = 0;
            }
            else
            {
                if (max == color.r)
                {
                    h = (color.g - color.b) / d + (color.g < color.b ? 6f : 0);
                }
                else if (max == color.g)
                {
                    h = (color.b - color.r) / d + 2f;
                }
                else
                {
                    h = (color.r - color.g) / d + 4f;
                }
                h /= 6f;
            }

            return new ColorHSV(h, s, v);
        }

        public static Color toRGB(this ColorHSV color)
        {
            float r = 0;
            float g = 0;
            float b = 0;

            int i = Mathf.FloorToInt(color.h * 6f);
            float f = color.h * 6f - i;
            float p = color.v * (1f - color.s);
            float q = color.v * (1f - f * color.s);
            float t = color.v * (1f - (1f - f) * color.s);

            switch (i % 6)
            {
                case 0:
                    r = color.v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = color.v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = color.v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = color.v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = color.v;
                    break;
                case 5:
                    r = color.v;
                    g = p;
                    b = q;
                    break;
            }

            return new Color(r, g, b);
        }

        public static float H(this Color color)
        {
            return color.toHSV().h;
        }

        public static float S(this Color color)
        {
            return color.toHSV().s;
        }

        public static float V(this Color color)
        {
            return color.toHSV().v;
        }

        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            float ascending = ((div % 1));
            float descending = 1f - ascending;

            switch ((int)div)
            {
                case 0:
                    return new Color(1, ascending, 0);
                case 1:
                    return new Color(descending, 1, 0);
                case 2:
                    return new Color(0, 1, ascending);
                case 3:
                    return new Color(0, descending, 1);
                case 4:
                    return new Color(ascending, 0, 1);
                default: // case 5:
                    return new Color(1, 0, descending);
            }
        }

        public static Color LerpColors(float value, Color one, Color two, Color three)
        {
            if (value <= 0.5f)
            {
                return Color.Lerp(one, two, value * 2f);
            }
            else
            {

                return Color.Lerp(two, three, (value - 0.5f) * 2f);
            }
        }

        public static Color BlendColors(Color source, Color destination, BlendMode blendMode)
        {
            float r = source.r;
            float g = source.g;
            float b = source.b;
            float a = source.a;
            switch (blendMode)
            {
                case BlendMode.NONE:
                    return source;
                case BlendMode.MIX_COLORS:
                    r = Mathf.Min(1f, (destination.r + source.r)/2f);
                    g = Mathf.Min(1f, (destination.g + source.g)/2f);
                    b = Mathf.Min(1f, (destination.b + source.b)/2f);
                    return new Color(r, g, b, a);
                case BlendMode.ALPHA_BLENDING:
                    r = Mathf.Min(1f, a*source.r+(1f-destination.a)*destination.r);
                    g = Mathf.Min(1f, a * source.g + (1f - destination.a) * destination.g);
                    b = Mathf.Min(1f, a * source.b + (1f - destination.a) * destination.b);
                    return new Color(r, g, b, Mathf.Max( destination.a, source.a));
                case BlendMode.ADDITIVE:
                    r = Mathf.Min(1f, (destination.r + source.r));
                    g = Mathf.Min(1f, (destination.g + source.g));
                    b = Mathf.Min(1f, (destination.b + source.b));
                    return new Color(r, g, b, a);
                case BlendMode.SUBTRACTIVE:
                    r = Mathf.Max(0f, (destination.r - source.r));
                    g = Mathf.Max(0f, (destination.g - source.g));
                    b = Mathf.Max(0f, (destination.b - source.b));
                    return new Color(r, g, b, a);
                case BlendMode.MULTIPLY:
                    r = Mathf.Min(1f, (destination.r * source.r));
                    g = Mathf.Min(1f, (destination.g * source.g));
                    b = Mathf.Min(1f, (destination.b * source.b));
                    return new Color(r, g, b, a);
                case BlendMode.DIVIDE:
                    r = Mathf.Min(1f, (destination.r / source.r));
                    g = Mathf.Min(1f, (destination.g / source.g));
                    b = Mathf.Min(1f, (destination.b / source.b));
                    return source;
                default:
                    return source;
            }
        }

        public static void SortByBrightness(List<Color> colors)
        {
            colors.Sort((color1, color2) =>
            (color1.V()).CompareTo(color2.V()));
        }

        public static void SortByHue(List<Color> colors)
        {
            colors.Sort((color1, color2) =>
            (color1.H()).CompareTo(color2.H()));
        }

        public static void SortBySaturation(List<Color> colors)
        {
            colors.Sort((color1, color2) =>
            (color1.S()).CompareTo(color2.S()));
        }
    }
}
