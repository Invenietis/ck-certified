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
using CK.Plugin;
using CK.Core;
using CK.SharedDic;
using CK.Plugin.Config;
using CommonServices;

namespace Certified.Tests.LogConfig
{

    [TestFixture]
    class LogConfigTests : TestBase
    {
        //Used as keys for the MockPluginConfigAccessor
        public static object user;
        public static object system;

        public IContext context;
        ISharedDictionary dic;

        INamedVersionedUniqueId idEdited = new SimpleNamedVersionedUniqueId( Guid.NewGuid(), new Version( 1, 0, 0 ), "MockPlugin" );

        [SetUp]
        public void Setup()
        {            
            TestBase.CleanupTestDir();
            
            user = new object();
            system = new object();
            context = Context.CreateInstance();
            dic = SharedDictionary.Create( context.BaseServiceProvider );
        }

        [TearDown]
        public void TearDown()
        {
            TestBase.CleanupTestDir();
        }

        string sAm1_1 = "System.Void Method1_1(System.String,System.Int32)";
        string sAm1_2 = "System.Void Method1_2(System.String,System.Int32)";
        string sBm2_1 = "System.Void Method2_1(System.String,System.Int32)";
        string sBm2_2 = "System.Void Method2_2(System.String,System.Int32)";

        string serviceA = "CK.Certified.Tests.IServiceA";
        string serviceB = "CK.Certified.Tests.IServiceB";

        private void LogServiceConfigIsClearTest( VMLogServiceConfig vmLogSC )
        {
            Assert.That( vmLogSC.DoLog, Is.EqualTo( false ) );

            Assert.That( vmLogSC.Name, Is.Not.EqualTo( String.Empty ) );
            Assert.That( vmLogSC.IsBound, Is.EqualTo( true ) );

            foreach( VMLogMethodConfig vmLogMC in vmLogSC.Methods )
            {
                Assert.That( vmLogMC.DoLog, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogErrors, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogEnter, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogParameters, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogCaller, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogLeave, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogReturnValue, Is.EqualTo( false ) );

                Assert.That( vmLogMC.Name, Is.Not.EqualTo( String.Empty ) );
                Assert.That( vmLogMC.ReturnType, Is.Not.Null );
                Assert.That( vmLogMC.Parameters, Is.Not.Null );
                Assert.That( vmLogMC.IsBound, Is.EqualTo( true ) );
            }

            foreach( VMLogEventConfig vmLogEC in vmLogSC.Events )
            {
                Assert.That( vmLogEC.DoLog, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogErrors, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogStartRaise, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogParameters, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogEndRaise, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoCatchEventWhenServiceStopped, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogCaughtEventWhenServiceStopped, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoCatchBadEventHandling, Is.EqualTo( false ) );

                Assert.That( vmLogEC.Name, Is.Not.EqualTo( String.Empty ) );
                Assert.That( vmLogEC.IsBound, Is.EqualTo( true ) );
            }

            //foreach( VMLogPropertyConfig vmLogPC in vmLogSC.Properties )
            //{
            //    Assert.That( vmLogPC.DoLog, Is.EqualTo( false ) );
            //    Assert.That( vmLogPC.DoLogSet, Is.EqualTo( false ) );
            //    Assert.That( vmLogPC.DoLogGet, Is.EqualTo( false ) );
            //    Assert.That( vmLogPC.DoLogCaller, Is.EqualTo( false ) );

            //    Assert.That( vmLogPC.Name, Is.Not.EqualTo( String.Empty ) );
            //    Assert.That( vmLogPC.IsBound, Is.EqualTo( true ) );
            //    Assert.That( vmLogPC.PropertyType, Is.Not.Null );
            //}
        }

        private void LogServiceConfigIsDefaultTest( VMLogServiceConfig vmLogSC )
        {
            Assert.That( vmLogSC.DoLog, Is.EqualTo( true ) );

            Assert.That( vmLogSC.Name, Is.Not.EqualTo( String.Empty ) );
            Assert.That( vmLogSC.IsBound, Is.EqualTo( true ) );

            foreach( VMLogMethodConfig vmLogMC in vmLogSC.Methods )
            {
                Assert.That( vmLogMC.DoLog, Is.EqualTo( true ) );
                Assert.That( vmLogMC.DoLogErrors, Is.EqualTo( true ) );
                Assert.That( vmLogMC.DoLogEnter, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogParameters, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogCaller, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogLeave, Is.EqualTo( false ) );
                Assert.That( vmLogMC.DoLogReturnValue, Is.EqualTo( false ) );

                Assert.That( vmLogMC.Name, Is.Not.EqualTo( String.Empty ) );
                Assert.That( vmLogMC.ReturnType, Is.Not.Null );
                Assert.That( vmLogMC.Parameters, Is.Not.Null );
                Assert.That( vmLogMC.IsBound, Is.EqualTo( true ) );
            }

            foreach( VMLogEventConfig vmLogEC in vmLogSC.Events )
            {
                Assert.That( vmLogEC.DoLog, Is.EqualTo( true ) );
                Assert.That( vmLogEC.DoLogErrors, Is.EqualTo( true ) );
                Assert.That( vmLogEC.DoLogStartRaise, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogParameters, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogEndRaise, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoCatchEventWhenServiceStopped, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoLogCaughtEventWhenServiceStopped, Is.EqualTo( false ) );
                Assert.That( vmLogEC.DoCatchBadEventHandling, Is.EqualTo( false ) );

                Assert.That( vmLogEC.Name, Is.Not.EqualTo( String.Empty ) );
                Assert.That( vmLogEC.IsBound, Is.EqualTo( true ) );
            }

            //foreach( VMLogPropertyConfig vmLogPC in vmLogSC.Properties )
            //{
            //    Assert.That( vmLogPC.DoLog, Is.EqualTo( false ) );
            //    Assert.That( vmLogPC.DoLogSet, Is.EqualTo( false ) );
            //    Assert.That( vmLogPC.DoLogGet, Is.EqualTo( false ) );
            //    Assert.That( vmLogPC.DoLogCaller, Is.EqualTo( false ) );

            //    Assert.That( vmLogPC.Name, Is.Not.EqualTo( String.Empty ) );
            //    Assert.That( vmLogPC.IsBound, Is.EqualTo( true ) );
            //    Assert.That( vmLogPC.PropertyType, Is.Not.Null );
            //}
        }

