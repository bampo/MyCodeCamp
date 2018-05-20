using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class SpeakerUrlResolver : IValueResolver<Speaker, SpeakerModel, string>
    {
        private readonly HttpContext _httpContext;

        public SpeakerUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }
        public string Resolve(Speaker source, SpeakerModel destination, string destMember, ResolutionContext context)
        {
            return (_httpContext.Items[BaseController.URLHELPER] as IUrlHelper)?.Link("SpeakerGet", new {moniker = source.Camp.Moniker, id = source.Id});
        }
    }
}