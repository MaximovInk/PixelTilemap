using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MaxivmoInk.PixelTilemap
{
    public static class ptUtils
    {

        #region publicMethods

        public static Vector3 GetGridWorldPos(ptTilemap tilemap, int gridX, int gridY)
        {
            return tilemap.transform.TransformPoint(new Vector2((gridX + .5f) * tilemap.CellSize.x, (gridY + .5f) * tilemap.CellSize.y));
        }

        public static Vector3 GetGridWorldPos(int gridX, int gridY, Vector2 cellSize)
        {
            return new Vector2((gridX + .5f) * cellSize.x, (gridY + .5f) * cellSize.y);
        }

        public static Vector2Int GetGridPosition(ptTilemap tilemap, Vector2 locPosition)
        {
            int x = Mathf.FloorToInt(locPosition.x / tilemap.CellSize.x + ptTilemap.CHUNK_SIZE_X/2f);
            int y = Mathf.FloorToInt(locPosition.y / tilemap.CellSize.y + ptTilemap.CHUNK_SIZE_Y / 2f);
            return new Vector2Int(x, y);
        }

        public static int GetGridX(ptTilemap tilemap, Vector2 locPosition)
        {
            return GetGridX(locPosition, tilemap.CellSize);
        }

        public static int GetGridY(ptTilemap tilemap, Vector2 locPosition)
        {
            return GetGridY(locPosition, tilemap.CellSize);
        }

        #endregion

        #region privateMethods

        private static int GetGridX(Vector2 position, Vector2 cellSize)
        {
            return Mathf.FloorToInt((position.x + Vector2.kEpsilon) / cellSize.x);
        }

        private static int GetGridY(Vector2 position, Vector2 cellSize)
        {
            return Mathf.FloorToInt((position.y + Vector2.kEpsilon) / cellSize.y);
        }

        #endregion


    }
}
