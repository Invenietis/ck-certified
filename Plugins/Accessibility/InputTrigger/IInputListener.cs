using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputTrigger
{
    /// <summary>
    /// Describe a class wich is able to catch inputs from the supported devices
    /// </summary>
    public interface IInputListener
    {
        /// <summary>
        /// Listen to inputs and return the corresponding ITrigger object when pressed
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<ITrigger> ListenAsync( int timeout = 0 );

        /// <summary>
        /// Start listening to inputs
        /// </summary>
        void BeginListen();

        /// <summary>
        /// Stop listening to inputs
        /// </summary>
        /// <returns></returns>
        ITrigger EndListen();
    }
}
