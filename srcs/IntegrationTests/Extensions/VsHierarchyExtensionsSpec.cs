﻿#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Extensions
{
    using Clide.Solution;
    using Clide.Solution.Implementation;
    using Clide.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

    [TestClass]
	public class VsHierarchyExtensionsSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestInitialize]
		public override void TestInitialize()
		{
			base.TestInitialize();

			base.OpenSolution(GetFullPath(TestContext.TestDeploymentDir, "SampleSolution\\SampleSolution.sln"));
		}

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenGettingIdFromExtension_ThenAlwaysReturnsSameValue()
		{
			var solution = ServiceProvider.GetService<IVsSolution>();
			var hierarchy = solution as IVsHierarchy;

			var solutionFolder1 = new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT).Children.FirstOrDefault(n => 
                n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder1");

			Assert.Equal(solutionFolder1.ItemId, solutionFolder1.VsHierarchy.Properties(solutionFolder1.ItemId).ItemId);

			var solutionFolder2 = solutionFolder1.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder2");
			var project = solutionFolder2.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "ClassLibrary");

			Assert.Equal(solutionFolder2.ItemId, solutionFolder2.VsHierarchy.Properties(solutionFolder2.ItemId).ItemId);
			Assert.Equal(project.ItemId, project.VsHierarchy.Properties(project.ItemId).ItemId);
		}
	}
}
