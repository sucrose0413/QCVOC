// <copyright file="PatronRepository.cs" company="QC Coders (JP Dillingham, Nick Acosta, et. al.)">
//     Copyright (c) QC Coders (JP Dillingham, Nick Acosta, et. al.). All rights reserved. Licensed under the GPLv3 license. See LICENSE file
//     in the project root for full license information.
// </copyright>

namespace QCVOC.Api.Domain.Patrons.Data.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper;
    using QCVOC.Api.Common;
    using QCVOC.Api.Common.Data.ConnectionFactory;
    using QCVOC.Api.Common.Data.Repository;
    using QCVOC.Api.Domain.Patrons.Data.Model;

    /// <summary>
    ///     Provides data access for <see cref="Patron"/>.
    /// </summary>
    public class PatronRepository : IRepository<Patron>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PatronRepository"/> class.
        /// </summary>
        /// <param name="connectionFactory">The database connection factory used for data access.</param>
        public PatronRepository(IDbConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        private IDbConnectionFactory ConnectionFactory { get; }

        /// <summary>
        ///     Creates a new Patron from the specified <paramref name="patron"/>.
        /// </summary>
        /// <param name="patron">The Patron to create.</param>
        /// <returns>The created Patron.</returns>
        public Patron Create(Patron patron)
        {
            var builder = new SqlBuilder();

            var query = builder.AddTemplate(@"
                INSERT INTO patrons (
                    id,
                    memberid,
                    firstname,
                    lastname,
                    lastupdatedate,
                    lastupdatebyid,
                    address,
                    primaryphone,
                    secondaryphone,
                    email,
                    enrollmentdate
                )
                VALUES (
                    @id,
                    @memberid,
                    @firstname,
                    @lastname,
                    @lastupdatedate,
                    @lastupdatebyid,
                    @address,
                    @primaryphone,
                    @secondaryphone,
                    @email,
                    @enrollmentdate
                )
            ");

            builder.AddParameters(new
            {
                id = patron.Id,
                memberid = patron.MemberId,
                firstname = patron.FirstName,
                lastname = patron.LastName,
                lastupdatedate = patron.LastUpdateDate,
                lastupdatebyid = patron.LastUpdateById,
                address = patron.Address,
                primaryphone = patron.PrimaryPhone,
                secondaryphone = patron.SecondaryPhone,
                email = patron.Email,
                enrollmentdate = patron.EnrollmentDate,
            });

            using (var db = ConnectionFactory.CreateConnection())
            {
                db.Execute(query.RawSql, query.Parameters);
            }

            return Get(patron.Id);
        }

        /// <summary>
        ///     Deletes the Patron matching the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the Patron to delete.</param>
        public void Delete(Guid id)
        {
            var builder = new SqlBuilder();

            var query = builder.AddTemplate(@"
                DELETE FROM patrons
                WHERE id = @id
            ");

            builder.AddParameters(new { id });

            using (var db = ConnectionFactory.CreateConnection())
            {
                db.Execute(query.RawSql, query.Parameters);
            }
        }

        /// <summary>
        ///     Deletes the specified <paramref name="patron"/>.
        /// </summary>
        /// <param name="patron">The Patron to delete.</param>
        public void Delete(Patron patron)
        {
            Delete(patron.Id);
        }

        /// <summary>
        ///     Retrieves the Patron matching the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Patron"/> to retrieve.</param>
        /// <returns>The Patron matching the specified id.</returns>
        public Patron Get(Guid id)
        {
            return GetAll(new PatronFilters() { Id = id }).SingleOrDefault();
        }

        /// <summary>
        ///     Retrieves all Patrons after applying optional <paramref name="filters"/>.
        /// </summary>
        /// <param name="filters">Optional query filters.</param>
        /// <returns>A list of Patrons</returns>
        public IEnumerable<Patron> GetAll(Filters filters = null)
        {
            filters = filters ?? new Filters();
            var builder = new SqlBuilder();

            var query = builder.AddTemplate($@"
                SELECT
                    p.id,
                    memberid,
                    firstname,
                    lastname,
                    p.lastupdatedate,
                    COALESCE(a.name, '(Deleted user)') AS lastupdateby,
                    p.lastupdatebyid,
                    address,
                    primaryphone,
                    secondaryphone,
                    email,
                    enrollmentdate
                FROM patrons p
                LEFT JOIN accounts a ON p.lastupdatebyid = a.id 
                /**where**/
                ORDER BY (firstname || lastname) {filters.OrderBy.ToString()}
                LIMIT @limit OFFSET @offset
            ");

            builder.AddParameters(new
            {
                limit = filters.Limit,
                offset = filters.Offset,
                orderby = filters.OrderBy.ToString(),
            });

            if (filters is PatronFilters patronFilters)
            {
                if (!string.IsNullOrWhiteSpace(patronFilters.Address))
                {
                    builder.Where("address = @address", new { address = patronFilters.Address });
                }

                if (!string.IsNullOrWhiteSpace(patronFilters.Email))
                {
                    builder.Where("email = @email", new { email = patronFilters.Email });
                }

                if (patronFilters.EnrollmentDateStart != null && patronFilters.EnrollmentDateEnd != null)
                {
                    builder.Where("enrollmentdate BETWEEN @start AND @end", new { start = patronFilters.EnrollmentDateStart, end = patronFilters.EnrollmentDateEnd });
                }

                if (!string.IsNullOrWhiteSpace(patronFilters.FirstName))
                {
                    builder.Where("firstname = @firstname", new { firstname = patronFilters.FirstName });
                }

                if (patronFilters.Id != null)
                {
                    builder.Where("p.id = @id", new { id = patronFilters.Id });
                }

                if (!string.IsNullOrWhiteSpace(patronFilters.LastName))
                {
                    builder.Where("lastname = @lastname", new { lastname = patronFilters.LastName });
                }

                if (patronFilters.LastUpdateDateStart != null && patronFilters.LastUpdateDateEnd != null)
                {
                    builder.Where("lastupdatedate BETWEEN @start AND @end", new { start = patronFilters.LastUpdateDateStart, end = patronFilters.LastUpdateDateEnd });
                }

                if (!string.IsNullOrWhiteSpace(patronFilters.LastUpdateBy))
                {
                    builder.Where("a.name = @lastupdateby", new { lastupdateby = patronFilters.LastUpdateBy });
                }

                if (patronFilters.LastUpdateById != null)
                {
                    builder.Where("lastupdatebyid = @lastupdatebyid", new { lastupdatebyid = patronFilters.LastUpdateById });
                }

                if (patronFilters.MemberId != null)
                {
                    builder.Where("memberid = @memberid", new { memberid = patronFilters.MemberId });
                }

                if (!string.IsNullOrWhiteSpace(patronFilters.PrimaryPhone))
                {
                    builder.Where("primaryphone = @primaryphone", new { primaryphone = patronFilters.PrimaryPhone });
                }

                if (!string.IsNullOrWhiteSpace(patronFilters.SecondaryPhone))
                {
                    builder.Where("secondaryphone = @secondaryphone", new { secondaryphone = patronFilters.SecondaryPhone });
                }
            }

            using (var db = ConnectionFactory.CreateConnection())
            {
                return db.Query<Patron>(query.RawSql, query.Parameters);
            }
        }

        /// <summary>
        ///     Updates the specified <paramref name="patron"/>.
        /// </summary>
        /// <param name="patron">The Patron to update.</param>
        /// <returns>The updated Patron.</returns>
        public Patron Update(Patron patron)
        {
            var builder = new SqlBuilder();

            var query = builder.AddTemplate(@"
                UPDATE patrons
                SET
                    memberid = @memberId,
                    firstname = @firstName,
                    lastname = @lastName,
                    lastupdatedate = @lastupdatedate,
                    lastupdatebyid = @lastupdatebyid,
                    address = @address,
                    primaryphone = @primaryPhone,
                    secondaryphone = @secondaryPhone,
                    email = @email,
                    enrollmentdate = @enrollmentDate
                WHERE id = @id
            ");

            builder.AddParameters(new
            {
                memberId = patron.MemberId,
                firstName = patron.FirstName,
                lastName = patron.LastName,
                lastupdatedate = patron.LastUpdateDate,
                lastupdatebyid = patron.LastUpdateById,
                address = patron.Address,
                primaryPhone = patron.PrimaryPhone,
                secondaryPhone = patron.SecondaryPhone,
                email = patron.Email,
                enrollmentDate = patron.EnrollmentDate,
                id = patron.Id
            });

            using (var db = ConnectionFactory.CreateConnection())
            {
                db.Execute(query.RawSql, query.Parameters);
            }

            return Get(patron.Id);
        }
    }
}