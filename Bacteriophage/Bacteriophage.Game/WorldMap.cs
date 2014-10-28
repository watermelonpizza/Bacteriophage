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
        public const float MinBacteriaFlow = 0.0f;
        public const float MinBacteriaDraw = 0.05f;
        public const float MaxBacteriaFlowBack = 0.08f;
        public const float BacteriaFlowAmountPerTick = 0.05f;
        public const float BacteriaJumpUpTheshold = 0.08f;
        public const ulong MaxBacteria = 100000000;
        public const int BacteriaPercentFlowAmountPerTick = 18;
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

        public ulong totalSpawnedBacteria = 0;
        public ulong totalBacteria = 0;

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
                Grid[0, 0].TerraignInfo.BufferBacteriaHeight = GlobalConstants.MaxBacteria;
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
                        foreach (Cel cel in thisCel.OrderedLowerBufferedCels())
                        {
                            if (thisCel.TerraignInfo.BacteriaHeight + thisCel.TerraignInfo.BufferBacteriaHeight > GlobalConstants.MinBacteriaFlow)
                            {
                                ulong flowAmount = (thisCel.TerraignInfo.BacteriaHeight / 100) * GlobalConstants.BacteriaPercentFlowAmountPerTick;
                                cel.TerraignInfo.BufferBacteriaHeight += flowAmount;
                                thisCel.TerraignInfo.BufferBacteriaHeight -= flowAmount;
                            }
                        }
                    }
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