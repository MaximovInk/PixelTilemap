using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.PixelTilemap
{
    [CreateAssetMenu(fileName = "ptPalette",menuName = "Create ptPalette")]
    public class ptPalette : ScriptableObject
    {
        public List<Color> colors = new List<Color>();
    }
}