        private void LogConfigIsClearTest( VMLogConfig c )
        {
            Assert.That( c, Is.Not.Null );
            Assert.That( c.Services, Is.Not.Null );

            //Test the fact that unbound items have been removed (if there were any)
            Assert.That( c.Find( "DUMMY" ), Is.Null );

            Assert.That( c.Services.Count, Is.GreaterThan( 0 ) );
            Assert.That( c.IsEmpty, Is.True );
            foreach( VMLogServiceConfig vmLogSC in c.Services )
            {
                LogServiceConfigIsClearTest( vmLogSC );
            }
        }

        private void LogConfigIsDefaultTest( VMLogConfig c )
        {
            Assert.That( c, Is.Not.Null );
            Assert.That( c.Services, Is.Not.Null );

            //Test the fact that unbound items have been removed (if there were any)
            Assert.That( c.Find( "DUMMY" ), Is.Null );

            Assert.That( c.Services.Count, Is.GreaterThan( 0 ) );
            Assert.That( c.DoLog, Is.True );
            foreach( VMLogServiceConfig vmLogSC in c.Services )
            {
                LogServiceConfigIsDefaultTest( vmLogSC );
            }
        }

        private void ChangeLogConfiguration( VMLogConfig c )
        {
            //ServiceA is full - logging
            VMLogServiceConfig s = c.Find( serviceA );
            s.FindEvent( "Event1_1" ).DoLog = true;
            s.FindEvent( "Event1_1" ).DoLogErrors = true;
            s.FindEvent( "Event1_1" ).DoLogStartRaise = true;
            s.FindEvent( "Event1_1" ).DoLogParameters = true;
            s.FindEvent( "Event1_1" ).DoLogEndRaise = true;
            s.FindEvent( "Event1_1" ).DoCatchEventWhenServiceStopped = true;
            s.FindEvent( "Event1_1" ).DoLogCaughtEventWhenServiceStopped = true;
            s.FindEvent( "Event1_1" ).DoCatchBadEventHandling = true; // --> = 127          

            s.FindEvent( "Event1_2" ).DoLog = true;
            s.FindEvent( "Event1_2" ).DoLogErrors = true;
            s.FindEvent( "Event1_2" ).DoLogStartRaise = true;
            s.FindEvent( "Event1_2" ).DoLogParameters = true;
            s.FindEvent( "Event1_2" ).DoLogEndRaise = true;
            s.FindEvent( "Event1_2" ).DoCatchEventWhenServiceStopped = true;
            s.FindEvent( "Event1_2" ).DoLogCaughtEventWhenServiceStopped = true;
            s.FindEvent( "Event1_2" ).DoCatchBadEventHandling = true; // --> = 127

            s.FindMethod( sAm1_1 ).DoLog = true;
            s.FindMethod( sAm1_1 ).DoLogErrors = true;
            s.FindMethod( sAm1_1 ).DoLogEnter = true;
            s.FindMethod( sAm1_1 ).DoLogParameters = true;
            s.FindMethod( sAm1_1 ).DoLogCaller = true;
            s.FindMethod( sAm1_1 ).DoLogLeave = true;
            s.FindMethod( sAm1_1 ).DoLogReturnValue = true; // --> = 63

            s.FindMethod( sAm1_2 ).DoLog = true;
            s.FindMethod( sAm1_2 ).DoLogErrors = true;
            s.FindMethod( sAm1_2 ).DoLogEnter = true;
            s.FindMethod( sAm1_2 ).DoLogParameters = true;
            s.FindMethod( sAm1_2 ).DoLogCaller = true;
            s.FindMethod( sAm1_2 ).DoLogLeave = true;
            s.FindMethod( sAm1_2 ).DoLogReturnValue = true; // --> = 63

            s.FindProperty( "Property1_1" ).DoLog = true;
            s.FindProperty( "Property1_1" ).DoLogErrors = true;
            s.FindProperty( "Property1_1" ).LogFilter = (LogPropertyFilter)7;

            s.FindProperty( "Property1_2" ).DoLog = true;
            s.FindProperty( "Property1_2" ).DoLogErrors = true;
            s.FindProperty( "Property1_2" ).LogFilter = (LogPropertyFilter)7;

            //ServiceB is half-full logging
            s = c.Find( serviceB );

            s.FindEvent( "Event2_1" ).DoLog = true;
            s.FindEvent( "Event2_1" ).DoLogErrors = true;
            s.FindEvent( "Event2_1" ).DoLogStartRaise = true;
            s.FindEvent( "Event2_1" ).DoLogParameters = true;
            s.FindEvent( "Event2_1" ).DoLogEndRaise = true;
            s.FindEvent( "Event2_1" ).DoCatchEventWhenServiceStopped = true;
            s.FindEvent( "Event2_1" ).DoLogCaughtEventWhenServiceStopped = true;
            s.FindEvent( "Event2_1" ).DoCatchBadEventHandling = true;

            s.FindMethod( sBm2_1 ).DoLog = true;
            s.FindMethod( sBm2_1 ).DoLogErrors = true;
            s.FindMethod( sBm2_1 ).DoLogEnter = true;
            s.FindMethod( sBm2_1 ).DoLogParameters = true;
            s.FindMethod( sBm2_1 ).DoLogCaller = false; //*
            s.FindMethod( sBm2_1 ).DoLogLeave = false; //*
            s.FindMethod( sBm2_1 ).DoLogReturnValue = false; //* --> = 7

            s.FindProperty( "Property2_1" ).DoLog = true;
            s.FindProperty( "Property2_1" ).DoLogErrors = true;
            s.FindProperty( "Property2_1" ).LogFilter = (LogPropertyFilter)7;


            //DummyService is full - logging and from scratch : unbound
            #region Creation of a Dummy Service
            VMLogServiceConfig d = new VMLogServiceConfig( "DUMMY", false );            
            d.DoLog = true;

            //Dummy Event Creation
            List<ILogParameterInfo> eList = new List<ILogParameterInfo>();
            eList.Add( new LogParameterInfo( "p1", "t1" ) );
            eList.Add( new LogParameterInfo( "p2", "t2" ) );
            VMLogEventConfig e = new VMLogEventConfig(d, "e1", eList, (ServiceLogEventOptions)127, true );            

            e.DoLog = true;
            //e.DoLogErrors = true;
            //e.DoLogStartRaise = true;
            //e.DoLogParameters = true;
            //e.DoLogEndRaise = true;
            //e.DoCatchEventWhenServiceStopped = true;
            //e.DoLogCaughtEventWhenServiceStopped = true;
            //e.DoCatchBadEventHandling = true; // --> = 127

            d.Events.Add( e );

            //Dummy Method Creation
            List<ILogParameterInfo> mList = new List<ILogParameterInfo>();
            mList.Add( new LogParameterInfo( "p3", "t3" ) );
            mList.Add( new LogParameterInfo( "p4", "t4" ) );
            VMLogMethodConfig m = new VMLogMethodConfig( d, "m1", "Void", mList, true );

            m.DoLog = true;
            m.DoLogErrors = true;
            m.DoLogEnter = true;
            m.DoLogParameters = true;
            m.DoLogCaller = true;
            m.DoLogLeave = true;
            m.DoLogReturnValue = true; // --> = 63

            m.DoLogErrors = true;
            m.DoLog = true;
            d.Methods.Add( m );
            //Dummy Property Creation
            VMLogPropertyConfig p = new VMLogPropertyConfig( "n1", "t1", true, (LogPropertyFilter)7, true );
            p.DoLogErrors = true;
            p.DoLog = true;
            d.Properties.Add( p );
            VMLogServiceConfig dummyS = VMLogServiceConfig.CreateFrom( c, d );
            c.EventRegistration( dummyS );
            c.Services.Add( dummyS );

            c.Find( "DUMMY" ).IsDirty = true;
            #endregion
        }
        private void ChangeLogConfigurationTest( VMLogConfig c, bool lastUpdateIsFromKernel )
        {
            ServiceATest( c );
            ServiceBTest( c );
            DummyServiceTest( c );
            #region IsDirty Test

            //IsDirty Test, if the last update was from kernel, nothing should be dirty.
            if( lastUpdateIsFromKernel )
            {
                Assert.That( c.Find( serviceB ).IsDirty, Is.False );
                Assert.That( c.Find( serviceA ).IsDirty, Is.False );
                Assert.That( c.Find( "DUMMY" ).IsDirty, Is.False );
            }
            else
            {
                Assert.That( c.Find( serviceB ).IsDirty, Is.True );
                Assert.That( c.Find( serviceA ).IsDirty, Is.True );
                Assert.That( c.Find( "DUMMY" ).IsDirty, Is.True );
            }

            #endregion
        }

