using System;
using System.IO;
using System.Net;
using System.Text;

namespace TextImagesFinder
{
    public class Http
    {
        public Http()
        {
        }

		public string get(string url){
			HttpWebRequest request;
            HttpWebResponse response;
			string html;
			request = (HttpWebRequest)WebRequest.Create(url);

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
                catch (WebException)
                {
                    return null;
                }
                catch (ArgumentException)
                {
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
            catch (WebException)
            {
                Console.WriteLine("Cant load page " + request.Address.ToString());
				return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't load insights page" + ex.Message);
				return null;
            }
		}

        public int DownloadFile(string url, string fname)
        {
            HttpWebResponse response = null;
            Stream remoteStream = null;
            Stream localStream = null;
            int bytesProcessed = 0;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                remoteStream = response.GetResponseStream(); ;
                localStream = File.Create(fname);
                byte[] buffer = new byte[1024];
                int bytesRead;

                do
                {
                    bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                    localStream.Write(buffer, 0, bytesRead);
                    bytesProcessed += bytesRead;
                }
                while (bytesRead > 0);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }

            return bytesProcessed;

        }
    }
}
