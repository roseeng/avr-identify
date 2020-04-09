using System;
using System.IO;

namespace avr_identify
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 2)
                Usage();

            var targetfile = args[0];
            var sourcefiles = Directory.GetFiles(args[1], "*.hex");

            foreach (var sourcefile in sourcefiles)
            {
                if (Compare(sourcefile, targetfile))
                {
                    Console.WriteLine("Matching archive file: " + sourcefile);
                    var metadata = Path.Combine( Path.GetDirectoryName(sourcefile), Path.GetFileNameWithoutExtension(sourcefile) + ".txt");
                    if (File.Exists(metadata))
                    {
                        using (var sr = new StreamReader(metadata))
                        {
                            var text = sr.ReadToEnd();
                            Console.WriteLine(text);
                            Console.WriteLine("--------------------------------------");
                        }
                    }
                }
            }

            /*
            if (files_are_equal)
                Environment.Exit(0);
            else
                Environment.Exit(1);
            */
        }

        static bool Compare(string sourcename, string targetname, bool verbose = false)
        {

            var sourcefile = new Hexfile(sourcename);
            var targetfile = new Hexfile(targetname);

            int sectionNo = 1;
            bool files_are_equal = true;
            while (files_are_equal)
            {
                var source = sourcefile.Read();
                var target = targetfile.Read();

                if (source == null) // End of source file, we are done.
                    break;

                if (target == null)
                {
                    if (verbose) Console.WriteLine($"Target ended prematurely on section {sectionNo}");
                    files_are_equal = false;
                    break;
                }

                if (source.address != target.address)
                {
                    if (verbose) Console.WriteLine($"Differing addresses on section {sectionNo}: source {source.address}, target {target.address}");
                    files_are_equal = false;
                    break;
                }

                if (source.data.Length > target.data.Length)
                {
                    if (verbose) Console.WriteLine($"Target section to short {sectionNo}: source {source.data.Length} bytes, target {target.data.Length} bytes");
                    files_are_equal = false;
                    break;
                }

                for (int i = 0; i < source.data.Length; i++)
                {
                    var sb = source.data[i];
                    var tb = target.data[i];
                    if (sb != tb)
                    {
                        if (verbose) Console.WriteLine($"Target differs on section {sectionNo}, byte {i}");
                        files_are_equal = false;
                        break;
                    }
                }
            }

            if (files_are_equal)
                return true;
            else
                return false;
        }

        static void Usage()
        {
            Console.Error.WriteLine("Usage: avr-identify <file> <archive folder>");
            Console.Error.WriteLine("to test the data in <file> against all hex files in <archive folder>");
            Environment.Exit(1);
        }
    }
}
