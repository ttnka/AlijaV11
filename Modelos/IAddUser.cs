using System;
namespace DashBoard.Modelos
{
	public interface IAddUser
	{
        Task<ApiRespuesta<AddUser>> FirmaIn(AddUser addUsuario);
        Task<ApiRespuesta<AddUser>> InsertNewUser(AddUser NewUser);
    }
}

