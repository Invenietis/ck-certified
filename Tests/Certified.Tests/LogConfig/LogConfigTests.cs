using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CK.Plugin.Runner;
using CK.Model;
using CK.Kernel;
using System.IO;
using CK.Tests;
using CK.StandardPlugins.ObjectExplorer.ViewModels.LogViewModels;
using CK.StandardPlugins.ObjectExplorer;

namespace Certified.Tests.LogConfig
{
    [TestFixture]
    class LogConfigTests : CK.Tests.TestBase
    {
        [Test]
        public void IsDirtyTest()
        {
            IContext ctx = new Context();
            ctx.PluginManager.Discoverer.Discover(new DirectoryInfo(TestBase.PluginFolder), true);
            VMIContext vmiCtx = new VMIContext(ctx);

            VMLogConfig c = new VMLogConfig(vmiCtx);
            
            //After Initialization
            c.Initialize();
            Assert.That(c.IsDirty, Is.False);
            foreach(VMLogServiceConfig s in c.Services)
            {
                Assert.That(s.IsDirty, Is.False);
            }

            //After an update that tracks changes (load from XML conf for example)
            c.UpdateFrom(CreateLogConfigStub(ctx),true);
            Assert.That(c.IsDirty, Is.True);
            foreach (VMLogServiceConfig s in c.Services)
            {
                if (s.Name == "CommonServices.IModeCommandHandlerService" || s.Name == "CommonServices.ICommonTimer" || s.Name == "DUMMY")
                    Assert.That(s.IsDirty, Is.True);
                else
                    Assert.That(s.IsDirty, Is.False);
            }

            //After cancelling modifications on a service
            c.Find("CommonServices.ICommonTimer").CancelModifications();
            Assert.That(c.IsDirty, Is.True);
            foreach (VMLogServiceConfig s in c.Services)
            {
                if (s.Name == "CommonServices.IModeCommandHandlerService" || s.Name == "DUMMY")
                    Assert.That(s.IsDirty, Is.True);
                else
                    Assert.That(s.IsDirty, Is.False);
            }

            //After cancelling modifications (back to the "after Initialize" state)
            c.CancelAllModifications();
            Assert.That(c.IsDirty, Is.False);
            foreach (VMLogServiceConfig s in c.Services)
            {
                Assert.That(s.IsDirty, Is.False);
            }
        }

        [Test]
        public void VMLogConfigInitializeTest()
        {
            IContext ctx = new Context();
            ctx.PluginManager.Discoverer.Discover( new DirectoryInfo( TestBase.PluginFolder ), true );
            VMIContext vmiContext = new VMIContext( ctx );
            VMLogConfig vmLogC = new VMLogConfig( vmiContext );

            Assert.That( vmLogC, Is.Not.Null );
            Assert.That( vmLogC.Services, Is.Not.Null );
            Assert.That( vmLogC.Services.Count, Is.EqualTo( 0 ) );

            vmLogC.FillFromLoader( vmLogC.Services );

            Assert.That( vmLogC.Services.Count, Is.GreaterThan( 0 ) );

            foreach( VMLogServiceConfig vmLogSC in vmLogC.Services )
            {
                Assert.That( vmLogSC.DoLog, Is.EqualTo( false ) );

                Assert.That( vmLogSC.Name, Is.Not.EqualTo( String.Empty ) );
                Assert.That( vmLogSC.IsBound, Is.EqualTo( true ) );

                foreach( VMLogMethodConfig vmLogMC in vmLogSC.Methods )
                {
                    Assert.That( vmLogMC.DoLog, Is.EqualTo( false ) );
                    Assert.That( vmLogMC.DoLogParameters, Is.EqualTo( false ) );
                    Assert.That( vmLogMC.DoLogReturnValue, Is.EqualTo( false ) );
                    Assert.That( vmLogMC.DoLogCaller, Is.EqualTo( false ) );


                    Assert.That( vmLogMC.Name, Is.Not.EqualTo( String.Empty ) );
                    Assert.That( vmLogMC.ReturnType, Is.Not.Null );
                    Assert.That( vmLogMC.Parameters, Is.Not.Null );
                    Assert.That( vmLogMC.IsBound, Is.EqualTo( true ) );
                }

                foreach( VMLogEventConfig vmLogEC in vmLogSC.Events )
                {
                    Assert.That( vmLogEC.DoLog, Is.EqualTo( false ) );
                    Assert.That( vmLogEC.DoLogParameters, Is.EqualTo( false ) );
                    Assert.That( vmLogEC.DoLogDelegates, Is.EqualTo( false ) );
                    Assert.That( vmLogEC.DoLogCaller, Is.EqualTo( false ) );

                    Assert.That( vmLogEC.Name, Is.Not.EqualTo( String.Empty ) );
                    Assert.That( vmLogEC.IsBound, Is.EqualTo( true ) );
                }

                foreach( VMLogPropertyConfig vmLogPC in vmLogSC.Properties )
                {
                    Assert.That( vmLogPC.DoLog, Is.EqualTo( false ) );
                    Assert.That( vmLogPC.DoLogSet, Is.EqualTo( false ) );
                    Assert.That( vmLogPC.DoLogGet, Is.EqualTo( false ) );
                    Assert.That( vmLogPC.DoLogCaller, Is.EqualTo( false ) );

                    Assert.That( vmLogPC.Name, Is.Not.EqualTo( String.Empty ) );
                    Assert.That( vmLogPC.IsBound, Is.EqualTo( true ) );
                    Assert.That( vmLogPC.PropertyType, Is.Not.Null );
                }
            }
        }

