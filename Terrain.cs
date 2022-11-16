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
        public int X { get; private set; }
        public int Y { get; private set; }

        private int spacing = 4;
        public string[] Map { get; private set; }

        private List<Treasure> treasures = new List<Treasure>();
        private List<Mountain> mountains = new List<Mountain>();
        private List<Adventurer> adventurers = new List<Adventurer>();

        private static readonly Regex RegNumber = new(@"\d+");
        private static readonly Regex RegString = new(@"[^- \d]+");

        public Terrain(string[] lines)
        {
            List<int> lineData;
            foreach (string line in lines)
            {
                lineData = new List<int>();
                switch (line[0])
                {
                    case 'C': //map
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if(lineData.Count != 2) throw new ArgumentException("Input document is formatted wrong");
                        if(lineData[0]<1 || lineData[1]<1) throw new ArgumentException("Map size is wrong");
                        X = lineData[0];
                        Y = lineData[1];
                        break;
                    case 'M': //mountain
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (lineData.Count != 2) throw new ArgumentException("Input document is formatted wrong");
                        if (lineData[0] < 0 || lineData[1] < 0) throw new ArgumentException("Mountain coords are wrong");
                        mountains.Add(new Mountain(lineData[0], lineData[1]));

                        break;
                    case 'T': //treasure
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (lineData.Count != 3) throw new ArgumentException("Input document is formatted wrong");
                        if (lineData[0] < 0 || lineData[1] < 0) throw new ArgumentException("Treasure coords are wrong");
                        treasures.Add(new Treasure(lineData[0], lineData[1], lineData[2]));
                        break;
                    case 'A': //adventurer
                        List<string> strData = new List<string>();
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        strData.AddRange(ExtractString(line, RegString));
                        if (lineData.Count != 2 || strData.Count != 4) throw new ArgumentException("Input document is formatted wrong");
                        if (lineData[0] < 0 || lineData[1] < 0) throw new ArgumentException("Adventurer coords are wrong");
                        adventurers.Add(new Adventurer(lineData[0], lineData[1], strData[1], strData[2], strData[3]));
                        spacing = Math.Max(spacing, strData[1].Length+3); //automatically adjust spacing for the name
                        break;
                    case '#': //comment
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
                Map[a.Y] = Map[a.Y].Remove(a.X * (spacing + 1), str.Length).Insert(a.X * (spacing + 1), str);
            }


        }

        private char ReturnDirFromVector(int[] vector)
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

        private int[] ReturnVectorFromDir(char currentdir,char inputdir)
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
                    break;
            }

            switch (inputdir)
            {
                case 'D':
                    if (vector[0] == 0)
                        vector[0] *= -1;
                    vector = vector.Reverse().ToArray<int>();
                    break;
                case 'G':
                    if (vector[0] == 1)
                        vector[0] *= -1;
                    vector = vector.Reverse().ToArray<int>();
                    break;

                default:
                    break;
            }

            return vector;
        }

        public void PlayGame()
        {
            int[] vector = new int[] { 0, 0 };
            foreach (Adventurer a in adventurers)
            {
                foreach(char c in a.path)
                {
                    if (a.Direction == "I") throw new ArgumentException("Invalid direction");
                    vector = ReturnVectorFromDir(a.Direction[0], c);
                    a.SetX(a.X + vector[0]);
                    a.SetY(a.Y + vector[1]);
                    a.Direction = ReturnDirFromVector(vector).ToString();
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
                Y = x;
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
                t.AdventurerTakesTreasure();
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

    }
}
