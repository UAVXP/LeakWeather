using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Collections.Specialized;
using System.Web;

namespace LeakWeather
{
	class JSON
	{
		public static Dictionary<string, string> ParseJson(string res)
		{
			var lines = res.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var ht = new Dictionary<string, string>(20);
			var st = new Stack<string>(20);

			for (int i = 0; i < lines.Length; ++i)
			{
				var line = lines[i];
				var pair = line.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

				if (pair.Length == 2)
				{
					var key = ClearString(pair[0]);
					var val = ClearString(pair[1]);

					if (val == "{")
					{
						st.Push(key);
					}
					else
					{
						if (st.Count > 0)
						{
							key = string.Join("_", st) + "_" + key;
						}

						if (ht.ContainsKey(key))
						{
							ht[key] += "&" + val;
						}
						else
						{
							ht.Add(key, val);
						}
					}
				}
				else if (line.IndexOf('}') != -1 && st.Count > 0)
				{
					st.Pop();
				}
			}

			return ht;
		}

		static string ClearString(string str)
		{
			str = str.Trim();

			var ind0 = str.IndexOf("\"");
			var ind1 = str.LastIndexOf("\"");

			if (ind0 != -1 && ind1 != -1)
			{
				str = str.Substring(ind0 + 1, ind1 - ind0 - 1);
			}
			else if (str[str.Length - 1] == ',')
			{
				str = str.Substring(0, str.Length - 1);
			}

			str = HttpUtility.UrlDecode(str);

			return str;
		}

		public static string webRequest(string str/*, string[][] tokens*/)
		{
		/*	string server = str;
			WebClient webClient = new WebClient();
			NameValueCollection formData = new NameValueCollection();
			for (int i = 0; i < tokens.Length; i++)
			{
				formData[tokens[i][0]] = tokens[i][1];
			}
			//	var content = client.DownloadString("http://example.com");
			byte[] responseBytes;
			string response = "";
			try
			{
				responseBytes = webClient.UploadValues(server, "GET", formData);
				if (responseBytes != null)
					response = Encoding.UTF8.GetString(responseBytes);
				webClient.Dispose();
			}
			catch (Exception ex)
			{
				//	MessageBox.Show(ex.ToString());
				Console.WriteLine(ex.Message);
			}
			return response;*/

			WebClient webClient = new WebClient();
			string resp = "";
			try
			{
				resp = webClient.DownloadString(str);
			}
			catch(WebException ex)
			{
			//	throw ex;
				Console.WriteLine(ex.Message);
				return "";
			}
			return resp;
		}
	}
}
