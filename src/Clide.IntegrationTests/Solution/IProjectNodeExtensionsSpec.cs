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

namespace Clide.Solution
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;

    [TestClass]
    public class IProjectNodeExtensionsSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionIsOpened_ThenCanGetProjectOutputAssembly()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var lib = new ITreeNode[] {explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            Assert.NotNull(lib);

            var asm = lib.GetOutputAssembly();

            Assert.NotNull(asm, "Failed to retrieve output assembly from class library project.");
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionIsOpened_ThenCanGetProjectReferencedAssemblies()
        {
            var slnFile = base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var proj = XDocument.Load(GetFullPath(Path.GetDirectoryName(slnFile), "ClassLibrary\\ClassLibrary.csproj"));
            var refs = proj.Descendants(XName.Get("{http://schemas.microsoft.com/developer/msbuild/2003}Reference"))
                .Select(e => e.Attribute("Include").Value)
                .ToList();

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var lib = new ITreeNode[] { explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            Assert.NotNull(lib);

            var asm = lib.GetReferencedAssemblies().ToList();

            //asm.ForEach(a => Console.WriteLine(a.FullName));

            // 7 actual references + mscorlib which is always added automatically.
            Assert.Equal(1 + refs.Count, asm.Count);
            Assert.True(refs.All(r => asm.Any(a => a.GetName().Name == r)));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingOutputAssemblyButNoBuild_ThenCanOptOutOfBuild()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = explorer.Solution.Traverse().OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");
			var outDir = Path.Combine(lib.Properties.MSBuildProjectDirectory, 
				lib.Adapt().AsMsBuildProject().AllEvaluatedProperties
				.Where(p => p.Name == "IntermediateOutputPath")
				.Select(p => lib.Adapt().AsMsBuildProject().ExpandString(p.UnevaluatedValue))
				.First());

			if (Directory.Exists(outDir))
				Directory.Delete(outDir, true);

			var asm = lib.GetOutputAssembly(false).Result;

			Assert.Null(asm);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingOutputAssembly_ThenCanInspectAssemblyUsingReflection()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = explorer.Solution.Traverse().OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");
			
			var asm = lib.GetOutputAssembly().Result;

            var type = asm.GetTypes().First(t => t.GetConstructor(new Type[0]) != null);

            Assert.Throws<ArgumentException>(() => Activator.CreateInstance(type));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingOutputAssembly_ThenTryingToInstantiateTypeThrows()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = explorer.Solution.Traverse().OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");
            var asm = lib.GetOutputAssembly().Result;

            var type = asm.GetTypes().First(t => t.GetConstructor(new Type[0]) != null);

            Assert.Throws<ArgumentException>(() => Activator.CreateInstance(type));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenBuildingProject_ThenCanWaitForSuccess()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = explorer.Solution.FindProject(project => project.DisplayName == "ClassLibrary");

            Assert.True(lib.Build().Result);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenBuildWaitIsCancelled_ThenResultThrowsTaskCancelled()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = explorer.Solution.FindProject(project => project.DisplayName == "ClassLibrary");

			var cancellation = new CancellationTokenSource();
			var task = lib.Build(cancellation.Token);
			cancellation.Cancel();

			Assert.Throws<AggregateException> (() => { Console.WriteLine (task.Result); });
        }
	}
}