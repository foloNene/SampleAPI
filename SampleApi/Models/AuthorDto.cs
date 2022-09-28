using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.Models
{
    /// <summary>
    /// An Author with Id, Age and MainCatgory
    /// </summary>
    public class AuthorDto
    {
        /// <summary>
        /// The Id of the Author
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The Name of the Author
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The age of the Author
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// The Category belong to.
        /// </summary>
        public string MainCategory { get; set; }

    }
}
