using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class HLSave
    {
        string IDString;
        Int32 SaveVersion;
        Int32 TokenTableFileTableOffset;
        Int32 TokenCount;
        Int32 TokenTableSize;
        List<string> tokennames;
        List<Tuple<Int16, Int16, char[]>> HeaderTokens = new List<Tuple<short, short, char[]>>();
        List<StateFile> stateFiles = new List<StateFile>();

        bool verbose;


        public HLSave(bool verboseparse)
        {
            verbose = verboseparse;
        }

        public byte[] mw = new[]
        {
            (byte)'J', (byte)'S', (byte)'A', (byte)'V',
        };

        public void Parse(BinaryReader br)
        {
            if(verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[WARNING] - Parsing with verbose output!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            IDString                    = (Encoding.ASCII.GetString(br.ReadBytes(sizeof(int))));
            SaveVersion                 = br.ReadInt32();
            TokenTableFileTableOffset   = br.ReadInt32();
            TokenCount                  = br.ReadInt32();
            TokenTableSize              = br.ReadInt32();
            tokennames                 = PeekParse(br.ReadBytes(TokenTableSize));
            byte[] table = br.ReadBytes(TokenTableFileTableOffset);
            //HeaderTokens.Add(ReadToken(br)); //Comment
            //HeaderTokens.Add(ReadToken(br)); //Map

            //Console.WriteLine("Map: " + new String(HeaderTokens[1].Item3));
            //HeaderTokens.ForEach(x => Console.WriteLine("\t" + new String(x.Item3)));
            br.ReadInt32();
            bool end = false;
            while (!end)
            {
                var statefilename = new String(br.ReadChars(260)).Trim('\0');
                var filelength = br.ReadInt32();
                var Data = br.ReadBytes(filelength);
#if DEBUG
                //File.WriteAllBytes(statefilename, Data);
#endif
                StateFile s = new StateFile(statefilename, Data);
                stateFiles.Add(s);
                if (br.BaseStream.Position == br.BaseStream.Length)
                    end = true;
            }
        }

        public static Tuple<Int16, Int16, char[]> ReadToken(BinaryReader br)
        {
            var len = br.ReadInt16();
            var idx = br.ReadInt16();
            var contents = br.ReadChars(len);
            return new Tuple<short, short, char[]>(len, idx, contents);
        }

        public void PrettyPrint()
        {

        }

        public static List<string> PeekParse(byte[] arr)
        {
            List<string> ret = new List<string>();
            using (var br = new BinaryReader(new MemoryStream(arr)))
            {
                bool done = false;
                bool started = false;
                bool charsdone = false;
                string currs = "";
                do
                {
                    //We are done
                    if(br.BaseStream.Position == br.BaseStream.Length)
                    {
                        done = true;
                        continue;
                    }
                    if(!started)
                    {
                        var currc = (char)br.ReadByte();
                        if(currc == '\0')
                        {
                            //Failsafe
                            continue;
                        }
                        else
                        {
                            started = true;
                            currs += currc;
                        }
                    }
                    else
                    {
                        //We are reading the string...
                        var currc = (char)br.ReadByte();
                        //Reached the end..
                        if(!charsdone)
                        {
                            if(currc == '\0')
                            {
                                charsdone = true;
                            }
                            else
                            {
                                currs += currc;
                            }
                        }
                        if (currc != '\0' && charsdone)
                        {
                            br.BaseStream.Seek(-1, SeekOrigin.Current);
                            started = false;
                            charsdone = false;
                            ret.Add(currs);
                            currs = "";
                        }
                    }

                } while (!done);
            }
            return ret;
        }
    }
}
