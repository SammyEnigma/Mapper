using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Mapper.UnitTests
{
    class ConsoleObserver : IObserver<string>
    {
        public List<string> Values = new List<string>();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            Console.Error.WriteLine(error);
        }

        public void OnNext(string value)
        {
            Console.WriteLine(value);
            Values.Add(value);
        }
    }
}
