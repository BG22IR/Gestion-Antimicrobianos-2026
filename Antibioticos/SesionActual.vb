Public Class SesionActual
    Public Shared Property IdUsuario As Integer = 0
    Public Shared Property Usuario As String = ""
    Public Shared Property NombreCompleto As String = ""
    Public Shared Property Rol As String = "" ' "ADMIN" o "USUARIO"

    Public Shared Function EsAdmin() As Boolean
        Return Rol.Trim().ToUpper() = "ADMIN"
    End Function

    Public Shared Sub CerrarSesion()
        IdUsuario = 0
        Usuario = ""
        NombreCompleto = ""
        Rol = ""
    End Sub
End Class