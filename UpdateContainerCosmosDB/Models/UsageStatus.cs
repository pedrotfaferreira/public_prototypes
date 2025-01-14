using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateContainerCosmosDB.Models
{
    /// <summary>
    /// Defines the types of a metric.
    /// </summary>
    internal enum UsageStatus
    {
        /// <summary>
        /// The pending status.
        /// </summary>
        Pending,

        /// <summary>
        /// The completed status.
        /// </summary>
        Completed,
    }
}
