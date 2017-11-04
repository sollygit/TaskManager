using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace TaskManagerUnitTest
{
    public class HomeController
    {
        [Test]
        public void SampleTest()
        {
            Assert.AreEqual("HomeController", "HomeController");
        }

        [Test]
        public void ReturnsContactView()
        {
            var controller = new TaskManager.Controllers.HomeController();
            var result = controller.Contact() as ViewResult;
            Assert.AreEqual("Contact", result.ViewName);
        }
    }
}
