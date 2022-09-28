using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.OperationFilters
{
    public class CreateAuthorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId != "CreateAuthor")
            {
                return;
            }

            operation.RequestBody.Content.Add(
                "application/vnd.marvin.authorforcreationwithdateofdeath+json",
                new OpenApiMediaType()
                {
                    
                    //Schema = context.SchemaRegistry.GetOrRegister(
                    // typeof(CreateAuthorWithDateOfDeath))
                });
        }
    }
}
