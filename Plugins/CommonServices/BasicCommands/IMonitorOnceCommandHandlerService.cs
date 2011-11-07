using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Keyboard.Model;

namespace CommonServices
{
    public interface IMonitorOnceCommandHandlerService : IDynamicService
    {
        /// <summary>
        /// Registers a command to raise when a key has been sent
        /// </summary>
        /// <param name="key">Identifier of the query</param>
        /// <param name="command">Command to raise</param>
        void RegisterOnSendKey( string key, string command );
    }
}
