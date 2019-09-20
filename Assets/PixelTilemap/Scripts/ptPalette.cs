using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MaximovInk.PixelTilemap
{
    [CreateAssetMenu(fileName = "ptPalette",menuName = "Create ptPalette")]
    public class ptPalette : ScriptableObject
    {
        public List<Color> colors = new List<Color>();
    }
}
