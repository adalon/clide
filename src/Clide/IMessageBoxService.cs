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

namespace Clide
{
    using System.Windows;

    /// <summary>
    /// Provides a contract to show messages to the user.
    /// </summary>
    public interface IMessageBoxService
	{
        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        /// <returns>
		/// <see langword="true"/> if the user clicked on Yes/OK, 
		/// <see langword="false"/> if the user clicked No, 
		/// <see langword="null"/> if the user cancelled the dialog or clicked 
		/// <c>Cancel</c> or any other value other than the Yes/OK/No.
		/// </returns>
		bool? Show(string message, string title = MessageBoxService.DefaultTitle, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK);

        /// <summary>
        /// Prompts the user for a response.
        /// </summary>
        MessageBoxResult Prompt(string message, string title = MessageBoxService.DefaultTitle, MessageBoxButton button = MessageBoxButton.OKCancel, MessageBoxImage icon = MessageBoxImage.Question, MessageBoxResult defaultResult = MessageBoxResult.OK);

        /// <summary>
        /// Gets a string inputs from the user.
        /// </summary>
        string InputBox(string message, string title = MessageBoxService.DefaultTitle);
    }
}
