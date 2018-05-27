using System;
using System.Collections;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Moq;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Models;
using Xunit;

namespace MyCodeCampTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            var repo = new Mock<ICampRepository>();
            repo.Setup(p => p.GetAllCamps())
                .Returns( new List<Camp>()
                    {
                        new Camp(){Description = "sdfsfdsf", EventDate = DateTime.Today}
                    }
                
                );

            var logger =  Mock.Of<ILogger<CampsController>>();

            var mc = new MapperConfiguration(c =>
            {
                c.AddProfile<CampMappingProfile>();
            });
            var mapper = new Mapper(mc);
            
            //Mapper.Initialize(c => c.AddProfile(typeof(CampMappingProfile)));
            
            var camp = new CampsController(repo.Object, logger, mapper);
            camp.ControllerContext = new ControllerContext(new ActionContext(){HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(), ActionDescriptor = new ControllerActionDescriptor()});
            var ret = camp.Get();
        }
    }
}
