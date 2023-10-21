﻿using System;
using DashBoard.Data;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using MimeKit;

namespace DashBoard.Modelos
{
    public class REnviarMail : IEnviarMail
    {
        private readonly ApplicationDbContext _appDbContext;
        public REnviarMail(ApplicationDbContext appDbContext)
        {
            _appDbContext = appDbContext;

        }
        [Inject]
        public Repo<Z190_Bitacora, ApplicationDbContext> bitacoraRepo { get; set; } = default!;
        [Inject]
        public Repo<Z192_Logs, ApplicationDbContext> logRepo { get; set; } = default!;

        public async Task<ApiRespuesta<MailCampos>> EnviarMail(MailCampos mailCampos)
        {
            ApiRespuesta<MailCampos> apiRespuesta = new()
            {
                Exito = false,
                MsnError = new List<string>(),
                Data = mailCampos
            };
            #region EVALUAR Info de envio y cuentas
            if (mailCampos == null)
            {
                apiRespuesta.Exito = false;
                apiRespuesta.MsnError.Add("No hay datos para enviar mail!");
                apiRespuesta.Data = mailCampos;
                await WriteBitacora("Sistema", "Sistema", "No hay datos para enviar mail",
                    "Sistema", "Sistema", true);
                return apiRespuesta;
            }
            if (string.IsNullOrEmpty(mailCampos.SenderEmail))
                apiRespuesta.MsnError.Add("No hay direccion de envio Sender");


            if (string.IsNullOrEmpty(mailCampos.Titulo))
                apiRespuesta.MsnError.Add("No hay titulo del mail!");

            if (string.IsNullOrEmpty(mailCampos.Cuerpo))
                apiRespuesta.MsnError.Add("No hay cuerpo del mail");

            if (apiRespuesta.MsnError.Count > 0)
            {
                string texto = "";
                foreach (var me in apiRespuesta.MsnError)
                {
                    texto += me.ToString() + " ";
                }
                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId, texto,
                    mailCampos.OrgId, mailCampos.OrgId, true);
                return apiRespuesta;
            }

            #endregion
            #region Evaluar si es correo de pruebas

            if (mailCampos.Para.EndsWith(".com1") ||
                mailCampos.Para == Constantes.DeMail01)
            {
                apiRespuesta.Exito = true;
                apiRespuesta.MsnError.Add("Email de prueba exitos!");
                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId,
                    "Se supespendio el envio de mail ya que es un correo de prueba!",
                    mailCampos.OrgId, mailCampos.OrgId, true);
                return apiRespuesta;
            }
            #endregion
            try
            {
                MimeMessage email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(mailCampos.SenderEmail));
                email.To.Add(MailboxAddress.Parse(mailCampos.Para));
                email.Subject = mailCampos.Titulo;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = mailCampos.Cuerpo };
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(mailCampos.Server, mailCampos.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(mailCampos.UserName, mailCampos.Password);
                smtp.Send(email);
                smtp.Disconnect(true);

                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId,
                    $"Se envio un Email a {mailCampos.Para} Titulo {mailCampos.Titulo}",
                    mailCampos.OrgId, mailCampos.OrgId, true);
                apiRespuesta.Exito = true;
                return apiRespuesta;

            }
            catch (Exception ex)
            {
                apiRespuesta.MsnError.Add(ex.Message);
                string text = $"Hubo un error al enviar MAIL {ex} Para {mailCampos.Para} ";
                text += $"Titulo {mailCampos.Titulo} ";
                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId, text,
                    mailCampos.OrgId, mailCampos.OrgId, true);

                return apiRespuesta;
            }

        }
        public MyFunc MyFunc { get; set; } = new MyFunc();
        protected async Task WriteBitacora(string userId, string orgId,
            string desc, string corp, string emp, bool sistema)
        {
            try
            {
                if (sistema)
                {
                    Z192_Logs logT = MyFunc.MakeLog(userId, orgId, desc, corp, orgId);
                    await logRepo.Insert(logT);
                }
                else
                {
                    Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(userId, orgId, desc,
                        corp, orgId);
                    await bitacoraRepo.Insert(bitaTemp);
                }
            }
            catch (Exception ex)
            {
                string eTxt = $"Se genero un error al intentar escribir en bitacora un nuevo usuario {ex}";
                string userT = userId ?? "Sin Usuario";
                string orgT = orgId ?? "Sin Organizacion";
                string corpT = corp ?? "Sin Corporativo";
                await WriteBitacora(userT, orgT, eTxt, corpT, orgT, true);
            }

        }

    }
}

