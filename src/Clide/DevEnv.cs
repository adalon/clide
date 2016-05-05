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
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Entry point to the Clide developer environment APIs.
    /// </summary>
    public static class DevEnv
    {
        private static Lazy<DevEnvFactory> defaultFactory = new Lazy<DevEnvFactory>(() => new DevEnvFactory());
        //private static AmbientSingleton<Func<IServiceProvider, IDevEnv>> devEnvFactory =
        //    new AmbientSingleton<Func<IServiceProvider, IDevEnv>>(services => defaultFactory.Value.Get(services));

        /// <summary>
        /// Gets the developer environment for the given service provider. 
        /// Make sure you pass your package to get your scoped dev env.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="IDevEnv"/> instance is cached for the 
        /// given service provider, and only created once, using the given 
        /// IDE services as necessary. This default behavior can 
        /// be overriden by setting the <see cref="DevEnvFactory"/>.
        /// </remarks>
        /// <param name="serviceProvider">Typically, this is your package service provider, 
        /// so that all provided ClideComponents are properly resolved and available through 
        /// it. If you pass in the global VS service provider, you will get an instance 
        /// that only contains the core Clide services but does not export any of the 
        /// package Clide components.</param>
        public static IDevEnv Get(IServiceProvider serviceProvider)
        {
            return defaultFactory.Value.Get(serviceProvider);
        }

        /// <summary>
        /// Gets the developer environment for the given package identifier. 
        /// Make sure you pass your package GUID to get your scoped dev env.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="IDevEnv"/> instance is cached for the 
        /// given service provider, and only created once, using the given 
        /// IDE services as necessary. This default behavior can 
        /// be overriden by setting the <see cref="DevEnvFactory"/>.
        /// </remarks>
        /// <param name="packageId">This is your package identifier, 
        /// so that all provided ClideComponents are properly resolved and available through 
        /// it. If you pass in the global VS service provider, you will get an instance 
        /// that only contains the core Clide services but does not export any of the 
        /// package Clide components.</param>
        public static IDevEnv Get(Guid packageId)
        {
            return defaultFactory.Value.Get(GlobalServiceProvider.Instance.GetLoadedPackage(packageId));
        }
    }
}
