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

namespace Clide
{
    using Clide.Composition;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio.ComponentModelHost;
    using System;

    /// <summary>
    /// Locates global services and components inside Visual Studio, in a thread-safe way, 
    /// including Clide own components and VS MEF-exported values in a unified way.
    /// </summary>
    /// <remarks>
    /// This locator allows retrieval of everything exposed in VS including Clide, such 
    /// as <code>ServiceLocator.GlobalLocator.GetInstance&lt;DTE&gt;()</code>, 
    /// <code>ServiceLocator.GlobalLocator.GetInstance&lt;IVsShell&gt;()</code>, 
    /// <code>ServiceLocator.GlobalLocator.GetInstance&lt;IDevEnv&gt;()</code>, etc.
    /// </remarks>
    public static class ServiceLocator
    {
        private static readonly Lazy<IServiceLocator> globalLocator = new Lazy<IServiceLocator>(() =>
            new FallbackServiceLocator(
                DevEnv.Get(GlobalServiceProvider.Instance).ServiceLocator,
				new FallbackServiceLocator(
					new ExportsServiceLocator(GlobalServiceProvider.Instance.GetService<SComponentModel, IComponentModel>().DefaultExportProvider),
					new ServiceProviderLocator(GlobalServiceProvider.Instance))));

        /// <summary>
        /// Gets the global locator.
        /// </summary>
        public static IServiceLocator GlobalLocator { get { return globalLocator.Value; } }
    }
}