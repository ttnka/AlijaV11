Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.


/*
    async void AddOrg(Z100_Org org)
    {
        Editando = !Editando;
        org.Estado = 1;
        org.Rfc = org.Rfc.ToUpper();
        org.Moral = org.Rfc.Length == 13;

        var resultado = await Servicio("Insert", org);
        //bool sistema = false;
        string txt = $"{org.Rfc} {org.RazonSocial} " +
                    $"tipo: {org.Tipo}, Estado:{org.Estado} status:{org.Status}";
        string txt1 = "";
        if (resultado.Exito)
        {
            ShowNotification(ElMsn("Ok", "Nueva",
                        $"Estamos creado un nueva ORGANIZACION {org.Comercial}!!! ", 0));
            txt = $"{TBita}, Creo una nueva organizacion {org.Comercial} " + txt;
            ShowNotification(ElMsn("Ok", "Nuevo administrador",
                        $"Estamos creando un nuevo ADMINISTRADOR de {org.Comercial}!!!", 0));
            txt1 = $"{TBita}, Creo un nuevo administrador en Comercial:{org.Comercial} {org.NumCliente}";
        }
        else
        {
            ShowNotification(ElMsn("Error", "Error",
                        $"No pudo ser creada un nueva ORGANIZACION!!! {org.Comercial} {org.NumCliente}", 0));
            txt = $"{TBita}, NO se creo una nueva organizacion {org.Comercial} " + txt;
            sistema = true;
        }
        var bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId, txt, ElUser.OrgId, sistema);
        await BitacoraAll(bitaTemp);
        if (txt1.Length > 1)
        {
            bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId, txt1, ElUser.OrgId, sistema);
            await BitacoraAll(bitaTemp);
        }
    }
    */
