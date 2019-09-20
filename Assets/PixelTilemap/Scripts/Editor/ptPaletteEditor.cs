using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MaximovInk.PixelTilemap
{
    [CustomEditor(typeof(ptPalette))]
    public class ptPaletteEditor : Editor
    {
        private Texture2D colorBox;
        private Texture2D paletteElement;

        private Color color = Color.white;

        private float Xpicker=0;
        private float Ypicker = 0;

        private bool isRGB;

        private Vector2 scrollPos;

        private int rows = 1;

        private void UpdateColorBox()
        {
            if (colorBox != null)
                return;

            colorBox = new Texture2D(128, 128);

            var colors = new Color[colorBox.width * colorBox.height];

            for (int x = 0; x < colorBox.width; x++)
            {
                var c = ptColorUtils.Rainbow((float)x / colorBox.width);
                for (int y = 0; y < colorBox.height; y++)
                {
                    colors[x + y * colorBox.width] = ptColorUtils.LerpColors(1f - (float)y / colorBox.height, Color.white, c, Color.black);
                }
            }

            colorBox.SetPixels(colors);
            colorBox.Apply();
        }

        private void UpdateColor()
        {
            float a = color.a;
            color = ptColorUtils.LerpColors(Ypicker, Color.white, ptColorUtils.Rainbow(Xpicker), Color.black);
            color.a = a;
        }

        private void DrawColorPicker(ptPalette palette)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            color = EditorGUILayout.ColorField("Color:", color);
            GUILayout.BeginHorizontal();
            GUILayout.Box(colorBox, GUILayout.Width(100), GUILayout.Height(100));

            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
            {
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    var pickerRect = GUILayoutUtility.GetLastRect();
                    var gui = Event.current.mousePosition;

                    Xpicker = Mathf.Abs((pickerRect.x - gui.x) / pickerRect.width);
                    Ypicker = Mathf.Abs((pickerRect.y - gui.y) / pickerRect.height);

                    UpdateColor();

                    Repaint();
                }
            }

            GUILayout.BeginVertical();

            if (GUILayout.Button(isRGB ? "RGB" : "HSV"))
            {
                isRGB = !isRGB;
            }

            if (isRGB)
            {
                color.r = EditorGUILayout.Slider(color.r, 0f, 1f);
                color.g = EditorGUILayout.Slider(color.g, 0f, 1f);
                color.b = EditorGUILayout.Slider(color.b, 0f, 1f);
                color.a = EditorGUILayout.Slider(color.a, 0f, 1f);
            }
            else
            {
                var hsv = color.toHSV();
                hsv.h = EditorGUILayout.Slider(hsv.h, 0f, 1f);
                hsv.s = EditorGUILayout.Slider(hsv.s, 0f, 1f);
                hsv.v = EditorGUILayout.Slider(hsv.v, 0f, 1f);
                color = hsv.toRGB();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Add"))
            {
                palette.colors.Add(color);
            }
            GUILayout.EndVertical();
        }

        private void DrawPalette(ptPalette palette)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Palette:");
            rows = EditorGUILayout.IntSlider(rows, 1, 50);

            scrollPos = GUILayout.BeginScrollView(scrollPos, true, false);
            
            int end_count = 0;
            int r_counter = rows + 1;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(50 * rows));
            for (int i = 0; i < palette.colors.Count; i++)
            {
                r_counter++;
                if (r_counter > rows)
                {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                    r_counter = 0;
                    end_count++;
                }

                GUI.color = palette.colors[i];
                GUILayout.Box(paletteElement);
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    palette.colors.RemoveAt(i);
                    i--;
                    Repaint();
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            if (GUILayout.Button("Remove duplicates"))
            {
                palette.colors = palette.colors.Distinct().ToList();
            }
            if (GUILayout.Button("Sort by brightness"))
            {
                ptColorUtils.SortByBrightness(palette.colors);
            }
            if (GUILayout.Button("Sort by saturation"))
            {
                ptColorUtils.SortBySaturation(palette.colors);
            }
            if (GUILayout.Button("Sort by hue"))
            {
                ptColorUtils.SortByHue(palette.colors);
            }

            GUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            if (paletteElement == null)
            {
                paletteElement = new Texture2D(50, 20);
                var colors = new Color[paletteElement.width * paletteElement.height];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                paletteElement.SetPixels(colors);
                paletteElement.Apply();
            }
            var palette = (ptPalette)target;
            UpdateColorBox();
            DrawColorPicker(palette);
            DrawPalette(palette);
        }
    }
}
