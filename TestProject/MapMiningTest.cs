﻿using TankBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject
{
    
    
    /// <summary>
    ///这是 MapMiningTest 的测试类，旨在
    ///包含所有 MapMiningTest 单元测试
    ///</summary>
    [TestClass()]
    public class MapMiningTest
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
        ///genRouteTagMap 的测试
        ///</summary>
        [TestMethod()]
        public void genRouteTagMapTest()
        {
            string _map_name = "42_north_america"; // TODO: 初始化为适当的值
            MapMining target = new MapMining(_map_name); // TODO: 初始化为适当的值
            Point source = new Point(3.528571, 10.0547619); // TODO: 初始化为适当的值
            Point enemybase = new Point (9.6506024, 2.369879) ; // TODO: 初始化为适当的值
            
            Trajectory actual;
            actual = target.genRouteTagMap(source, enemybase);
            Assert.AreEqual(actual.Count > 0, true);
            

        }
    }
}
