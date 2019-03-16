using System;
using System.IO;
using System.Net;
using System.Text;

namespace InstantRemover
{
    public class Http
    {
        public Http()
        {
        }

		public string get(string url, string method=null){
			HttpWebRequest request;
            HttpWebResponse response;
			string html;
			request = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
            {
                request.Method = method;
            }

            try
            {
                response = (HttpWebResponse)request.GetResponse();

				try
                {
                    StreamReader streamRead = null;
                    Stream streamRecv = response.GetResponseStream();

                    if (response.CharacterSet == null)
                    {
                        streamRead = new StreamReader(streamRecv);
                    }
                    else
                    {
                        streamRead = new StreamReader(streamRecv, Encoding.GetEncoding(response.CharacterSet.Replace('"', '"').Trim()));
                    }

                    html = streamRead.ReadToEnd();
                    streamRead.Close();
                    return html;
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex);
                    if (response.CharacterSet.ToLower().Contains("utf-8"))
                    {
                        Stream streamRecv = response.GetResponseStream();
                        StreamReader streamRead = new StreamReader(streamRecv, Encoding.UTF8);
                        html = streamRead.ReadToEnd();
                        streamRead.Close();
                        return html;
                    }
                    return null;
                }

            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Cant load page " + request.Address.ToString());
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't load insights page" + ex.Message);
				return null;
            }
		}
    }
}
