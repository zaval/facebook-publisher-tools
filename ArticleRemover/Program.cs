using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace ArticleRemover
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
            StreamWriter fw = new StreamWriter("404_no_article.csv");
            string line;
            while ((line = f.ReadLine()) != null)
            {
                Regex rx = new Regex(@"^""*([\S\s]+?)""*,\s*(https*://.+)");
                var match = rx.Match(line);

                if (match.Length == 0)
                {
                    continue;
                }

                string title = match.Groups[1].Value;
                string postLink = match.Groups[2].Value;
                postLink = postLink.Split(',')[0];
                postLink = postLink.Replace("\"", "");
                postLink = postLink.Split('?')[0];

                Console.WriteLine("checking {0}", title);
                //continue;

                string link = String.Format("https://graph.facebook.com/v2.5/{0}/posts?access_token={1}&_reqName=object%3Apage%2Fposts&_reqSrc=PageContentTabPublishedPostsConfig&fields=%5B%22admin_creator%22%2C%22link%22%2C%22edit_actions%7Bedit_time%2Ceditor%7D%22%2C%22picture%22%2C%22created_time%22%2C%22message%22%2C%22story%22%2C%22updated_time%22%2C%22insights.metric(%5B%5C%22post_impressions_unique%5C%22%2C%5C%22post_engaged_users%5C%22%5D)%22%2C%22privacy%22%2C%22type%22%2C%22object_id%22%2C%22attachments.fields(%5B%5C%22type%5C%22%5D).limit(2)%22%2C%22is_live_audio%22%2C%22is_crossposting_eligible%22%5D&locale=ru_RU&method=get&pretty=0&q={2}&suppress_http_code=1", groupId, token, Uri.EscapeDataString(title));

                //Console.WriteLine(link);

                string p = http.get(link);
                //Console.WriteLine(p);
                var data = ToJson(p);
                if (data == null)
                {
                    continue;
                }
                var articles = data["data"].ToArray();

                if (articles.Length == 0)
                {
                    fw.WriteLine(line);
                    continue;
                }
                foreach (var article in articles)
                {
                    try
                    {
                        string articleLink = article["link"].ToString();
                        if (articleLink != postLink)
                        {
                            Console.WriteLine("link are not eq");
                            continue;
                        }

                        string articleId = article["id"].ToString();
                        Console.WriteLine(articleId);

                        var deleteLink = String.Format("https://graph.facebook.com/v2.5/{0}?access_token={1}", articleId, token);
                        Console.WriteLine("removing {0}", articleId);

                        p = http.get(deleteLink, "DELETE");
                        Console.WriteLine(p);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            f.Close();
            fw.Close();

            Console.WriteLine("All done");
            Console.ReadKey();
        }
    }
}
