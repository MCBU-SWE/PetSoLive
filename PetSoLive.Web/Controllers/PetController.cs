using Microsoft.AspNetCore.Mvc;
using PetSoLive.Core.Entities;
using PetSoLive.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace PetSoLive.Web.Controllers
{
    public class PetController : Controller
    {
        private readonly IPetService _petService;
        private readonly IUserService _userService; // Add IUserService to access user data
        private readonly IAdoptionService _adoptionService;

        public PetController(IPetService petService, IUserService userService, IAdoptionService adoptionService)
        {
            _petService = petService;
            _userService = userService;
            _adoptionService = adoptionService;
        }

        // GET: /Pet/Create
        public IActionResult Create()
        {
            // Check if user session exists
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            // Check if user session exists
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the user from session
            var user = await _userService.GetUserByUsernameAsync(username);

            // Check if 'IsNeutered' value is null when not checked
            if (pet.IsNeutered == null)
            {
                pet.IsNeutered = false; // or leave it as null, depending on your requirements
            }

            if (ModelState.IsValid)
            {
                // Create the pet first
                await _petService.CreatePetAsync(pet);

                // Now create the PetOwner relationship
                var petOwner = new PetOwner
                {
                    PetId = pet.Id,
                    UserId = user.Id,
                    OwnershipDate = DateTime.Now  // Track when the user became the owner
                };

                // Associate the pet with the user as an owner
                await _petService.AssignPetOwnerAsync(petOwner);

                // Redirect to AdoptionController.Index or another page
                return RedirectToAction("Index", "Adoption");
            }

            // Return to the form with validation errors if any
            return View(pet);
        }




// GET: /Pet/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

            // Check if the pet has been adopted
            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);

            // Retrieve the username from session
            var username = HttpContext.Session.GetString("Username");
            var isUserLoggedIn = username != null;
            var isOwner = false;

            if (isUserLoggedIn)
            {
                // Get the logged-in user
                var user = await _userService.GetUserByUsernameAsync(username);
        
                // Check if the user is the owner of this pet
                isOwner = await _petService.IsUserOwnerOfPetAsync(id, user.Id);
            }

            // Pass the necessary information to the view
            ViewBag.IsUserLoggedIn = isUserLoggedIn;
            ViewBag.Adoption = adoption;
            ViewBag.IsOwner = isOwner;  // Indicate whether the logged-in user is the pet's owner
    
            return View(pet);
        }

// GET: /Pet/Edit/{id}
// GET: /Pet/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            // Check if user session exists
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the pet details by its ID
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
            {
                return NotFound(); // Pet not found, return 404
            }

            // Check if the pet has already been adopted
            var adoption = await _adoptionService.GetAdoptionByPetIdAsync(id);
            if (adoption != null)
            {
                // If adopted, prevent editing
                return Unauthorized("This pet has already been adopted and cannot be edited.");
            }

            // Retrieve user data based on username from session
            var user = await _userService.GetUserByUsernameAsync(username);

            // Check if the user is the owner of the pet
            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                return Unauthorized("You are not authorized to edit this pet.");
            }

            return View(pet); // Return the pet data to the Edit view
        }


// POST: /Pet/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet updatedPet)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userService.GetUserByUsernameAsync(username);
            var pet = await _petService.GetPetByIdAsync(id);

            if (pet == null)
            {
                return NotFound();
            }

            // Check if the user is the owner of the pet
            if (!await _petService.IsUserOwnerOfPetAsync(id, user.Id))
            {
                return Unauthorized("You are not authorized to edit this pet.");
            }

            // If IsNeutered is null, set it to false
            if (updatedPet.IsNeutered == null)
            {
                updatedPet.IsNeutered = false;
            }

            // Update the pet
            await _petService.UpdatePetAsync(id, updatedPet, user.Id);

            return RedirectToAction("Details", new { id = pet.Id }); // Redirect to the pet details page
        }




    }
}

