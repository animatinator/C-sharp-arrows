using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrowPerformanceTests
{
    public class TestResults
    {
        public string Name { get; set; }
        public Dictionary<string, double> resultsList { get; set; }

        public TestResults(string name)
        {
            Name = name;
            resultsList = new Dictionary<string, double>();
        }

        public void AddResult(string name, double value)
        {
            resultsList.Add(name, value);
        }
    }
}
