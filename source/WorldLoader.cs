using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

partial class WorldLoader : Node3D
{
	LatLon vehicleWorldCoordinate = new LatLon(59.34911, 18.06648);

	Dictionary<long, TrackNode> trackNodes = new Dictionary<long, TrackNode>();

	public WorldLoader()
	{
	
	}

	public override void _Ready()
	{
		base._Ready();
		LoadMapData(new Dictionary<string, string>() { { "railway", "rail" } });
	}

	private void LoadMapData(Dictionary<string, string> searchTerms, string objectType = "way", int distance = 1000)
	{
		LatLon neCorner = vehicleWorldCoordinate.MovedMeters(distance, distance);
		LatLon swCorner = vehicleWorldCoordinate.MovedMeters(-distance, -distance);
		string payload = $"[out:xml];(";
		
		foreach (var entry in searchTerms)
		{
			payload += $"{objectType}[{'"'}{entry.Key}{'"'}={'"'}{entry.Value}{'"'}](" +
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
			XmlNode node = doc.DocumentElement.ChildNodes[i];
			if (node.Name == "way")
			{
				List<XmlNode> trackNodeRefNodes = new List<XmlNode>();
				foreach (XmlNode node2 in node.ChildNodes)
				{
					if (node2.Name == "nd")
					{
						trackNodeRefNodes.Add(node2);
					}
				}
				for (int j = 0; j < trackNodeRefNodes.Count; j++)
				{
					XmlNode current = trackNodeRefNodes[j];
					long nodeRef = long.Parse(current.Attributes["ref"].Value);
					if (!trackNodes.ContainsKey(nodeRef))
					{
						TrackNode newNode = new TrackNode(
							//new LatLon(
							//		double.Parse(current.Attributes["lat"].Value),
							//		double.Parse(current.Attributes["lon"].Value)
							//	)
							);
						newNode.WorldCoordinate = new LatLon(double.Parse(current.Attributes["lat"].Value), double.Parse(current.Attributes["lon"].Value));
						trackNodes.Add(nodeRef, newNode);
					}
					if (j > 0)
					{
						trackNodes[nodeRef].AddNeighbour(trackNodes[long.Parse(trackNodeRefNodes[j - 1].Attributes["ref"].Value)]);
					}
				}
			}
		}
		RenderTrackPoints();
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


	private void RenderTrackPoints()
	{
		foreach (var entry in trackNodes)
		{
			TrackNode trackNode = entry.Value;
			Vector2 localPosition = trackNode.WorldCoordinate.ToLocal(vehicleWorldCoordinate);
			Node3D point = new Node3D();
			this.AddChild(point);
			point.Position = new Vector3(localPosition.X, localPosition.Y, 0);
		}
	}
}
