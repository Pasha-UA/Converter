namespace ConverterProject
{
    public class FillKeywords
    {
        public class Keyword
        {
            public string Id { get; set; }
            public IEnumerable<string> Keys { get; set; }
        }

        private IEnumerable<Keyword> InitKeywordsFromFile()
        {
            var keywords = new List<Keyword>();
            foreach (string line in System.IO.File.ReadLines(@"..\\..\\..\\Data\\export.csv"))
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var arr = line.Split(';')
                        .Where(w => !String.IsNullOrEmpty(w))
                        .Select(w => w.ToLower())
                        .Select(w => w.Trim())
                        .Distinct().ToArray();

                    keywords.Add(new Keyword { Id = arr[0], Keys = arr.Skip(1) });

                }
            }
            return keywords;
        }

        public IEnumerable<Keyword> Keywords { get; set; }

        public FillKeywords(bool init = false)
        {
            this.Keywords = new List<Keyword>();
            if (init) this.Keywords = InitKeywordsFromFile();
           
        }


    }
}
