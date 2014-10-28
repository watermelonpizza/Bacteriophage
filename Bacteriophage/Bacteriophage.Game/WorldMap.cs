using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SiliconStudio.Paradox.Games;

namespace Bacteriophage
{
    public struct GlobalConstants
    {
        public const int MaxTerraignHeight = 9;
        public const int MinTerraignHeight = 1;
        public const int MaxWorldHeight = 10;
        public const int MinWorldHeight = 0;
        public const float MinBacteriaFlow = 0.2f;
        public const float MinBacteriaDraw = 0.05f;
        public const float MaxBacteriaFlowBack = 0.08f;
        public const float BacteriaJumpUpTheshold = 0.08f;
        public const int BacteriaDumpKeepPercent = 5;
        public const int BacteriaSuckPercent = 5;
    }

    public class WorldMap : ScriptContext
    {
        public double TerraignFlatness { get { return (int)_terraignFlatness; } private set { _terraignFlatness = MathUtil.Clamp(value, 1, 100); } }
        public int TerraignStepDetail { get { return _terraignStepDetail; } private set { _terraignStepDetail = MathUtil.Clamp(value, 1, 50); } }
        public int WidthResolution { get { return _widthResolution; } private set { _widthResolution = MathUtil.Clamp(value, 1, int.MaxValue); } }
        public int HeightResolution { get { return _heightResolution; } private set { _heightResolution = MathUtil.Clamp(value, 1, int.MaxValue); } }

        public Cel[,] Grid { get; private set; }

        public bool run = false;
        public bool stepMode = true;
        public bool stepForward = false;
        public int stepCount = 0;

        public float totalSpawnedBacteria = 0;
        public float totalBacteria = 0;

        private double _terraignFlatness = 1;
        private int _terraignStepDetail = 1;
        private int _widthResolution = 100;
        private int _heightResolution = 100;

        private int tickDelay = 100;
        private int spawnTickDelay = 5000;
        private TimeSpan timeSinceLastUpdate = new TimeSpan();
        private TimeSpan timeSinceLastSpawn = new TimeSpan();
        private double seed = DateTime.Now.Millisecond;
        private Texture2D celTexture { get; set; }
        private SpriteBatch spriteBatch { get; set; }

        public WorldMap(IServiceRegistry registry, int widthResolution, int heightResolution, int terraignFlatness)
            : base(registry)
        {
            TerraignFlatness = terraignFlatness;
            TerraignStepDetail = 10;
            WidthResolution = widthResolution;
            HeightResolution = heightResolution;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            GenerateMap();
        }

        private void GenerateMap()
        {
            Grid = new Cel[WidthResolution, HeightResolution];

            float celWidth = VirtualResolution.X / WidthResolution;
            float celHeight = VirtualResolution.Y / HeightResolution;

            celTexture = Texture2D.New(GraphicsDevice, Asset.Load<Texture2D>("grid_4"));

            for (int i = 0; i < WidthResolution; i++)
            {
                for (int j = 0; j < HeightResolution; j++)
                {
                    float input = Noise.Noise.GetNoise(i / TerraignFlatness, j / TerraignFlatness, seed) * 100;
                    int testvalue = Convert.ToInt32((Test.RoundToInterval(input, TerraignStepDetail, Test.IntervalRounding.Nearest) / 100) * 10);

                    Grid[i, j] = new Cel(celTexture, celWidth, celHeight, celWidth * i, celHeight * j, i, j, testvalue);
                }
            }

            // Map the surrounding cels
            for (int i = 0; i < WidthResolution; i++)
            {
                for (int j = 0; j < HeightResolution; j++)
                {
                    if (j > 0)
                    {
                        Grid[i, j].UpCel = Grid[i, j - 1];

                        if (Grid[i, j].TerraignInfo.GroundHeight > Grid[i, j - 1].TerraignInfo.GroundHeight)
                            Grid[i, j].LowerCels.Add(Grid[i, j - 1]);
                        else if (Grid[i, j].TerraignInfo.GroundHeight < Grid[i, j - 1].TerraignInfo.GroundHeight)
                            Grid[i, j].HigherCels.Add(Grid[i, j - 1]);
                    }

                    if (j < (HeightResolution - 1))
                    {
                        Grid[i, j].DownCel = Grid[i, j + 1];

                        if (Grid[i, j].TerraignInfo.GroundHeight > Grid[i, j + 1].TerraignInfo.GroundHeight)
                            Grid[i, j].LowerCels.Add(Grid[i, j + 1]);
                        else if (Grid[i, j].TerraignInfo.GroundHeight < Grid[i, j + 1].TerraignInfo.GroundHeight)
                            Grid[i, j].HigherCels.Add(Grid[i, j + 1]);
                    }

                    if (i > 0)
                    {
                        Grid[i, j].LeftCel = Grid[i - 1, j];

                        if (Grid[i, j].TerraignInfo.GroundHeight > Grid[i - 1, j].TerraignInfo.GroundHeight)
                            Grid[i, j].LowerCels.Add(Grid[i - 1, j]);
                        else if (Grid[i, j].TerraignInfo.GroundHeight < Grid[i - 1, j].TerraignInfo.GroundHeight)
                            Grid[i, j].HigherCels.Add(Grid[i - 1, j]);
                    }

                    if (i < (WidthResolution - 1))
                    {
                        Grid[i, j].RightCel = Grid[i + 1, j];

                        if (Grid[i, j].TerraignInfo.GroundHeight > Grid[i + 1, j].TerraignInfo.GroundHeight)
                            Grid[i, j].LowerCels.Add(Grid[i + 1, j]);
                        else if (Grid[i, j].TerraignInfo.GroundHeight < Grid[i + 1, j].TerraignInfo.GroundHeight)
                            Grid[i, j].HigherCels.Add(Grid[i + 1, j]);
                    }
                }
            }
        }

