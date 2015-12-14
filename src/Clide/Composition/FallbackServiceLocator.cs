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

namespace Clide.Composition
{
    using Microsoft.Practices.ServiceLocation;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides component location from a primary and a fallback locator.
    /// </summary>
    internal class FallbackServiceLocator : ServiceLocatorImplBase
    {
        private IServiceLocator primary;
        private IServiceLocator fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackServiceLocator"/> class.
        /// </summary>
        /// <param name="primary">The primary service locator.</param>
        /// <param name="fallback">The fallback service locator.</param>
        public FallbackServiceLocator(IServiceLocator primary, IServiceLocator fallback)
        {
            this.primary = primary;
            this.fallback = fallback;
        }

        /// <summary>
        /// Aggregates the results of the primary and secondary locators.
        /// </summary>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return primary.GetAllInstances(serviceType).Concat(fallback.GetAllInstances(serviceType));
        }

        /// <summary>
        /// Returns the instance from the primary locator if any, otherwise, from the fallback.
        /// </summary>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            try
            {
                return primary.GetInstance(serviceType, key) ?? fallback.GetInstance(serviceType, key);
            }
            catch (ActivationException)
            {
                return fallback.GetInstance(serviceType, key);
            }
        }
    }
}