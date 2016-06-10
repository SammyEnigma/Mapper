using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.UnitTests
{
    public class ApiTests
    {
        public void can_read_single()
        {
            var reader = new StubDataReader
            {
                Names = new[] { "ID" },
                Types = new[] { typeof(long) },
                Values = new object[] { 1L },
            };
            reader.AsSequenceOf<long>().Single();
        }

    }
}