        public ILogConfig CreateLogConfigStub( IContext ctx )
        {
            VMLogConfig vmC = new VMLogConfig( new VMIContext( ctx ) );
            vmC.FillFromLoader( vmC.Services );

            #region Creation of a Dummy Service
            VMLogServiceConfig dummyS = new VMLogServiceConfig( "DUMMY", false );

            //Dummy Event Creation
            List<ILogParameterInfo> dummyEList = new List<ILogParameterInfo>();
            dummyEList.Add(new LogParameterInfo("p1", "t1"));
            dummyEList.Add(new LogParameterInfo("p2", "t2"));
            VMLogEventConfig vmE = new VMLogEventConfig("e1", dummyEList, (LogEventErrorFilter)3, (LogEventFilter)7, true);
            vmE.DoLog = true;
            dummyS.Events.Add(vmE);

            //Dummy Method Creation
            List<ILogParameterInfo> dummyMList = new List<ILogParameterInfo>();
            dummyMList.Add(new LogParameterInfo("p3", "t3"));
            dummyMList.Add(new LogParameterInfo("p4", "t4"));
            VMLogMethodConfig vmM = new VMLogMethodConfig("m1", "Void", dummyMList, true, (LogMethodFilter)7, true);
            vmM.DoLog = true;
            dummyS.Methods.Add(vmM);
            //Dummy Property Creation
            VMLogPropertyConfig vmP = new VMLogPropertyConfig("n1", "t1", true, (LogPropertyFilter)7, true);
            vmP.DoLog = true;
            dummyS.Properties.Add(vmP);
            vmC.Services.Add(VMLogServiceConfig.CreateFrom(dummyS));
            #endregion

            //Changes in the LogService's properties
            vmC.DoLog = true;
            // Changes in a LogMethod 
            vmC.Find("CommonServices.IModeCommandHandlerService").FindMethod("System.Void ChangeMode(System.String)").LogFilter = (LogMethodFilter)7;
            vmC.Find("CommonServices.IModeCommandHandlerService").FindMethod("System.Void ChangeMode(System.String)").DoLogErrors = true;            
            // Changes in a LogEvent
            vmC.Find("CommonServices.ICommonTimer").FindEvent("Tick").ErrorFilter = (LogEventErrorFilter)3;
            vmC.Find("CommonServices.ICommonTimer").FindEvent("Tick").LogFilter = (LogEventFilter)7;            
            // Changes in a LogProperty            
            vmC.Find("CommonServices.ICommonTimer").FindProperty("Interval").LogFilter = (LogPropertyFilter)7;
            vmC.Find("CommonServices.ICommonTimer").FindProperty("Interval").DoLogErrors = true;

            return (ILogConfig)vmC;
        }

