using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueBasin.TerrainSets
{
    class Pond
    {
        public Point Origin;

        public int DiggingChance { get; set; }
        public int MaxTries { get; set; }
        public double PercOpenRequired { get; set; }
        public double RequiredStairDistance { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        private FieldMap<bool> basemap;

        public Pond(Point origin, int width, int height)
        {
            Origin = origin;
            Width = width;
            Height = height;
        }

        public IEnumerable<Point> Generate()
        {
            DiggingChance = 20;
            PercOpenRequired = 0.4;
            MaxTries = 10;

            basemap = new FieldMap<bool>(Width, Height);

            //Start digging from a random point
            int noDiggingPoints = 4 + Game.Random.Next(4);

            for (int i = 0; i < noDiggingPoints; i++)
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                Dig(x, y);
            }

            //Check if we are too small, and add more digs
            int loopIters = 0;
            while (CalculatePercentageOpen() < PercOpenRequired && loopIters < MaxTries)
            {
                int x = Game.Random.Next(Width);
                int y = Game.Random.Next(Height);

                Dig(x, y);
                loopIters++;
            }

            LogFile.Log.LogEntryDebug("Finished pond after " + loopIters + "/" + MaxTries + " attempts", LogDebugLevel.Medium);

            var allMapPoints = basemap.getAllCells(true, (x, y) => (x == y));

            return allMapPoints.Select(p => p + Origin - new Point(Width / 2, Height / 2));
        }

        private void SetSquareOpen(int i, int j)
        {
            basemap.setCell(new Point(i, j), true);
        }

        private double CalculatePercentageOpen()
        {
            int totalOpen = 0;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (basemap.getCell(new Point(i, j)))
                        totalOpen++;
                }
            }

            double percOpen = totalOpen / (double)(Width * Height);

            return percOpen;
        }

        public void Dig(int x, int y)
        {
            //Check this is a valid square to dig
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            //Already dug
            if (basemap.getCell(new Point(x, y)))
                return;

            //Set this as open
            SetSquareOpen(x, y); 

            //Did in all the directions

            Random rand = Game.Random;

            //TL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y - 1);
            //T
            if (rand.Next(100) < DiggingChance)
                Dig(x, y - 1);
            //TR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y - 1);
            //CL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y);
            //CR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y);
            //BL
            if (rand.Next(100) < DiggingChance)
                Dig(x - 1, y + 1);
            //B
            if (rand.Next(100) < DiggingChance)
                Dig(x, y + 1);
            //BR
            if (rand.Next(100) < DiggingChance)
                Dig(x + 1, y + 1);
        }
    }
}
