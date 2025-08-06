using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieFeedback.Services;
using MovieFeedback.ViewModels;
using System.Threading.Tasks;

namespace MovieFeedback.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminManagementPanelService _movieManagementService;

        public AdminController(IAdminManagementPanelService movieManagementService)
        {
            _movieManagementService = movieManagementService;
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ManageMovies(string searchQuery, int? genreId = null, double? minRating = null, int page = 1)
        {
            const int pageSize = 50;

            var movies = await _movieManagementService.GetMoviesForManagementAsync(searchQuery, genreId, minRating);

            var pagedMovies = movies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling(movies.Count / (double)pageSize);

            var model = new ManageMoviesPageViewModel
            {
                Movies = pagedMovies,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalMovies = movies.Count,
                SearchQuery = searchQuery
            };

            return View("ManageMovies", model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var result = await _movieManagementService.DeleteMovieAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Movie deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete movie.";
            }

            return RedirectToAction("ManageMovies");
        }

        [HttpGet]
        public async Task<IActionResult> EditMovie(int id)
        {
            var movie = await _movieManagementService.GetMovieDetailsForEditAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            var availableGenres = await _movieManagementService.GetAllGenresAsync(); // витягти всі жанри з БД

            var model = new MovieEditViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                Genres = movie.Genres, // поточні жанри фільму
                AvailableGenres = availableGenres
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(MovieEditViewModel model)
        {
            if (!string.IsNullOrEmpty(Request.Form["Genres"]))
            {
                model.Genres = Request.Form["Genres"].ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .ToList();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _movieManagementService.UpdateMovieAsync(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Movie updated successfully.";
                return RedirectToAction("ManageMovies");
            }

            TempData["ErrorMessage"] = "Failed to update movie.";
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ManageUsers(string searchQuery, int page = 1)
        {
            const int pageSize = 50;

            var users = await _movieManagementService.GetAllUsersAsync();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                users = users.Where(u => u.Username.Contains(searchQuery)).ToList();
            }

            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ManageUsersPageViewModel
            {
                Users = pagedUsers,
                CurrentPage = page,
                TotalUsers = users.Count,
                SearchQuery = searchQuery
            };

            return View("ManageUsers", model);
        }




        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ToggleRole(int userId, string role)
        {
            try
            {
                await _movieManagementService.UpdateUserRoleAsync(userId, role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("ManageUsers");
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ToggleBan(int userId)
        {
            try
            {
                await _movieManagementService.ToggleUserBanAsync(userId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("ManageUsers");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _movieManagementService.DeleteUserAsync(userId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("ManageUsers");
        }


    }
}