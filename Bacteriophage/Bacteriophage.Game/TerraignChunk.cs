using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacteriophage
{
    public class TerraignChunk
    {
        private int _height = 5;
        private float _bacteriaHeight = 0;

        public int GroundHeight
        {
            get { return _height; }
            set { _height = MathUtil.Clamp(value, GlobalConstants.MinTerraignHeight, GlobalConstants.MaxTerraignHeight); }
        }

        public float BacteriaHeight
        {
            get { return _bacteriaHeight; }
            private set
            {
                if (value < GlobalConstants.MinBacteriaDraw)
                    _bacteriaHeight = 0f;
                else
                    _bacteriaHeight = (float)MathUtil.Clamp(value, 0, GlobalConstants.MaxWorldHeight - _height);
            }
        }

        public bool HadBacteria { get; set; }

        public float BufferBacteriaHeight { get; set; }
        public float TotalHeight { get { return _height + _bacteriaHeight; } }
        public float TotalWithBuffer { get { return TotalHeight + BufferBacteriaHeight; } }

        public void FlushBuffer()
        {
            if (BufferBacteriaHeight != 0)
            {
                HadBacteria = true;
                BacteriaHeight = BufferBacteriaHeight;
                BufferBacteriaHeight = 0;
            }
        }
    }
}
