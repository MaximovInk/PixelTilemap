using MaxivmoInk.PixelTilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MaximovInk.PixelTilemap
{
    public class TesterScript : MonoBehaviour
    {
        public Vector2 cellSize = Vector2.one;

        private ptTilemap tilemap;

        private void Start()
        {
            tilemap = GetComponent<ptTilemap>();

            tilemap.SetPixel(1, 1, Color.green);
            tilemap.SetPixel(1, 50, Color.red);

            tilemap.UpdateMap();


        }

        public void Update()
        {
            tilemap.CellSize = cellSize;
        }
    }
}
