using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace OpenTrainER.source.file
{
    internal static class TextureLoader
    {
        public static Texture2D LoadFile(string path, string fileName)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, "OpenTrainER", path, fileName);

            return ImageTexture.CreateFromImage(Image.LoadFromFile(filePath));
        }
    }
}
