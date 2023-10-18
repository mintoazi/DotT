using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class CSVLoader
{
    //[RuntimeInitializeOnLoadMethod]
    public static List<string[]> Load(TextAsset csv, bool isFirstLine)
    {
        //string path = Application.dataPath + @"\" + csv.name+".csv";
        //if (!File.Exists(path)) CreateCSV(csv, path);
        //string s = File.ReadAllText(path, Encoding.GetEncoding("utf-8"));
        StringReader reader = new StringReader(csv.text);
        List<string[]> loadedCSV = new List<string[]>(128);
        string line = null;
        if(!isFirstLine) line = reader.ReadLine();
        while (reader.Peek() != -1)
        {
            line = reader.ReadLine();
            loadedCSV.Add(line.Split(','));
        }
        reader.Close();
        return loadedCSV;
    }


    public static void CreateCSV(TextAsset csv, string path)
    {
        
        using (File.Create(path)) ;
        using (StreamWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
        {
            //StringReader reader = new StringReader(stri);
            List<string[]> defaultCsv = Load(csv, isFirstLine: true);
            for (int i = 0; i < defaultCsv.Count; i++)
            {
                string s = string.Join(",", defaultCsv[i]);
                streamWriter.WriteLine(string.Join(",", defaultCsv[i]));
                Debug.Log(s);
            }
            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}
