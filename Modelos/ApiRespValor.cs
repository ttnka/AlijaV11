using System;
namespace DashBoard.Modelos
{
	public class ApiRespValor
	{
        public bool Exito { get; set; }
        public List<string> MsnError { get; set; } = new List<string>();
        public string Texto { get; set; }
       
    }
}

