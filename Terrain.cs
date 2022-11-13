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
        public string[] Map { get; private set; }

        private List<Treasure> treasures = new List<Treasure>();
        private List<Mountain> mountains = new List<Mountain>();

        private static readonly Regex RegNumber = new(@"\d+");

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
                        X = lineData[0];
                        Y = lineData[1];
                        break;
                    case 'M': //mountain
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (lineData.Count != 2) throw new ArgumentException("Input document is formatted wrong");
                        mountains.Add(new Mountain(lineData[0], lineData[1]));

                        break;
                    case 'T': //treasure
                        lineData.AddRange(ExtractNumbers(line, RegNumber));
                        if (lineData.Count != 3) throw new ArgumentException("Input document is formatted wrong");
                        treasures.Add(new Treasure(lineData[0], lineData[1], lineData[2]));
                        break;
                    case 'A': //adventurer
                        break;
                    default:
                        throw new ArgumentException("Input document is formatted wrong");
                        
                }
            }
            Map = new string[Y];
            GenerateMap();            

        }

        public void GenerateMap()
        {
            Map = new string[Y];
            for (int i = 0; i < Y; i++)
            {
                Map[i] = String.Concat(Enumerable.Repeat(".    ", X));
                Map[i] = Map[i].Trim();
            }
            foreach(Treasure t in treasures)
            {
                if (t.Amount == 0) continue;
                string str = "T(" + t.Amount + ")";
                Map[t.Y] = Map[t.Y].Remove(t.X*5, str.Length).Insert(t.X*5, str);
            }
            foreach (Mountain m in mountains)
            {
                string str = "M";
                Map[m.Y] = Map[m.Y].Remove(m.X*5, str.Length).Insert(m.X*5, str);
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