        [Test]
        public void VMLogConfigUpdateTest()
        {
            IContext ctx = new Context();
            ctx.PluginManager.Discoverer.Discover( new DirectoryInfo( TestBase.PluginFolder ), true );
            VMIContext vmiCtx = new VMIContext( ctx );            

            VMLogConfig vmLogConf = new VMLogConfig( vmiCtx );
            vmLogConf.FillFromLoader( vmLogConf.Services );

            // Create a configured LogConfig
            vmLogConf.UpdateFrom(CreateLogConfigStub(ctx),true);                        

            // Test that LogFilter changes in a LogEvent have been taken account of
            VMLogEventConfig vmLogEvConf = vmLogConf.Find("CommonServices.ICommonTimer").FindEvent("Tick");
            Assert.That(vmLogEvConf.DoLogCaller, Is.True);
            Assert.That(vmLogEvConf.DoLogParameters, Is.True);
            Assert.That(vmLogEvConf.DoLogDelegates, Is.True);
            Assert.That(vmLogEvConf.IsBound, Is.True);
            Assert.That(vmLogEvConf.DoLog, Is.False);
            Assert.That(vmLogEvConf.ErrorFilter, Is.EqualTo((LogEventErrorFilter)3));
            Assert.That(vmLogEvConf.LogFilter, Is.EqualTo((LogEventFilter)7));            

            // Test that LogFilter changes in a LogMethod have been taken account of
            VMLogMethodConfig vmLogMethConf = vmLogConf.Find("CommonServices.IModeCommandHandlerService").FindMethod("System.Void ChangeMode(System.String)");
            Assert.That(vmLogMethConf.DoLogCaller, Is.True);
            Assert.That(vmLogMethConf.DoLogParameters, Is.True);
            Assert.That(vmLogMethConf.DoLogReturnValue, Is.True);
            Assert.That(vmLogMethConf.IsBound, Is.True);
            Assert.That(vmLogMethConf.DoLog, Is.False);            
            Assert.That(vmLogMethConf.LogFilter, Is.EqualTo((LogMethodFilter)7));
            Assert.That(vmLogMethConf.DoLogErrors, Is.True);

            // Test that LogFilter changes in a LogProperty have been taken account of
            VMLogPropertyConfig vmLogPropConf = vmLogConf.Find("CommonServices.ICommonTimer").FindProperty("Interval");
            Assert.That(vmLogPropConf.DoLogCaller, Is.True);
            Assert.That(vmLogPropConf.DoLogGet, Is.True);
            Assert.That(vmLogPropConf.DoLogSet, Is.True);
            Assert.That(vmLogPropConf.IsBound, Is.True);
            Assert.That(vmLogPropConf.DoLog, Is.False);            
            Assert.That(vmLogPropConf.LogFilter, Is.EqualTo((LogPropertyFilter)7));
            Assert.That(vmLogPropConf.DoLogErrors, Is.True);

            //IsDirty Test
            Assert.That(vmLogConf.Find("CommonServices.IModeCommandHandlerService").IsDirty, Is.True);
            Assert.That(vmLogConf.Find("CommonServices.ICommonTimer").IsDirty, Is.True);
            Assert.That(vmLogConf.Find("CommonServices.ICommandManagerService").IsDirty, Is.False);
            Assert.That(vmLogConf.Find("CommonServices.ISendKeyCommandHandlerService").IsDirty, Is.False);
            Assert.That(vmLogConf.Find("DUMMY").IsDirty, Is.True);
            //TODO : IsDirty Tests must be carried on more thoroughly

            // Test Find method and Creation of unbound ViewModels
            VMLogServiceConfig dummyLogSC = vmLogConf.Find( "DUMMY" );
            Assert.That( dummyLogSC.IsBound, Is.False );
            Assert.That(dummyLogSC.IsDirty, Is.True);

            Assert.That(dummyLogSC.FindEvent("e1").Name, Is.EqualTo("e1"));
            Assert.That(dummyLogSC.FindEvent("e1").Parameters.ElementAt(0).ParameterName, Is.EqualTo("p1"));
            Assert.That(dummyLogSC.FindEvent("e1").Parameters.ElementAt(0).ParameterType, Is.EqualTo("t1"));
            Assert.That(dummyLogSC.FindEvent("e1").Parameters.ElementAt(1).ParameterName, Is.EqualTo("p2"));
            Assert.That(dummyLogSC.FindEvent("e1").Parameters.ElementAt(1).ParameterType, Is.EqualTo("t2"));
            Assert.That(dummyLogSC.FindEvent("e1").DoLogCaller, Is.True);
            Assert.That(dummyLogSC.FindEvent("e1").DoLogParameters, Is.True);
            Assert.That(dummyLogSC.FindEvent("e1").DoLogDelegates, Is.True);
            Assert.That(dummyLogSC.FindEvent("e1").IsBound, Is.False);
            Assert.That(dummyLogSC.FindEvent("e1").DoLog, Is.True);
            Assert.That(dummyLogSC.FindEvent("e1").ErrorFilter, Is.EqualTo((LogEventErrorFilter)3));

            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").Name, Is.EqualTo("m1"));
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").ReturnType, Is.EqualTo("Void"));
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").Parameters.ElementAt(0).ParameterName, Is.EqualTo("p3"));
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").Parameters.ElementAt(0).ParameterType, Is.EqualTo("t3"));
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").Parameters.ElementAt(1).ParameterName, Is.EqualTo("p4"));
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").Parameters.ElementAt(1).ParameterType, Is.EqualTo("t4"));
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").DoLogCaller, Is.True);
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").DoLogParameters, Is.True);
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").DoLogReturnValue, Is.True);
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").IsBound, Is.False);
            Assert.That(dummyLogSC.FindMethod("Void m1(t3,t4)").DoLog, Is.True);            

            Assert.That(dummyLogSC.FindProperty("n1").Name, Is.EqualTo("n1"));
            Assert.That(dummyLogSC.FindProperty("n1").PropertyType, Is.EqualTo("t1"));
            Assert.That(dummyLogSC.FindProperty("n1").DoLogCaller, Is.True);
            Assert.That(dummyLogSC.FindProperty("n1").DoLogGet, Is.True);
            Assert.That(dummyLogSC.FindProperty("n1").DoLogSet, Is.True);
            Assert.That(dummyLogSC.FindProperty("n1").IsBound, Is.False);
            Assert.That(dummyLogSC.FindProperty("n1").DoLog, Is.True);            
        }

