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
        private ulong _bacteriaHeight = 0;

        public int GroundHeight
        {
            get { return _height; }
            set { _height = MathUtil.Clamp(value, GlobalConstants.MinTerraignHeight, GlobalConstants.MaxTerraignHeight); }
        }

        public ulong BacteriaHeight
        {
            get { return _bacteriaHeight; }
            private set
            {
                if (value < GlobalConstants.MinBacteriaDraw)
                    _bacteriaHeight = 0;
                else
                    _bacteriaHeight = value;
                    //_bacteriaHeight = MathUtil.Clamp(value, 0, GlobalConstants.MaxWorldHeight - _height);
            }
        }

        public bool HadBacteria { get; set; }

        public ulong BufferBacteriaHeight { get; set; }
        public ulong TotalHeight { get { return Convert.ToUInt64(_height) + _bacteriaHeight; } }
        public ulong TotalWithBuffer { get { return TotalHeight + BufferBacteriaHeight; } }

        public void FlushBuffer()
        {
            if (BufferBacteriaHeight != 0)
            {
                HadBacteria = true;
                BacteriaHeight += BufferBacteriaHeight;
                BufferBacteriaHeight = 0;
            }
        }
    }
}
