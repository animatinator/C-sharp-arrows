using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.ListTestFunctions
{
    public class OrderByThenMap : ListTestFunction
    {
        public OrderByThenMap()
        {
            Name = "OrderByThenMap";
            Iterations = 300000;
        }

        protected override void InitialiseArrow()
        {
            arrow = ListArrow.OrderBy((Person a, Person b) => b.Age - a.Age).Map((Person p) =>
            {
                return new Person { Name = p.Name + " mapped", Age = p.Age + 1, Employer = p.Employer };
            });
        }

        protected override List<Person> LinqFunction(List<Person> list)
        {
            return list.OrderBy((Person p) => p.Age).ToList()
                .Select((Person p, int i) => new Person { Name = p.Name + " mapped", Age = p.Age + 1, Employer = p.Employer })
                .ToList();
        }

        protected override List<Person> Function(List<Person> list)
        {
            List<Person> sorted = list.OrderBy((Person p) => p.Age).ToList();
            List<Person> result = new List<Person>();

            foreach (Person p in sorted)
            {
                result.Add(
                    new Person { Name = p.Name + " mapped", Age = p.Age + 1, Employer = p.Employer }
                    );
            }

            return result;
        }
    }
}
