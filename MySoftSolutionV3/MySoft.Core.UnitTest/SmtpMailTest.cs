﻿using MySoft.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using MySoft.Logger;

namespace MySoft.Core.UnitTest
{


    /// <summary>
    ///这是 SmtpMailTest 的测试类，旨在
    ///包含所有 SmtpMailTest 单元测试
    ///</summary>
    [TestClass()]
    public class SmtpMailTest
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
        ///Send 的测试
        ///</summary>
        [TestMethod()]
        public void SendTest()
        {
            SmtpMail target = SmtpMail.Instance; // TODO: 初始化为适当的值
            string title = "测试"; // TODO: 初始化为适当的值
            string body = "你好"; // TODO: 初始化为适当的值
            string to = "maoyong@fund123.cn"; // TODO: 初始化为适当的值
            SendResult actual = target.Send(title, body, to);
            Assert.AreEqual(actual.Success, true, actual.Message);
        }

        /// <summary>
        ///SendAsync 的测试
        ///</summary>
        [TestMethod()]
        public void SendAsyncTest()
        {
            SmtpMail target = SmtpMail.Instance; // TODO: 初始化为适当的值
            string title = "测试"; // TODO: 初始化为适当的值
            string body = "你好"; // TODO: 初始化为适当的值
            string to = "maoyong@fund123.cn"; // TODO: 初始化为适当的值
            target.SendAsync(title, body, to);
            Thread.Sleep(10000);
        }

        /// <summary>
        ///SendExceptionAsync 的测试
        ///</summary>
        [TestMethod()]
        public void SendExceptionAsyncTest()
        {
            SmtpMail target = SmtpMail.Instance; // TODO: 初始化为适当的值
            Exception ex = new Exception("出错了！"); // TODO: 初始化为适当的值
            string title = "测试"; // TODO: 初始化为适当的值
            string to = "maoyong@fund123.cn"; // TODO: 初始化为适当的值
            target.SendExceptionAsync(ex, title, to);
            Thread.Sleep(10000);
        }

        [TestMethod]
        public void WriteLogTest()
        {
            Exception ex = new Exception("出错了");
            ex.Data["ApplicationName"] = "我的应用程序";

            SimpleLog.Instance.WriteLog(ex);
        }

        public void Register(string name, string email, int age)
        {
            name = "mm";
            email = "my181@1.com";
            age = 20;

            var v = ValidateHelper.Begin()
                           .NotNull(name)
                           .NotNull(email)
                           .InRange(age, 18, 120)
                           .IsEmail(email);
            bool a = v.IsValid;
        }
    }
}
