using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TreasureHunt
{
    class Program
    {
        public static string textFile = "Input.txt";

        public static void PrintInput(string[] lines)
        {
            foreach (string line in lines)
                Console.WriteLine(line);
        }

        
        public static void PrintMap(Terrain terrain)
        {
            foreach (string line in terrain.Map)
                Console.WriteLine(line);
        }

        

        static void Main(string[] args)
        {

            string path = Directory.GetCurrentDirectory();
            if(!path.Contains("TreasureHunt"))
                throw new DirectoryNotFoundException("Couldn't find \\TreasureHunt\\ directory");
            path = string.Concat(path.AsSpan(0, path.IndexOf("TreasureHunt")), "\\TreasureHunt\\");

            string[] lines = File.ReadAllLines(path+textFile);
            Terrain game = new Terrain(lines);
            PrintMap(game);
        }
    }
}