using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaxivmoInk.PixelTilemap
{
    [ExecuteInEditMode]
    public class ptTilemap : MonoBehaviour {
        //Changing it breaks the existing maps!
        public const int CHUNK_SIZE_X = 32;
        //Changing it breaks the existing maps!
        public const int CHUNK_SIZE_Y = 32;
        /// <summary>
        /// Auto remove unused chunks
        /// </summary>
        public bool AutoTrim = true;

        public Vector2 CellSize { get { return cell_size; } set { cell_size = value; UpdateSizeOfChunks(); } }

        public bool DisplayChunksInHierarchy { get { return display_chunks_in_hierarchy; } set { display_chunks_in_hierarchy = value; UpdateChunksFlags(); } }

        public bool ColliderEnabled { get { return colliderEnabled; } set { colliderEnabled = value; UpdateCollidersActive(); } }

        public int SortingOrder { get { return sortingOrder; } set { sortingOrder = value; UpdateChunksRenderer(); } }

        public Color ColorTint { get { return colorTint; } set { colorTint = value; UpdateChunksRenderer(); } }

        public Material Material { get { return material; } set { material = value; UpdateChunksRenderer(); } }

        public string Tag { get { return _tag; } set { _tag = value; UpdateChunksFlags(); } }
        
        public LayerMask LayerMask { get { return layerMask; } set { layerMask = value; UpdateChunksFlags(); } }

        public PhysicsMaterial2D PhysicsMaterial2D { get { return physMaterial; }set { physMaterial = value; UpdateCollidersProperties(); } }

        public bool ColliderIsTrigger { get { return isTrigger; } set { isTrigger = value; UpdateCollidersProperties(); } }
        /// <summary>
        /// Palette, for update use UpdatePalette()
        /// </summary>
        public Color[] Palette { get { return palette; } }

        [SerializeField]
        private Vector2 cell_size = Vector2.one;
        [SerializeField]
        private bool display_chunks_in_hierarchy = true;
        [SerializeField]
        private bool colliderEnabled;
        [SerializeField]
        private bool allowPaintingOutOfBounds = true;
        [SerializeField]
        private int sortingOrder;
        [SerializeField]
        private LayerMask layerMask;
        [SerializeField]
        private bool isTrigger;
        [SerializeField]
        private PhysicsMaterial2D physMaterial;
        [SerializeField]
        private Color colorTint = Color.white;
        [SerializeField]
        private Material material;
        [SerializeField]
        private string _tag = "Untagged";
        [SerializeField]
        private List<ptChunk> chunks = new List<ptChunk>();
        [SerializeField]
        private Color[] palette = new Color[0];
        /// <summary>
        /// Set a color in the given coordinates
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="color">Color</param>
        public void SetPixel(int x, int y, Color color)
        {
            var ch = GetOrCreateChunk(x, y);
            if (ch == null)
                return;
            ch.SetPixel(x, y, color);
        }
        /// <summary>
        /// Get color in given coordinates
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Color</returns>
        public Color GetPixel(int x, int y)
        {
            var ch = GetOrCreateChunk(x, y);
            if (ch == null)
                return Color.clear;
            return ch.GetPixel(x, y);
        }

        private ptChunk GetOrCreateChunk(int x, int y)
        {
            var ch_x = (x < 0 ? (x + 1 - CHUNK_SIZE_X) : x) / CHUNK_SIZE_X;
            var ch_y = (y < 0 ? (y + 1 - CHUNK_SIZE_Y) : y) / CHUNK_SIZE_Y;

            var ch = chunks.FirstOrDefault(c => c.x == ch_x && c.y == ch_y);

            if (ch == null)
            {
                if (!allowPaintingOutOfBounds)
                {
                    Debug.LogError("Operation goes out of bounds tilemap");
                    return null;
                }
                var go = new GameObject();
                go.transform.SetParent(transform);
                if (!display_chunks_in_hierarchy)
                    go.hideFlags = HideFlags.HideInHierarchy;
                ch = go.AddComponent<ptChunk>();
                ch.Init(this);
                ch.transform.localPosition = new Vector2(ch_x * CHUNK_SIZE_X, ch_y * CHUNK_SIZE_Y);
                chunks.Add(ch);
                ch.x = ch_x;
                ch.y = ch_y;
            }

            return ch;
        }

        private void UpdateCollidersProperties()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].UpdateColliderProperties();
            }
        }

        private void UpdateCollidersActive()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].UpdateColliderActive();
            }
        }

        private void UpdateChunksRenderer()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].UpdateRenderer();
            }
        }

        private void UpdateChunksFlags()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].UpdateFlags();
            }

        }

        private void UpdateSizeOfChunks()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].transform.localScale = new Vector3(cell_size.x,cell_size.y,1);
                chunks[i].transform.localPosition = new Vector3(chunks[i].x*CHUNK_SIZE_X*cell_size.x,chunks[i].y*CHUNK_SIZE_Y*cell_size.y);
            }
        }
        
        /// <summary>
        /// Clear all map to given color
        /// </summary>
        /// <param name="color"></param>
        public void Fill(Color color)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].Fill(color);
            }
        }
        /// <summary>
        /// Clear all map to transparent color
        /// </summary>
        public void Clear()
        {
            Fill(Color.clear);
        }
        /// <summary>
        /// Update palette property
        /// </summary>
        public void UpdatePalette()
        {
            HashSet<Color> newPallete = new HashSet<Color>();

            for (int i = 0; i < chunks.Count; i++)
            {
                newPallete.UnionWith(chunks[i].GetPalette());
            }

            palette = newPallete.ToArray();
        }
        /// <summary>
        /// Update map
        /// </summary>
        public void UpdateMap()
        {
            if (AutoTrim)
                Trim();

            for (int i = 0; i < chunks.Count; i++)
            {
                if(chunks[i].texIsDirty)
                    chunks[i].UpdateMesh();
            }
        }
        /// <summary>
        /// Regenerate collider
        /// </summary>
        public void RegenerateCollider()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                if(chunks[i].colIsDirty)
                    chunks[i].GenerateCollider();
            }
        }
        /// <summary>
        /// Trim a map
        /// </summary>
        public void Trim()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].IsEmpty())
                {
                    DestroyImmediate(chunks[i].gameObject);
                    chunks.RemoveAt(i);
                    i--;
                }
            }
        }
        
       
    }
}