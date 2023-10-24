﻿using System;
namespace DashBoard.Modelos
{
	public class ZFiltros
	{
		
	}

    public class FiltroFolio : Z200_Folio
    {
        public bool Datos { get; set; } = false;
        public int FolioInicial { get; set; }
        public int FolioFinal { get; set; }
        public DateTime FolioFecInicial { get; set; }
        public DateTime FolioFecFinal { get; set; }


    }

    public class FiltroFactura : Z220_Factura
	{
		public bool Datos { get; set; } = false;
        public DateTime FactFecInicial { get; set; }
        public DateTime FactFecFinal { get; set; }

    }
    public class FiltroPago : Z230_Pago
    {
        public bool Datos { get; set; } = false;
        public DateTime PagoFecInicial { get; set; }
        public DateTime PagoFecFinal { get; set; }

    }
}