        [Test]
        public void VMLogConfigClearTest()
        {
            IContext ctx = new Context();
            ctx.PluginManager.Discoverer.Discover( new DirectoryInfo( TestBase.PluginFolder ), true );
            VMIContext vmiCtx = new VMIContext( ctx );            

            VMLogConfig vmLogC = new VMLogConfig( vmiCtx );
            vmLogC.FillFromLoader( vmLogC.Services );

            // Create a configured LogConfig
            vmLogC.UpdateFrom(CreateLogConfigStub(ctx), false);

            Assert.That( vmLogC.Find( "DUMMY" ), Is.Not.Null );

            vmLogC.ClearConfig();

            Assert.That( vmLogC, Is.Not.Null );
            Assert.That( vmLogC.Services, Is.Not.Null );

            //Test the fact that unbound items have been removed
            Assert.That( vmLogC.Find( "DUMMY" ), Is.Null );

            Assert.That( vmLogC.Services.Count, Is.GreaterThan( 0 ) );

            foreach( VMLogServiceConfig vmLogSC in vmLogC.Services )
            {
                Assert.That( vmLogSC.DoLog, Is.EqualTo( false ) );

                Assert.That( vmLogSC.Name, Is.Not.EqualTo( String.Empty ) );
                Assert.That( vmLogSC.IsBound, Is.EqualTo( true ) );

                foreach( VMLogMethodConfig vmLogMC in vmLogSC.Methods )
                {
                    Assert.That( vmLogMC.DoLog, Is.EqualTo( false ) );
                    Assert.That( vmLogMC.DoLogParameters, Is.EqualTo( false ) );
                    Assert.That( vmLogMC.DoLogReturnValue, Is.EqualTo( false ) );
                    Assert.That( vmLogMC.DoLogCaller, Is.EqualTo( false ) );

                    Assert.That( vmLogMC.Name, Is.Not.EqualTo( String.Empty ) );
                    Assert.That( vmLogMC.ReturnType, Is.Not.Null );
                    Assert.That( vmLogMC.Parameters, Is.Not.Null );
                    Assert.That( vmLogMC.IsBound, Is.EqualTo( true ) );
                }

                foreach( VMLogEventConfig vmLogEC in vmLogSC.Events )
                {
                    Assert.That( vmLogEC.DoLog, Is.EqualTo( false ) );
                    Assert.That( vmLogEC.DoLogParameters, Is.EqualTo( false ) );
                    Assert.That( vmLogEC.DoLogDelegates, Is.EqualTo( false ) );
                    Assert.That( vmLogEC.DoLogCaller, Is.EqualTo( false ) );

                    Assert.That( vmLogEC.Name, Is.Not.EqualTo( String.Empty ) );
                    Assert.That( vmLogEC.IsBound, Is.EqualTo( true ) );
                }

                foreach( VMLogPropertyConfig vmLogPC in vmLogSC.Properties )
                {
                    Assert.That( vmLogPC.DoLog, Is.EqualTo( false ) );
                    Assert.That( vmLogPC.DoLogSet, Is.EqualTo( false ) );
                    Assert.That( vmLogPC.DoLogGet, Is.EqualTo( false ) );
                    Assert.That( vmLogPC.DoLogCaller, Is.EqualTo( false ) );

                    Assert.That( vmLogPC.Name, Is.Not.EqualTo( String.Empty ) );
                    Assert.That( vmLogPC.IsBound, Is.EqualTo( true ) );
                    Assert.That( vmLogPC.PropertyType, Is.Not.Null );
                }
            }
        }

