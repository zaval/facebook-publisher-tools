using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace TextImagesFinder
{
    class MainClass
    {
        static string settingsFile = "settings.json";

        private static dynamic ToJson(string text)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            dynamic dobj = null;
            try
            {
                dobj = jsonSerializer.Deserialize<dynamic>(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Can't parse json");
            }

            return dobj;

        }

        private static dynamic ParseSettings(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine("file {0} does not exists", file);
                return null;
            }
            string data = File.ReadAllText(file);

            return ToJson(data);

        }


        public static void Main(string[] args)
        {

            dynamic settings = ParseSettings(settingsFile);
            if (settings == null)
            {
                Console.WriteLine("Can't parse settings file");
                return;
            }

            string domain;

            try
            {
                domain = settings["domain"].ToString();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("No domain in settings!");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            Http http = new Http();

            StreamReader f = new StreamReader(String.Format("{0}.csv", domain));
            StreamWriter fw = new StreamWriter(String.Format("{0}-with-text.csv", domain));
            StreamWriter fwb = new StreamWriter(String.Format("{0}-404.csv", domain));
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string line;
            while ((line = f.ReadLine()) != null)
            {
                Regex rx = new Regex(@",\s*(https*://.+)");
                var match = rx.Match(line);

                if (match.Length == 0)
                {
                    continue;
                }
                string url = match.Groups[1].Value;
                if (!url.StartsWith("http", StringComparison.Ordinal))
                {
                    Console.WriteLine("{0} is not link", url);
                    continue;
                }

                url = url.Split(',')[0];
                url = url.Replace(@"""", string.Empty);

                Console.WriteLine(url);

                var page = http.get(url);
                if (page == null)
                {
                    Console.WriteLine(String.Format("bad link: {0}", url));
                    fwb.WriteLine(line);
                    continue;
                }

                //Console.WriteLine(page);

                rx = new Regex("og:image\" content=\"([^\"]+)");
                match = rx.Match(page);
                string image = null;
                if (match.Length == 0)
                {
                    Console.WriteLine("can't parse image");
                    continue;
                }
                image = match.Groups[1].Value;
                if (image == null)
                {
                    continue;
                }

                Console.WriteLine(image);

                var imageFname = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray()) + ".jpg";

                var loaded = http.DownloadFile(image, imageFname);
                if (loaded == 0)
                {
                    Console.WriteLine("can't load image file");
                    continue;
                }

                var programName = "";

                int p = (int)Environment.OSVersion.Platform;
                if ((p == 4) || (p == 6) || (p == 128))
                {
                    programName = "tesseract/tesseract";
                }
                else
                {
                    programName = @"tesseract\tesseract.exe";
                }

                Process proc = null;
                ProcessStartInfo StartInfo = new ProcessStartInfo
                {
                    FileName = programName,
                    Arguments = String.Format("{0} - -l rus --oem 1", imageFname),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                StartInfo.EnvironmentVariables["TESSDATA_PREFIX"] = String.Format("{0}{1}tesseract{1}ts", Directory.GetCurrentDirectory(), Path.DirectorySeparatorChar);

                string output = "";

                proc = Process.Start(StartInfo);
                while (!proc.StandardOutput.EndOfStream)
                {
                    output += proc.StandardOutput.ReadLine();
                }

                try
                {
                    File.Delete(imageFname);
                }
                catch (Exception)
                {

                }


                Console.WriteLine(output);
                if (output.Trim().Length > 0)
                {
                    fw.WriteLine(String.Format("{0},\"{1}\",\"{2}\"", line, output.Replace("\"", ""), image));
                }
            }

            f.Close();
            fw.Close();
            fwb.Close();
            Console.WriteLine("All done");
            Console.ReadKey();
        }
    }
}
