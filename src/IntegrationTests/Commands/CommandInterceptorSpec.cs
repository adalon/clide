﻿#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Commands
{
    using IntegrationPackage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Clide.Solution;
    using System.Threading;

    [TestClass]
    public class CommandInterceptorSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAddingInterceptor_ThenCanInterceptIdeOperation()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var interceptor = new Mock<ICommandInterceptor>();
            var commands = this.ServiceLocator.GetInstance<ICommandManager>();

            commands.AddInterceptor(interceptor.Object, new CommandInterceptorAttribute(
                Constants.PackageGuid, "{5efc7975-14bc-11cf-9b2b-00aa00573819}", 0x372));

            var mre = new ManualResetEventSlim();
            var events = this.Dte.Events.BuildEvents;
            EnvDTE._dispBuildEvents_OnBuildDoneEventHandler done = (scope, action) => mre.Set();
            events.OnBuildDone += done;

            this.Dte.ExecuteCommand("Build.BuildSolution");

            mre.Wait();

            Assert.True(BuildCommandInterceptor.BeforeExecuteCalled);
            Assert.True(BuildCommandInterceptor.AfterExecuteCalled);

            interceptor.Verify(x => x.BeforeExecute());
            interceptor.Verify(x => x.AfterExecute());
        }
    }
}
