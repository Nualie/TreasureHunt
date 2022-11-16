using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace TreasureHunt
{
    public class Terrain
    {
        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;

        private int spacing = 4;
        public string[] Map { get; private set; }

        private readonly List<Treasure> treasures = new List<Treasure>();
        private readonly List<Mountain> mountains = new List<Mountain>();
        private readonly List<Adventurer> adventurers = new List<Adventurer>();
        public string InputFile { get; private set; } = "Input.txt";
        public string OutputFile { get; private set; } = "Output.txt";

        private static readonly Regex RegNumber = new(@"\d+");
        private static readonly Regex RegString = new(@"[^-\d]+");
        private static readonly List<string> ValidDirections = new List<string>{ "N", "S", "E", "W" };

        public Terrain(string[] lines)
        {
            if (lines.Length == 0) throw new ArgumentNullException("The input file is empty.");
            List<int> lineData;
            foreach (string line in lines)
            {
                lineData = new List<int>();
                if (line.Length == 0) continue; //skip empty lines
                switch (line[0])
                {
                    case 'C': //map
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (X > 0 || Y > 0) throw new ArgumentException("Several maps in the same input file");
                        if (lineData.Count != 2) throw new ArgumentException("C: Input document is formatted wrong");
                        if(lineData[0]<1 || lineData[1]<1) throw new ArgumentException("Map size is wrong");
                        X = lineData[0];
                        Y = lineData[1];
                        break;
                    case 'M': //mountain
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (lineData.Count != 2) throw new ArgumentException("M: Input document is formatted wrong");
                        if (lineData[0] < 0 || lineData[1] < 0) throw new ArgumentException("Mountain coords are wrong");
                        mountains.Add(new Mountain(lineData[0], lineData[1]));

                        break;
                    case 'T': //treasure
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (lineData.Count != 3) throw new ArgumentException("T: Input document is formatted wrong");
                        if (lineData[0] < 0 || lineData[1] < 0) throw new ArgumentException("Treasure coords are wrong");
                        treasures.Add(new Treasure(lineData[0], lineData[1], lineData[2]));
                        break;
                    case 'A': //adventurer
                        List<string> strData = new List<string>();
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        strData.AddRange(ExtractString(line, RegString));
                        strData.RemoveAll(str => str ==" "); //not catching spaces, so they have to be removed manually
                        for (int i = 0; i < strData.Count; i++) strData[i] = strData[i].Trim(); //this now allows for names with spaces in them
                        if (lineData.Count != 2 || strData.Count != 4) throw new ArgumentException("A: Input document is formatted wrong");
                        if (lineData[0] < 0 || lineData[1] < 0) throw new ArgumentException("Adventurer coords are wrong");
                        adventurers.Add(new Adventurer(lineData[0], lineData[1], strData[1], strData[2], strData[3]));
                        spacing = Math.Max(spacing, strData[1].Length+3); //automatically adjust spacing for the name
                        break;
                    case '#': //comment, just ignore
                        break;
                    default:
                        throw new ArgumentException("Input document is formatted wrong");
                        
                }
            }
            Map = new string[Y];
            GenerateMap();            

        }

        public void SetSpacing(int spacing)
        {
            this.spacing = spacing;
            GenerateMap(); //automatically updates the map
        }

        public void GenerateMap()
        {
            Map = new string[Y];
            for (int i = 0; i < Y; i++)
            {
                Map[i] = String.Concat(Enumerable.Repeat(String.Concat(".", String.Concat(Enumerable.Repeat(" ", spacing))), X));
                Map[i] = Map[i].Trim();
            }
            foreach(Treasure t in treasures)
            {
                if (t.Amount == 0) continue;
                string str = "T(" + t.Amount + ")";
                Map[t.Y] = Map[t.Y].Remove(t.X*(spacing+1), str.Length).Insert(t.X* (spacing+1), str);
            }
            foreach (Mountain m in mountains)
            {
                string str = "M";
                Map[m.Y] = Map[m.Y].Remove(m.X * (spacing + 1), str.Length).Insert(m.X * (spacing + 1), str);
            }
            foreach (Adventurer a in adventurers)
            {
                string str = "A("+a.Name+")";

                if (a.X == X - 1)
                    Map[a.Y] = String.Concat(Map[a.Y].Remove(a.X * (spacing + 1), 1),str);
                else
                    Map[a.Y] = Map[a.Y].Remove(a.X * (spacing + 1), str.Length).Insert(a.X * (spacing + 1), str);
            }


        }

        private static char ReturnDirFromVector(int[] vector)
        {
            char dir = 'I'; //invalid value

            if (vector.Length != 2) throw new ArgumentOutOfRangeException("Direction vector dimension isn't 2");
            
            if (vector[1] == -1)
            {
                dir = 'N';
            }
            if (vector[0] == 1)
            {
                dir = 'E';
            }
            if (vector[1] == 1)
            {
                dir = 'S';
            }
            if (vector[0] == -1)
            {
                dir = 'W';
            }

            return dir;
        }

        private static int[] ReturnVectorFromDir(char currentdir,char inputdir)
        {
            int[] vector = new int[] { 0, 0 };

            switch (currentdir)
            {
                case 'N':
                    vector[1] = -1;// 0 -1
                    break;
                case 'E':
                    vector[0] = 1; // 1 0
                    break;
                case 'S':
                    vector[1] = 1; // 0 1
                    break;
                case 'W':
                    vector[0] = -1;// -1 0
                    break;
                default:
                    throw new ArgumentException("Current direction is invalid");
            }

            switch (inputdir)
            {
                case 'D':
                    if (vector[0] == 0)
                        vector[1] *= -1;
                    vector = vector.Reverse().ToArray<int>();
                    break;
                case 'G':
                    if (vector[1] == 0)
                        vector[0] *= -1;
                    vector = vector.Reverse().ToArray<int>();
                    break;
                case '0': //used for init
                    break;
                default:
                    throw new ArgumentException("Input direction is invalid");
            }

            return vector;
        }

        public void PlayGame()
        {
            int[] vector;
            foreach (Adventurer a in adventurers)
            {
                vector = ReturnVectorFromDir(a.Direction[0], '0'); //initialize adventurer direction
                int[] projected = { vector[0], vector[1] }; //initialize test vector
                foreach (char c in a.path)
                {
                    if (!ValidDirections.Contains(a.Direction)) throw new ArgumentException("Invalid direction");
                    projected[0] = Math.Min(Math.Max(a.X + vector[0], 0), X - 1);//test vector (cleaner)
                    projected[1] = Math.Min(Math.Max(a.Y + vector[1], 0), Y - 1);//test vector (cleaner)

                    if (c == 'A' && !(Map[projected[1]][projected[0] * (spacing + 1)] == 'M') && !(Map[projected[1]][projected[0] * (spacing + 1)] == 'A'))
                        //checking there are no obstacles through the map is faster than looking through lists
                    {
                        a.SetX(projected[0]);
                        a.SetY(projected[1]);

                        if (Map[a.Y][a.X * (spacing + 1)] == 'T')
                        {
                            foreach (Treasure t in treasures)
                            {
                                if (t.X == a.X && t.Y == a.Y && t.Amount > 0)
                                {
                                    a.AdventurerTakesTreasure(t);
                                    t.AdventurerTakesTreasure();
                                    break;
                                }
                            }
                        }
                    }
                    else if (c == 'G' || c == 'D')
                    {
                        vector = ReturnVectorFromDir(a.Direction[0], c);
                        a.Direction = ReturnDirFromVector(vector).ToString();
                    }

                    GenerateMap();
                }
                
            }
        }

        internal class Mountain
        {
            public int X { get; private set; }
            public int Y { get; private set; }

            public Mountain(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        internal class Treasure 
        { 
            public int X { get; private set; }  
            public int Y { get; private set; }  
            public int Amount { get; private set; }

            public Treasure(int x, int y, int amount)
            {
                X = x;
                Y = y;
                Amount = amount;
            }

            public void AdventurerTakesTreasure()
            {
                if (Amount > 0) Amount--;
            }
        }

        internal class Adventurer
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public int TreasuresGotten { get; private set; } = 0;
            public string Name { get; private set; } = "A";

            public string Direction = "S";

            public string path = "";

            public Adventurer(int x, int y, string name, string initDirection, string path)
            {
                X= x;
                Y = y;
                Name = name;
                Direction = initDirection;
                this.path = path;
            }
            public void SetX(int value)
            {
                X = value;
            }
            public void SetY(int value)
            {
                Y = value;
            }
            public void AdventurerTakesTreasure(Treasure t)
            {
                if (t == null || t.Amount < 1) return;
                TreasuresGotten++;
            }
        }

        public static string[] ExtractString(string text, Regex regex)
        {
            MatchCollection match = regex.Matches(text);

            string[] result = new string[match.Count];
            for (int i = 0; i < match.Count; i++)
            {
                result[i] = match[i].ToString();
            }
            return result;
        }
        public static int[] ExtractNumbers(string text, Regex regex)
        {
            MatchCollection match = regex.Matches(text);
            
            int[] result = new int[match.Count];
            for (int i = 0; i < match.Count; i++)
            {
                if (!int.TryParse(match[i].ToString(), out result[i])) throw new ArgumentException("Try parse failed");
            }
            return result;
        }

        public static string GetUpstreamDirectory(string directory)
        {
            string path = Directory.GetCurrentDirectory();
            if (!path.Contains(directory))
                throw new DirectoryNotFoundException($"Couldn't find \\{directory}\\ directory");
            path = string.Concat(path.AsSpan(0, path.IndexOf($"{directory}")), $"\\{directory}\\");
            return path;
        }

        public void SetInputFile(string name)
        {
            InputFile = name + ".txt";
        }
        public void SetOutputFile(string name)
        {
            OutputFile = name + ".txt";
        }

        public static string[] ReadFile(string path, string InputFile)
        {
            return File.ReadAllLines(path + InputFile);
        }

        public static void WriteOutputFile(string path, string OutputFile,Terrain game)
        {
            System.IO.File.WriteAllLines(path + OutputFile, game.ExportOutput());
        }

        public string[] ExportOutput()
        {
            List<string> output = new List<string>();
            output.Add($"C - {X} - {Y}");
            foreach(Mountain m in mountains)
                output.Add($"M - {m.X} - {m.Y}");
            foreach (Treasure t in treasures)
                if(t.Amount>0) output.Add($"T - {t.X} - {t.Y} - {t.Amount}");
            foreach (Adventurer a in adventurers)
                output.Add($"A - {a.Name} - {a.X} - {a.Y} - {a.Direction} - {a.TreasuresGotten}");
            return output.ToArray<string>();
        }

        public static void PrintInput(string[] lines)
        {
            foreach (string line in lines)
                Console.WriteLine(line);
        }


        public static void PrintMap(Terrain terrain)
        {
            Console.WriteLine();
            foreach (string line in terrain.Map)

                Console.WriteLine(line);
        }

    }
}
