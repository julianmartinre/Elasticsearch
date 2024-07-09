using ElasticsearchApi;
using Microsoft.AspNetCore.Mvc;

namespace Elasticsearch.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {

        private readonly ILogger<DataController> _logger;
        private readonly IRepository _repository;

        public DataController(ILogger<DataController> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet]
        [Route("createIndex")]
        public IActionResult CreateIndex()
        {
            _repository.DeleteIndex();
            Thread.Sleep(2000);
            _repository.CreateIndex();

            var products = new List<Producto>
                        {
                            new Producto { Nombre = "Notebook 2", Descripcion = "A", Marca = "Asus", Modelo = "A1", Precio = 100 },
                            new Producto { Nombre = "Teclado 5", Descripcion = "B", Marca = "HyperX", Modelo = "B1", Precio = 150 },
                            new Producto { Nombre = "Notebook 1", Descripcion = "C", Marca = "Logitech", Modelo = "C1", Precio = 200 },
                            new Producto { Nombre = "Teclado 3", Descripcion = "D", Marca = "Redragon", Modelo = "D1", Precio = 250 },
                            new Producto { Nombre = "Notebook 3", Descripcion = "E", Marca = "Apple", Modelo = "E1", Precio = 300 },
                            new Producto { Nombre = "Teclado 4", Descripcion = "F", Marca = "Asus", Modelo = "F1", Precio = 350 },
                            new Producto { Nombre = "Notebook 5", Descripcion = "G", Marca = "HyperX", Modelo = "G1", Precio = 400 },
                            new Producto { Nombre = "Teclado 2", Descripcion = "H", Marca = "Logitech", Modelo = "H1", Precio = 450 },
                            new Producto { Nombre = "Notebook 4", Descripcion = "I", Marca = "Redragon", Modelo = "I1", Precio = 500 },
                            new Producto { Nombre = "Teclado 1", Descripcion = "J", Marca = "Apple", Modelo = "J1", Precio = 550 },
                            new Producto { Nombre = "Notebook 6", Descripcion = "A", Marca = "Asus", Modelo = "A2", Precio = 600 },
                            new Producto { Nombre = "Teclado 6", Descripcion = "B", Marca = "HyperX", Modelo = "B2", Precio = 650 }
                        };

            foreach (var product in products)
            {
                _repository.AddProducto(product);
            }

            return Ok();
        }

        [HttpGet]
        [Route("all")]
        public IActionResult GetProducts()
        {
            var result = _repository.GetAll();

            return Ok(result);
        }

        [HttpGet]
        [Route("search/nombre:{nombre}/descripcion:{descripcion}")]
        public IActionResult SearchProduct(string nombre, string descripcion)
        {
            var result = _repository.SearchProduct(nombre, descripcion);

            return Ok(result);
        }

        [HttpGet]
        [Route("searchAggregation/nombre:{nombre}/descripcion:{descripcion}")]
        public IActionResult SearchProductAggregation(string nombre, string descripcion)
        {
            var result = _repository.SearchProductAggregation(nombre, descripcion);

            return Ok(result);
        }

        [HttpGet]
        [Route("searchAggregationMarcaModelo/nombre:{nombre}/descripcion:{descripcion}")]
        public IActionResult SearchProductAggregationMarcaModelo(string nombre, string descripcion)
        {
            var result = _repository.SearchProductAggregationMarcaModelo(nombre, descripcion);

            return Ok(result);
        }

        [HttpGet]
        [Route("marcaPromedioSuma")]
        public IActionResult GetMarcaPromedio()
        {
            var result = _repository.GetMarcaPromedio();

            return Ok(result);
        }

        [HttpGet]
        [Route("rangoPrecio")]
        public IActionResult GetRangoPrecios()
        {
            var result = _repository.GetRangoPrecios();

            return Ok(result);
        }

        [HttpPost]
        [Route("add")]
        public IActionResult Create(Producto producto)
        {
            _repository.AddProducto(producto);

            return Ok();
        }
    }
}