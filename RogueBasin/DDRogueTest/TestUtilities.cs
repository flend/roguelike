using RogueBasin;
using System.IO;
using System.Reflection;

namespace DDRogueTest
{
    public static class TestUtilities
    {
        public static RoomTemplate LoadTemplateFromAssemblyFile(string filePath)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream roomFileStream = _assembly.GetManifestResourceStream(filePath);
            return new RoomTemplateLoader(roomFileStream, StandardTemplateMapping.terrainMapping).LoadTemplateFromFile();
        }
    }
}
