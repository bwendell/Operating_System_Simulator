using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulater;
using System.Collections.Generic;
using Simulater.Interfaces;
using Simulater.Queues;

namespace Test_1
{
    [TestClass]
    public class MetaTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            MetaData x = new MetaData();
            IQueue<Process> bob = new RegQueue();
            x.readFromFile(null, null, ref bob);

          // Console.Read();
        }
    }
}