        public void RegenerateMap(double seed)
        {
            this.seed = MathUtil.Clamp(seed, 0, 10000);

            GenerateMap();
        }

        public void RegenerateMap(int widthResolution, int heightResolution)
        {
            HeightResolution = heightResolution;

            GenerateMap();
            WidthResolution = widthResolution;
        }

        public void RegenerateMap(int widthResolution, int heightResolution, int terraignFlatness, int terraignStepDetail)
        {
            TerraignFlatness = terraignFlatness;
            TerraignStepDetail = terraignStepDetail;
            WidthResolution = widthResolution;
            HeightResolution = heightResolution;

            GenerateMap();
        }

        public void DrawMap()
        {
            Random rand = new Random(Convert.ToInt32(seed));

            if (run && (Game.PlayTime.TotalTime - timeSinceLastSpawn).TotalMilliseconds > spawnTickDelay)
            {
                Grid[0, 0].TerraignInfo.BufferBacteriaHeight = 10;
                Grid[0, 0].TerraignInfo.FlushBuffer();
                totalSpawnedBacteria += Grid[0, 0].TerraignInfo.BacteriaHeight;
                timeSinceLastSpawn = Game.PlayTime.TotalTime;
            }

            if (!stepMode)
            {
                if ((Game.PlayTime.TotalTime - timeSinceLastUpdate).Milliseconds > tickDelay)
                {
                    UpdateCels();
                    timeSinceLastUpdate = Game.PlayTime.TotalTime;
                }
            }
            else
            {
                if (stepForward)
                {
                    UpdateCels();
                    stepForward = false;
                }
            }

            spriteBatch.Begin();

            for (int i = 0; i < WidthResolution; i++)
            {
                for (int j = 0; j < HeightResolution; j++)
                {
                    Grid[i, j].UpdateState();
                    Grid[i, j].Draw(spriteBatch);
                }
            }

            spriteBatch.End();
        }

