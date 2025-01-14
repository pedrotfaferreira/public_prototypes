using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateContainerCosmosDB.Models
{
    /// <summary>
    /// Defines the usage source type.
    /// </summary>
    public enum UsageSourceType
    {
        #region Members

        /// <summary>
        /// The session item source type.
        /// </summary>
        SessionItem = 0,

        /// <summary>
        /// The session item source type.
        /// </summary>
        Session = 1,

        #endregion
    }
}
