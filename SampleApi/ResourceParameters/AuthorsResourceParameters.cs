using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApi.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        //Also Implement const Page size
        const int maxPageSize = 20;

        public string MainCategory { get; set; }

        public string SearchQuery { get; set; }

        //Implementing Pagination for PageNumber
        public int PageNumber { get; set; } = 1;

        //Implementing Pagination for PageSize

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }
    }
}
