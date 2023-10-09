using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CSVLoader
{
    public static List<string[]> Load(TextAsset csv)
    {
        StringReader reader = new StringReader(csv.text);
        List<string[]> _loadedCSV = new List<string[]>(128);
        string line = null;
        line = reader.ReadLine();
        while (reader.Peek() != -1)
        {
            line = reader.ReadLine();
            _loadedCSV.Add(line.Split(','));
        }
        reader.Close();
        foreach (string[] list in _loadedCSV)
        {
            foreach(string str in list)
            {
                // Debug.Log(str);
            }
        }
        return _loadedCSV;
    }
}
