using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

partial class WorldLoader : Node3D
{
	public Dictionary<long, TrackNode> trackNodes = new Dictionary<long, TrackNode>();

	public WorldManager worldManager;

	public WorldLoader()
	{
	
	}

	public void Load()
	{
		LoadMapData();
	}


	private void LoadMapData()
	{
		Tuple<string, string, string>[] searchTerms = new Tuple<string, string, string>[] {
			new Tuple<string, string, string>( "way", "railway", "rail" ),
			new Tuple<string, string, string>( "way", "railway", "narrow_gauge" ),
		};

        int distance = 300;

        LatLon neCorner = VehicleManager.vehicleWorldCoordinate.Moved_M(distance, distance);
		LatLon swCorner = VehicleManager.vehicleWorldCoordinate.Moved_M(-distance, -distance);
		string payload = $"[out:xml];(";
		
		foreach (var entry in searchTerms)
		{
			payload += $"{entry.Item1}[{'"'}{entry.Item2}{'"'}={'"'}{entry.Item3}{'"'}](" +
				$"{swCorner.Latitude.ToString().Replace(',','.')}," +
				$"{swCorner.Longitude.ToString().Replace(',', '.')}," +
				$"{neCorner.Latitude.ToString().Replace(',', '.')}," +
				$"{neCorner.Longitude.ToString().Replace(',', '.')});";
		}
		payload += ");out geom;";

		HttpRequest httpRequest = new HttpRequest();
		this.AddChild(httpRequest);
		
		string url = $"https://overpass-api.de/api/interpreter";
		string body = $"data={EncodeURIPayload(payload)}";
		
		httpRequest.RequestCompleted += OnOSMRequestCompleted;
		httpRequest.Request(url, requestData:body);

	}

	private void OnOSMRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
	{
		XmlDocument doc = new XmlDocument();
		string response = Encoding.UTF8.GetString(body);
		doc.LoadXml(response);
		int c = doc.DocumentElement.ChildNodes.Count;
		for (int i = 0; i<c; i++)
		{
			XmlNode mapObject = doc.DocumentElement.ChildNodes[i];
			if (mapObject.Name == "way")
			{
				List<XmlNode> mapObjectRefNodes = new List<XmlNode>();
				foreach (XmlNode mapObjectComponent in mapObject.ChildNodes)
				{
					if (mapObjectComponent.Name == "nd")
					{
						mapObjectRefNodes.Add(mapObjectComponent);
					}
				}
				for (int j = 0; j < mapObjectRefNodes.Count; j++)
				{
					XmlNode current = mapObjectRefNodes[j];
					long nodeRef = long.Parse(current.Attributes["ref"].Value);
					if (!trackNodes.ContainsKey(nodeRef))
					{
						TrackNode newNode = new TrackNode();
						newNode.WorldCoordinate = new LatLon(double.Parse(current.Attributes["lat"].Value.Replace('.',',')), double.Parse(current.Attributes["lon"].Value.Replace('.', ',')));
						trackNodes.Add(nodeRef, newNode);
					}
					if (j > 0)
					{
						trackNodes[nodeRef].AddNeighbour(trackNodes[long.Parse(mapObjectRefNodes[j - 1].Attributes["ref"].Value)]);
					}
				}
			}
		}
		worldManager.trackNodes = trackNodes.Values.ToList<TrackNode>();
		worldManager.NetworkingDone();
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
}
