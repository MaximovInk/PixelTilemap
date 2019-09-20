using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MaxivmoInk.PixelTilemap;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MaximovInk.PixelTilemap
{


    [CustomEditor(typeof(ptTilemap))]
    public class ptTilemapEditor : Editor
    {

        [MenuItem("GameObject/2D Object/PixelTilemap", false, 10)]
        static void CreateTilemap(MenuCommand menuCommand)
        {
            GameObject obj = new GameObject("Pixel Tilemap");
            obj.AddComponent<ptTilemap>();
            if (menuCommand.context is GameObject)
                obj.transform.SetParent((menuCommand.context as GameObject).transform);
            Selection.activeGameObject = obj;
        }

        private int panel = 0;

        public Color color = Color.white;

        private List<Color> history = new List<Color>();
        private bool showHistoryPalette;
        private Vector2 historyScrollPos;

        private int tmpPaletteRows = 1;
        private Vector2 tmpPalleteScrollPos;
        private bool showTilemapPalette = false;

        private ptPalette palette;

        private int customPaletteRows = 1;
        private Vector2 customPaletteScrollPos;
        private bool showCustomPalette = false;

        private BlendMode blendMode;

        private float Xpicker = 0;
        private float Ypicker = 0;

        private bool isRGB;

        private bool alphaLock = false;

        private Texture2D colorBox;

        private bool showHelp;

        public Texture2D paletteElement;

        private void UpdateColorBox()
        {
            if (colorBox != null)
                return;

            colorBox = new Texture2D(128, 128);

            var colors = new Color[colorBox.width*colorBox.height];

            for (int x = 0; x < colorBox.width; x++)
            {
                var c = ptColorUtils.Rainbow((float)x / colorBox.width);
                for (int y = 0; y < colorBox.height; y++) 
                {
                    colors[x + y * colorBox.width] = ptColorUtils.LerpColors(1f-(float)y/colorBox.height,Color.white, c,Color.black);
                }
            }

            colorBox.SetPixels(colors);
            colorBox.Apply();
        }

        private void UpdateColor()
        {
            float a = color.a;
            color = ptColorUtils.LerpColors(Ypicker , Color.white, ptColorUtils.Rainbow(Xpicker), Color.black);
            color.a = a;
        }

        private void DrawColorPicker(ptTilemap tmp)
        {
            color = EditorGUILayout.ColorField("Paint color:", color);

            blendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend mode:",blendMode);

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
                var a = color.a;
                var hsv = color.toHSV();
                hsv.h = EditorGUILayout.Slider(hsv.h, 0f, 1f);
                hsv.s = EditorGUILayout.Slider(hsv.s, 0f, 1f);
                hsv.v = EditorGUILayout.Slider(hsv.v, 0f, 1f);
                color = hsv.toRGB();
                color.a = a;
            }

            alphaLock = EditorGUILayout.Toggle("Alpha lock", alphaLock);

            if (GUILayout.Button("Clear map to color"))
            {
                tmp.Fill(color);
                tmp.UpdateMap();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawCustomPalette()
        {
            customPaletteRows = EditorGUILayout.IntSlider(customPaletteRows, 1, 50);
            customPaletteScrollPos = GUILayout.BeginScrollView(customPaletteScrollPos, true, false);
            int end_count = 0;
            int r_counter = customPaletteRows + 1;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(50 * customPaletteRows));
            for (int i = 0; i < palette.colors.Count; i++)
            {
                r_counter++;
                if (r_counter > customPaletteRows)
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
                    color = palette.colors[i];
                    Repaint();
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }
       
        private void OnInspectorGUI_Paint(ptTilemap tmp)
        {
            if (paletteElement == null)
            {
                paletteElement = new Texture2D(50, 20);
                var colors = new Color[paletteElement.width*paletteElement.height];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                paletteElement.SetPixels(colors);
                paletteElement.Apply();
            }
            GUILayout.Label("Paint:");
            EditorGUILayout.Space();
           
            UpdateColorBox();

            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            if (showHelp)
            {
                GUILayout.Label("RMB - place pixel \nRMB+Shift - erase pixel \nRMB+Control - pick color");
            }
            EditorGUILayout.Space();

            DrawColorPicker(tmp);

            EditorGUILayout.Space();
            showHistoryPalette = EditorGUILayout.Foldout(showHistoryPalette, "History of colors");
            if (showHistoryPalette)
            {
                historyScrollPos = GUILayout.BeginScrollView(historyScrollPos, true, false, GUILayout.Height(50));
                GUILayout.BeginHorizontal();
               
                for (int i = history.Count - 1; i >= 0; i--)
                {
                    GUI.color = history[i];
                    GUILayout.Box(paletteElement);
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        color = history[i];
                        Repaint();
                    }
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                if (GUILayout.Button("Clear"))
                {
                    history.Clear();
                }
            }

            EditorGUILayout.Space();
            showCustomPalette = EditorGUILayout.Foldout(showCustomPalette, "Custom palette");
            if (showCustomPalette)
            {

                palette = (ptPalette)EditorGUILayout.ObjectField("palette:", palette, typeof(ptPalette), false);

                if (palette != null)
                    DrawCustomPalette();
            }

            EditorGUILayout.Space();
            showTilemapPalette = EditorGUILayout.Foldout(showTilemapPalette, "Tilemap pallete");
            if (showTilemapPalette)
            {
                tmpPaletteRows = EditorGUILayout.IntSlider(tmpPaletteRows, 1, 50);
                tmpPalleteScrollPos = GUILayout.BeginScrollView(tmpPalleteScrollPos, true, false);
                int end_count = 0;
                int r_counter = tmpPaletteRows + 1;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.Width(50 * tmpPaletteRows));
                for (int i = 0; i < tmp.Palette.Length; i++)
                {
                    r_counter++;
                    if (r_counter > tmpPaletteRows)
                    {
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        r_counter = 0;
                        end_count++;
                    }

                    GUI.color = tmp.Palette[i];
                    GUILayout.Box(paletteElement);
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        color = tmp.Palette[i];
                        Repaint();
                    }
                }
                GUI.color = Color.white;




                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }

            

        }
        private void OnInspectorGUI_Renderer(ptTilemap tmp)
        {
            GUILayout.Label("Renderer:");
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            tmp.CellSize = EditorGUILayout.Vector2Field("Cell size:", tmp.CellSize);
            tmp.ColorTint = EditorGUILayout.ColorField("Color tint:", tmp.ColorTint);
            tmp.SortingOrder = EditorGUILayout.IntField("Sorting order:", tmp.SortingOrder);
            tmp.Material = (Material)EditorGUILayout.ObjectField("Material:", tmp.Material, typeof(Material), false);
            tmp.AutoTrim = EditorGUILayout.Toggle("Auto trim:" , tmp.AutoTrim);

            EditorGUILayout.Space();
            if (GUILayout.Button("Clear"))
            {
                tmp.Clear();
                tmp.UpdateMap();
            }

            if (GUILayout.Button("Refresh mesh"))
            {
                tmp.UpdateMap();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tmp.gameObject);
                
            }
        }
        private void OnInspectorGUI_Collider(ptTilemap tmp)
        {
            tmp.ColliderEnabled = GUILayout.Toggle(tmp.ColliderEnabled, "Collider2D");
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!tmp.ColliderEnabled);
            tmp.ColliderIsTrigger = GUILayout.Toggle(tmp.ColliderIsTrigger, "Is trigger");
            tmp.LayerMask = EditorGUILayout.LayerField("Layer", tmp.LayerMask);
            tmp.PhysicsMaterial2D = (PhysicsMaterial2D)EditorGUILayout.ObjectField("Physics material 2D:", tmp.PhysicsMaterial2D, typeof(PhysicsMaterial2D), false);
            EditorGUILayout.Space();
            if (GUILayout.Button("Update collider"))
            {
                tmp.RegenerateCollider();
            }
            EditorGUI.EndDisabledGroup();
        }
        private void OnInspectorGUI_Other(ptTilemap tmp)
        {
            GUILayout.Label("Other:");
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            tmp.Tag = EditorGUILayout.TagField("Tag:", tmp.Tag);
            tmp.DisplayChunksInHierarchy = GUILayout.Toggle(tmp.DisplayChunksInHierarchy, "Display chunks in hierarchy");
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tmp.gameObject);
            }
        }

        public override void OnInspectorGUI()
        {

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Paint"))
            {
                Tools.current = Tool.None;
                panel = 0;
            }

            if (GUILayout.Button("Renderer"))
            {
                panel = 1;
            }

            if (GUILayout.Button("Collider"))
            {
                panel = 2;
            }

            if (GUILayout.Button("Other"))
            {
                panel = 3;
            }
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.EndHorizontal();

            var tmp = (ptTilemap)target;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            switch (panel)
            {
                case 0:
                    OnInspectorGUI_Paint(tmp);
                    EditorGUI.EndChangeCheck();
                    EditorGUI.BeginChangeCheck();
                    break;
                case 1:
                    OnInspectorGUI_Renderer(tmp);
                    break;
                case 2:
                    OnInspectorGUI_Collider(tmp);
                    break;
                case 3:
                    OnInspectorGUI_Other(tmp);
                    break;
                default:
                    panel = 0;
                    break;
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                MakeSceneDirty();
            }
        }

        private void MakeSceneDirty()
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private bool CheckForAlphaLock(Color color)
        {
            if (!alphaLock)
                return true;
            return color != Color.clear;
        }

        private void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Vector2 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.origin;

            var tmp = (ptTilemap)target;

            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
            {
                var pos = ptUtils.GetGridPosition(tmp, tmp.transform.InverseTransformPoint(mousePosition));
                bool colUpdate = false;


                if (Event.current.shift)
                {
                    if (CheckForAlphaLock(tmp.GetPixel(pos.x, pos.y)))
                    {
                        if (tmp.GetPixel(pos.x, pos.y) != Color.clear)
                            colUpdate = true;
                        tmp.SetPixel(pos.x, pos.y, Color.clear);
                    }

                }
                else if (Event.current.control)
                {
                    var px = tmp.GetPixel(pos.x, pos.y);
                    if (px != Color.clear)
                    {
                        color = px;
                    }
                }
                else if (CheckForAlphaLock(tmp.GetPixel(pos.x, pos.y)))
                {
                    if (tmp.GetPixel(pos.x, pos.y) == Color.clear)
                        colUpdate = true;
                    if (history.Count == 0 || history.Last() != color)
                    {
                        history.Add(color);
                        tmp.UpdatePalette();
                    }

                    tmp.SetPixel(pos.x, pos.y, ptColorUtils.BlendColors(color, tmp.GetPixel(pos.x, pos.y), blendMode));
                }
               
                tmp.UpdateMap();
                if (colUpdate)
                    tmp.RegenerateCollider();

                MakeSceneDirty();
            }


        }


       
    }
}