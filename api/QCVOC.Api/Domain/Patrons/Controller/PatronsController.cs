// <copyright file="PatronsController.cs" company="JP Dillingham, Nick Acosta, et. al.">
//     Copyright (c) JP Dillingham, Nick Acosta, et. al.. All rights reserved. Licensed under the GPLv3 license. See LICENSE file
//     in the project root for full license information.
// </copyright>

namespace QCVOC.Api.Domain.Patrons.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using QCVOC.Api.Common.Data.Repository;
    using QCVOC.Api.Domain.Patrons.Data.Model;
    using QCVOC.Api.Security;

    /// <summary>
    ///     Provides endpoints for manipulation of Patron records.
    /// </summary>
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class PatronsController : Controller
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PatronsController"/> class.
        /// </summary>
        /// <param name="patronRepository">The repository used for Patron data access.</param>
        public PatronsController(IRepository<Patron> patronRepository)
        {
            PatronRepository = patronRepository;
        }

        private IRepository<Patron> PatronRepository { get; set; }

        /// <summary>
        ///     Returns a list of Patrons.
        /// </summary>
        /// <param name="filters">Optional filtering and pagination options.</param>
        /// <returns>See attributes.</returns>
        /// <response code="200">The list was retrieved successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="500">The server encountered an error while processing the request.</response>
        [HttpGet("")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Patron>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(Exception), 500)]
        public IActionResult GetAll([FromQuery]PatronFilters filters)
        {
            return Ok(PatronRepository.GetAll(filters));
        }

        /// <summary>
        ///     Returns the Patron matching the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the Patron to retrieve.</param>
        /// <returns>See attributes.</returns>
        /// <response code="200">The Patron was retrieved successfully.</response>
        /// <response code="400">The specified id was invalid.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">A Patron matching the specified id could not be found.</response>
        /// <response code="500">The server encountered an error while processing the request.</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Patron>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(Exception), 500)]
        public IActionResult Get(Guid id)
        {
            var patron = PatronRepository.Get(id);

            if (patron == default(Patron))
            {
                return NotFound();
            }

            return Ok(patron);
        }

        /// <summary>
        ///     Enrolls a new Patron.
        /// </summary>
        /// <param name="patron">The Patron to enroll.</param>
        /// <returns>See attributes.</returns>
        /// <response code="201">The Patron was enrolled successfully.</response>
        /// <response code="400">The specified Patron was invalid.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="409">A Patron with the same member id or first and last names and address already exists.</response>
        /// <response code="500">The server encountered an error while processing the request.</response>
        [HttpPost("")]
        [Authorize]
        [ProducesResponseType(typeof(Patron), 201)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        [ProducesResponseType(typeof(Exception), 500)]
        public IActionResult Enroll(Patron patron)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPatron = PatronRepository.GetAll(new PatronFilters()
            {
                MemberId = patron.MemberId,
            });

            if (existingPatron != default(Patron))
            {
                return Conflict($"A Patron with member id '{patron.MemberId}' already exists.");
            }

            existingPatron = PatronRepository.GetAll(new PatronFilters()
            {
                FirstName = patron.FirstName,
                LastName = patron.LastName,
                Address = patron.Address,
            });

            if (existingPatron != default(Patron))
            {
                return Conflict($"A Patron with a matching first name, last name and address a.ready exists.");
            }

            try
            {
                var createdPatron = PatronRepository.Create(patron);
                return StatusCode(201, createdPatron);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating the specified Patron: {ex.Message}. See inner exception for details.", ex);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PatronResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        [ProducesResponseType(typeof(Exception), 500)]
        public IActionResult Put(Patron patron)
        {
            if (patron == null)
            {
                return BadRequest("Patron cannot be null.");
            }

            ModelStateDictionary err = ValidatePatron(patron);

            if (err.Keys.Any())
            {
                return BadRequest(err);
            }

            Patron patronResponse = default(Patron);

            try
            {
                patronResponse = PatronRepository.Create(patron);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception("Error createing the specified Patron. See inner exception for details.", ex));
            }

            return Ok(MapPatronResponseFrom(patronResponse));
        }

        /// <summary>
        ///     Deletes the Patron matching the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the Patron to delete.</param>
        /// <returns>See attributes.</returns>
        /// <response code="204">The Patron was deleted successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">The user has insufficient rights to perform this operation.</response>
        /// <response code="404">A Patron matching the specified id could not be found.</response>
        /// <response code="500">The server encountered an error while processing the request.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(Role.Administrator) + "," + nameof(Role.Supervisor))]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(Exception), 500)]
        public IActionResult Delete(Guid id)
        {
            var patron = PatronRepository.Get(id);

            if (patron == default(Patron))
            {
                return NotFound();
            }

            try
            {
                PatronRepository.Delete(patron);
                return NoContent();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting the specified Patron: {ex.Message}. See inner exception for details.", ex));
            }
        }

        public ModelStateDictionary ValidatePatron(Patron patron)
        {
            var err = new ModelStateDictionary();

            if (patron.MemberId <= 0)
            {
                err.AddModelError("memberId", "The patron's memberId must be a positive number.");
            }

            if (string.IsNullOrWhiteSpace(patron.FirstName))
            {
                err.AddModelError("firstName", "The patron's first name must be alphanumeric.");
            }

            if (string.IsNullOrWhiteSpace(patron.LastName))
            {
                err.AddModelError("lastName", "The patron's last name must be alphanumeric.");
            }

            if (string.IsNullOrWhiteSpace(patron.Address))
            {
                err.AddModelError("address", "The patron's address must be alphanumeric.");
            }

            // TODO: Better phone number validation.
            if (string.IsNullOrWhiteSpace(patron.PrimaryPhone))
            {
                err.AddModelError("primaryPhone", "The patron's primary phone number must be a valid phone number.");
            }

            // TODO: Better phone number validation.
            if (patron.SecondaryPhone != null && patron.SecondaryPhone.All(p => char.IsWhiteSpace(p)))
            {
                err.AddModelError("secondayPhone", "The patron's secondary phone number must be a valid phone number.");
            }

            if (string.IsNullOrWhiteSpace(patron.Email))
            {
                err.AddModelError("email", "The patron's email must be alphanumeric.");
            }

            if (patron.EnrollmentDate == null)
            {
                err.AddModelError("enrollmentDate", "The patron's enrollment date must be a valid date.");
            }

            return err;
        }

        private PatronResponse MapPatronResponseFrom(Patron patron)
        {
            return new PatronResponse(patron.Id, patron.MemberId, patron.FirstName,
            patron.LastName, patron.Address, patron.PrimaryPhone, patron.SecondaryPhone,
            patron.Email, patron.EnrollmentDate);
        }
    }
}