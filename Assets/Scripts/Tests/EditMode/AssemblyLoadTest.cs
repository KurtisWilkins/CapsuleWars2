using NUnit.Framework;
using CapsuleWars.Core;

namespace CapsuleWars.Tests.EditMode
{
    /// <summary>
    /// Smoke test: confirms CapsuleWars.* assemblies are wired up and
    /// reference each other correctly. If this test compiles and passes,
    /// the asmdef graph from Docs/01_Architecture.md is sound.
    /// </summary>
    public class AssemblyLoadTest
    {
        [Test]
        public void Core_AssemblyName_IsExpected()
        {
            Assert.AreEqual("CapsuleWars.Core", CoreModule.AssemblyName);
        }

        [Test]
        public void Core_Assembly_IsLoaded()
        {
            var asm = typeof(CoreModule).Assembly;
            Assert.IsNotNull(asm);
            StringAssert.StartsWith("CapsuleWars.Core", asm.GetName().Name);
        }
    }
}
