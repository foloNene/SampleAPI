using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Models
{
    public class AuthorForCreationWithDateOfDeathDto : AuthorForCreationDto
    {
        public DateTimeOffset? DateOfDeath { get; set; }
    }
}
