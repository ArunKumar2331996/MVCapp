﻿using System;
using System.Linq;
using DataLayer.DataClasses.Concrete;
using GenericServices;
using GenericServices.Actions;
using GenericServices.Logger;
using log4net.Appender;
using log4net.Config;
using NUnit.Framework;
using SampleWebApp.ActionProgress;
using Tests.Helpers;
using Tests.MocksAndFakes;

namespace Tests.UnitTests.Group08ActionRunner
{
    class Test05HubRunnerBasic
    {

        private MemoryAppender _log4NetMemoryLog;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            _log4NetMemoryLog = new MemoryAppender();
            BasicConfigurator.Configure(_log4NetMemoryLog);
            GenericLoggerFactory.SetLoggerMethod = name => new Log4NetGenericLogger(name);
        }

        [SetUp]
        public void Setup()
        {
            ActionHub.LifeTimeScopeProvider = () => new MockHubDependencyResolver();
            _log4NetMemoryLog.Clear();
        }

        [Test]
        public void Test01CheckHubRunnerCtorOk()
        {
            //SETUP

            //ATTEMPT
            var hr = new HubRunner<Tag>("aaa", typeof(IEmptyTestAction), new Tag());

            //VERIFY
            hr.ShouldNotEqualNull(); 
        }

        [Test]
        public void Test02CheckHubRunnerCtorBad()
        {
            //SETUP
            ActionHub.LifeTimeScopeProvider = null;

            //ATTEMPT
            var ex = Assert.Throws<NullReferenceException>( () => new HubRunner<Tag>("aaa", typeof(IEmptyTestAction), new Tag()));

            //VERIFY
            ex.Message.ShouldStartWith("You must set up the static varable HubRunner.LifeTimeScopeProvider");
        }

        //----------------------------------------------------------

        [Test]
        public void Test05CheckRunActionSynchronouslyOk()
        {
            //SETUP
            var hr = new HubRunner<Tag>("aaa", typeof(IEmptyTestAction), new Tag());
            var mockHub = new MockActionHubSend();

            //ATTEMPT
            var lastMessage = hr.RunActionSynchronously("aaa", "123", mockHub);

            //VERIFY
            lastMessage.MessageType.ShouldEqual(ProgressMessageTypes.Finished);
            var logs = _log4NetMemoryLog.GetEvents();
            Assert.GreaterOrEqual(logs.Length, 2);
            logs[0].Level.ShouldEqual( log4net.Core.Level.Info );
            logs[1].Level.ShouldEqual(log4net.Core.Level.Info);
        }

        [Test]
        public void Test06CheckRunActionSynchronouslyFail()
        {
            //SETUP
            var hr = new HubRunner<Tag>("aaa", typeof(IEmptyTestAction), new Tag{ TagId = 2});      //TagId = 2 means force fail
            var mockHub = new MockActionHubSend();

            //ATTEMPT
            var lastMessage = hr.RunActionSynchronously("aaa", "123", mockHub);

            //VERIFY
            lastMessage.MessageType.ShouldEqual(ProgressMessageTypes.Failed);
            var logs = _log4NetMemoryLog.GetEvents();
            Assert.GreaterOrEqual(logs.Length, 2);
            logs[0].Level.ShouldEqual(log4net.Core.Level.Info);
            logs[1].Level.ShouldEqual(log4net.Core.Level.Warn);
        }
    }
}