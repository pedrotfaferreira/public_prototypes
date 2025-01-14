using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateContainerCosmosDB.Models
{
    internal class UsageRecord
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the business key.
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the reference type.
        /// </summary>
        public UsageReferenceType ReferenceType { get; set; }

        /// <summary>
        /// Gets or sets the source identifier.
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public UsageSourceType SourceType { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the creation by.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the usage record is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the usage record is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the metric id.
        /// </summary>
        public string MetricId { get; set; }

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets the physical key.
        /// </summary>
        public string PhysicalKey { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public UsageStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        #endregion
    }
}
