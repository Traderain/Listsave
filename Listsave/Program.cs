using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class Options
    {
        [Option("dir", HelpText = "Directory to scan for *.sav files and parse", Required = false)]
        public string dir { get; set; }

        [Option("file", HelpText = "*.sav file to parse", Required = false)]
        public string file { get; set; }

        [Option("verbose", HelpText = "Verbose parsing info", Required = false, Default = false)]
        public bool verbose { get; set; }
    }

    static class Program
    {
        static bool verbose = false;
        const string version = "Listsave v0.3 by Traderain";
        static string file = null;
        static string dir = null;

        static void Main(string[] args)
        {
#if DEBUG
            args = new string[]
            {
                "--file",
                "010-corky-0585s.sav"
            };
#endif
            Console.Title = version;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            if (args.Length > 0)
            {
                var parser = new CommandLine.Parser(config =>
                {
                    config.AutoVersion = true;
                    config.HelpWriter = null;
                });

                var parserResult = parser.ParseArguments<Options>(args);
                parserResult
                  .WithParsed<Options>(options => Run(options))
                  .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No arguments! (For help run -> \"Listsave.exe --help\")");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Heading = "by Traderain"; //change header
                h.Copyright = "https://github.com/Traderain/Listsave";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        static void Run(Options opt)
        {
            if(opt.file != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Parsing save: {opt.file}");
                Console.ForegroundColor = ConsoleColor.White;
                ParseSave(opt.file, opt);
            }
            else if(opt.dir != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Parsing saves from directory: {opt.dir}");
                Console.ForegroundColor = ConsoleColor.White;
                ParseDir(opt.dir, opt);
            }
        }

        static void ParseDir(string path, Options opt)
        {
           if(Directory.Exists(path))
            {
                Directory.GetFiles(path, "*.sav", SearchOption.AllDirectories)
                    .ToList().ForEach(x => ParseSave(x, opt));
            }
        }

        static void ParseSave(string path, Options opt)
        {
            Console.WriteLine($"Parsing {path}...");
            HLSave save = new HLSave(opt.verbose);
            using(var fs = new FileStream(path, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    try
                    {
                        save.Parse(br);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
            save.PrettyPrint();
        }
    }
}
