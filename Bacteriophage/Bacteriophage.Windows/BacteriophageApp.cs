
namespace Bacteriophage
{
    class BacteriophageApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new BacteriophageGame())
            {
                game.Run();
            }
        }
    }
}
