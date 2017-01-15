using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    static class Program
    {
        static readonly int[] Empty = new int[0];

        public static int[] Locate(this byte[] self, byte[] candidate)
        {
            if (IsEmptyLocate(self, candidate))
                return Empty;

            var list = new List<int>();

            for (var i = 0; i < self.Length; i++)
            {
                if (!IsMatch(self, i, candidate))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            return !candidate.Where((t, i) => array[position + i] != t).Any();
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }


        /*
         *      Note
         *      Time(4byte) -> Length(4byte->32) -> Mapname(32byte) -> Length(4byte) -> Skyname(32byte)
         * 
         * 
         */
        static void Main(string[] args)
        {
            try
            {
                Console.Title = ("Listsave v0.1");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Listsave by Traderain");
                Console.WriteLine("---------------------");
                Console.WriteLine();
                if (args.Length < 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No file supplied!");
                    Console.ReadKey();
                    Environment.Exit(0x01);
                }
                if (Path.GetExtension(args[0]) != ".sav")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Not a save file!");
                    Console.ReadKey();
                    Environment.Exit(0x02);
                }
                var file = args[0];
                Console.Title += " - " + Path.GetFileName(file);
                var filedata = File.ReadAllBytes(file);
                var time = 0f;
                var goodoffset = 0;
                var map = "";
                using (var br = new BinaryReader(new MemoryStream(filedata)))
                {
                    var skylocations = filedata.Locate(Encoding.UTF8.GetBytes("sky_"));
                    Console.WriteLine("Found _sky locations:");
                    foreach (var loc in skylocations)
                    {
                        br.BaseStream.Seek(loc, SeekOrigin.Begin);
                        var skyname = new string(br.ReadChars(32).Where(x => char.IsLetter(x) || char.IsNumber(x) || x == '_').ToArray());
                        if (skyname.Contains("day"))
                        {
                            goodoffset = loc;
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        Console.WriteLine("Offset: " + loc + "\tSkyname=>" + skyname);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.WriteLine();
                    br.BaseStream.Seek(goodoffset - 44,SeekOrigin.Begin);
                    time = br.ReadSingle();
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    map = new string(br.ReadChars(32).Where(x => char.IsLetter(x) || char.IsNumber(x) || x == '_').ToArray());
                }
                if (time == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Couldn't find the timestamp :/");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Found the timestamp! The save has been made " + time + "s after the loading of " + map + ".");
                }
            }
            catch (Exception)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sorry but this save is unfortunately not supported yet! :/ Please contact Traderain!");
            }
            Console.ReadKey();
        }
    }
}
