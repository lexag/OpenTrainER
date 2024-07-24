using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class JSONLoader
{
    public static T LoadFile<T>(string path, string fileName)
    {
        var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        var filePath = Path.Combine(documentsPath, "OpenTrainER", path, fileName);
        
        using (StreamReader r = new StreamReader(filePath))
        {
            string json = r.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public static T ParseString<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str);
    }

    public static T Reparse<T>(object obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }
}
