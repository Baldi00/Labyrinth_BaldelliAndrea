using DBGA.Common;
using DBGA.Tiles;

namespace DBGA.ThroughScenes
{
    public static class ThroughScenesParameters
    {
        public static int GridSize { set; get; }
        public static TilesList TilesList { set; get; }
        public static MapElementsList MapElementsList { set; get; }
        public static bool AnimateGeneration { set; get; }
        public static bool MapGenerationError { set; get; }
    }
}
