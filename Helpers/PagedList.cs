using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Helpers
{
    public class PagedList<T> : List<T>
    {
        

        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPage = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        //This Static Class use for Create new Instance of the PageList at return 
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize) 
        {
            var count = await source.CountAsync();   // Store Number Of Elements(Count all the users)
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();   // Give Paging Information 
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }


    }
}
