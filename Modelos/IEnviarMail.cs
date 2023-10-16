using System;
namespace DashBoard.Modelos
{
	public interface IEnviarMail
	{
        Task<ApiRespuesta<MailCampos>> EnviarMail(MailCampos mailCampos);
    }
}

