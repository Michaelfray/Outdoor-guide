using System.Text;

namespace OutdoorGuide.Business.Migrations
{
   
    public static class ContentCsv
    {
        public static List<Dictionary<string, string>> Read(string path)
        {
            var rows = Parse(System.IO.File.ReadAllText(path, Encoding.UTF8));
            var header = rows[0];

            return rows.Skip(1)
                .Where(row => row.Count == header.Count)
                .Select(row => header.Zip(row).ToDictionary(x => x.First, x => x.Second))
                .ToList();
        }

        private static List<List<string>> Parse(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var quoted = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        quoted = false;
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
                else if (c == '"')
                {
                    quoted = true;
                }
                else if (c == ';')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\n')
                {
                    row.Add(field.ToString().TrimEnd('\r'));
                    field.Clear();
                    rows.Add(row);
                    row = [];
                }
                else
                {
                    field.Append(c);
                }
            }

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString().TrimEnd('\r'));
                rows.Add(row);
            }

            return rows;
        }
    }
}