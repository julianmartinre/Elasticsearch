using Nest;
using System;
using System.Collections.Generic;

namespace ElasticsearchApi
{
    public interface IRepository
    {
        bool CreateIndex(string indexName = "products");
        bool DeleteIndex(string indexName = "products");
        bool AddProducto(IProducto producto);
        IList<IProducto> GetAll();
        IList<IProducto> SearchProduct(string nombre, string descripcion);
        IList<IMarcaAgrupada> SearchProductAggregation(string nombre, string descripcion);
        IList<IMarcaModeloAgrupada> SearchProductAggregationMarcaModelo(string nombre, string descripcion);
        IList<IMarcaPromedio> GetMarcaPromedio();
        IList<IRangoPrecio> GetRangoPrecios();
    }

    public class Repository : IRepository
    {
        ElasticClient client;

        public Repository()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("products");

            client = new ElasticClient(settings);

            var pingResponse = client.Ping();
            if (pingResponse.IsValid)
            {
                Console.WriteLine("Conexión exitosa a Elasticsearch");
            }
            else
            {
                Console.WriteLine("Error al conectar a Elasticsearch: " + pingResponse.DebugInformation);
            }
        }

        public bool CreateIndex(string indexName = "products")
        {
            var createIndexResponse = client.Indices.Create(indexName, c => c
              .Map<Producto>(m => m
                  .Properties(p => p
                      .Text(t => t
                          .Name(n => n.Nombre)
                          .Analyzer("standard")
                          .Fields(f => f
                              .Keyword(k => k.Name("keyword"))
                          )
                      )
                      .Text(t => t
                           .Name(n => n.Descripcion)
                           .Analyzer("standard")
                           .Fields(f => f
                               .Keyword(k => k.Name("keyword"))
                           )
                        )
                      .Text(t => t
                           .Name(n => n.Marca)
                           .Analyzer("standard")
                           .Fields(f => f
                               .Keyword(k => k.Name("keyword"))
                           )
                        )
                      .Text(t => t
                           .Name(n => n.Modelo)
                           .Analyzer("standard")
                           .Fields(f => f
                               .Keyword(k => k.Name("keyword"))
                           )
                        )
                      .Number(n => n.Name(n => n.Precio).Type(NumberType.Double))
                  )
              )
            );

            return createIndexResponse.IsValid;
        }

        public bool DeleteIndex(string indexName = "products")
        {
            var deleteIndexResponse = client.Indices.Delete(indexName);

            return deleteIndexResponse.IsValid;
        }

        public bool AddProducto(IProducto producto)
        {
            var indexResponse = client.IndexDocument(producto);
            if (indexResponse.IsValid)
            {
                Console.WriteLine($"Producto {producto.Nombre} indexado correctamente");
            }
            else
            {
                Console.WriteLine($"Error al indexar producto {producto.Nombre}: " + indexResponse.DebugInformation);
            }
            return indexResponse.IsValid;
        }

        public IList<IProducto> GetAll()
        {
            IList<IProducto> productos = new List<IProducto>();
            var searchResponse = client.Search<Producto>(s => s
                .Index("products")
                .MatchAll()
                .Size(1000)
            );

            if (searchResponse.IsValid)
            {
                foreach (var hit in searchResponse.Hits)
                {
                    Producto producto = hit.Source;
                    productos.Add(producto);
                }
            }

            return productos;
        }

