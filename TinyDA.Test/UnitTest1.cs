using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyDA.Mappers;

namespace TinyDA.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAttributeMapper()
        {
            IFieldMapper fieldMapper = new AttributeFieldMapper(typeof(Student));
            
            Assert.IsNull(fieldMapper.MapField("SSS"));

            Assert.AreEqual(fieldMapper.MapField("S_ID"), "Id");
            Assert.AreEqual(fieldMapper.MapField("S_NAME"), "Name");

        }
    }
}
