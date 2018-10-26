﻿// <copyright file="ServiceUpdateRequest.cs" company="QC Coders">
//     Copyright (c) QC Coders. All rights reserved. Licensed under the GPLv3 license. See LICENSE file
//     in the project root for full license information.
// </copyright>

namespace QCVOC.Api.Services.Data.DTO
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     DTO containing updated Service details for a service update request.
    /// </summary>
    public class ServiceUpdateRequest
    {
        /// <summary>
        ///     Gets or sets the name of the Service.
        /// </summary>
        [StringLength(maximumLength: 256, MinimumLength = 1)]

        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the description of the service.
        /// </summary>
        [StringLength(maximumLength: 256, MinimumLength = 1)]
        public string Description { get; set; }
    }
}