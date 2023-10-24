using System;
namespace DashBoard.Modelos
{
	public class MyFunc
	{
        public Z190_Bitacora MakeBitacora(string userId, string orgId, string desc,
            string corporativo, string empresaId)
        {
            Z190_Bitacora bitacora = new();
            bitacora.UserId = userId;
            bitacora.OrgId = orgId;
            bitacora.Desc = desc;
            bitacora.Corporativo = corporativo;
            bitacora.EmpresaId = empresaId;

            return bitacora;
        }
        public Z192_Logs MakeLog(string userId, string orgId, string desc,
            string corporativo, string empresaId)
        {
            Z192_Logs bitalog = new();
            bitalog.UserId = userId;
            bitalog.OrgId = orgId;
            bitalog.Desc = desc;
            bitalog.Corporativo = corporativo;
            bitalog.EmpresaId = empresaId;

            return bitalog;
        }

        public static string GeneraEtiquetaEstados(string tipo)
        {
            List<string> lista = new List<string>();
            string resp = "El ESTADO de ";
            if (tipo == "Factura")
            {
                lista.AddRange(Constantes.FacturaEstado.Split(","));
                resp = "las FACTURAS: ";
            }
            else if (tipo == "Pago")
            {
                lista.AddRange(Constantes.PagoEstado.Split(","));
                resp = "los PAGOS: ";
            }
            else if (tipo == "Folio")
            {
                lista.AddRange(Constantes.FolioEstado.Split(","));
                resp = "los FOLIOS: ";
            }

            for (var i = 0; i < lista.Count; i++)
            {
                resp += lista.ElementAt(i).Substring(0, 1) +
                    " - " + lista.ElementAt(i) + ". ";
            }
            return resp;
        }

        public static string DiaTitulo(int dia, int completo = 0)
        {
            string valores = "Dom,Lun,Mar,Mie,Jue,Vie,Sab,Domingo,Lunes,Martes,Miercoles,Jueves,Viernes,Sabado";
            var arr = valores.Split(",");
            completo = completo > 1 ? 0 : completo * 7;

            return arr[(dia - 1 + completo)];
        }
        public static string MesTitulo(int mes, int completo = 0)
        {
            string valores = "Ene,Feb,Mar,Abr,May,Jun,Jul,Ago,Sep,Oct,Nov,Dic,Enero,Febrero,Marzo,Abril,Mayo,Junio,Julio,Agosto,Septiembre,Octubre,Noviembre,Diciembre";
            var arr = valores.Split(",");
            completo = completo > 1 ? 0 : completo * 12;

            return arr[(mes - 1 + completo)];
        }
        public static int Ejercicio(DateTime fecha)
        {
            int year = fecha.Year;
            return year < 2000 ? year - 1900 : year - 2000;
        }
        public static string LaHora(DateTime lahora, string formato)
        {
            switch (formato)
            {
                case "M":
                    string cero = lahora.Minute < 10 ? "0" : "";
                    return $"{lahora.Hour}:{cero}{lahora.Minute}";

                default:
                    string mincero = lahora.Minute < 10 ? "0" : "";
                    string segcero = lahora.Second < 10 ? "0" : "";
                    return $"{lahora.Hour}:{mincero}{lahora.Minute}:{segcero}{lahora.Second}";
            }
        }
        public static string FormatoFecha(string formato, DateTime lafecha)
        {
            string resultado = string.Empty;

            switch (formato)
            {
                case "DD/MMM/AA":
                    resultado = $"{lafecha.Day}/";
                    resultado += $"{MesTitulo(lafecha.Month, 0)}/";
                    resultado += $"{Ejercicio(lafecha)}";
                    break;

            }
            return resultado;
        }
        public static string FormatoRFC(string rfc)
        {
            if (rfc == null) return string.Empty;
            int i = rfc.Length == 13 ? 1 : 0;

            return rfc.Substring(0, 3 + i) + "-" +
                rfc.Substring(3 + i, 6) + "-" + rfc.Substring(9 + i, 3);

        }
        public static int DameRandom(int inicio, int final)
        {
            Random rnd = new Random();
            return rnd.Next(inicio, final);
        }
    }
}

