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
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

    [TestClass]
    public class ProjectNodeSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionIsOpened_ThenCanGetProjectConfigurations()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var lib = new ITreeNode[] {explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            Assert.NotNull(lib);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenComparingNodes_ThenCheckForEquality()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var lib1 = new ITreeNode[] {explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            var lib2 = new ITreeNode[] {explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

			Assert.Equal(lib1, lib2);
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

            // 7 actual references + mscorlib which is always added automatically.
            Assert.Equal(1 + refs.Count, asm.Count);
            Assert.True(refs.All(r => asm.Any(a => a.GetName().Name == r)));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingConfigurations_ThenRetrievesDebugAndRelease()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = new ITreeNode[] { explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            var configs = lib.Configuration.Configurations.ToList();

            Assert.True(configs.Contains("Debug"));
            Assert.True(configs.Contains("Release"));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingPlatforms_ThenRetrievesAnyCPU()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = new ITreeNode[] { explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            var platforms = lib.Configuration.Platforms.ToList();

            Assert.Equal(1, platforms.Count);
            Assert.True(platforms.Contains("AnyCPU"));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenGettingActiveConfigurationName_ThenRetrievesDebugAnyCPU()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var lib = new ITreeNode[] { explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            Assert.Equal("Debug|AnyCPU", lib.Configuration.ActiveConfigurationName);
        }
    }
}