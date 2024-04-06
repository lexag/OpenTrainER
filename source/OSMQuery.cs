using System.IO;
using System;
using System.Collections;
using System.Xml.Serialization;
using Godot;
using System.Text;
using static System.Net.WebRequestMethods;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

internal class OSMQuery
{
	public int timeout = 25;
	public string query = "";
	public Callable callback;
	
	public XmlDocument xmlDocument = new XmlDocument();
	
	HttpRequest httpRequest;
	bool midCall = false;



	public OSMQuery() 
	{
		httpRequest = new HttpRequest();
		WorldManager.worldRoot.AddChild(httpRequest);
		httpRequest.RequestCompleted += RequestCompleted;
	}

	public static string SpecificArea(string areaRef)
	{
		return $"<area-query ref=\"{areaRef}\"/>";
	}

	public static string WithinRadius(int radius)
	{
		return $"<around radius=\"{radius}\"/>";
	}

	public static string SpecificKeyValue(string key, string value)
	{
		return $"<has-kv k=\"{key}\" v=\"{value}\"/>";
	}

	public static string SpecificKeyValueFuzzy(string key, string value)
	{
		return $"<has-kv k=\"{key}\" regv=\"{value}\"/>";
	}

	public static string QuerySpecificId(string type, string id)
	{
		return $"<id-query type=\"{type}\" ref=\"{id}\"/>";
	}

	public static string Query(string query)
	{
		return $"<query type=\"nwr\">{query}<area-query from=\"searchArea\"/></query>";
	}

	public static string Recurse(string type, string first, string second)
	{
		return $"{first}<recurse type=\"{type}\"/>{second}";
	}


	public string Run()
	{
		query = $"<osm-script output=\"xml\" output-config=\"\" timeout=\"{timeout}\"><query into=\"searchArea\" type=\"area\"><id-query type=\"area\" ref=\"3600052822\" into=\"searchArea\"/></query>{query}<print e=\"\" from=\"_\" geometry=\"full\" ids=\"yes\" limit=\"\" mode=\"body\" n=\"\" order=\"id\" s=\"\" w=\"\"/></osm-script>"; //query sweden
		string url = $"https://overpass-api.de/api/interpreter";
		string body = $"data={EncodeURIPayload(query)}";

		midCall = true;
		httpRequest.Request(url, requestData: body);
		return query;
	}

	public void Free()
	{
		if (!midCall)
		{
			httpRequest.QueueFree();
		}
	}

	public List<XmlNode> GetByXPath(string XPath)
	{
		XmlNodeList nodeList = xmlDocument.SelectNodes(XPath);
		List<XmlNode> list = new List<XmlNode>();
		foreach (XmlNode node in nodeList)
		{
			list.Add(node);
		}
		return list;
	}

	public XmlNode GetByXPathSingle(string XPath)
	{
		return xmlDocument.SelectSingleNode(XPath);
	}





	private void RequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		xmlDocument.LoadXml(Encoding.UTF8.GetString(body));
		midCall = false;
		callback.Call();
	}








	private string EncodeURIPayload(string payload)
	{
		StringBuilder sb = new StringBuilder();
		foreach (char c in payload.ToCharArray())
		{
			switch (c)
			{
				case '\n': sb.Append("%0A"); break;
				case ' ': sb.Append("%20"); break;
				case '[': sb.Append("%5B"); break;
				case ',': sb.Append("%2C"); break;
				case ']': sb.Append("%5D"); break;
				case ';': sb.Append("%3B"); break;
				default: sb.Append(c); break;
			}
		}
		return sb.ToString();
	}


	private T DeserializeXML<T>(string input) where T : class
	{
		System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

		using (StringReader sr = new StringReader(input))
		{
			return (T)ser.Deserialize(sr);
		}
	}

	private string SerializeToXML<T>(T ObjectToSerialize)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

		using (StringWriter textWriter = new StringWriter())
		{
			xmlSerializer.Serialize(textWriter, ObjectToSerialize);
			return textWriter.ToString();
		}
	}
}
