using TankBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace TestProject1
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
        ///MapMining 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void MapMiningConstructorTest()
        {
            string map_name = "23_westfeld";
            MapMining target = new MapMining(map_name);
            Tuple<Trajectory, double> x=target.gen_route_obsolete(new Point(1.9, 5.4), new Point(2.3, 3.4));
            Console.WriteLine(x.Item2);
            Assert.IsTrue(x.Item2 < 1);
            Tuple<double, int> y = target.distance_obsolete(new Point(1.9, 5.4), x.Item1);
            //Point p = target.getInterestPoint(new Point(1.9, 5.4), 0);
            Point p = target.getInterestPoint_obsolete(new Point(6.1, 1.7), 100);
            
            Assert.AreEqual(target.trajs.Count, 168);

        }

        /// <summary>
        ///add_to_reachable 的测试
        ///</summary>
        [TestMethod()]
        public void add_to_reachableTest()
        {
            string _map_name = ""; // TODO: 初始化为适当的值
            MapMining target = new MapMining(_map_name); // TODO: 初始化为适当的值
            int x0 = 20; // TODO: 初始化为适当的值
            int y0 = 10; // TODO: 初始化为适当的值
            int x1 = 0; // TODO: 初始化为适当的值
            int y1 = 2; // TODO: 初始化为适当的值
            target.addToHeatmap(x0, y0, x1, y1);
            
            //Assert.Inconclusive("无法验证不返回值的方法。");
        }

        /// <summary>
        ///getHTRoute_2 的测试
        ///</summary>
        [TestMethod()]
        public void getHTRoute_2Test()
        {
            string _map_name = "01_karelia";
            MapMining target = new MapMining(_map_name); 
            Point start = new Point(2.41, 8.24); 
            Trajectory actual;
            actual = target.genRouteToFirepos_osbolete(start);
            //Assert.AreEqual(expected, actual);

            //Assert.Inconclusive("验证此测试方法的正确性。");
        }        
        /// <summary>
        ///getHTRoute_2 的测试
        ///</summary>
        [TestMethod()]
        public void getHTRoute_2Test2()
        {
            string _map_name = "39_crimea";
            MapMining target = new MapMining(_map_name);
            Point start = new Point(6.04, 10.28);
            Trajectory actual;
            actual = target.genRouteToFirepos_osbolete(start);
            //Assert.AreEqual(expected, actual);

            //Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///loadTag 的测试
        ///</summary>
        [TestMethod()]
        public void loadTagTest()
        {
            MapMining target = new MapMining ("01_karelia"); // TODO: 初始化为适当的值
            
        }

        /// <summary>
        /// make sure all start points has a route to fire pos
        ///</summary>
        [TestMethod()]
        public void alwaysHasPathTest()
        {
            MapMining target = new MapMining("01_karelia"); // TODO: 初始化为适当的值
            foreach (Point p in target.startPoints)
            {
                Trajectory t=target.genRouteToFireposTagMap(p);
                Assert.AreEqual(t.Count > 0, true);
            }
        }
    }
}
