// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.DataFactory.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Azure Data Factory nested object which contains a flow with data
    /// movements and transformations.
    /// </summary>
    public partial class DataFlow
    {
        /// <summary>
        /// Initializes a new instance of the DataFlow class.
        /// </summary>
        public DataFlow()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the DataFlow class.
        /// </summary>
        /// <param name="description">The description of the data flow.</param>
        /// <param name="annotations">List of tags that can be used for
        /// describing the data flow.</param>
        /// <param name="folder">The folder that this data flow is in. If not
        /// specified, Data flow will appear at the root level.</param>
        public DataFlow(string description = default(string), IList<object> annotations = default(IList<object>), DataFlowFolder folder = default(DataFlowFolder))
        {
            Description = description;
            Annotations = annotations;
            Folder = folder;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the description of the data flow.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets list of tags that can be used for describing the data
        /// flow.
        /// </summary>
        [JsonProperty(PropertyName = "annotations")]
        public IList<object> Annotations { get; set; }

        /// <summary>
        /// Gets or sets the folder that this data flow is in. If not
        /// specified, Data flow will appear at the root level.
        /// </summary>
        [JsonProperty(PropertyName = "folder")]
        public DataFlowFolder Folder { get; set; }

    }
}