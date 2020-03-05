using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class StateFile
    {
        public string filename;
        public List<string> symbols = new List<string>();

        public StateFile(string name, byte[] data)
        {
            filename = name;
            ParseData(data);
        }

        /// <summary>
        /// SaveGameState is .hl1
        /// ClientState is .hl2</summary>
        /// EntityPatch is .hl3
        /// <param name="bytes">The bytes to read the file from</param>
        public void ParseData(byte[] bytes)
        {
            using(var ms = new MemoryStream(bytes))
            {
                using (var br = new BinaryReader(ms))
                {
                    var mws = new String(br.ReadChars(4));
                    var swit = new string(filename.TrimEnd('\0').ToArray().Reverse().Take(3).Reverse().ToArray());
                    switch (swit)
                    {
                        case "hl1":
                        {
                            Console.Write("\tParing statefile: " + filename);
                            HL1StateFile(br);
                            break;
                        }
                        default:
                            break;
                    }
                }
            }
        }

        public void HL1StateFile(BinaryReader br)
        {
            try
            {
                var version = br.ReadInt32();
                var nBytesSymbols = br.ReadInt32();
                var nSymbols = br.ReadInt32();
                var nBytesDataHeaders = br.ReadInt32();
                var nBytesData = br.ReadInt32();

                symbols = Encoding.ASCII.GetString(br.ReadBytes(nBytesSymbols)).Split('\0').ToList();
                symbols.RemoveAll(x => x == String.Empty); // Remove '\0'
                                                           //Console.WriteLine($"Symbols({symbols.Count}):");
                                                           //symbols.ForEach(x => Console.WriteLine("\t- " + x));

                var dataheaders = br.ReadBytes(nBytesDataHeaders);

                //Read remaining data len(nBytesData)
                //Console.WriteLine($"Reading the actual data - br at {br.BaseStream.Position}/{br.BaseStream.Length} ");
                List<Token> tokens = new List<Token>();
                br.BaseStream.Seek(0x24, SeekOrigin.Current);
                /*bool symbolsdone = false;
                for(int i = 0; i < 10; i++)
                {
                    //Read the token
                    Token t;
                    t.len = br.ReadUInt16();
                    t.idx = br.ReadUInt16();
                    if (br.BaseStream.Position+t.len >= br.BaseStream.Length)
                    {
                        symbolsdone = true;
                        continue;
                    }
                    t.contents = br.ReadBytes(t.len);
                    t.str = Encoding.ASCII.GetString(t.contents);
                    tokens.Add(t);

                    if (br.BaseStream.Position == br.BaseStream.Length)
                        symbolsdone = true;
                }*/
                //Console.WriteLine("Tokencount: " + tokens.Count);
                var time = br.ReadSingle();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("=> \tTime: " + time.ToString("G") + "s");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception)
            {

                throw;
            }
        }

        struct Token
        {
            public UInt16 len;
            public UInt16 idx;
            public byte[] contents;
            public string str;
        }
    }
}
