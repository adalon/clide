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

namespace UnitTests.Solution.ISolutionNodeExtensionsSpec
{
    using Clide.Solution;
    using System.Linq;
    using Xunit;

    public class given_a_solution
    {
        private ISolutionExplorer explorer;

        public given_a_solution()
        {
            explorer = new FakeSolutionExplorer
            {
                Solution = new FakeSolution
                {
                    Nodes =
                    {
                        new FakeSolutionFolder("Solution Items")
                        {
                            Nodes = 
                            {
                                new FakeSolutionItem("Readme.md"),
                            }
                        },
                        new FakeSolutionFolder("CSharp")
                        {
                            Nodes = 
                            {
                                new FakeProject("CsConsole")
                                {
                                    Nodes = 
                                    {
                                        new FakeItem("Class1.cs"),
                                    }
                                }
                            }
                        },
                        new FakeSolutionFolder("VB")
                        {
                            Nodes = 
                            {
                                new FakeProject("VbConsole")
                                {
                                    Nodes = 
                                    {
                                        new FakeItem("Class1.vb"),
                                    }
                                }
                            }
                        },
                    }
                }
            };

        }

        [Fact]
        public void when_finding_all_projects_then_gets_all()
        {
            var projects = explorer.Solution.FindProjects().ToList();

            Assert.Equal(2, projects.Count);
        }

        [Fact]
        public void when_finding_all_projects_with_filter_then_gets_all_matches()
        {
            var projects = explorer.Solution.FindProjects(p => p.DisplayName.Contains("Console")).ToList();

            Assert.Equal(2, projects.Count);
        }

        [Fact]
        public void when_finding_project_then_can_filter_by_name()
        {
            var project = explorer.Solution.FindProject(p => p.DisplayName == "CsConsole");

            Assert.NotNull(project);
        }
    }
}