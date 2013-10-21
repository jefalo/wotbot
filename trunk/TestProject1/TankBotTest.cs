using TankBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace TestProject1
{
    
    
    /// <summary>
    ///这是 TankBotTest 的测试类，旨在
    ///包含所有 TankBotTest 单元测试
    ///</summary>
    [TestClass()]
    public class TankBotTest
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
        ///loadRoute 的测试
        ///</summary>
        [TestMethod()]
        public void loadRouteTest()
        {
            TankBot_Accessor target = new TankBot_Accessor(); // TODO: 初始化为适当的值
            target.mapName = "06_ensk"; // TODO: 初始化为适当的值
            target.loadRoute();
            Assert.AreEqual(target.route.Count, 15);
        }

        /// <summary>
        ///update_ally_position 
        ///</summary>
        [TestMethod()]
        public void ally_position_test()
        {
            for (int i = 0; i < 30; i++)
            {
                //Trace.WriteLine(DateTime.Now.Ticks);
                double x = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalDays;
                Trace.WriteLine(x);
                Thread.Sleep(100);
            }
            string line = "[B:623] ally_location_minimap 77.9 -57.35 pliceher 2.55 -88.95 akamuss -11.8 -91.55 KGJIM 78.45 -63.95 vincent7777 1.4 -85.55 eddietank -12.8 -88.6 jeromepacificar 68.75 -91.3 kazuki1410 -14.45 -85.65 OrangeyOrange -5.65 -91.5 faribb 65.9 -61.6 gemmylhsn -0.85 -88.85 year2529 -15.55 -88.65 koubu -4.1 -88.9 nitro2000 63.65 -61.55 narushisu";
            XvmComm.getInstance().update_ally_position_minimap(line);
            Assert.AreEqual("pliceher",  TankBot.TankBot.getInstance().allyTank[0].username);
            Assert.AreEqual("narushisu", TankBot.TankBot.getInstance().allyTank[13].username);
            
        }



    }
}
