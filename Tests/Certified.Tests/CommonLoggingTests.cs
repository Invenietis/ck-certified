using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;
using CK.StandardPlugins.ObjectExplorer;
using CK.StandardPlugins.ObjectExplorer.ViewModels.LogViewModels;
using CK.WPF.ViewModel;
using CK.Tests;
using CK.Context;
using CK.Plugin.Hosting;
using CK.Core;

namespace Certified.Tests.LogConfig
{

    [TestFixture]
    public class CommonLoggingTests : TestBase
    {

        [SetUp]
        [TearDown]
        public void CleanupTestFolderDir()
        {
            TestBase.CleanupTestDir();
        }

        string sAm1_1 = "System.Void Method1_1(System.Int32,System.String)";
        string sAm1_2 = "System.Void Method1_2(System.Int32,System.String)";
        string sBm2_1 = "System.Void Method2_1(System.Int32,System.String)";
        string sBm2_2 = "System.Void Method2_2(System.Int32,System.String)";

        string serviceA = "CK.Certified.Tests.IServiceA";
        string serviceB = "CK.Certified.Tests.IServiceB";

        [Test]
        public void MethodCallTest()
        {
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
        }
    }
}
