using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal static class GLTFLoader
{
    static Dictionary<string, Tuple<GltfDocument, GltfState>> loadedResources = new();

    public static Node3D LoadFromPath(string path, string fileName, bool userFile = true)
    {
        string filePath;
        if (userFile)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            filePath = Path.Combine(documentsPath, "OpenTrainER", path, fileName);
        }
        else
        {
            filePath = Path.Combine(path, fileName);
        }

        GltfDocument document;
        GltfState state;
        Error error = Error.Ok;

        // Load from file or cache
        if (loadedResources.ContainsKey(filePath))
        {
            (document, state) = loadedResources[filePath];
        }
        else
        {
            document = new GltfDocument();
            state = new GltfState();
            error = document.AppendFromFile(filePath, state);
            if (error == Error.Ok)
            {
                loadedResources.Add(filePath, new(document, state));
            }
        }

        if (error == Error.Ok)
        {
            Node3D node = (Node3D)document.GenerateScene(state);
            foreach (var material in state.Materials)
            {
                ((BaseMaterial3D)material).TextureFilter = BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic;
            }
            return node;
        }
        GD.PrintErr($"Failed loading glTF scene from '{filePath}' (error code: {error}).");
        return null;
    }
}
