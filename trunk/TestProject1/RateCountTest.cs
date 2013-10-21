using TankBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace TestProject1
{
    
    
    /// <summary>
    ///这是 RateCountTest 的测试类，旨在
    ///包含所有 RateCountTest 单元测试
    ///</summary>
    [TestClass()]
    public class RateCountTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        // 
        //编写测试时，还可使用以下特性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///toStr 的测试
        ///</summary>
        [TestMethod()]
        public void toStrTest()
        {
            RateCount.add("haha");
            RateCount.add("haha");
            RateCount.add("haha");
            Assert.AreEqual("haha 3\r\n", RateCount.toStr());
            Thread.Sleep(2000);
            Assert.AreEqual("haha 0\r\n", RateCount.toStr());
            RateCount.add("h1");
            RateCount.add("h2");
            RateCount.add("h3");

            Assert.AreEqual("haha 0\r\nh1 1\r\nh2 1\r\nh3 1\r\n", RateCount.toStr());
        }
    }
}
