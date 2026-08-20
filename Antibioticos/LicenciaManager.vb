Imports System.IO
Imports System.Management
Imports System.Security.Cryptography
Imports System.Text

Public Class LicenciaManager

    ' =========================================================================
    ' CLAVE SECRETA MAESTRA (Firma Criptográfica)
    ' =========================================================================
    Private Const MasterSecretKey As String = "ADN_FARMACIAS_SECRET_KEY_@2026#MX_SECURE"

    Private Shared ReadOnly RutaArchivoLic As String = Path.Combine(Application.StartupPath, "licencia.lic")

    ' Propiedades de la licencia activa
    Public Shared Property HardwareIDActual As String = ""
    Public Shared Property TipoLicencia As String = ""
    Public Shared Property FechaVencimiento As String = ""

    ''' <summary>
    ''' Extrae el Hardware ID corto de 8 caracteres (CPU + Motherboard)
    ''' Formato: ADN-XXXX-XXXX (ej: ADN-946D-37E1)
    ''' </summary>
    Public Shared Function ObtenerHardwareID() As String
        Dim idHardware As String = ""
        Try
            Dim mbs As New ManagementObjectSearcher("Select ProcessorId From Win32_processor")
            For Each mo As ManagementObject In mbs.Get()
                If mo("ProcessorId") IsNot Nothing Then
                    idHardware &= mo("ProcessorId").ToString().Trim()
                    Exit For
                End If
            Next

            Dim mbsBoard As New ManagementObjectSearcher("Select SerialNumber From Win32_BaseBoard")
            For Each mo As ManagementObject In mbsBoard.Get()
                If mo("SerialNumber") IsNot Nothing Then
                    idHardware &= "-" & mo("SerialNumber").ToString().Trim()
                    Exit For
                End If
            Next
        Catch ex As Exception
            idHardware = Environment.MachineName & "-" & Environment.UserName
        End Try

        If String.IsNullOrWhiteSpace(idHardware) Then
            idHardware = Environment.MachineName & "-DEFAULT-ADN"
        End If

        Using sha As SHA256 = SHA256.Create()
            Dim hashBytes() As Byte = sha.ComputeHash(Encoding.UTF8.GetBytes(idHardware))
            Dim hashStr As String = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 8).ToUpper()
            HardwareIDActual = "ADN-" & hashStr.Substring(0, 4) & "-" & hashStr.Substring(4, 4)
            Return HardwareIDActual
        End Using
    End Function

    ''' <summary>
    ''' Valida la licencia registrada en disco
    ''' </summary>
    Public Shared Function ValidarLicenciaActual(ByRef mensajeError As String) As Boolean
        If Not File.Exists(RutaArchivoLic) Then
            mensajeError = "No se encontró ninguna licencia registrada en este equipo."
            Return False
        End If

        Dim serialGuardado As String = File.ReadAllText(RutaArchivoLic).Trim()
        Return ValidarSerial(serialGuardado, mensajeError)
    End Function

    ''' <summary>
    ''' Valida el Serial corto (ADN-XXXX-XXXX-YYMMDD-CCCCCCCC)
    ''' </summary>
    Public Shared Function ValidarSerial(serialKey As String, ByRef mensajeError As String) As Boolean
        Try
            Dim clean As String = serialKey.Replace(" ", "").Replace("-", "").Trim().ToUpper()

            ' Estructura: ADN (3) + HWID (8) + FECHA (6) + CHECKSUM (8) = 25 caracteres
            If Not clean.StartsWith("ADN") OrElse clean.Length <> 25 Then
                mensajeError = "El número de serie es inválido o está incompleto."
                Return False
            End If

            Dim hwidInKey As String = clean.Substring(3, 8)
            Dim expInKey As String = clean.Substring(11, 6)
            Dim sigInKey As String = clean.Substring(17, 8)

            ' 1. Verificar firma HMAC-SHA256
            Dim payload As String = hwidInKey & "-" & expInKey
            Dim sigBytes() As Byte
            Using hmac As New HMACSHA256(Encoding.UTF8.GetBytes(MasterSecretKey))
                sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))
            End Using

            Dim sigEsperada As String = BitConverter.ToString(sigBytes).Replace("-", "").Substring(0, 8).ToUpper()
            If sigInKey <> sigEsperada Then
                mensajeError = "La firma del número de serie no es auténtica o fue alterada."
                Return False
            End If

            ' 2. Verificar que pertenezca a esta computadora
            Dim hwidActual As String = ObtenerHardwareID().Replace("-", "").Replace("ADN", "").Trim().ToUpper()
            If hwidInKey <> hwidActual Then
                mensajeError = "Este número de serie fue generado para otra computadora."
                Return False
            End If

            ' 3. Comprobar vigencia
            If expInKey = "999999" Then
                TipoLicencia = "VITALICIA"
                FechaVencimiento = "PERPETUA (Sin límite)"
            Else
                TipoLicencia = "SUSCRIPCIÓN"
                Dim fechaLimite As Date = Date.ParseExact("20" & expInKey, "yyyyMMdd", Nothing)
                FechaVencimiento = fechaLimite.ToString("dd/MM/yyyy")

                If DateTime.Now.Date > fechaLimite Then
                    mensajeError = "Tu suscripción expiró el " & FechaVencimiento & ". Por favor renueva tu clave."
                    Return False
                End If
            End If

            mensajeError = ""
            Return True

        Catch ex As Exception
            mensajeError = "Error al procesar el serial: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Sub GuardarLicencia(serialKey As String)
        File.WriteAllText(RutaArchivoLic, serialKey.Trim().ToUpper())
    End Sub

End Class