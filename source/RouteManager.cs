
using System.Collections.Generic;
using System.Xml;

internal static class RouteManager
{
    public static Dictionary<long, TrackNode> trackNodesInRoute = new();
    public static Dictionary<string, LatLon> stationsInRoute = new();

	public static void QueueForLoad(XmlNode topNode)
	{
		List<XmlNode> mapObjectRefNodes = new List<XmlNode>();

		XmlNodeList wayNodes = topNode.SelectNodes("/osm/way");
		foreach (XmlNode wayNode in wayNodes)
		{
			XmlNodeList ndNodes = wayNode.SelectNodes("/osm/way/nd");
			foreach (XmlNode ndNode in ndNodes)
			{
				mapObjectRefNodes.Add(ndNode);
			}
			for (int i = 0; i < mapObjectRefNodes.Count; i++)
			{
				XmlNode current = mapObjectRefNodes[i];
				long nodeRef = long.Parse(current.Attributes["ref"].Value);
				if (!trackNodesInRoute.ContainsKey(nodeRef))
				{
					TrackNode newNode = new TrackNode();
					newNode.WorldCoordinate = new LatLon(double.Parse(current.Attributes["lat"].Value.Replace('.', ',')), double.Parse(current.Attributes["lon"].Value.Replace('.', ',')));
					trackNodesInRoute.Add(nodeRef, newNode);
				}
				if (i > 0)
				{
					trackNodesInRoute[nodeRef].AddNeighbour(trackNodesInRoute[long.Parse(mapObjectRefNodes[i - 1].Attributes["ref"].Value)]);
				}
			}
		}


        XmlNodeList stationNodes = topNode.SelectNodes("/osm/node");
        foreach (XmlNode stationNode in stationNodes)
        {
            stationsInRoute.TryAdd(stationNode.SelectSingleNode("/osm/node/tag[@k='railway:ref']").Attributes["v"].Value, new LatLon(stationNode.Attributes["lat"].Value, stationNode.Attributes["lon"].Value));
        }

        //XmlDocument doc = topNode.OwnerDocument;
        //int c = doc.DocumentElement.ChildNodes.Count;
        //for (int i = 0; i < c; i++)
        //{
        //	XmlNode mapObject = doc.DocumentElement.ChildNodes[i];
        //	if (mapObject.Name == "way")
        //	{
        //		List<XmlNode> mapObjectRefNodes = new List<XmlNode>();
        //		foreach (XmlNode mapObjectComponent in mapObject.ChildNodes)
        //		{
        //			if (mapObjectComponent.Name == "nd")
        //			{
        //				mapObjectRefNodes.Add(mapObjectComponent);
        //			}
        //		}
        //		for (int j = 0; j < mapObjectRefNodes.Count; j++)
        //		{
        //			XmlNode current = mapObjectRefNodes[j];
        //			long nodeRef = long.Parse(current.Attributes["ref"].Value);
        //			if (!trackNodesInRoute.ContainsKey(nodeRef))
        //			{
        //				TrackNode newNode = new TrackNode();
        //				newNode.WorldCoordinate = new LatLon(double.Parse(current.Attributes["lat"].Value.Replace('.', ',')), double.Parse(current.Attributes["lon"].Value.Replace('.', ',')));
        //				trackNodesInRoute.Add(nodeRef, newNode);
        //			}
        //			if (j > 0)
        //			{
        //				trackNodesInRoute[nodeRef].AddNeighbour(trackNodesInRoute[long.Parse(mapObjectRefNodes[j - 1].Attributes["ref"].Value)]);
        //			}
        //		}
        //	}
        //}
        //  } 
    }
}

