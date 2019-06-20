using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Rmdb.Web.Client.Data;
using Rmdb.Web.Client.Data.Contracts;
using Rmdb.Web.Client.Model;
using Rmdb.Web.Client.ViewModels.Actors;

namespace Rmdb.Web.Client.Controllers
{
    public class ActorsController : Controller
    {
        private readonly IActorService _actorService;
        private readonly IMapper _mapper;

        public ActorsController(IActorService actorService, IMapper mapper)
        {
            _actorService = actorService ?? throw new ArgumentNullException(nameof(actorService)); 
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IActionResult> Index()
        {
            var actors = (await _actorService.GetAllAsync());
            var viewModels = _mapper.Map<IEnumerable<ActorViewModel>>(actors);

            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ActorCreateViewModel viewModel)
        {

            if (!TryValidateModel(viewModel))
            {
                return View(viewModel);
            }

            var actor = _mapper.Map<Actor>(viewModel);
            await _actorService.AddAsync(actor);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var actor = await _actorService.GetAsync(id);
            var viewModel = _mapper.Map<ActorUpdateViewModel>(actor);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, ActorUpdateViewModel viewModel)
        {

            if (!TryValidateModel(viewModel))
            {
                return View(viewModel);
            }

            var actor = _mapper.Map<Actor>(viewModel);

            await _actorService.UpdateAsync(id, actor);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await _actorService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}