        public void UpdateCels()
        {
            stepCount++;
            totalBacteria = 0;

            for (int x = 0; x < WidthResolution; x++)
            {
                for (int y = 0; y < HeightResolution; y++)
                {
                    Cel thisCel = Grid[x, y];

                    if (thisCel.TerraignInfo.BacteriaHeight > GlobalConstants.MinBacteriaFlow)
                    {
                        float totalBacteriaPool = 0;

                        if (thisCel.LowerTotalCels.Count > 0)
                        {
                            totalBacteriaPool += thisCel.TerraignInfo.BacteriaHeight;

                            // Total up the pool
                            foreach (Cel lowerTotalCel in thisCel.LowerTotalCels)
                            {
                                if (thisCel.HigherCels.Contains(lowerTotalCel))
                                    totalBacteriaPool += lowerTotalCel.TerraignInfo.BacteriaHeight + (lowerTotalCel.TerraignInfo.GroundHeight - thisCel.TerraignInfo.GroundHeight);
                                else
                                    totalBacteriaPool += lowerTotalCel.TerraignInfo.BacteriaHeight;
                            }

                            // If not enough to fill lower gournd heigh cels just split it up between ground cels and move on
                            if (thisCel.LowerCels.Count > totalBacteriaPool)
                            {
                                float percentToKeep = (totalBacteriaPool / 100f) * GlobalConstants.BacteriaDumpKeepPercent;
                                float dividedBacteria = (totalBacteriaPool - percentToKeep) / thisCel.LowerCels.Count;

                                foreach (Cel lowerHeightCel in thisCel.LowerCels)
                                {
                                    lowerHeightCel.SetBid(dividedBacteria, dividedBacteria, thisCel);
                                }

                                thisCel.TerraignInfo.BufferBacteriaHeight = percentToKeep;
                            }
                            else
                            {
                                //if (thisCel.HigherCels.Count > 0)
                                //{
                                //    List<Cel> celsToPopulate = new List<Cel>();
                                //    int neededHeight = GlobalConstants.MaxWorldHeight;

                                //    foreach (Cel higherCel in thisCel.HigherCels)
                                //    {
                                //        if (higherCel.TerraignInfo.GroundHeight < neededHeight)
                                //        {
                                //            celsToPopulate.Clear();
                                //            neededHeight = higherCel.TerraignInfo.GroundHeight - thisCel.TerraignInfo.GroundHeight;
                                //            celsToPopulate.Add(higherCel);
                                //        }
                                //        else if (higherCel.TerraignInfo.GroundHeight == neededHeight)
                                //            celsToPopulate.Add(higherCel);
                                //    }

                                //    if (thisCel.TerraignInfo.BacteriaHeight > (neededHeight + GlobalConstants.BacteriaJumpUpTheshold))
                                //    {
                                //        float bacteriaDelta = (thisCel.TerraignInfo.BacteriaHeight - neededHeight - GlobalConstants.BacteriaJumpUpTheshold) / (celsToPopulate.Count + 1);

                                //        thisCel.TerraignInfo.BufferBacteriaHeight = neededHeight + bacteriaDelta + GlobalConstants.BacteriaJumpUpTheshold;

                                //        foreach (Cel higherCelToPopulate in celsToPopulate)
                                //        {
                                //            higherCelToPopulate.SetBid(bacteriaDelta, thisCel);
                                //        }

                                //        continue;
                                //    }
                                //}

                                totalBacteriaPool -= thisCel.LowerCels.Count;
                                foreach (Cel lowerHeightCel in thisCel.LowerCels)
                                {
                                    //lowerHeightCel.TerraignInfo.BufferBacteriaHeight += 1;
                                }

                                bool useAllLower = true;
                                float dividedBacteria = totalBacteriaPool / (thisCel.LowerTotalCels.Count + 1);
                                int lowestStep = GlobalConstants.MaxWorldHeight;

                                foreach (Cel higherCel in thisCel.HigherCels)
                                {
                                    if (thisCel.LowerTotalCels.Contains(higherCel) && higherCel.TerraignInfo.GroundHeight < lowestStep)
                                    {
                                        lowestStep = higherCel.TerraignInfo.GroundHeight - thisCel.TerraignInfo.GroundHeight;
                                    }
                                }

                                if (dividedBacteria < (lowestStep + GlobalConstants.BacteriaJumpUpTheshold))
                                {
                                    useAllLower = false;
                                    dividedBacteria = totalBacteriaPool / (thisCel.LowerTotalExcludeHigherCels.Count + 1);
                                }

                                if (useAllLower)
                                {
                                    foreach (Cel lowerCel in thisCel.LowerTotalCels)
                                    {
                                        if (thisCel.HigherCels.Contains(lowerCel))
                                        {
                                            lowerCel.SetBid(dividedBacteria - (lowerCel.TerraignInfo.GroundHeight - thisCel.TerraignInfo.GroundHeight), dividedBacteria, thisCel);
                                        }
                                        else if (!thisCel.LowerCels.Contains(lowerCel))
                                        {
                                            lowerCel.SetBid(dividedBacteria, dividedBacteria, thisCel);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Cel lowerCel in thisCel.LowerTotalExcludeHigherCels)
                                    {
                                        if (!thisCel.LowerCels.Contains(lowerCel))
                                        {
                                            lowerCel.SetBid(dividedBacteria, dividedBacteria, thisCel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (thisCel.LowerCels.Count > 0 && thisCel.TerraignInfo.BacteriaHeight < GlobalConstants.MinBacteriaFlow && thisCel.TerraignInfo.BacteriaHeight <= GlobalConstants.MaxBacteriaFlowBack)
                    {
                        float dividedBacteria = thisCel.TerraignInfo.BacteriaHeight / thisCel.LowerCels.Count;
                        thisCel.TerraignInfo.BufferBacteriaHeight -= thisCel.TerraignInfo.BacteriaHeight;

                        foreach (Cel lowerCel in thisCel.LowerCels)
                        {
                            lowerCel.TerraignInfo.BufferBacteriaHeight += dividedBacteria;
                        }
                    }
                }
            }

            for (int x = 0; x < WidthResolution; x++)
            {
                for (int y = 0; y < HeightResolution; y++)
                {
                    Grid[x, y].CalculateBids();
                }
            }

            for (int x = 0; x < WidthResolution; x++)
            {
                for (int y = 0; y < HeightResolution; y++)
                {
                    Grid[x, y].TerraignInfo.FlushBuffer();
                    totalBacteria += Grid[x, y].TerraignInfo.BacteriaHeight;
                }
            }
        }

        public Cel GetCelAtPosition(Vector2 position)
        {
            int xi = (int)Math.Floor(position.X * WidthResolution);
            int yi = (int)Math.Floor(position.Y * HeightResolution);
            return Grid[xi, yi];
        }

        private enum FlowDirection
        {
            None,
            Up,
            Down,
            Left,
            Right
        }
    }
}