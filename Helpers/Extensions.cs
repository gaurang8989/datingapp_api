using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebApplication1.Helpers
{
    public static class Extensions //must be static
    {
        public static int CalculateAge(this DateTime theDateTime) //must be static bcz of this
        {
            int age = DateTime.Today.Year - theDateTime.Year;
            return age;
        }

        public static void  AddPagination(this HttpResponse response,int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));  //Header Added to Client Side   
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
