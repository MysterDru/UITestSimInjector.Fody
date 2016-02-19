using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public class Helper
{
    public static Dictionary<string, string> GetSimulatorIds()
    {
        Dictionary<string, string> simulators = new Dictionary<string, string>();

        Process process = new Process();
        //        Process process =  Process.Start( ("xcrun", "instruments -s devices");
        process.StartInfo.FileName = "xcrun";
        process.StartInfo.Arguments = "instruments -s devices";

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();

        StringBuilder output = new StringBuilder();

        while (!process.HasExited)
        {
            output.Append(process.StandardOutput.ReadToEnd());
        }

        output.Append(process.StandardOutput.ReadToEnd());

        string data = output.ToString();

        string[] lines = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);


        foreach (var line in lines)
        {
            var pattern = @"\[(.*?)\]";
            var matches = Regex.Matches(line, pattern);

            if (matches.Count > 0)
            {

                var enumerator = matches.GetEnumerator();
                enumerator.MoveNext();

                var match = (Match)enumerator.Current;

                string id = match.Value;

                string key = line.Replace(id, string.Empty).Trim();
                id = id.Replace("[", "").Replace("]", "");

                simulators.Add(key, id);
            }

        }

        return simulators;
    }
}