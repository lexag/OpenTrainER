using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

[Obsolete]
partial class WorldLoader : Node3D
{
	public Dictionary<long, TrackNode> trackNodes = new Dictionary<long, TrackNode>();

	OSMQuery osmQuery;

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

        int distance = 500;

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

		osmQuery = new OSMQuery();
		osmQuery.query = payload ;
		osmQuery.callback = Callable.From(OnOSMRequestCompleted);

		GD.Print("Loading new data");

	}

	private void OnOSMRequestCompleted()
	{
		GD.Print("Received data");
		XmlDocument doc = osmQuery.xmlDocument;
		int c = doc.DocumentElement.ChildNodes.Count;
		for (int i = 0; i < c; i++)
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
						newNode.WorldCoordinate = new LatLon(double.Parse(current.Attributes["lat"].Value.Replace('.', ',')), double.Parse(current.Attributes["lon"].Value.Replace('.', ',')));
						trackNodes.Add(nodeRef, newNode);
					}
					if (j > 0)
					{
						trackNodes[nodeRef].AddNeighbour(trackNodes[long.Parse(mapObjectRefNodes[j - 1].Attributes["ref"].Value)]);
					}
				}
			}
		}
		WorldRenderer.RenderListOfTrackNodes(trackNodes.Values.ToList<TrackNode>());
		//WorldManager.NetworkingDone();
	}
}
