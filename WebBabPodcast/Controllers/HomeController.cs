using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebBanDienThoai.Models;
using WebBanDienThoai.ViewsModels;

namespace WebBanDienThoai.Controllers
{
	public class HomeController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository, ApplicationDbContext context)
        {
            _logger = logger;
            _productRepository = productRepository;
            _context = context;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index(string postTitle = null, int page = 1)
        {
            int productsPerPage = 8;
            List<Product> products;
            int totalCount;

            if (postTitle != null)
            {
                products = (await _productRepository.SearchAsync(postTitle)).ToList();
                totalCount = products.Count();
            }
            else
            {
                products = (await _productRepository.GetAllAsync()).ToList();
                totalCount = await _context.Products.CountAsync();
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)productsPerPage);

            if (postTitle == null)
            {
                products = products.OrderBy(p => p.Id)
                                   .Skip((page - 1) * productsPerPage)
                                   .Take(productsPerPage)
                                   .ToList();
            }

            var viewModel = new ProductViewModel
            {
                Prods = products,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string postTitle)
        {
            IEnumerable<Product> products;
            if (!string.IsNullOrEmpty(postTitle))
            {
                products = await _productRepository.SearchAsync(postTitle);
            }
            else
            {
                products = await _productRepository.GetAllAsync();
            }
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
