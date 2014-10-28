using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Engine;
using System.Linq;
using SiliconStudio.Paradox.Input;
using System.Collections.Generic;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Panels;
using System;

namespace Bacteriophage
{
    public class BacteriophageGame : Game
    {
        public WorldMap Map { get; set; }

        // Setting Virtual Resolution so that the screen has 640 and 1136 of Width and Height respectively.
        // Note that the Z component indicates the near and farplane [near, far] = [-10, 10]
        private static readonly Vector3 GameVirtualResolution = new Vector3(800, 500, 10f);

        public BacteriophageGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferWidth = (int)GameVirtualResolution.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = (int)GameVirtualResolution.Y;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var spriteRenderer = new SpriteRenderer(Services);
            RenderSystem.Pipeline.Renderers.Add(spriteRenderer);

            // Set virtual Resolution
            VirtualResolution = GameVirtualResolution;

            IsFixedTimeStep = false;

            Map = new WorldMap(Services, 16, 10, 100);

            Script.Add(RunLoop);
        }

        public async Task RunLoop()
        {
            while (true)
            {
                await Script.NextFrame();
                Map.DrawMap();

                // Set FPS stuff in title for easy viewing
                //Window.Title = String.Format("FPS: {0} [{1}, {2}, {3}, {4}]", DrawTime.FramePerSecond, DrawTime.FrameCount, DrawTime.TimePerFrame, DrawTime.IsRunningSlowly, DrawTime.Total);

                if (Input.IsKeyDown(Keys.Down))
                    Map.RegenerateMap(Map.WidthResolution, Map.HeightResolution - 1);
                if (Input.IsKeyDown(Keys.Up))
                    Map.RegenerateMap(Map.WidthResolution, Map.HeightResolution + 1);
                if (Input.IsKeyDown(Keys.Left))
                    Map.RegenerateMap(Map.WidthResolution + 1, Map.HeightResolution);
                if (Input.IsKeyDown(Keys.Right))
                    Map.RegenerateMap(Map.WidthResolution - 1, Map.HeightResolution);

                if (Input.IsKeyPressed(Keys.G))
                    Map.RegenerateMap(DateTime.Now.Millisecond);

                if (Input.IsKeyPressed(Keys.W))
                    Map.RegenerateMap(Map.WidthResolution, Map.HeightResolution, (int)Map.TerraignFlatness + 1, Map.TerraignStepDetail);
                if (Input.IsKeyPressed(Keys.S))
                    Map.RegenerateMap(Map.WidthResolution, Map.HeightResolution, (int)Map.TerraignFlatness - 1, Map.TerraignStepDetail);
                if (Input.IsKeyDown(Keys.A))
                    Map.RegenerateMap(Map.WidthResolution, Map.HeightResolution, (int)Map.TerraignFlatness, Map.TerraignStepDetail + 1);
                if (Input.IsKeyDown(Keys.D))
                    Map.RegenerateMap(Map.WidthResolution, Map.HeightResolution, (int)Map.TerraignFlatness, Map.TerraignStepDetail - 1);

                if (Input.IsKeyPressed(Keys.F))
                    Map.run = !Map.run;
                if (Input.IsKeyPressed(Keys.Z))
                    Map.stepMode = !Map.stepMode;

                if (Input.IsKeyPressed(Keys.Space))
                    Map.stepForward = true;

                Cel cel = Map.GetCelAtPosition(Input.MousePosition);
                Window.Title = String.Format("TBacteria: {0} | Ground: {1}, Bacteria: {2:N4} | FPS: {3} | {4}, StepMode: {5}, Step: {6}", Map.totalBacteria, cel.TerraignInfo.GroundHeight, cel.TerraignInfo.BacteriaHeight, DrawTime.FramePerSecond, Map.run ? "Play" : "Pause", Map.stepMode, Map.stepCount);
            }
        }
    }
}
