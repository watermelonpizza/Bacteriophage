using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacteriophage
{
    public class Cel
    {
        public Texture Texture { get; private set; }
        public Color TerraignColour { get; private set; }
        public Vector2 MapCoordinate { get; private set; }

        public float Width { get; private set; }
        public float Height { get; private set; }
        public TerraignChunk TerraignInfo { get; private set; }

        public Cel UpCel { get; set; }
        public Cel DownCel { get; set; }
        public Cel LeftCel { get; set; }
        public Cel RightCel { get; set; }

        public Cel BiddingCel { get; private set; }
        public ulong BiddingCelBacteriaBuffer { get; private set; }

        public List<Cel> LowerCels { get; set; }
        public List<Cel> HigherCels { get; set; }
        public List<Cel> LowerTotalCels { get; private set; }
        public List<Cel> HigherTotalCels { get; private set; }

        public List<Cel> LowerTotalExcludeHigherCels
        {
            get
            {
                List<Cel> list = new List<Cel>(4);

                foreach (Cel cel in LowerTotalCels)
                {
                    if (!HigherCels.Contains(cel))
                        list.Add(cel);
                }

                return list;
            }
        }

        private Vector2 PixelPosition { get; set; }
        private RectangleF drawRectange { get; set; }

        public Cel(Texture texture, float width, float height, float x, float y, int xCoord, int yCoord, int terraignHeight)
        {
            Texture = texture;

            Width = width;
            Height = height;
            PixelPosition = new Vector2(x, y);
            MapCoordinate = new Vector2(xCoord, yCoord);

            TerraignInfo = new TerraignChunk();
            TerraignInfo.GroundHeight = terraignHeight;

            LowerCels = new List<Cel>(4);
            HigherCels = new List<Cel>(4);
            LowerTotalCels = new List<Cel>(4);
            HigherTotalCels = new List<Cel>(4);

            drawRectange = new RectangleF(PixelPosition.X, PixelPosition.Y, Width, Height);

            TerraignColour = new Color(terraignHeight / 10f, terraignHeight / 10f, terraignHeight / 10f);
        }

        public void UpdateState()
        {
            HigherTotalCels.Clear();
            LowerTotalCels.Clear();

            if (UpCel != null)
            {
                if (TerraignInfo.TotalHeight > UpCel.TerraignInfo.TotalHeight)
                    LowerTotalCels.Add(UpCel);
                else if (TerraignInfo.TotalHeight < UpCel.TerraignInfo.TotalHeight)
                    HigherTotalCels.Add(UpCel);
            }

            if (DownCel != null)
            {
                if (TerraignInfo.TotalHeight > DownCel.TerraignInfo.TotalHeight)
                    LowerTotalCels.Add(DownCel);
                else if (TerraignInfo.TotalHeight < DownCel.TerraignInfo.TotalHeight)
                    HigherTotalCels.Add(DownCel);
            }

            if (LeftCel != null)
            {
                if (TerraignInfo.TotalHeight > LeftCel.TerraignInfo.TotalHeight)
                    LowerTotalCels.Add(LeftCel);
                else if (TerraignInfo.TotalHeight < LeftCel.TerraignInfo.TotalHeight)
                    HigherTotalCels.Add(LeftCel);
            }

            if (RightCel != null)
            {
                if (TerraignInfo.TotalHeight > RightCel.TerraignInfo.TotalHeight)
                    LowerTotalCels.Add(RightCel);
                else if (TerraignInfo.TotalHeight < RightCel.TerraignInfo.TotalHeight)
                    HigherTotalCels.Add(RightCel);
            }

            if (TerraignInfo.BacteriaHeight > 0)
                TerraignColour = new Color(0f, TerraignInfo.TotalHeight / 10f, 0f);
            else
                TerraignColour = new Color(TerraignInfo.GroundHeight / 10f, TerraignInfo.TotalHeight / GlobalConstants.MaxBacteria, TerraignInfo.GroundHeight / 10f);
        }

        public int CellBorderCount()
        {
            int count = 0;

            count = UpCel != null ? ++count : count;
            count = DownCel != null ? ++count : count;
            count = LeftCel != null ? ++count : count;
            count = RightCel != null ? ++count : count;

            return count;
        }

        public List<Cel> OrderedLowerBufferedCels()
        {
            var orderedCelList = from cel in LowerTotalCels
                                 where TerraignInfo.BacteriaHeight > cel.TerraignInfo.BacteriaHeight + cel.TerraignInfo.BufferBacteriaHeight
                                 orderby cel.TerraignInfo.TotalWithBuffer ascending
                                 select cel;

            return orderedCelList.ToList();
        }

        public void CalculateBids()
        {
            if (BiddingCel != null)
                BiddingCel.TerraignInfo.BufferBacteriaHeight = BiddingCelBacteriaBuffer;

            BiddingCel = null;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, drawRectange, TerraignColour);
        }
    }
}
