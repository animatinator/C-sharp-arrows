using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.ListTestFunctions
{
    public class Filter : ListTestFunction
    {
        public Filter()
        {
            Name = "Filter";
        }

        protected override void InitialiseArrow()
        {
            arrow = ListArrow.Filter((Person p) => p.Age > 10);
        }

        protected override List<Person> LinqFunction(List<Person> list)
        {
            return list.Where(person => person.Age > 10).ToList();
        }

        protected override List<Person> Function(List<Person> list)
        {
            List<Person> result = new List<Person>();

            foreach (Person p in list)
            {
                if (p.Age > 10) result.Add(p);
            }

            return result;
        }
    }
}
