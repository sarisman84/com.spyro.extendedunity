using NUnit.Framework;
using Spyro.Debug;
public class DebugSystemTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void VerifyConsoleSystemInit()
    {
        // Use the Assert class to test conditions
        Assert.IsTrue(CommandSystem.HasInitialized);
    }

    [Test]
    public void VerifyCommandExecution()
    {
        var test = false;
        CommandSystem.AddCommand("test", "This is a test command!", (args) =>
        {
            test = !test;
            return true;
        });

        Assert.IsTrue(CommandSystem.Execute("test"));
        Assert.IsFalse(CommandSystem.Execute("test2"));
        Assert.IsTrue(test);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator DebugSystemTestWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
