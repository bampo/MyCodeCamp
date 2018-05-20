using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class CampUrlResolver : IValueResolver<Camp, CampModel, string>
    {
        private readonly HttpContext _httpContext;

        public CampUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }
        public string Resolve(Camp source, CampModel destination, string destMember, ResolutionContext context)
        {
            return (_httpContext.Items[BaseController.URLHELPER] as IUrlHelper)?.Link("CampGet", new {moniker = source.Moniker});
        }
    }
}