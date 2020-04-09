using System;

namespace avr_identify
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 2)
                Usage();

            var sourcefile = new Hexfile(args[0]);
            var targetfile = new Hexfile(args[1]);

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
                    Console.WriteLine($"Target ended prematurely on section {sectionNo}");
                    files_are_equal = false;
                    break;
                }

                if (source.address != target.address)
                {
                    Console.WriteLine($"Differing addresses on section {sectionNo}: source {source.address}, target {target.address}");
                    files_are_equal = false;
                    break;
                }

                if (source.data.Length > target.data.Length)
                {
                    Console.WriteLine($"Target section to short {sectionNo}: source {source.data.Length} bytes, target {target.data.Length} bytes");
                    files_are_equal = false;
                    break;
                }

                for (int i = 0; i < source.data.Length; i++)
                {
                    var sb = source.data[i];
                    var tb = target.data[i];
                    if (sb != tb)
                    {
                        Console.WriteLine($"Target differs on section {sectionNo}, byte {i}");
                        files_are_equal = false;
                        break;
                    }
                }
            }

            if (files_are_equal)
                Environment.Exit(0);
            else
                Environment.Exit(1);

        }

        static void Usage()
        {
            Console.Error.WriteLine("Usage: avr-identify <file1> <file2>");
            Console.Error.WriteLine("to test the data in file2, truncated to the lengths of data in file1");
            Environment.Exit(1);
        }
    }
}
