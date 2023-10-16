using System;
namespace DashBoard.Modelos
{
	public class ApiRespuestas<TEntity> where TEntity : class
    {
        public bool Exito { get; set; }
        public List<string> MsnError { get; set; } = new List<string>();
        public IEnumerable<TEntity> Data { get; set; }
    }
}