        private void ServiceATest( VMLogConfig c )
        {
            #region ServiceA full log Test

            //VMLogServiceConfig s = c.Find( serviceA );

            //s.FindEvent( "Event1_1" ).DoLog = true;
            //s.FindEvent( "Event1_1" ).DoLogErrors = true;
            //s.FindEvent( "Event1_1" ).DoLogStartRaise = true;
            //s.FindEvent( "Event1_1" ).DoLogParameters = true;
            //s.FindEvent( "Event1_1" ).DoLogEndRaise = true;
            //s.FindEvent( "Event1_1" ).DoCatchEventWhenServiceStopped = true;
            //s.FindEvent( "Event1_1" ).DoLogCaughtEventWhenServiceStopped = true;
            //s.FindEvent( "Event1_1" ).DoCatchBadEventHandling = true; // --> = 127          

            //s.FindEvent( "Event1_2" ).DoLog = true;
            //s.FindEvent( "Event1_2" ).DoLogErrors = true;
            //s.FindEvent( "Event1_2" ).DoLogStartRaise = true;
            //s.FindEvent( "Event1_2" ).DoLogParameters = true;
            //s.FindEvent( "Event1_2" ).DoLogEndRaise = true;
            //s.FindEvent( "Event1_2" ).DoCatchEventWhenServiceStopped = true;
            //s.FindEvent( "Event1_2" ).DoLogCaughtEventWhenServiceStopped = true;
            //s.FindEvent( "Event1_2" ).DoCatchBadEventHandling = true; // --> = 127


            // Test that LogFilter changes in a LogEvent have been taken account of
            VMLogEventConfig vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_1" );
            Assert.That( vmLogEvConf.IsBound, Is.True );

            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.DoLogErrors, Is.True );
            Assert.That( vmLogEvConf.DoLogStartRaise, Is.True );
            Assert.That( vmLogEvConf.DoLogParameters, Is.True );
            Assert.That( vmLogEvConf.DoLogEndRaise, Is.True );
            Assert.That( vmLogEvConf.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmLogEvConf.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmLogEvConf.DoCatchBadEventHandling, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_2" );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.DoLogErrors, Is.True );
            Assert.That( vmLogEvConf.DoLogStartRaise, Is.True );
            Assert.That( vmLogEvConf.DoLogParameters, Is.True );
            Assert.That( vmLogEvConf.DoLogEndRaise, Is.True );
            Assert.That( vmLogEvConf.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmLogEvConf.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmLogEvConf.DoCatchBadEventHandling, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            VMLogMethodConfig vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_1 );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.DoLogErrors, Is.True );
            Assert.That( vmLogMethConf.DoLogEnter, Is.True );
            Assert.That( vmLogMethConf.DoLogParameters, Is.True );
            Assert.That( vmLogMethConf.DoLogCaller, Is.True );
            Assert.That( vmLogMethConf.DoLogLeave, Is.True );
            Assert.That( vmLogMethConf.DoLogReturnValue, Is.True );

