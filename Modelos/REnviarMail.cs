    using System;
using DashBoard.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using MimeKit;

namespace DashBoard.Modelos
{
    public class REnviarMail : IEnviarMail
    {
        public async Task<ApiRespuesta<MailCampos>> EnviarMail(MailCampos mailCampos)
        {
            ApiRespuesta<MailCampos> respuesta = new();

            #region EVALUAR Info de envio y cuentas
            if (mailCampos == null)
            {
                respuesta.Exito = false;
                respuesta.MsnError.Add("No hay datos para enviar mail!");
                return respuesta;
            }
            if (string.IsNullOrEmpty(mailCampos.SenderEmail))
                respuesta.MsnError.Add("No hay direccion de envio Sender");


            if (string.IsNullOrEmpty(mailCampos.Titulo))
                respuesta.MsnError.Add("No hay titulo del mail!");

            if (string.IsNullOrEmpty(mailCampos.Cuerpo))
                respuesta.MsnError.Add("No hay cuerpo del mail");

            if (respuesta.MsnError.Count > 0)
            {
                string texto = "";
                foreach (var me in respuesta.MsnError)
                {
                    texto += me.ToString() + " ";
                }
                return respuesta;
            }

            #endregion
            #region Evaluar si es correo de pruebas

            if (mailCampos.ParaEmail.EndsWith(".com1") ||
                mailCampos.ParaEmail == Constantes.DeMail01)
            {
                respuesta.Exito = true;
                respuesta.MsnError.Add("Email de prueba exitos!");

                return respuesta;
            }
            #endregion

            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(Constantes.DeNombreMail01, Constantes.DeMail01));
                message.To.Add(new MailboxAddress(mailCampos.ParaNombre, mailCampos.ParaEmail));
                message.Subject = mailCampos.Titulo;

                message.Body = new TextPart("html")
                {
                    Text = mailCampos.Cuerpo
                };



                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(mailCampos.Server, mailCampos.Port, false);
                    await client.AuthenticateAsync(mailCampos.UserName, mailCampos.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }


                respuesta.Exito = true;
                respuesta.Data = mailCampos;

            }
            catch (Exception ex)
            {
                respuesta.MsnError.Add(ex.Message);
                respuesta.Exito = false;
                string text = $"Hubo un error al enviar MAIL {ex} Para {mailCampos.ParaNombre} {mailCampos.ParaEmail} ";
                respuesta.MsnError.Add(text);

            }
            return respuesta;
            
        }
        

    }
}

