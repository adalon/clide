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

namespace Clide.Sdk.Solution
{
    using Clide.Events;
    using Clide.Patterns.Adapter;
    using Clide.Properties;
    using Clide.Solution;
    using Clide.Solution.Implementation;
    using Clide.VisualStudio;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Default implementation of the root solution node.
    /// </summary>
    public class SolutionNode : SolutionTreeNode, ISolutionNode
    {
        private ISolutionEvents events;
        private ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory;
        private ISolutionExplorerNodeFactory explorerNodeFactory;
        private Lazy<IVsMonitorSelection> selection;
        private IUIThread uiThread;

        // Optimize code path when the active project hierarchy didn't change.
        private IProjectNode lastActiveProject;
        private IVsHierarchy lastActiveHierarchy;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="childNodeFactory">The factory for child nodes.</param>
        /// <param name="looseNodeFactory">The explorer node factory used to create "loose" nodes from solution explorer.</param>
        /// <param name="locator">The service locator.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        /// <param name="solutionEvents">The solution events.</param>
        /// <param name="uiThread">The UI thread.</param>
        public SolutionNode(
            IVsSolutionHierarchyNode hierarchyNode,
            // This is the regular node factory for trees, that receives a lazy 
            // pointer to the parent tree node.
            ITreeNodeFactory<IVsSolutionHierarchyNode> childNodeFactory,
            // This factory is used to create "loose" nodes from solution explorer
            ISolutionExplorerNodeFactory looseNodeFactory,
            IServiceLocator locator,
            IAdapterService adapter,
            ISolutionEvents solutionEvents, 
            // Retrieving current selection must be done on the UI thread.
            IUIThread uiThread)
            : base(SolutionNodeKind.Solution, hierarchyNode, null, childNodeFactory, adapter)
        {
            this.Solution = new Lazy<EnvDTE.Solution>(() => hierarchyNode.ServiceProvider.GetService<EnvDTE.DTE>().Solution);
            this.nodeFactory = childNodeFactory;
            this.explorerNodeFactory = looseNodeFactory;
            this.events = solutionEvents;
            this.selection = new Lazy<IVsMonitorSelection>(() => locator.GetService<SVsShellMonitorSelection, IVsMonitorSelection>());
            this.uiThread = uiThread;
        }

        /// <summary>
        /// Gets the currently active project (if single), which can be the selected project, or
        /// the project owning the currently selected item or opened designer file.
        /// </summary>
        /// <remarks>
        /// If there are multiple active projects, this property will be null. This can happen
        /// when multiple selection is enabled for items across more than one project
        /// </remarks>
        public virtual IProjectNode ActiveProject
        {
            get
            {
                var selected = selection.Value.GetSelectedHierarchy(uiThread);
                if (selected == null)
                    return null;
                if (selected == lastActiveHierarchy)
                    return lastActiveProject;

                lastActiveHierarchy = selected;
                lastActiveProject = explorerNodeFactory.Create(new VsSolutionHierarchyNode(selected, VSConstants.VSITEMID_ROOT)) as IProjectNode;
                return lastActiveProject;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a solution is open.
        /// </summary>
        public virtual bool IsOpen
        {
            get { return Solution.Value.IsOpen; }
        }

        /// <summary>
        /// Gets the currently selected nodes in the solution.
        /// </summary>
        public virtual IEnumerable<ISolutionExplorerNode> SelectedNodes
        {
            get
            {
                return this.selection.Value
                    .GetSelection(uiThread, HierarchyNode.VsHierarchy)
                    .Select(sel => explorerNodeFactory.Create(new VsSolutionHierarchyNode(sel.Item1, sel.Item2)));
            }
        }

        /// <summary>
        /// Opens the specified solution file.
        /// </summary>
        public virtual void Open(string solutionFile)
        {
            Guard.NotNullOrEmpty(() => solutionFile, solutionFile);

            this.Solution.Value.Open(solutionFile);
        }

        /// <summary>
        /// Creates a new blank solution with the specified solution file location.
        /// </summary>
        public virtual void Create(string solutionFile)
        {
            Guard.NotNullOrEmpty(() => solutionFile, solutionFile);

            Guard.IsValid(
                () => solutionFile, solutionFile,
                s => Path.IsPathRooted(s),
                Strings.SolutionNode.InvalidSolutionFile);

            ((EnvDTE80.Solution2)this.Solution.Value).Create(Path.GetDirectoryName(solutionFile), Path.GetFileNameWithoutExtension(solutionFile));
            this.Save();
        }

        /// <summary>
        /// Closes the solution.
        /// </summary>
        /// <param name="saveFirst">If set to <c>true</c> saves the solution before closing.</param>
        public virtual void Close(bool saveFirst = true)
        {
            if (saveFirst)
                Save();

            this.Solution.Value.Close(saveFirst);
        }

        /// <summary>
        /// Saves the current solution.
        /// </summary>
        public virtual void Save()
        {
            ErrorHandler.ThrowOnFailure(this
                .HierarchyNode
                .ServiceProvider
                .GetService<SVsSolution, IVsSolution>()
                .SaveSolutionElement(
                    (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave,
                    null,
                    0));
        }

        /// <summary>
        /// Saves the current solution to the specified target file.
        /// </summary>
        /// <param name="solutionFile"></param>
        public virtual void SaveAs(string solutionFile)
        {
            this.Solution.Value.SaveAs(solutionFile);
            this.Save();
        }

        /// <summary>
        /// Creates a solution folder under the solution root.
        /// </summary>
        public virtual ISolutionFolderNode CreateSolutionFolder(string name)
        {
            Guard.NotNullOrEmpty(() => name, name);

            ((EnvDTE80.Solution2)this.Solution.Value).AddSolutionFolder(name);

            var solutionfolder =
                this.HierarchyNode.Children.Single(child => child.VsHierarchy.Properties(child.ItemId).DisplayName == name);

            return this.CreateNode(solutionfolder) as ISolutionFolderNode;
        }

        /// <summary>
        /// Accepts the specified visitor for traversal.
        /// </summary>
        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>
		/// The casted value or null if it cannot be converted to that type.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override T As<T>()
		{
			return this.Adapter.Adapt(this).As<T>();
		}

        event EventHandler ISolutionEvents.SolutionOpened
        {
            add { this.events.SolutionOpened += value; }
            remove { this.events.SolutionOpened -= value; }
        }

        event EventHandler ISolutionEvents.SolutionClosing
        {
            add { this.events.SolutionClosing += value; }
            remove { this.events.SolutionClosing -= value; }
        }

        event EventHandler ISolutionEvents.SolutionClosed
        {
            add { this.events.SolutionClosed += value; }
            remove { this.events.SolutionClosed -= value; }
        }

        event EventHandler<ProjectEventArgs> ISolutionEvents.ProjectOpened
        {
            add { this.events.ProjectOpened += value; }
            remove { this.events.ProjectOpened -= value; }
        }

        event EventHandler<ProjectEventArgs> ISolutionEvents.ProjectClosing
        {
            add { this.events.ProjectClosing += value; }
            remove { this.events.ProjectClosing -= value; }
        }

        /// <summary>
        /// Gets the solution represented by this node.
        /// </summary>
        internal Lazy<EnvDTE.Solution> Solution { get; private set; }
    }
}