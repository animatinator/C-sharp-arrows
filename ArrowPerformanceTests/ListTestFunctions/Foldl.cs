using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrowDataBinding.Arrows;
using ArrowDataBinding.Combinators;

namespace ArrowPerformanceTests.ListTestFunctions
{
    public class Foldl : ListTestFunction
    {
        public Foldl()
        {
            Name = "Foldl";
            Iterations = 300000;
        }

        protected override void InitialiseArrow()
        {
            Arrow<IEnumerable<Person>, Person> tempArrow = ListArrow.Foldl((Person a, Person b) =>
                new Person
                {
                    Age = a.Age + b.Age,
                    Name = String.Format("{0} and {1}", a.Name, b.Name),
                    Employer = a.Employer
                },
                    new Person
                    {
                        Name = "",
                        Age = 0,
                        Employer = new Employer { Name = "Employer", Size = 10 }
                    });
            arrow = new ListArrow<Person, Person>(
                x => new List<Person> {tempArrow.Invoke(x)});
        }

        protected override List<Person> LinqFunction(List<Person> list)
        {
            return new List<Person> {list.Aggregate(
                 new Person
                 {
                     Name = "",
                     Age = 0,
                     Employer = new Employer { Name = "Employer", Size = 10 }
                 },
                 (Person a, Person b) =>
                    new Person
                    {
                        Age = a.Age + b.Age,
                        Name = String.Format("{0} and {1}", a.Name, b.Name),
                        Employer = a.Employer
                    }
                 )};
        }

        protected override List<Person> Function(List<Person> list)
        {
            Person result = new Person
            {
                Name = "",
                Age = 0,
                Employer = new Employer { Name = "Employer", Size = 10 }
            };

            foreach (Person p in list)
            {
                result = new Person
                {
                    Age = result.Age + p.Age,
                    Name = String.Format("{0} and {1}", result.Name, p.Name),
                    Employer = result.Employer
                };
            }

            return new List<Person> {result};
        }
    }
}
