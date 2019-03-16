using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BadFounder
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            Console.WriteLine(args.Length);

            if (args.Length < 1)
            {
                Console.WriteLine("File not specified");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File {0} not found", args[0]);
                return;
            }

            StreamReader f = new StreamReader(args[0]);
            StreamWriter fwg = new StreamWriter("good.csv");
            StreamWriter fwb = new StreamWriter("404.csv");
            string line;
            Http http = new Http();
            while((line = f.ReadLine()) != null)
            {
                //string[] fileLine = line.Split([ '"', ',']);
                Regex rx = new Regex(@",\s*(https*://.+)");
                var match = rx.Match(line);

                if (match.Length == 0)
                {
                    continue;
                }

                string url = match.Groups[1].Value;
                if (!url.StartsWith("http"))
                {
                    Console.WriteLine("{0} is not link", url);
                    continue;
                }

                Console.WriteLine("checking {0}", url);

                string p = http.get(url);
                if (p == null)
                {
                    fwb.WriteLine(line);
                }
                else
                {
                    fwg.WriteLine(line);
                }
            }

            f.Close();
            fwg.Close();
            fwb.Close();

            Console.ReadKey();
        }
    }
}
