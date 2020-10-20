using AWSLambda1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void dbAccessTest()
        {
            Logic logic = new Logic();
            logic.callNextGame(null, null);

        }
    }
}
