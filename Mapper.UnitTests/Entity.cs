using System;
using System.Collections.Generic;

namespace BusterWood.Mapper.UnitTests
{
    public class Entity
    {
        public bool Bit { get; set; }
        public string Text { get; set; }
        public int Int { get; set; }
        public short Short { get; set; }
        public long Long { get; set; }
        public decimal Decimal { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public DateTime DateTime { get; set; }
        public byte[] RowVersion { get; set; }

        public List<Entity> Collection { get; set; }
    }
}