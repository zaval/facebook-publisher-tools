using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace InstantRemover
{
    class MainClass
    {
        static string file404 = "404.csv";
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
            string token;
            string groupId;
            try
            {
                token = settings["token"].ToString();
                groupId = settings["id"].ToString();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("No token or id in settings!");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            try
            {
                file404 = settings["404"].ToString();
            }
            catch (KeyNotFoundException)
            {

            }


            if (!File.Exists(file404))
            {
                Console.WriteLine("No {0} file in the directory", file404);
                return;
            }

            Http http = new Http();

            StreamReader f = new StreamReader(file404);
            StreamWriter fw = new StreamWriter("404_no_instant.csv");
            string line;
            while ((line = f.ReadLine()) != null)
            {
                Regex rx = new Regex(@",\s*(https*://[-a-zA-Z0-9./?&_=]+)");
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

                string link = String.Format("https://graph.facebook.com/?id={0}&fields=instant_article&access_token={1}", Uri.EscapeDataString(url), token);
                //string link = String.Format("https://graph.facebook.com/?id={0}&fields=instant_article&access_token={1}", url, token);

                string p = http.get(link);
                //Console.WriteLine(p);

                if (p == null)
                {
                    continue;
                }

                var data = ToJson(p);
                if (data == null)
                {
                    continue;
                }

                string articleId;

                try
                {
                    articleId = data["instant_article"]["id"].ToString();
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine("this instant article does not exists");
                    fw.WriteLine(line);
                    continue;
                }

                Console.WriteLine("link {0} id {1}", url, articleId);

                link = String.Format("https://graph.facebook.com/{0}/?access_token={1}", articleId, token);

                Console.WriteLine("deleting article {0}", articleId);

                p = http.get(link, "DELETE");
                Console.WriteLine(p);

            }
            f.Close();
            fw.Close();

            //https://graph.facebook.com/?id=https://cinnamon.one/1970-e-godyi-bez-prikras-zhizn-sovetskih-lyudey-na-fotografiyah-vladimira-syicheva/&fields=instant_article&access_token=EAADxLOAX6scBAK7HKSLBvZBPWsYCgZBxN08j5aPWhqVsSBqh0v7ZCZCI4JfOx0FQxoPbDTiSmNeeDe6B4Yqqv6ySSuGUFfoljfeiLlxHxfP0XBnSOiSOAtMv86n2QoIAZC8tiUFYTSo9dGrY4VGwOLo55ZBKLKQ8iuZABF1qA7YHWkQktIyEZBziHyuUtadBe3cQDVrAVzfpDrRCWfdsMdwm
            Console.WriteLine("All done");
            Console.ReadKey();
        }
    }
}