            Assert.That( vmLogMethConf.IsBound, Is.True );

            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_2 );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.DoLogErrors, Is.True );
            Assert.That( vmLogMethConf.DoLogEnter, Is.True );
            Assert.That( vmLogMethConf.DoLogParameters, Is.True );
            Assert.That( vmLogMethConf.DoLogCaller, Is.True );
            Assert.That( vmLogMethConf.DoLogLeave, Is.True );
            Assert.That( vmLogMethConf.DoLogReturnValue, Is.True );

            Assert.That( vmLogMethConf.IsBound, Is.True );

            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            #region Prop
            // Test that LogFilter changes in a LogProperty have been taken account of
            VMLogPropertyConfig vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );
            #endregion

            #endregion
        }
        private void ServiceBTest( VMLogConfig c )
        {
            #region ServiceB log test -- event2_1 : 127 -  method2_1 : 7

            // Test that LogFilter changes in a LogEvent have been taken account of
            VMLogEventConfig vmLogEvConf = c.Find( serviceB ).FindEvent( "Event2_1" );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.DoLogErrors, Is.True );
            Assert.That( vmLogEvConf.DoLogStartRaise, Is.True );
            Assert.That( vmLogEvConf.DoLogParameters, Is.True );
            Assert.That( vmLogEvConf.DoLogEndRaise, Is.True );
            Assert.That( vmLogEvConf.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmLogEvConf.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmLogEvConf.DoCatchBadEventHandling, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            VMLogMethodConfig vmLogMethConf = c.Find( serviceB ).FindMethod( sBm2_1 );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.DoLogErrors, Is.True );
            Assert.That( vmLogMethConf.DoLogEnter, Is.True );
            Assert.That( vmLogMethConf.DoLogParameters, Is.True );
            Assert.That( vmLogMethConf.DoLogCaller, Is.False );
            Assert.That( vmLogMethConf.DoLogLeave, Is.False );
            Assert.That( vmLogMethConf.DoLogReturnValue, Is.False );

            Assert.That( vmLogMethConf.IsBound, Is.True );

            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)7 ) );

            // Test that LogFilter changes in a LogProperty have been taken account of
            VMLogPropertyConfig vmLogPropConf = c.Find( serviceB ).FindProperty( "Property2_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceB ).FindProperty( "Property2_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.False );
            Assert.That( vmLogPropConf.DoLogGet, Is.False );
            Assert.That( vmLogPropConf.DoLogSet, Is.False );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.False );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)0 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.False );

            #endregion
        }
        private void DummyServiceTest( VMLogConfig c )
        {
            #region DummyService full log Test

            // Test Find method and Creation of unbound ViewModels
            VMLogServiceConfig dummyLogSC = c.Find( "DUMMY" );
            Assert.That( dummyLogSC.IsBound, Is.False );

            VMLogEventConfig vmE = dummyLogSC.FindEvent( "e1" );
            Assert.That( vmE.Name, Is.EqualTo( "e1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t1" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p2" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t2" ) );

            Assert.That( vmE.IsBound, Is.False );

            Assert.That( vmE.DoLog, Is.True );
            Assert.That( vmE.DoLogErrors, Is.True );
            Assert.That( vmE.DoLogStartRaise, Is.True );
            Assert.That( vmE.DoLogParameters, Is.True );
            Assert.That( vmE.DoLogEndRaise, Is.True );
            Assert.That( vmE.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoCatchBadEventHandling, Is.True );

            Assert.That( vmE.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            VMLogMethodConfig vmM = dummyLogSC.FindMethod( "Void m1(t3,t4)" );
            Assert.That( vmM != null );
            Assert.That( vmM.Name, Is.EqualTo( "m1" ) );
            Assert.That( vmM.ReturnType, Is.EqualTo( "Void" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p3" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t3" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p4" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t4" ) );
            Assert.That( vmM.IsBound, Is.False );
            Assert.That( vmM.DoLog, Is.True );
            Assert.That( vmM.DoLogErrors, Is.True );

            Assert.That( vmM.DoLogEnter, Is.True );
            Assert.That( vmM.DoLogParameters, Is.True );
            Assert.That( vmM.DoLogCaller, Is.True );
            Assert.That( vmM.DoLogLeave, Is.True );
            Assert.That( vmM.DoLogReturnValue, Is.True );

            VMLogPropertyConfig vmP = dummyLogSC.FindProperty( "n1" );
            Assert.That( vmP.Name, Is.EqualTo( "n1" ) );
            Assert.That( vmP.PropertyType, Is.EqualTo( "t1" ) );
            Assert.That( vmP.DoLogCaller, Is.True );
            Assert.That( vmP.DoLogGet, Is.True );
            Assert.That( vmP.DoLogSet, Is.True );
            Assert.That( vmP.IsBound, Is.False );
            Assert.That( vmP.DoLog, Is.True );
            Assert.That( vmP.DoLogErrors, Is.True );

            #endregion
        }

        private ILogConfig CreateChangedLogConfiguration( IContext ctx )
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
            VMLogConfig c = new VMLogConfig( new VMIContextViewModel( ctx, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService ), logService );
            c.FillFromDiscoverer( c.Services );
            ChangeLogConfiguration( c );
            ChangeLogConfigurationTest( c, false );
            return c;
        }
        public void SnapShotTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            bool serviceNameStep = false;
            bool eventNameStep = false;
            bool eventPropertyNameStep = false;
            bool propertyNameStep = false;
            bool methodNameStep = false;
            bool methodPropertyNameStep = false;

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( PluginFolder ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.FillFromDiscoverer( c.Services );
            //vmLogConf.UpdateFrom(CreateLogConfigStub(context), false);
            ChangeLogConfiguration( c );

            ILogConfig logConf = (ILogConfig)c;
            ILogConfig copiedLogConf = logConf.Clone();

            Assert.That( logConf.DoLog, Is.EqualTo( copiedLogConf.DoLog ) );

            //Service step
            Assert.That( logConf.Services.Count, Is.GreaterThan( 0 ) );
            Assert.That( logConf.Services.Count, Is.EqualTo( copiedLogConf.Services.Count ) );
            foreach( ILogServiceConfig logServConf in logConf.Services )
            {
                foreach( ILogServiceConfig copiedLogServConf in copiedLogConf.Services )
                {
                    if( logServConf.Name == copiedLogServConf.Name )
                    {
                        Assert.IsFalse( serviceNameStep );
                        serviceNameStep = true;

                        Assert.That( logServConf.DoLog, Is.EqualTo( copiedLogServConf.DoLog ) );

                        //Event step
                        Assert.That( logServConf.Events.Count, Is.EqualTo( copiedLogServConf.Events.Count ) );
                        foreach( ILogEventConfig lEvConf in logServConf.Events )
                        {
                            foreach( ILogEventConfig copiedLogEvConf in copiedLogServConf.Events )
                            {
                                if( lEvConf.Name == copiedLogEvConf.Name )
                                {
                                    Assert.IsFalse( eventNameStep );
                                    eventNameStep = true;

                                    Assert.That( lEvConf.DoLog, Is.EqualTo( copiedLogEvConf.DoLog ) );
                                    Assert.That( lEvConf.LogOptions, Is.EqualTo( copiedLogEvConf.LogOptions ) );

                                    foreach( ILogParameterInfo parameter in lEvConf.Parameters )
                                    {
                                        foreach( ILogParameterInfo copiedParameter in copiedLogEvConf.Parameters )
                                        {
                                            if( parameter.ParameterName == copiedParameter.ParameterName )
                                            {
                                                Assert.IsFalse( eventPropertyNameStep );
                                                eventPropertyNameStep = true;
                                                Assert.That( parameter.ParameterType, Is.EqualTo( copiedParameter.ParameterType ) );
                                            }
                                        }
                                        Assert.IsTrue( eventPropertyNameStep );
                                        eventPropertyNameStep = false;
                                    }
                                }
                            }
                            Assert.IsTrue( eventNameStep );
                            eventNameStep = false;
                        }

                        //Property step
                        Assert.That( logServConf.Properties.Count, Is.EqualTo( copiedLogServConf.Properties.Count ) );
                        foreach( ILogPropertyConfig logPropConf in logServConf.Properties )
                        {
                            foreach( ILogPropertyConfig copiedLogPropConf in copiedLogServConf.Properties )
                            {
                                if( logPropConf.Name == copiedLogPropConf.Name )
                                {
                                    Assert.IsFalse( propertyNameStep );
                                    propertyNameStep = true;

                                    Assert.That( logPropConf.DoLog, Is.EqualTo( copiedLogPropConf.DoLog ) );
                                    Assert.That( logPropConf.PropertyType, Is.EqualTo( copiedLogPropConf.PropertyType ) );
                                    Assert.That( logPropConf.DoLogErrors, Is.EqualTo( copiedLogPropConf.DoLogErrors ) );
                                    Assert.That( logPropConf.LogFilter, Is.EqualTo( copiedLogPropConf.LogFilter ) );
                                }
                            }
                            Assert.IsTrue( propertyNameStep );
                            propertyNameStep = false;
                        }

                        //Method step
                        Assert.That( logServConf.Methods.Count, Is.EqualTo( copiedLogServConf.Methods.Count ) );
                        foreach( ILogMethodConfig logMethdConf in logServConf.Methods )
                        {
                            foreach( ILogMethodConfig copiedLogMethdConf in copiedLogServConf.Methods )
                            {
                                if( logMethdConf.GetSimpleSignature() == copiedLogMethdConf.GetSimpleSignature() )
                                {
                                    Assert.IsFalse( methodNameStep );
                                    methodNameStep = true;

                                    Assert.That( logMethdConf.DoLog, Is.EqualTo( copiedLogMethdConf.DoLog ) );
                                    Assert.That( logMethdConf.LogOptions, Is.EqualTo( copiedLogMethdConf.LogOptions ) );
                                    Assert.That( logMethdConf.ReturnType, Is.EqualTo( copiedLogMethdConf.ReturnType ) );

                                    Assert.That( logMethdConf.Parameters.Count, Is.EqualTo( copiedLogMethdConf.Parameters.Count ) );
                                    foreach( ILogParameterInfo parameter in logMethdConf.Parameters )
                                    {
                                        foreach( ILogParameterInfo copiedParameter in copiedLogMethdConf.Parameters )
                                        {
                                            if( parameter.ParameterName == copiedParameter.ParameterName )
                                            {
                                                Assert.IsFalse( methodPropertyNameStep );
                                                methodPropertyNameStep = true;
                                                Assert.That( parameter.ParameterType, Is.EqualTo( copiedParameter.ParameterType ) );
                                            }
                                        }
                                        Assert.IsTrue( methodPropertyNameStep );
                                        methodPropertyNameStep = false;
                                    }
                                }
                            }
                            Assert.IsTrue( methodNameStep );
                            methodNameStep = false;
                        }
                    }
                }
                Assert.IsTrue( serviceNameStep );
                serviceNameStep = false;
            }
        }

        [Test]
        public void InitializeOneServiceTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );

            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();

            Assert.That( c.Services.Count, Is.EqualTo( 1 ) );

            LogConfigIsDefaultTest( c );

        }
        [Test]
        public void InitializeTwoServicesTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();

            Assert.That( c.Services.Count, Is.EqualTo( 2 ) );

            LogConfigIsDefaultTest( c );
        }

        [Test]
        public void ChangesTest()
        {
            ILogService logService = null;
            CleanupTestDir();
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.FillFromDiscoverer( c.Services );

            ChangeLogConfiguration( c );
            ChangeLogConfigurationTest( c, false );
        }
        [Test]
        public void IsDirtyTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" ); //will be set to full log
            CopyPluginToTestDir( "Plugin02.dll" ); //will be set to half log
            CopyPluginToTestDir( "Plugin03.dll" ); //will be set to no log

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );

            VMLogConfig c = new VMLogConfig( vmiCtx, logService );

            //After Initialization
            c.Initialize();
            Assert.That( c.IsDirty, Is.False );
            foreach( VMLogServiceConfig s in c.Services )
            {
                Assert.That( s.IsDirty, Is.False );
            }

            //After an update that tracks changes (load from XML conf for example)
            c.UpdateFrom( CreateChangedLogConfiguration( context ), true );
            Assert.That( c.IsDirty, Is.True );
            foreach( VMLogServiceConfig s in c.Services )
            {
                if( s.Name == serviceA || s.Name == serviceB || s.Name == "DUMMY" )
                    Assert.That( s.IsDirty, Is.True );
                else
                    Assert.That( s.IsDirty, Is.False );
            }

            //After cancelling modifications on a service
            VMCommand<VMLogServiceConfig> cancelCommand = (VMCommand<VMLogServiceConfig>)(c.Find( serviceA )).CancelCommand;
            cancelCommand.Execute( null );

            Assert.That( c.IsDirty, Is.True );
            foreach( VMLogServiceConfig s in c.Services )
            {
                if( s.Name == serviceB || s.Name == "DUMMY" )
                    Assert.That( s.IsDirty, Is.True );
                else
                    Assert.That( s.IsDirty, Is.False );
            }

            //After cancelling modifications (back to the "after Initialize" state)
            c.CancelAllModifications();
            Assert.That( c.IsDirty, Is.False );
            foreach( VMLogServiceConfig s in c.Services )
            {
                Assert.That( s.IsDirty, Is.False );
            }
        }
        [Test]
        public void InitializeTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
            CopyPluginToTestDir( "Plugin03.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();

            LogConfigIsDefaultTest( c );
        }
        [Test]
        public void Changes_ClearAll_Test()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
            CopyPluginToTestDir( "Plugin03.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();
            ChangeLogConfiguration( c );
            ChangeLogConfigurationTest( c, false );
            c.ClearConfig();
            LogConfigIsClearTest( c );
        }
        [Test]
        public void Changes_Apply_Test()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
            CopyPluginToTestDir( "Plugin03.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();
            ChangeLogConfiguration( c );
            ChangeLogConfigurationTest( c, false );
            c.Apply();
            ChangeLogConfigurationTest( c, true );
        }

        [Test]
        public void Changes_Apply_ClearAll_CancelAll_Test()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
            CopyPluginToTestDir( "Plugin03.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();
            ChangeLogConfiguration( c );
            ChangeLogConfigurationTest( c, false );
            c.Apply();
            ChangeLogConfigurationTest( c, true );
            c.ClearConfig();
            LogConfigIsClearTest( c );
            c.CancelAllModifications();
            ChangeLogConfigurationTest( c, true );
        }

        [Test]
        public void Changes_ApplyService_CancelService_Test()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );
            CopyPluginToTestDir( "Plugin03.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig c = new VMLogConfig( vmiCtx, logService );
            c.Initialize();
            ChangeLogConfiguration( c );
            ChangeLogConfigurationTest( c, false );

            VMLogServiceConfig vmS = c.Find( serviceA );
            VMCommand<VMLogServiceConfig> command = (VMCommand<VMLogServiceConfig>)(vmS.ApplyCommand);
            command.Execute( vmS );

            vmS = c.Find( "DUMMY" );
            command = (VMCommand<VMLogServiceConfig>)(vmS.ApplyCommand);
            command.Execute( vmS );

            #region ServiceA full log Test

            // Test that LogFilter changes in a LogEvent have been taken account of
            VMLogEventConfig vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_1" );

            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );

            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_2" );
            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            VMLogMethodConfig vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_1 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_2 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            #region Prop

            // Test that LogFilter changes in a LogProperty have been taken account of
            VMLogPropertyConfig vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            #endregion

            #endregion
            #region ServiceB half logging Test 

            // Test that LogFilter changes in a LogEvent have been taken account of
            vmLogEvConf = c.Find( serviceB ).FindEvent( "Event2_1" );
            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmLogEvConf = c.Find( serviceB ).FindEvent( "Event2_2" );
            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( ServiceLogEventOptions.LogErrors ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            vmLogMethConf = c.Find( serviceB ).FindMethod( sBm2_1 );

            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)7 ) );

            vmLogMethConf = c.Find( serviceB ).FindMethod( sBm2_2 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( ServiceLogMethodOptions.LogError ) );

            #region Prop

            // Test that LogFilter changes in a LogProperty have been taken account of
            vmLogPropConf = c.Find( serviceB ).FindProperty( "Property2_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceB ).FindProperty( "Property2_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.False );
            Assert.That( vmLogPropConf.DoLogGet, Is.False );
            Assert.That( vmLogPropConf.DoLogSet, Is.False );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.False );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)0 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.False );

            #endregion

            #endregion
            #region DummyService full log Test

            // Test Find method and Creation of unbound ViewModels
            VMLogServiceConfig dummyLogSC = c.Find( "DUMMY" );
            Assert.That( dummyLogSC.IsBound, Is.False );

            VMLogEventConfig vmE = dummyLogSC.FindEvent( "e1" );
            Assert.That( vmE.Name, Is.EqualTo( "e1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t1" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p2" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t2" ) );

            Assert.That( vmE.IsBound, Is.False );

            Assert.That( vmE.DoLog, Is.True );
            Assert.That( vmE.DoLogErrors, Is.True );
            Assert.That( vmE.DoLogStartRaise, Is.True );
            Assert.That( vmE.DoLogParameters, Is.True );
            Assert.That( vmE.DoLogEndRaise, Is.True );
            Assert.That( vmE.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoCatchBadEventHandling, Is.True );

            Assert.That( vmE.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            VMLogMethodConfig vmM = dummyLogSC.FindMethod( "Void m1(t3,t4)" );
            Assert.That( vmM != null );
            Assert.That( vmM.Name, Is.EqualTo( "m1" ) );
            Assert.That( vmM.ReturnType, Is.EqualTo( "Void" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p3" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t3" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p4" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t4" ) );
            Assert.That( vmM.IsBound, Is.False );
            Assert.That( vmM.DoLog, Is.True );
            Assert.That( vmM.DoLogErrors, Is.True );

            Assert.That( vmM.DoLogEnter, Is.True );
            Assert.That( vmM.DoLogParameters, Is.True );
            Assert.That( vmM.DoLogCaller, Is.True );
            Assert.That( vmM.DoLogLeave, Is.True );
            Assert.That( vmM.DoLogReturnValue, Is.True );

            VMLogPropertyConfig vmP = dummyLogSC.FindProperty( "n1" );
            Assert.That( vmP.Name, Is.EqualTo( "n1" ) );
            Assert.That( vmP.PropertyType, Is.EqualTo( "t1" ) );
            Assert.That( vmP.DoLogCaller, Is.True );
            Assert.That( vmP.DoLogGet, Is.True );
            Assert.That( vmP.DoLogSet, Is.True );
            Assert.That( vmP.IsBound, Is.False );
            Assert.That( vmP.DoLog, Is.True );
            Assert.That( vmP.DoLogErrors, Is.True );

            #endregion

            Assert.That( c.Find( serviceA ).IsDirty, Is.False );
            Assert.That( c.Find( serviceB ).IsDirty, Is.True );
            Assert.That( c.Find( "DUMMY" ).IsDirty, Is.False );

            vmS = c.Find( serviceA );
            command = (VMCommand<VMLogServiceConfig>)(vmS.CancelCommand);
            command.Execute( vmS );

            vmS = c.Find( "DUMMY" );
            command = (VMCommand<VMLogServiceConfig>)(vmS.CancelCommand);
            command.Execute( vmS );

            #region ServiceA full log Test

            // Test that LogFilter changes in a LogEvent have been taken account of
            vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_1" );

            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );

            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_2" );
            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_1 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_2 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            #region Prop

            // Test that LogFilter changes in a LogProperty have been taken account of
            vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            #endregion

            #endregion
            #region ServiceB full log Test

            // Test that LogFilter changes in a LogEvent have been taken account of
            vmLogEvConf = c.Find( serviceB ).FindEvent( "Event2_1" );
            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            vmLogMethConf = c.Find( serviceB ).FindMethod( sBm2_1 );

            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)7 ) );

            #region Prop

            // Test that LogFilter changes in a LogProperty have been taken account of
            vmLogPropConf = c.Find( serviceB ).FindProperty( "Property2_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceB ).FindProperty( "Property2_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.False );
            Assert.That( vmLogPropConf.DoLogGet, Is.False );
            Assert.That( vmLogPropConf.DoLogSet, Is.False );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.False );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)0 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.False );

            #endregion

            #endregion
            #region DummyService full log Test

            // Test Find method and Creation of unbound ViewModels
            dummyLogSC = c.Find( "DUMMY" );
            Assert.That( dummyLogSC.IsBound, Is.False );

            vmE = dummyLogSC.FindEvent( "e1" );
            Assert.That( vmE.Name, Is.EqualTo( "e1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t1" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p2" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t2" ) );

            Assert.That( vmE.IsBound, Is.False );

            Assert.That( vmE.DoLog, Is.True );
            Assert.That( vmE.DoLogErrors, Is.True );
            Assert.That( vmE.DoLogStartRaise, Is.True );
            Assert.That( vmE.DoLogParameters, Is.True );
            Assert.That( vmE.DoLogEndRaise, Is.True );
            Assert.That( vmE.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoCatchBadEventHandling, Is.True );

            Assert.That( vmE.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmM = dummyLogSC.FindMethod( "Void m1(t3,t4)" );
            Assert.That( vmM != null );
            Assert.That( vmM.Name, Is.EqualTo( "m1" ) );
            Assert.That( vmM.ReturnType, Is.EqualTo( "Void" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p3" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t3" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p4" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t4" ) );
            Assert.That( vmM.IsBound, Is.False );
            Assert.That( vmM.DoLog, Is.True );
            Assert.That( vmM.DoLogErrors, Is.True );

            Assert.That( vmM.DoLogEnter, Is.True );
            Assert.That( vmM.DoLogParameters, Is.True );
            Assert.That( vmM.DoLogCaller, Is.True );
            Assert.That( vmM.DoLogLeave, Is.True );
            Assert.That( vmM.DoLogReturnValue, Is.True );

            vmP = dummyLogSC.FindProperty( "n1" );
            Assert.That( vmP.Name, Is.EqualTo( "n1" ) );
            Assert.That( vmP.PropertyType, Is.EqualTo( "t1" ) );
            Assert.That( vmP.DoLogCaller, Is.True );
            Assert.That( vmP.DoLogGet, Is.True );
            Assert.That( vmP.DoLogSet, Is.True );
            Assert.That( vmP.IsBound, Is.False );
            Assert.That( vmP.DoLog, Is.True );
            Assert.That( vmP.DoLogErrors, Is.True );

            #endregion

            Assert.That( c.Find( serviceA ).IsDirty, Is.False );
            Assert.That( c.Find( serviceB ).IsDirty, Is.True );
            Assert.That( c.Find( "DUMMY" ).IsDirty, Is.False );

            vmS = c.Find( serviceB );
            command = (VMCommand<VMLogServiceConfig>)vmS.CancelCommand;
            command.Execute( vmS );

            #region ServiceA full log Test

            // Test that LogFilter changes in a LogEvent have been taken account of
            vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_1" );

            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );

            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmLogEvConf = c.Find( serviceA ).FindEvent( "Event1_2" );
            Assert.That( vmLogEvConf.IsBound, Is.True );
            Assert.That( vmLogEvConf.DoLog, Is.True );
            Assert.That( vmLogEvConf.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            // Test that LogFilter changes in a LogMethod have been taken account of
            vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_1 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            vmLogMethConf = c.Find( serviceA ).FindMethod( sAm1_2 );
            Assert.That( vmLogMethConf.IsBound, Is.True );
            Assert.That( vmLogMethConf.DoLog, Is.True );
            Assert.That( vmLogMethConf.LogOptions, Is.EqualTo( (ServiceLogMethodOptions)63 ) );

            #region Prop

            // Test that LogFilter changes in a LogProperty have been taken account of
            vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_1" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            vmLogPropConf = c.Find( serviceA ).FindProperty( "Property1_2" );
            Assert.That( vmLogPropConf.DoLogCaller, Is.True );
            Assert.That( vmLogPropConf.DoLogGet, Is.True );
            Assert.That( vmLogPropConf.DoLogSet, Is.True );
            Assert.That( vmLogPropConf.IsBound, Is.True );
            Assert.That( vmLogPropConf.DoLog, Is.True );
            Assert.That( vmLogPropConf.LogFilter, Is.EqualTo( (LogPropertyFilter)7 ) );
            Assert.That( vmLogPropConf.DoLogErrors, Is.True );

            #endregion

            #endregion
            LogServiceConfigIsDefaultTest( vmS );
            #region DummyService full log Test

            // Test Find method and Creation of unbound ViewModels
            dummyLogSC = c.Find( "DUMMY" );
            Assert.That( dummyLogSC.IsBound, Is.False );

            vmE = dummyLogSC.FindEvent( "e1" );
            Assert.That( vmE.Name, Is.EqualTo( "e1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p1" ) );
            Assert.That( vmE.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t1" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p2" ) );
            Assert.That( vmE.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t2" ) );

            Assert.That( vmE.IsBound, Is.False );

            Assert.That( vmE.DoLog, Is.True );
            Assert.That( vmE.DoLogErrors, Is.True );
            Assert.That( vmE.DoLogStartRaise, Is.True );
            Assert.That( vmE.DoLogParameters, Is.True );
            Assert.That( vmE.DoLogEndRaise, Is.True );
            Assert.That( vmE.DoCatchEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoLogCaughtEventWhenServiceStopped, Is.True );
            Assert.That( vmE.DoCatchBadEventHandling, Is.True );

            Assert.That( vmE.LogOptions, Is.EqualTo( (ServiceLogEventOptions)127 ) );

            vmM = dummyLogSC.FindMethod( "Void m1(t3,t4)" );
            Assert.That( vmM != null );
            Assert.That( vmM.Name, Is.EqualTo( "m1" ) );
            Assert.That( vmM.ReturnType, Is.EqualTo( "Void" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterName, Is.EqualTo( "p3" ) );
            Assert.That( vmM.Parameters.ElementAt( 0 ).ParameterType, Is.EqualTo( "t3" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterName, Is.EqualTo( "p4" ) );
            Assert.That( vmM.Parameters.ElementAt( 1 ).ParameterType, Is.EqualTo( "t4" ) );
            Assert.That( vmM.IsBound, Is.False );
            Assert.That( vmM.DoLog, Is.True );
            Assert.That( vmM.DoLogErrors, Is.True );

            Assert.That( vmM.DoLogEnter, Is.True );
            Assert.That( vmM.DoLogParameters, Is.True );
            Assert.That( vmM.DoLogCaller, Is.True );
            Assert.That( vmM.DoLogLeave, Is.True );
            Assert.That( vmM.DoLogReturnValue, Is.True );

            vmP = dummyLogSC.FindProperty( "n1" );
            Assert.That( vmP.Name, Is.EqualTo( "n1" ) );
            Assert.That( vmP.PropertyType, Is.EqualTo( "t1" ) );
            Assert.That( vmP.DoLogCaller, Is.True );
            Assert.That( vmP.DoLogGet, Is.True );
            Assert.That( vmP.DoLogSet, Is.True );
            Assert.That( vmP.IsBound, Is.False );
            Assert.That( vmP.DoLog, Is.True );
            Assert.That( vmP.DoLogErrors, Is.True );

            #endregion

            Assert.That( c.Find( serviceA ).IsDirty, Is.False );
            Assert.That( c.Find( serviceB ).IsDirty, Is.False );
            Assert.That( c.Find( "DUMMY" ).IsDirty, Is.False );
        }
        [Test]
        public void Changes_Cancel_UnBoundServiceConfigTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig vmLogConf = new VMLogConfig( vmiCtx, logService );
            vmLogConf.Initialize();
            ChangeLogConfiguration( vmLogConf );

            //Cancel modifications on the DUMMY Service, interceptor doesn't have this service in its conf, so DUMMY should not be findable in the LogConf
            vmLogConf.CancelModifications( vmLogConf.Find( "DUMMY" ) );
            Assert.That( vmLogConf.Find( "DUMMY" ), Is.Null );
        }
        [Test]
        public void Update_Apply_Delete_CancelUnboundServiceConfigTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig vmLogConf = new VMLogConfig( vmiCtx, logService );
            vmLogConf.Initialize();
            // This stub contains an unbound service named "DUMMY"
            ChangeLogConfiguration( vmLogConf );
            // Applying these modifications, DUMMY is now in the interceptor's conf
            vmLogConf.Apply();

            //Deleting DUMMY Service from the LogConfig
            VMCommand<VMLogServiceConfig> deleteCommand = (VMCommand<VMLogServiceConfig>)(vmLogConf.Find( "DUMMY" )).DeleteCommand;
            deleteCommand.Execute( null );
            Assert.That( vmLogConf.Find( "DUMMY" ), Is.Null );

            // Cancelling modifications, DUMMY should be back on the LogConfig
            vmLogConf.CancelAllModifications();
            Assert.That( vmLogConf.Find( "DUMMY" ), Is.Not.Null );
        }
        [Test]
        public void Update_Delete_UnBoundServiceConfigTest()
        {
            ILogService logService = null;
            CopyPluginToTestDir( "Plugin01.dll" );
            CopyPluginToTestDir( "Plugin02.dll" );

            IContext context = Context.CreateInstance();
            ISimplePluginRunner pluginRunner = context.PluginRunner;

            pluginRunner.Discoverer.Discover( new DirectoryInfo( TestFolderDir.ToString() ), true );
            VMIContextViewModel vmiCtx = new VMIContextViewModel( context, new MockPluginConfigAccessor( idEdited, user, system, context, dic ), logService );
            VMLogConfig vmLogConf = new VMLogConfig( vmiCtx, logService );
            vmLogConf.Initialize();
            ChangeLogConfiguration( vmLogConf );

            // Cancel modifications on the DUMMY Service, interceptor doesn't have this service in its conf, so DUMMY should not be findable in the LogConf

            VMCommand<VMLogServiceConfig> deleteCommand = (VMCommand<VMLogServiceConfig>)(vmLogConf.Find( "DUMMY" )).DeleteCommand;
            deleteCommand.Execute( null );
            Assert.That( vmLogConf.Find( "DUMMY" ), Is.Null );
        }
    }
}
