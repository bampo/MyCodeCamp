using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class CampMappingProfile : Profile
    {
        public CampMappingProfile()
        {
            CreateMap<Camp, CampModel>()
                .ForMember(m=>m.StartDate,opt=> opt.MapFrom(camp=> camp.EventDate))
                .ForMember(m => m.EndDate, opt => opt.ResolveUsing(camp => camp.EventDate.AddDays(camp.Length-1)))
                .ForMember(m => m.Url, opt=> opt.ResolveUsing<CampUrlResolver>())
                .ReverseMap()
                .ForMember(m => m.Length, opt => opt.ResolveUsing(m => (m.EndDate - m.StartDate).Days+1))
                ;

            CreateMap<Speaker, SpeakerModel>()
                .ForMember(m=>m.Url, opt => opt.ResolveUsing<SpeakerUrlResolver>())
                .ReverseMap()
                ;
            
            CreateMap<Talk, TalkModel>()
                .ForMember(m=>m.Url, opt => opt.ResolveUsing<TalkUrlResolver>())
                .ReverseMap()
                ;
        }
    }
}