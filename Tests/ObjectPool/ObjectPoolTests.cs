using NUnit.Framework;
namespace Spyro
{
    public class ObjectPoolTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestPoolIteration()
        {
            // Use the Assert class to test conditions

            ObjectPool list = new ObjectPool(100);
            VerifyIfNoElementsAreFound(list);
            for (int i = 0; i < 50; ++i)
            {
                var obj = list.UseFirstAvailableEntity();
            }
            VerifyIfElementsAreFound(list);

        }

        private static void VerifyIfNoElementsAreFound(ObjectPool list)
        {
            foreach (var (go, _) in list)
            {
                Assert.IsFalse(go);
            }
        }

        private static void VerifyIfElementsAreFound(ObjectPool list)
        {
            var count = 1;
            foreach (var (go, _) in list)
            {
                count++;
                Assert.IsTrue(go && go.activeSelf);
            }

            Assert.AreEqual(50, count);
        }
    }
}

