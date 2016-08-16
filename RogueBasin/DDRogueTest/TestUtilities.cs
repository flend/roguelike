using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueBasin;
using System.Reflection;
using System.IO;

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
