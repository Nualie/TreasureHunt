using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TreasureHunt
{
    public class Program
    {
        static void Main(string[] args)
        {
            string path = Terrain.GetUpstreamDirectory("TreasureHunt");
            string[] lines = Terrain.ReadFile(path, "Input.txt");
            Terrain game = new Terrain(lines);
            Terrain.PrintMap(game);
            game.PlayGame();
            Terrain.PrintMap(game);
            Terrain.WriteOutputFile(path, game.OutputFile, game);
        }
    }
}