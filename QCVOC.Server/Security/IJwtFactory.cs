﻿namespace QCVOC.Server.Security
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using QCVOC.Server.Data.Model;

    public interface IJwtFactory
    {
        #region Public Methods

        JwtSecurityToken GetAccessToken(Account account);

        JwtSecurityToken GetJwtSecurityToken(int expiry, params Claim[] claims);

        JwtSecurityToken GetRefreshToken(Guid id);

        #endregion Public Methods
    }
}