using PhilApprovalFlow.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow.Managers
{
    /// <summary>
    /// Manages metadata operations for approval workflows.
    /// </summary>
    /// <typeparam name="T">The type of the approval transition.</typeparam>
    internal class MetadataManager<T> where T : IPAFTransition
    {
        private readonly Dictionary<string, string> metadata;
        private readonly IApprovalFlow<T> approvalFlowEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataManager{T}"/> class.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public MetadataManager(IApprovalFlow<T> entity)
        {
            approvalFlowEntity = entity ?? throw new ArgumentNullException(nameof(entity));
            
            // Use non-generic GetCustomAttributes for .NET Framework 4.8 compatibility
            var attributes = entity.GetType().GetCustomAttributes(typeof(PAFMetadataAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                metadata = attributes.Cast<PAFMetadataAttribute>()
                                   .ToDictionary(c => c.Key, c => c.Value);
            }
            else
            {
                metadata = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Adds or updates metadata associated with the approval flow.
        /// </summary>
        /// <param name="key">The key of the metadata entry to add or update.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="key"/> is null, empty, or contains only whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is null.
        /// </exception>
        public void SetMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null");
            }

            metadata[key] = value;
        }

        /// <summary>
        /// Retrieves a metadata value by its key.
        /// </summary>
        /// <param name="key">The key of the metadata entry to retrieve.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified <paramref name="key"/> does not exist in the metadata collection.
        /// </exception>
        public string GetMetadata(string key)
        {
            return metadata[key];
        }

        /// <summary>
        /// Sets metadata for the approval flow entity by adding key details such as ID, short description, and long description.
        /// </summary>
        /// <remarks>
        /// This method extracts metadata from the entity using its <see cref="IApprovalFlow{T}.GetID"/>,
        /// <see cref="IApprovalFlow{T}.GetShortDescription"/>, and <see cref="IApprovalFlow{T}.GetLongDescription"/> methods
        /// and adds it to the internal metadata dictionary with the keys "id", "shortDescription", and "longDescription" respectively.
        /// </remarks>
        public void SetEntityMetaData()
        {
            metadata["id"] = approvalFlowEntity.GetID().ToString();
            metadata["shortDescription"] = approvalFlowEntity.GetShortDescription();
            metadata["longDescription"] = approvalFlowEntity.GetLongDescription();
        }
    }
}