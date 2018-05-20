using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;

namespace MyCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/speakers")]
    [ValidateModel]
    public class SpeakersController : BaseController
    {
        private readonly ICampRepository _repo;
        private readonly ILogger<SpeakersController> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<CampUser> _userMgr;

        public SpeakersController(ICampRepository repository, ILogger<SpeakersController> logger, IMapper mapper,
            UserManager<CampUser> userMgr)
        {
            _repo = repository;
            _logger = logger;
            _mapper = mapper;
            _userMgr = userMgr;
        }

        [HttpGet]
        public IActionResult Get(string moniker, bool includeTalks = false)
        {
            var speakers = includeTalks
                ? _repo.GetSpeakersByMonikerWithTalks(moniker)
                : _repo.GetSpeakersByMoniker(moniker);

            return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
        }

        [HttpGet("{id}", Name = "SpeakerGet")]
        public IActionResult Get(string moniker, int id, bool includeTalks = false)
        {
            Speaker speaker = null;
            speaker = includeTalks ? _repo.GetSpeakerWithTalks(id) : _repo.GetSpeaker(id);
            if (speaker == null) return NotFound();
            if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker not in specified Camp");

            return Ok(_mapper.Map<SpeakerModel>(speaker));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(string moniker, [FromBody] SpeakerModel model)
        {
            try
            {
                var camp = _repo.GetCampByMoniker(moniker);
                if (camp == null) return BadRequest("Could not find camp");

                var speaker = _mapper.Map<Speaker>(model);
                speaker.Camp = camp;
                var campUser = await _userMgr.FindByNameAsync(this.User.Identity.Name);
                if (campUser != null)
                {
                    speaker.User = campUser;

                    _repo.Add(speaker);

                    if (await _repo.SaveAllAsync())
                    {
                        var url = Url.Link("SpeakerGet", new {moniker = camp.Moniker, id = speaker.Id});
                        return Created(url, _mapper.Map<SpeakerModel>(speaker));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exceptio thrown while adding speaker: {e}");
            }

            return BadRequest("Could not add new speaker");
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(string moniker, int id, [FromBody] SpeakerModel model)
        {
            try
            {
                var speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                if (speaker.User.UserName != this.User.Identity.Name) return Forbid();
                _mapper.Map(model, speaker);


                if (await _repo.SaveAllAsync())
                {
                    return Ok(_mapper.Map<SpeakerModel>(speaker));
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while updating speaker: {e}");
            }

            return BadRequest("Could not update speaker");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var speaker = _repo.GetSpeaker(id);
                if (speaker == null) return NotFound();
                if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker and Camp do not match");

                _repo.Delete(speaker);
                if (await _repo.SaveAllAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while delete speaker: {id}");
            }

            return BadRequest("Couldn't delete Speaker");
        }
    }
}