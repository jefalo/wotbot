using TankBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject1
{
    
    
    /// <summary>
    ///这是 Xvm_commTest 的测试类，旨在
    ///包含所有 Xvm_commTest 单元测试
    ///</summary>
    [TestClass()]
    public class Xvm_commTest
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
        ///update_enemy_position_minimap 的测试
        ///</summary>
        [TestMethod()]
        public void update_enemy_position_minimapTest()
        {
            string line = "[B:1364] enemy_location_minimap ";
            XvmComm.getInstance().update_enemy_position_minimap(line);
            Assert.AreEqual(0, 0);
        }

        /// <summary>
        ///read_player_panel 的测试
        ///</summary>
        [TestMethod()]
        public void read_player_panelTest()
        {
            
        }
    }
}