        [Test]
        public void SnapShotTest()
        {            
            bool serviceNameStep = false;
            bool eventNameStep = false;
            bool eventPropertyNameStep = false;
            bool propertyNameStep = false;
            bool methodNameStep = false;
            bool methodPropertyNameStep = false;

            IContext context = new Context();
            context.PluginManager.Discoverer.Discover(new DirectoryInfo(TestBase.PluginFolder), true);
            VMIContext vmiCtx = new VMIContext(context);
            VMLogConfig vmLogConf = new VMLogConfig(vmiCtx);
            vmLogConf.FillFromLoader(vmLogConf.Services);
            vmLogConf.UpdateFrom(CreateLogConfigStub(context), false);

            ILogConfig logConf = (ILogConfig)vmLogConf;
            ILogConfig copiedLogConf = LogConfigHelper.LogConfigCopy(logConf);

            Assert.That(logConf.DoLog, Is.EqualTo(copiedLogConf.DoLog));

            //Service step
            Assert.That(logConf.Services.Count, Is.GreaterThan(0));
            Assert.That(logConf.Services.Count, Is.EqualTo(copiedLogConf.Services.Count));
            foreach (ILogServiceConfig logServConf in logConf.Services)
            {
                foreach (ILogServiceConfig copiedLogServConf in copiedLogConf.Services)
                {
                    if (logServConf.Name == copiedLogServConf.Name)
                    {
                        Assert.IsFalse(serviceNameStep);
                        serviceNameStep = true;

                        Assert.That(logServConf.DoLog, Is.EqualTo(copiedLogServConf.DoLog));

                        //Event step
                        Assert.That(logServConf.Events.Count, Is.EqualTo(copiedLogServConf.Events.Count));
                        foreach (ILogEventConfig lEvConf in logServConf.Events)
                        {
                            foreach (ILogEventConfig copiedLogEvConf in copiedLogServConf.Events)
                            {
                                if (lEvConf.Name == copiedLogEvConf.Name)
                                {
                                    Assert.IsFalse(eventNameStep);
                                    eventNameStep = true;

                                    Assert.That(lEvConf.DoLog, Is.EqualTo(copiedLogEvConf.DoLog));
                                    Assert.That(lEvConf.ErrorFilter, Is.EqualTo(copiedLogEvConf.ErrorFilter));
                                    Assert.That(lEvConf.LogFilter, Is.EqualTo(copiedLogEvConf.LogFilter));

                                    foreach (ILogParameterInfo parameter in lEvConf.Parameters)
                                    {
                                        foreach (ILogParameterInfo copiedParameter in copiedLogEvConf.Parameters)
                                        {
                                            if (parameter.ParameterName == copiedParameter.ParameterName)
                                            {
                                                Assert.IsFalse(eventPropertyNameStep);
                                                eventPropertyNameStep = true;
                                                Assert.That(parameter.ParameterType, Is.EqualTo(copiedParameter.ParameterType));
                                            }
                                        }
                                        Assert.IsTrue(eventPropertyNameStep);
                                        eventPropertyNameStep = false;
                                    }
                                }
                            }
                            Assert.IsTrue(eventNameStep);
                            eventNameStep = false;
                        }

                        //Property step
                        Assert.That(logServConf.Properties.Count, Is.EqualTo(copiedLogServConf.Properties.Count));
                        foreach (ILogPropertyConfig logPropConf in logServConf.Properties)
                        {
                            foreach (ILogPropertyConfig copiedLogPropConf in copiedLogServConf.Properties)
                            {
                                if (logPropConf.Name == copiedLogPropConf.Name)
                                {
                                    Assert.IsFalse(propertyNameStep);
                                    propertyNameStep = true;

                                    Assert.That(logPropConf.DoLog, Is.EqualTo(copiedLogPropConf.DoLog));
                                    Assert.That(logPropConf.PropertyType, Is.EqualTo(copiedLogPropConf.PropertyType));
                                    Assert.That(logPropConf.DoLogErrors, Is.EqualTo(copiedLogPropConf.DoLogErrors));
                                    Assert.That(logPropConf.LogFilter, Is.EqualTo(copiedLogPropConf.LogFilter));
                                }
                            }
                            Assert.IsTrue(propertyNameStep);
                            propertyNameStep = false;
                        }

                        //Method step
                        Assert.That(logServConf.Methods.Count, Is.EqualTo(copiedLogServConf.Methods.Count));
                        foreach (ILogMethodConfig logMethdConf in logServConf.Methods)
                        {
                            foreach (ILogMethodConfig copiedLogMethdConf in copiedLogServConf.Methods)
                            {
                                if (logMethdConf.GetSimpleSignature() == copiedLogMethdConf.GetSimpleSignature())
                                {
                                    Assert.IsFalse(methodNameStep);
                                    methodNameStep = true;

                                    Assert.That(logMethdConf.DoLog, Is.EqualTo(copiedLogMethdConf.DoLog));
                                    Assert.That(logMethdConf.DoLogErrors, Is.EqualTo(copiedLogMethdConf.DoLogErrors));
                                    Assert.That(logMethdConf.LogFilter, Is.EqualTo(copiedLogMethdConf.LogFilter));
                                    Assert.That(logMethdConf.ReturnType, Is.EqualTo(copiedLogMethdConf.ReturnType));

                                    Assert.That(logMethdConf.Parameters.Count, Is.EqualTo(copiedLogMethdConf.Parameters.Count));
                                    foreach (ILogParameterInfo parameter in logMethdConf.Parameters)
                                    {
                                        foreach (ILogParameterInfo copiedParameter in copiedLogMethdConf.Parameters)
                                        {
                                            if (parameter.ParameterName == copiedParameter.ParameterName)
                                            {
                                                Assert.IsFalse(methodPropertyNameStep);
                                                methodPropertyNameStep = true;
                                                Assert.That(parameter.ParameterType, Is.EqualTo(copiedParameter.ParameterType));
                                            }
                                        }
                                        Assert.IsTrue(methodPropertyNameStep);
                                        methodPropertyNameStep = false;
                                    }
                                }
                            }
                            Assert.IsTrue(methodNameStep);
                            methodNameStep = false;
                        }
                    }
                }
                Assert.IsTrue(serviceNameStep);
                serviceNameStep = false;
            }
        }
    }
}
