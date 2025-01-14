using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateContainerCosmosDB.Models
{
    /// <summary>
    /// Defines the usage reference type.
    /// </summary>
    public enum UsageReferenceType
    {
        #region Members

        /// <summary>
        /// The assistant source type.
        /// </summary>
        Assistant = 0,

        /// <summary>
        /// The prompt source type.
        /// </summary>
        Prompt = 1,

        /// <summary>
        /// The agent source type.
        /// </summary>
        Agent = 2,

        #endregion
    }
}
