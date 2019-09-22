using System.Collections.Generic;
using UnityEngine;

namespace MaxivmoInk.PixelTilemap
{
    [ExecuteInEditMode]
    public class ptChunk : MonoBehaviour
    {
        [SerializeField]
        public int x;
        [SerializeField]
        public int y;
        [SerializeField]
        private ptTilemap ptTilemap;
        [SerializeField]
        public SpriteRenderer spriteRenderer;
        [SerializeField]
        private PolygonCollider2D polygonCollider2D;
        [SerializeField]
        private Texture2D texture;
        [SerializeField]
        private Color[] colors;
        [SerializeField]
        private Material defaultMat;

        public bool texIsDirty = false;
        public bool colIsDirty = false;

        public void Init(ptTilemap ptTilemap)
        {
            this.ptTilemap = ptTilemap;
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            defaultMat = new Material(Shader.Find("Sprites/Default"));
            texture = new Texture2D(ptTilemap.CHUNK_SIZE_X, ptTilemap.CHUNK_SIZE_Y);
            texture.filterMode = FilterMode.Point;
            var sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),1);
            colors = new Color[ptTilemap.CHUNK_SIZE_X*ptTilemap.CHUNK_SIZE_Y];
            spriteRenderer.sprite = sp;
           
            UpdateRenderer();
            UpdateColliderActive();
            Fill(Color.clear);
        }

        #region Renderer

        public Color GetPixel(int x, int y)
        {
            int innerX = (x < 0 ? -x - 1 : x) % ptTilemap.CHUNK_SIZE_X;
            int innerY = (y < 0 ? -y - 1 : y) % ptTilemap.CHUNK_SIZE_Y;
            if (x < 0) innerX = ptTilemap.CHUNK_SIZE_X - 1 - innerX;
            if (y < 0) innerY = ptTilemap.CHUNK_SIZE_Y - 1 - innerY;
            return colors[innerX + innerY * ptTilemap.CHUNK_SIZE_Y];
        }

        public void SetPixel(int x, int y, Color color)
        {
            var px = GetPixel(x, y);

            if (px == color)
                return;

            if (px == Color.clear || color == Color.clear)
            {
                colIsDirty = true;
            }

            texIsDirty = true;

            int innerX = (x < 0 ? -x - 1 : x) % ptTilemap.CHUNK_SIZE_X;
            int innerY = (y < 0 ? -y - 1 : y) % ptTilemap.CHUNK_SIZE_Y;
            if (x < 0) innerX = ptTilemap.CHUNK_SIZE_X - 1 - innerX;
            if (y < 0) innerY = ptTilemap.CHUNK_SIZE_Y - 1 - innerY;
            colors[innerX + innerY * ptTilemap.CHUNK_SIZE_Y] = color;
        }

        public void Fill(Color color)
        {
            texIsDirty = true;
            colIsDirty = true;
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
        }

        public HashSet<Color> GetPalette()
        {
            HashSet<Color> newPallete = new HashSet<Color>();
            for (int i = 0; i < colors.Length; i++)
            {
                newPallete.Add(colors[i]);
            }
            return newPallete;
        }

        public void UpdateMesh()
        {
            texIsDirty = false;
            texture.SetPixels(colors);
            texture.Apply();
        }

        public void UpdateRenderer()
        {
       
            spriteRenderer.sortingOrder = ptTilemap.SortingOrder;
            spriteRenderer.color = ptTilemap.ColorTint;

            if (ptTilemap.Material == null)
                spriteRenderer.material = defaultMat;
            else
                spriteRenderer.material = ptTilemap.Material;

        }

        public void UpdateFlags()
        {
            if (ptTilemap.DisplayChunksInHierarchy)
            {
                gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
            }
            else
            {
                gameObject.hideFlags |= HideFlags.HideInHierarchy;
            }
            tag = ptTilemap.Tag;
            gameObject.layer = ptTilemap.LayerMask;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != Color.clear)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Collider

        public void GenerateCollider()
        {
            colIsDirty = false;

            if (polygonCollider2D == null)
                return;

            List<ColliderSegment> segments = GetSegments();
            List<List<Vector2>> paths = CentredPoints(FindPaths(segments));

            polygonCollider2D.pathCount = paths.Count;

            for (int p = 0; p < paths.Count; p++)
            {
                polygonCollider2D.SetPath(p, paths[p].ToArray());
            }
        }

        public void UpdateColliderProperties()
        {
            if (polygonCollider2D == null)
                return;
            polygonCollider2D.sharedMaterial = ptTilemap.PhysicsMaterial2D;
            polygonCollider2D.isTrigger = ptTilemap.ColliderIsTrigger;
        }

        public void UpdateColliderActive()
        {
            if (ptTilemap.ColliderEnabled)
            {
                if (polygonCollider2D == null)
                {
                    polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                    polygonCollider2D.pathCount = 0;
                    GenerateCollider();
                }
            }
            else
            {
                if (polygonCollider2D != null)
                {
                    DestroyImmediate(polygonCollider2D);
                }
            }
            colIsDirty = true;
        }

        private List<List<Vector2>> CentredPoints(List<List<Vector2>> original)
        {
            for (int p = 0; p < original.Count; p++)
            {
                for (int o = 0; o < original[p].Count; o++)
                {
                    original[p][o] -= new Vector2(
                        ptTilemap.CHUNK_SIZE_X * ptTilemap.CellSize.x,
                        ptTilemap.CHUNK_SIZE_Y * ptTilemap.CellSize.y
                        ) / 2;
                }
            }
            return original;
        }

        List<List<Vector2>> FindPaths(List<ColliderSegment> segments)
        {
            List<List<Vector2>> output = new List<List<Vector2>>();
            while (segments.Count > 0)
            {
                Vector2 currentpoint = segments[0].Point2;
                List<Vector2> currentpath = new List<Vector2> { segments[0].Point1, segments[0].Point2 };
                segments.Remove(segments[0]);

                bool currentpathcomplete = false;
                while (currentpathcomplete == false)
                {
                    currentpathcomplete = true;
                    for (int s = 0; s < segments.Count; s++)
                    {
                        if (segments[s].Point1 == currentpoint)
                        {
                            currentpathcomplete = false;
                            currentpath.Add(segments[s].Point2);
                            currentpoint = segments[s].Point2;
                            segments.Remove(segments[s]);
                        }
                        else if (segments[s].Point2 == currentpoint)
                        {
                            currentpathcomplete = false;
                            currentpath.Add(segments[s].Point1);
                            currentpoint = segments[s].Point1;
                            segments.Remove(segments[s]);
                        }
                    }
                }
                output.Add(currentpath);
            }
            return output;
        }

        private bool IsEmpty(Color color)
        {
            return color == Color.clear;
        }

        private List<ColliderSegment> GetSegments()
        {
            List<ColliderSegment> segments = new List<ColliderSegment>();

            for (int i = 0; i < colors.Length; i++)
            {
                if (IsEmpty(colors[i]))
                    continue;

                int x = i % ptTilemap.CHUNK_SIZE_Y;
                int y = i / ptTilemap.CHUNK_SIZE_Y;

                if (y + 1 >= ptTilemap.CHUNK_SIZE_Y || IsEmpty(colors[x + (y + 1) * ptTilemap.CHUNK_SIZE_Y]))
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y + 1), new Vector2(x + 1, y + 1)));
                }
                if (y - 1 < 0 || IsEmpty(colors[x + (y - 1) * ptTilemap.CHUNK_SIZE_Y]))
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x + 1, y)));
                }
                if (x + 1 >= ptTilemap.CHUNK_SIZE_X || IsEmpty(colors[x + 1 + y * ptTilemap.CHUNK_SIZE_Y]))
                {
                    segments.Add(new ColliderSegment(new Vector2(x + 1, y), new Vector2(x + 1, y + 1)));
                }
                if (x - 1 < 0 || IsEmpty(colors[x - 1 + y * ptTilemap.CHUNK_SIZE_Y]))
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x, y + 1)));
                }
            }

            return segments;
        }

        private struct ColliderSegment
        {
            public Vector2 Point1;
            public Vector2 Point2;
            public ColliderSegment(Vector2 Point1, Vector2 Point2)
            {
                this.Point1 = Point1;
                this.Point2 = Point2;
            }
        }

        #endregion
    }
}
