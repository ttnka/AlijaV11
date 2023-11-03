using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z201_FolioPrint
	{
		[Key]
		[StringLength(50)]
		public string LlaveId { get; set; } = "";
		public string FolioId { get; set; } = "";
		
	}
}

