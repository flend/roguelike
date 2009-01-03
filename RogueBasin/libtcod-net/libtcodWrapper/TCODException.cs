using System;
using System.Collections.Generic;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Base exception class for libtcod-net.
    /// </summary>
    public class TCODException : Exception
    {
        /// <summary>
        /// Constructor. Wraps System.Exception's constructor.
        /// </summary>
        /// <param name="message">
        /// Message to include in the exception.
        /// </param>
        public TCODException(String message)
            : base(message)
        {

        }
    }
}
