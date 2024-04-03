using MeruCommonLibrary;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeruPayment.Test.MeruCommonLibrary
{
    [TestFixture]
    class LogHelperTest
    {
        
        

        [Test]
        public void LogErrorForEmailCheck() 
        {
            LogHelper logHelper = new LogHelper("LogHelperTest");
            logHelper.AppSource = "abc";
            logHelper.MethodName = "LogErrorForEmailCheck";
            int _logErrorXTimes = 5;
            
            for (int i = 0; i < _logErrorXTimes; i++)
            {
                logHelper.WriteError(new Exception("Test Exception"), "Test Error " + i.ToString());
                logHelper.WriteInfo("Test Information " + i.ToString());
            }

            Assert.True(true);
        }

        [Test]
        public void LogErrorTest()
        {
            LogHelper logHelper = new LogHelper("LogHelperTest");
            logHelper.AppSource = "efg";
            logHelper.MethodName = "LogErrorTest";
            try
            {
                int i = 0;
                i = 123 / i;
            }
            catch (Exception exception)
            {
                logHelper.WriteError(exception, "error test");
                Assert.True(true);
            }
        }

        [Test]
        public void LogInfoTest()
        {
            LogHelper logHelper = new LogHelper("LogHelperTest");
            logHelper.AppSource = "hij";
            logHelper.MethodName = "LogInfoTest";
            logHelper.WriteInfo("info test");
            Assert.True(true);
        }
    }
}