        public IList<IProducto> SearchProduct(string nombre, string descripcion)
        {
            IList<IProducto> productos = new List<IProducto>();
            var searchResponse = client.Search<Producto>(s => s
                .Index("products")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.Match(m => m.Field(f => f.Nombre).Query(nombre).Boost(2)),
                            sh => sh.Match(m => m.Field(f => f.Descripcion).Query(descripcion))
                        )

                    )
                )
                .Sort(ss => ss
                    .Ascending(p => p.Nombre.Suffix("keyword"))
                )
            );

            if (searchResponse.IsValid)
            {
                foreach (var hit in searchResponse.Hits)
                {
                    Producto producto = hit.Source;
                    productos.Add(producto);
                }
            }

            return productos;
        }

        public IList<IMarcaAgrupada> SearchProductAggregation(string nombre, string descripcion)
        {
            IList<IMarcaAgrupada> marcas = new List<IMarcaAgrupada>();
            var searchResponse = client.Search<Producto>(s => s
                .Index("products")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.Match(m => m.Field(f => f.Nombre).Query(nombre).Boost(2)),
                            sh => sh.Match(m => m.Field(f => f.Descripcion).Query(descripcion))
                        )
                    )
                )
                .Sort(ss => ss
                    .Ascending(p => p.Nombre.Suffix("keyword"))
                )
                .Aggregations(a => a
                    .Terms("por_marca", t => t
                        .Field(f => f.Marca.Suffix("keyword"))
                        .Size(10)
                        .Aggregations(aa => aa
                            .TopHits("productos", th => th
                                .Size(100)
                                .Sort(ths => ths
                                    .Ascending(p => p.Nombre.Suffix("keyword"))
                                )
                                .Source(src => src
                                    .Includes(i => i
                                        .Fields(
                                            f => f.Nombre,
                                            f => f.Descripcion,
                                            f => f.Marca,
                                            f => f.Modelo,
                                            f => f.Precio
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                var marcasResult = searchResponse.Aggregations.Terms("por_marca");
                foreach (var marca in marcasResult.Buckets)
                {
                    MarcaAgrupada marcaAgrupada = new MarcaAgrupada();
                    marcaAgrupada.Marca = marca.Key;

                    var productos = marca.TopHits("productos").Hits<Producto>().ToList();
                    foreach (var producto in productos)
                    {
                        marcaAgrupada.Productos.Add(new Producto()
                        {
                            Nombre = producto.Source.Nombre,
                            Descripcion = producto.Source.Descripcion,
                            Marca = marca.Key,
                            Modelo = producto.Source.Modelo,
                            Precio = producto.Source.Precio
                        });
                    }
                    marcas.Add(marcaAgrupada);
                }
            }

            return marcas;
        }

        public IList<IMarcaModeloAgrupada> SearchProductAggregationMarcaModelo(string nombre, string descripcion)
        {
            IList<IMarcaModeloAgrupada> marcas = new List<IMarcaModeloAgrupada>();
            var searchResponse = client.Search<Producto>(s => s
                .Index("products")
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.Match(m => m.Field(f => f.Nombre).Query(nombre).Boost(2)),
                            sh => sh.Match(m => m.Field(f => f.Descripcion).Query(descripcion))
                        )
                    )
                )
                .Sort(ss => ss
                    .Ascending(p => p.Nombre.Suffix("keyword"))
                )
                .Aggregations(a => a
                    .Terms("por_marca", t => t
                        .Field(f => f.Marca.Suffix("keyword"))
                        .Size(10)
                        .Aggregations(aa => aa
                            .Terms("por_modelo", tm => tm
                                .Field(f => f.Modelo.Suffix("keyword"))
                                .Size(10)
                                .Aggregations(aaa => aaa
                                    .ValueCount("cantidad_documentos", vc => vc
                                        .Field(f => f.Precio)
                                    )
                                )
                            )
                        )
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                var marcasResult = searchResponse.Aggregations.Terms("por_marca");
                foreach (var marca in marcasResult.Buckets)
                {
                    MarcaModeloAgrupada marcaModeloAgrupada = new MarcaModeloAgrupada();
                    marcaModeloAgrupada.Marca = marca.Key;
                    marcaModeloAgrupada.CantidadModelos = marca.DocCount;

                    var modelos = marca.Terms("por_modelo");
                    foreach (var modelo in modelos.Buckets)
                    {
                        ModeloAgrupado modeloAgrupado = new ModeloAgrupado();
                        modeloAgrupado.Modelo = modelo.Key;
                        modeloAgrupado.CantidadProductos = modelo.DocCount;

                        marcaModeloAgrupada.Modelos.Add(modeloAgrupado);
                    }

                    marcas.Add(marcaModeloAgrupada);
                }
            }

            return marcas;
        }

        public IList<IMarcaPromedio> GetMarcaPromedio()
        {
            IList<IMarcaPromedio> result = new List<IMarcaPromedio>();
            var searchResponse = client.Search<Producto>(s => s
                .Index("products")
                .Size(0)
                .Aggregations(a => a
                    .Terms("por_marca", t => t
                        .Field(f => f.Marca.Suffix("keyword"))
                        .Size(10)
                        .Aggregations(aa => aa
                            .Sum("suma_precio", sm => sm
                                .Field(f => f.Precio)
                            )
                            .Average("promedio_precio", am => am
                                .Field(f => f.Precio)
                            )
                        )
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                var marcas = searchResponse.Aggregations.Terms("por_marca");
                foreach (var marca in marcas.Buckets)
                {
                    MarcaPromedio marcaPromedio = new MarcaPromedio();
                    marcaPromedio.Marca = marca.Key;
                    marcaPromedio.Suma = marca.Sum("suma_precio")?.Value;
                    marcaPromedio.Promedio = marca.Average("promedio_precio")?.Value;
                    result.Add(marcaPromedio);
                }
            }

            return result;
        }

        public IList<IRangoPrecio> GetRangoPrecios()
        {
            IList<IRangoPrecio> result = new List<IRangoPrecio>();
            var searchResponse = client.Search<Producto>(s => s
               .Index("products")
               .Size(0)
               .Aggregations(a => a
                   .Range("rangos_precios", r => r
                       .Field(f => f.Precio)
                       .Ranges(
                           r => r.To(150),
                           r => r.From(150).To(300),
                           r => r.From(300)
                       )
                       .Aggregations(aa => aa
                           .ValueCount("cantidad_productos", vc => vc
                               .Field(f => f.Precio)
                           )
                       )
                   )
               )
            );

            if (searchResponse.IsValid)
            {
                var rangos = searchResponse.Aggregations.Range("rangos_precios");
                foreach (var rango in rangos.Buckets)
                {
                    RangoPrecio rangoPrecio = new RangoPrecio();
                    rangoPrecio.Desde = rango.From ?? double.MinValue;
                    rangoPrecio.Hasta = rango.To ?? double.MaxValue;
                    rangoPrecio.Cantidad = rango.ValueCount("cantidad_productos")?.Value;
                    result.Add(rangoPrecio);
                }
            }

            return result;
        }
    }
}
