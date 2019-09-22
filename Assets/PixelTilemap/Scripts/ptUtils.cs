using UnityEngine;

namespace MaxivmoInk.PixelTilemap
{
    public static class ptUtils
    {

        #region publicMethods
        /// <summary>
        /// From grid to world
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetGridWorldPos(ptTilemap tilemap, int gridX, int gridY)
        {
            return tilemap.transform.TransformPoint(new Vector2((gridX + .5f) * tilemap.CellSize.x, (gridY + .5f) * tilemap.CellSize.y));
        }

        /// <summary>
        /// From world to grid
        /// </summary>
        public static Vector2Int GetGridPosition(ptTilemap tilemap, Vector2 locPosition)
        {
            int x = Mathf.FloorToInt(locPosition.x / tilemap.CellSize.x + ptTilemap.CHUNK_SIZE_X/2f);
            int y = Mathf.FloorToInt(locPosition.y / tilemap.CellSize.y + ptTilemap.CHUNK_SIZE_Y / 2f);
            return new Vector2Int(x, y);
        }
        /// <summary>
        /// From world x to grid x
        /// </summary>
        public static int GetGridX(ptTilemap tilemap, Vector2 locPosition)
        {
            return GetGridX(locPosition, tilemap.CellSize);
        }
        /// <summary>
        /// From world x to grid x
        /// </summary>
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
