using Godot;
using System.Xml;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.ComponentModel.Design;

internal partial class RouteSelector : Container
{
    HttpRequest httpRequest = new HttpRequest();
    List<XmlNode> availableRoutes = new List<XmlNode>();
    OSMQuery osmQuery;

    public override void _Ready()
    {
        base._Ready();

        ((Button)FindChild("search_button")).Pressed += ButtonSearchPressed;
        ((Button)FindChild("continue_button")).Pressed += ButtonContinuePressed;
        ((ItemList)FindChild("routes_list")).ItemSelected += RouteListClicked;
    }

    void ButtonSearchPressed()
    {
        osmQuery = new OSMQuery();
        GD.Print("Searching location name");

        TextEdit searchBox = (TextEdit)FindChild("search_box");

        if (searchBox.Text.IsValidInt())
        {
            osmQuery.query = OSMQuery.QuerySpecificId("node", searchBox.Text);
            osmQuery.callback = Callable.From(OSMNameSearchReturn);
            string q = osmQuery.Run();
        }
        else
        {
            osmQuery.query = OSMQuery.Query(OSMQuery.SpecificKeyValue("railway", "station") + OSMQuery.SpecificKeyValueFuzzy("name", searchBox.Text));
            osmQuery.callback = Callable.From(OSMNameSearchReturn);
            string q = osmQuery.Run();
        }
    }

    void OSMNameSearchReturn()
    {
        GD.Print("Searching routes");

        string id;
        XmlNode node = osmQuery.GetByXPathSingle("/osm/node[1]");

        if (node != null)
        {
            id = node.Attributes.GetNamedItem("id").Value;
        }
        else
        {
            ((Label)FindChild("route_name")).Text = "Could not find station";
            return;
        }

        osmQuery.query = OSMQuery.QuerySpecificId("node", id) + OSMQuery.Query(OSMQuery.WithinRadius(100) + OSMQuery.SpecificKeyValue("route", "train"));
        osmQuery.callback = Callable.From(OSMRouteSearchReturn);
        string q = osmQuery.Run();
    }

    void OSMRouteSearchReturn()
    {
        GD.Print("Searching routes");

        availableRoutes = osmQuery.GetByXPath("/osm/relation");

        ItemList itemList = (ItemList)FindChild("routes_list");
        itemList.Clear();

        for (int i = 0; i < availableRoutes.Count; i++)
        {
            XmlNode node = availableRoutes[i];
            itemList.AddItem(
                $"{node.SelectSingleNode($"/osm/relation[{i + 1}]/tag[@k='from']").Attributes.GetNamedItem("v").Value}" +
                $" - {node.SelectSingleNode($"/osm/relation[{i + 1}]/tag[@k='to']").Attributes.GetNamedItem("v").Value}", selectable: false);
            itemList.SetItemSelectable(i, true);
        }
    }

    void RouteListClicked(long index)
    {
        ((Button)FindChild("continue_button")).Disabled = false;
        XmlNode rt = availableRoutes[0];
        XmlNode name;
        name = rt.SelectSingleNode($"/osm/relation[{index + 1}]/tag[@k='name']");
        if (name == null)
        {
            name = rt.SelectSingleNode($"/osm/relation[{index + 1}]/tag[@k='description']");
        }
        ((Label)FindChild("route_name")).Text = rt.SelectSingleNode($"/osm/relation[{index + 1}]/tag[@k='name']").Attributes.GetNamedItem("v").Value;
        ((Label)FindChild("route_from")).Text = rt.SelectSingleNode($"/osm/relation[{index + 1}]/tag[@k='from']").Attributes.GetNamedItem("v").Value;
        ((Label)FindChild("route_to")).Text = rt.SelectSingleNode($"/osm/relation[{index + 1}]/tag[@k='to']").Attributes.GetNamedItem("v").Value;
    }

    void ButtonContinuePressed()
    {
        ItemList itemList = (ItemList)FindChild("routes_list");
        int selectedIndex = itemList.GetSelectedItems()[0];
        XmlNode rt = availableRoutes[0];

        osmQuery.callback = Callable.From(OSMTrackSearchReturn);
        osmQuery.query = OSMQuery.Recurse(
            "relation-way",
            OSMQuery.Query(
                OSMQuery.SpecificKeyValue("route", "train") +
                OSMQuery.SpecificKeyValue("ref", rt.SelectSingleNode($"/osm/relation[{selectedIndex + 1}]/tag[@k='ref']").Attributes.GetNamedItem("v").Value) +
                OSMQuery.SpecificKeyValue("network", rt.SelectSingleNode($"/osm/relation[{selectedIndex + 1}]/tag[@k='network']").Attributes.GetNamedItem("v").Value)),
            OSMQuery.Both(OSMQuery.Both(
                OSMQuery.Query(
                    OSMQuery.WithinRadius(100) +
                    OSMQuery.SpecificKeyValue("railway", "narrow_gauge")
                    ),
                OSMQuery.Query(
                    OSMQuery.WithinRadius(100) +
                    OSMQuery.SpecificKeyValue("railway", "rail")
                    )
                ),
                OSMQuery.Query(
                    OSMQuery.WithinRadius(10) + 
                    OSMQuery.SpecificKeyValue("railway", "station")
                    )
                )
            );

        string q = osmQuery.Run();


    }

    void OSMTrackSearchReturn()
    {
        RouteManager.QueueForLoad(osmQuery.GetByXPath("/osm")[0]);
        GetTree().ChangeSceneToFile("res://test.tscn");
    }

}

