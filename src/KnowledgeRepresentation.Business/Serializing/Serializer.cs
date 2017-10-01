using System;
using System.IO;

namespace KnowledgeRepresentation.Business.Serializing
{
    public static class Serializer
    {
        public static (string, string, string) ReadFile(string path)
        {
            string inputTextSentence = string.Empty, inputTextScenario = string.Empty, inputTextQuery = string.Empty;
            using (var stream = new StreamReader(path))
            { 
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (line != null && (line.Contains(":") || line.Contains("=")))
                    {
                        inputTextScenario += line + Environment.NewLine;
                    }
                    else if (line != null && (line.Contains("always") || line.Contains("ever")))
                    {
                        inputTextQuery += line + Environment.NewLine;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        inputTextSentence += line + Environment.NewLine;
                    }
                }
            }
            return (inputTextSentence, inputTextScenario, inputTextQuery);
        }
    }
}
