using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchApi
{
    public interface IProducto
    {
       string Nombre { get; set; }
       string Descripcion { get; set; }
       string Marca { get; set; }
       string Modelo { get; set; }
       double Precio { get; set; }
    }

    public class Producto : IProducto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public double Precio { get; set; }
    }

    public interface IMarcaAgrupada
    {
        string Marca { get; set; }
        int CantidadProductos { get; }
        IList<IProducto> Productos { get; set; }
    }

    public class MarcaAgrupada : IMarcaAgrupada
    {
        public string Marca { get; set; }
        public int CantidadProductos { get { return Productos.Count(); } }
        public IList<IProducto> Productos { get; set; } = new List<IProducto>();
    }

    public interface IMarcaModeloAgrupada
    {
        string Marca { get; set; }
        long? CantidadModelos { get; }
        IList<IModeloAgrupado> Modelos { get; set; }
    }

    public class MarcaModeloAgrupada : IMarcaModeloAgrupada
    {
        public string Marca { get; set; }
        public long? CantidadModelos { get; set; }
        public IList<IModeloAgrupado> Modelos { get; set; } = new List<IModeloAgrupado>();
    }

    public interface IModeloAgrupado
    {
        string Modelo { get; set; }
        long? CantidadProductos { get; }
    }

    public class ModeloAgrupado : IModeloAgrupado
    {
        public string Modelo { get; set; }
        public long? CantidadProductos { get; set; }
    }

    public interface IMarcaPromedio
    {
        string Marca { get; set; }
        double? Suma { get; set; }
        double? Promedio {  get; set; }
    }

    public class MarcaPromedio : IMarcaPromedio
    {
        public string Marca { get; set; }
        public double? Suma { get; set; }
        public double? Promedio { get; set; }
    }

    public interface IRangoPrecio
    {
        double? Desde { get; set; }
        double? Hasta { get; set; }
        double? Cantidad {  get; set; }
    }

    public class RangoPrecio : IRangoPrecio
    {
        public double? Desde { get; set; }
        public double? Hasta { get; set; }
        public double? Cantidad { get; set; }
    }
}