using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Swisschain.Sdk.Server.Common;

namespace Service.WalletManager.Controllers.IsAlive
{
    [ApiController]
    [Route("api/[controller]")]
    public class IsAliveController : ControllerBase
    {
        [HttpGet]
        public IsAliveResponse Get()
        {
            var response = new IsAliveResponse()
            {
                Name = ApplicationInformation.AppName,
                Version = ApplicationInformation.AppVersion,
                StartedAt = ApplicationInformation.StartedAt,
                IssueIndicators = new List<IsAliveResponse.IssueIndicator>()
            };

            return response;
        }
    }
}