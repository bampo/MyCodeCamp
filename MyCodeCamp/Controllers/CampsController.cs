using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Authorize]
    [EnableCors("AnyGET")]
    [Route("api/[controller]")]
    [ValidateModel]
    public class CampsController : BaseController
    {
        private readonly ICampRepository _repo;
        private ILogger<CampsController> _logger;
        private readonly IMapper _mapper;

        //[HttpGet("api/camps")]
        public CampsController(ICampRepository repo, ILogger<CampsController> logger, IMapper mapper)
        {
            _repo = repo;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult Get()
        {
            var camps = _repo.GetAllCamps();
            return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
        }

        [HttpGet("{moniker}", Name = "CampGet")]
        public IActionResult Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                Camp camp;

                if (includeSpeakers) camp = _repo.GetCampByMonikerWithSpeakers(moniker);
                else camp = _repo.GetCampByMoniker(moniker);

                if (camp == null) return NotFound($"Camp {moniker} was not found");
                return Ok(_mapper.Map<CampModel>(camp));
            }
            catch
            {
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CampModel model)
        {
            try
            {

                _logger.LogInformation("Creating a new Code Camp");
                var camp = _mapper.Map<Camp>(model);
                _repo.Add(camp);
                if (await _repo.SaveAllAsync())
                {
                    var newUri = Url.Link("CampGet", new {moniker = model.Moniker});

                    return Created(newUri, _mapper.Map<CampModel>(camp));
                }
                else
                {
                    _logger.LogWarning("Could not save Camp to the database");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Threw exceptio while saving Camp: {e}");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<IActionResult> Put(string moniker, [FromBody] CampModel model)
        {
            try
            {

                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null) return NotFound($"Couldn't find Camp with ID of {moniker}");

                _mapper.Map(model, oldCamp);

                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<CampModel>(oldCamp));
                }
            }
            catch (Exception e)
            {
            }

            return BadRequest("Couldn't update Camp");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = _repo.GetCampByMoniker(moniker);
                if (oldCamp == null) return NotFound($"Couldn't find Camp with ID of {moniker}");
                _repo.Delete(oldCamp);
                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
            }

            return BadRequest("Couldn't delete Camp");
        }
    }
}