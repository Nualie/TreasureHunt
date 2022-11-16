using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreasureHunt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreasureHunt.Tests
{
    public class MapStubData
    {
        public static int spacing = 7;
        public static string[] ReturnStubMapInput()
        {
            List<string> data = new List<string>();
            data.Add("C - 3 - 4"); //Map size
            data.Add("M - 1 - 0"); //Mountain
            data.Add("M - 2 - 1");
            data.Add("T - 0 - 3 - 2"); //Treasure
            data.Add("T - 1 - 3 - 3");
            data.Add("A - Lara - 1 - 1 - S - AADADAGGA"); //Adventurer
            return data.ToArray<string>();
        }

        public static string[] ReturnStubMapInputDisplay()
        {
            List<string> data = new List<string>();
            data.Add(".       M       .");
            data.Add(".       A(Lara) M");
            data.Add(".       .       .");
            data.Add("T(2)    T(3)    .");
            return data.ToArray<string>();
        }

        public static string[] ReturnStubMapOutput()
        {
            List<string> data = new List<string>();
            data.Add("C - 3 - 4"); //Map size
            data.Add("M - 1 - 0"); //Mountain
            data.Add("M - 2 - 1");
            data.Add("T - 1 - 3 - 2");
            data.Add("A - Lara - 0 - 3 - S - 3"); //Adventurer
            return data.ToArray<string>();
        }

        public static string[] ReturnStubMapOutputDisplay()
        {
            List<string> data = new List<string>();
            data.Add(".       M       .");
            data.Add(".       .       M");
            data.Add(".       .       .");
            data.Add("A(Lara) T(2)    .");
            return data.ToArray<string>();
        }
    }


    [TestClass()]
    public class TerrainTests
    {
       
       

        [TestMethod()]
        public void TestMountainsBlockAdventurers()
        {
            string[] TestInput =    { "C - 5 - 1", "M - 3 - 0", "A - Test - 0 - 0 - E - AAAAAAAAA" };
            string[] FileOuput =    { "C - 5 - 1", "M - 3 - 0", "A - Test - 2 - 0 - E - 0" };
            string[] InputMap =     { "A(Test) .       .       M       ." };
            string[] OutputMap =    { ".       .       A(Test) M       ." };
            Terrain testgame = new Terrain(TestInput);
            GenerateInputMapTest(testgame, InputMap);
            testgame.PlayGame();
            TestFileOutput(testgame, FileOuput);
            GenerateOutputMapTest(testgame, OutputMap);
        }

        [TestMethod()]
        public void TestAdventurersPickUpTreasures()
        {
            string[] TestInput =    { "C - 5 - 1", "T - 2 - 0 - 1", "T - 3 - 0 - 3", "A - Test - 0 - 0 - E - AAAADDADDAAAA" };
            string[] FileOuput =    { "C - 5 - 1",                  "T - 3 - 0 - 1", "A - Test - 4 - 0 - E - 3" };
            string[] InputMap =     { "A(Test) .       T(1)    T(3)    ." };
            string[] OutputMap =    { ".       .       .       T(1)    A(Test)" };
            Terrain testgame = new Terrain(TestInput);
            GenerateInputMapTest(testgame, InputMap);
            testgame.PlayGame();
            TestFileOutput(testgame, FileOuput);
            GenerateOutputMapTest(testgame, OutputMap);
        }

        [TestMethod()]
        public void TestAdventurersDontBumpIntoThemselves()
        {
            string[] TestInput =    { "C - 5 - 1", "A - Test - 1 - 0 - E - AADDAAA" };
            string[] FileOuput =    { "C - 5 - 1", "A - Test - 0 - 0 - W - 0" };
            string[] InputMap =     { ".       A(Test) .       .       ." };
            string[] OutputMap =    { "A(Test) .       .       .       ." };
            Terrain testgame = new Terrain(TestInput);
            GenerateInputMapTest(testgame, InputMap);
            testgame.PlayGame();
            TestFileOutput(testgame, FileOuput);
            GenerateOutputMapTest(testgame, OutputMap);
        }

        [TestMethod()]
        public void TestCommentsAreIgnored()
        {
            string[] TestInput =    { "C - 5 - 1","#Hello world you shouldn't see me" ,"M - 3 - 0", "A - Test - 0 - 0 - E - AAAAAAAAA" };
            string[] FileOuput =    { "C - 5 - 1",                                     "M - 3 - 0", "A - Test - 2 - 0 - E - 0" };
            string[] InputMap =     { "A(Test) .       .       M       ." };
            string[] OutputMap =    { ".       .       A(Test) M       ." };
            Terrain testgame = new Terrain(TestInput);
            GenerateInputMapTest(testgame, InputMap);
            testgame.PlayGame();
            TestFileOutput(testgame, FileOuput);
            GenerateOutputMapTest(testgame, OutputMap);
        }

        [TestMethod()]
        public void TestWithLaraExample()
        {
            Terrain testgame = new Terrain(MapStubData.ReturnStubMapInput());

            GenerateInputMapTest(testgame, MapStubData.ReturnStubMapInputDisplay());
            testgame.PlayGame();
            TestFileOutput(testgame, MapStubData.ReturnStubMapOutput());
            GenerateOutputMapTest(testgame, MapStubData.ReturnStubMapOutputDisplay());
            Terrain.WriteOutputFile(Terrain.GetUpstreamDirectory("TreasureHuntTests"), testgame.OutputFile, testgame);
            TestWhetherOutputWasGeneratedCorrectly(Terrain.GetUpstreamDirectory("TreasureHuntTests"), testgame.OutputFile, MapStubData.ReturnStubMapOutput());
        }

        public void TestWhetherOutputWasGeneratedCorrectly(string path, string filename, string[] expected)
        {
            Assert.IsTrue(File.Exists(path + filename));
            string[] outputlines = Terrain.ReadFile(path,filename);
            Assert.AreEqual(outputlines.Length, expected.Length);
            for (int i = 0; i < expected.Length; i++)
                //line by line allows to quickly figure out what went wrong
                Assert.AreEqual(expected[i], outputlines[i]);
        }

        public void GenerateInputMapTest(Terrain testgame, string[] expected)
        {
            
            Assert.AreEqual(expected.Length, testgame.Map.Length);
            for (int i = 0; i < expected.Length; i++)
            //line by line allows to quickly figure out what went wrong
            {
                Assert.AreEqual(expected[i], testgame.Map[i]);
            }
        }

        public void GenerateOutputMapTest(Terrain testgame, string[] expected)
        {
            Assert.AreEqual(expected.Length, testgame.Map.Length);
            for (int i = 0; i < expected.Length; i++)
                //line by line allows to quickly figure out what went wrong
                Assert.AreEqual(expected[i], testgame.Map[i]);
        }

        public void TestFileOutput(Terrain testgame, string[] expected)
        {
            string[] exported = testgame.ExportOutput();
            Assert.AreEqual(expected.Length, exported.Length);
            for (int i = 0; i < expected.Length; i++)
                //line by line allows to quickly figure out what went wrong
                Assert.AreEqual(expected[i], exported[i]);
        }
    }
}