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
            CallNextGameLogic logic = new CallNextGameLogic();
            logic.callNextGame(null, null, CallNextGameLogic.SelectionPatternEnum.MixCall);

        }

    }
}
