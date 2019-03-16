using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using ZSoft;

namespace InstantFinder
{

    class MainClass
    {

        private static readonly string settingsFile = "settings.json";
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



            Http http = new Http();

            var url = String.Format("https://graph.facebook.com/v2.6/{0}/instant_articles?access_token={1}&_reqName=object%3Apage%2Finstant_articles&_reqSrc=PageContentTabInstantArticlesCommonConfig&fields=%5B%22id%22%2C%22date_created%22%2C%22canonical_url%22%2C%22most_recent_version%7Btitle%7Btext%7D%2Ccreation_time%2Chas_errors%2Cerrors%2Cmodified_timestamp%2Cpublish_status%2Cis_media_ingested%2Csource%2Cmodified_by%7D%22%5D&filtering=%5B%5D&limit=100&locale=en_US&method=get&pretty=0&suppress_http_code=1", groupId, token);

            /*            int i = 0;
                        var workbook = new Workbook();
                        var ws = new Worksheet("instant articles");
            */
            var fname = String.Format("{0}.csv", groupId);
            var fw = new StreamWriter(fname);

            do
            {
                var p = http.get(url);
                url = null;
                var data = ToJson(p);
                if (data == null)
                {
                    break;
                }

                try
                {

                    //var articles = data["data"].ToArray();
                    var articles = data["data"];

                    foreach (var article in articles)
                    {
                        var canonicalUrl = article["canonical_url"].ToString();
                        Console.WriteLine(canonicalUrl);
                        var title = article["most_recent_version"]["title"]["text"].ToString();
                        var id = article["id"].ToString();
                        var line = String.Format(@"""{0}"",{1}", title, canonicalUrl);
                        fw.WriteLine(line);
/*                        ws.Cells[i, 0] = new Cell(title);
                        ws.Cells[i, 1] = new Cell(canonicalUrl);
                        ws.Cells[i, 2] = new Cell(id);
                        i++;
*/                       
                    }
                    url = data["paging"]["next"].ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }

            }
            while (url != null);


            fw.Close();

/*
            workbook.Worksheets.Add(ws);
            workbook.Save("InstantArticles.xls");
*/
            Console.WriteLine("All done");
            Console.ReadKey();
        }
    }
}
