Imports System.Drawing.Drawing2D

Public Class FormActivacion

    Private txtHwid As New TextBox()
    Private WithEvents btnCopiarHwid As New Button()
    Private txtSerialKey As New TextBox()
    Private WithEvents btnPegarYActivar As New Button()
    Private WithEvents btnActivar As New Button()
    Private WithEvents btnSalir As New Button()

    Private Sub FormActivacion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Activación de Software - Farmacias ADN"
        Me.Size = New Size(540, 420)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Drawing.Color.White

        Dim Y As Integer = 20

        Dim lblTit As New Label With {
            .Text = "🔐 Activación de Licencia",
            .Font = New Font("Segoe UI", 15, FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(0, 102, 204),
            .Location = New Point(30, Y),
            .AutoSize = True
        }
        Me.Controls.Add(lblTit)
        Y += 38

        Dim lblDesc As New Label With {
            .Text = "1. Envía tu Código de Equipo a tu proveedor." & vbCrLf & "2. Cuando te entregue tu Clave de Serie, cópiala y presiona 'Pegar y Activar'.",
            .Font = New Font("Segoe UI", 9.5F),
            .ForeColor = Drawing.Color.FromArgb(90, 90, 90),
            .Location = New Point(30, Y),
            .Size = New Size(465, 38)
        }
        Me.Controls.Add(lblDesc)
        Y += 48

        ' Hardware ID
        Dim lblHwid As New Label With {.Text = "Tu Código de Equipo (Hardware ID):", .Location = New Point(30, Y), .AutoSize = True, .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
        Me.Controls.Add(lblHwid)
        Y += 22

        txtHwid.Text = LicenciaManager.ObtenerHardwareID()
        txtHwid.ReadOnly = True
        txtHwid.Font = New Font("Consolas", 12, FontStyle.Bold)
        txtHwid.BackColor = Drawing.Color.FromArgb(240, 245, 250)
        txtHwid.Location = New Point(30, Y)
        txtHwid.Size = New Size(325, 30)
        Me.Controls.Add(txtHwid)

        btnCopiarHwid.Text = "📋 Copiar ID"
        btnCopiarHwid.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btnCopiarHwid.Location = New Point(365, Y - 1)
        btnCopiarHwid.Size = New Size(130, 32)
        btnCopiarHwid.BackColor = Drawing.Color.FromArgb(230, 230, 230)
        btnCopiarHwid.FlatStyle = FlatStyle.Flat
        btnCopiarHwid.FlatAppearance.BorderSize = 0
        btnCopiarHwid.Cursor = Cursors.Hand
        Me.Controls.Add(btnCopiarHwid)
        Y += 48

        ' Caja de Número de Serie
        Dim lblKey As New Label With {.Text = "Número de Serie / Clave de Activación:", .Location = New Point(30, Y), .AutoSize = True, .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)}
        Me.Controls.Add(lblKey)
        Y += 22

        txtSerialKey.Font = New Font("Consolas", 11, FontStyle.Bold)
        txtSerialKey.Location = New Point(30, Y)
        txtSerialKey.Size = New Size(465, 30)
        txtSerialKey.TextAlign = HorizontalAlignment.Center
        Me.Controls.Add(txtSerialKey)
        Y += 45

        ' Botón Pegar y Activar en 1 clic
        btnPegarYActivar.Text = "📥 Pegar desde Portapapeles y Activar"
        btnPegarYActivar.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        btnPegarYActivar.BackColor = Drawing.Color.FromArgb(40, 167, 69)
        btnPegarYActivar.ForeColor = Drawing.Color.White
        btnPegarYActivar.FlatStyle = FlatStyle.Flat
        btnPegarYActivar.FlatAppearance.BorderSize = 0
        btnPegarYActivar.Cursor = Cursors.Hand
        btnPegarYActivar.Location = New Point(30, Y)
        btnPegarYActivar.Size = New Size(465, 42)
        Me.Controls.Add(btnPegarYActivar)
        Y += 50

        ' Botón Activar manual
        btnActivar.Text = "✔ Activar"
        btnActivar.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btnActivar.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnActivar.ForeColor = Drawing.Color.White
        btnActivar.FlatStyle = FlatStyle.Flat
        btnActivar.FlatAppearance.BorderSize = 0
        btnActivar.Cursor = Cursors.Hand
        btnActivar.Location = New Point(30, Y)
        btnActivar.Size = New Size(325, 38)
        Me.Controls.Add(btnActivar)

        btnSalir.Text = "Cancelar"
        btnSalir.Font = New Font("Segoe UI", 9.5F)
        btnSalir.BackColor = Drawing.Color.FromArgb(240, 240, 240)
        btnSalir.FlatStyle = FlatStyle.Flat
        btnSalir.FlatAppearance.BorderSize = 0
        btnSalir.Location = New Point(365, Y)
        btnSalir.Size = New Size(130, 38)
        Me.Controls.Add(btnSalir)
    End Sub

    Private Sub btnCopiarHwid_Click(sender As Object, e As EventArgs) Handles btnCopiarHwid.Click
        Clipboard.SetText(txtHwid.Text)
        MessageBox.Show("Código de equipo copiado. Ya puedes pegarlo y enviarlo por WhatsApp o Correo.", "Copiado", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnPegarYActivar_Click(sender As Object, e As EventArgs) Handles btnPegarYActivar.Click
        If Clipboard.ContainsText() Then
            txtSerialKey.Text = Clipboard.GetText().Trim()
            ProcesarActivacion()
        Else
            MessageBox.Show("El portapapeles está vacío. Copia primero la clave que te entregaron.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub btnActivar_Click(sender As Object, e As EventArgs) Handles btnActivar.Click
        ProcesarActivacion()
    End Sub

    Private Sub ProcesarActivacion()
        Dim clave As String = txtSerialKey.Text.Trim()
        If clave = "" Then
            MessageBox.Show("Por favor ingresa o pega el número de serie.", "Campo Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim errorMsg As String = ""
        If LicenciaManager.ValidarSerial(clave, errorMsg) Then
            LicenciaManager.GuardarLicencia(clave)
            MessageBox.Show("¡Software activado exitosamente!" & vbCrLf & vbCrLf &
                            "Tipo: " & LicenciaManager.TipoLicencia & vbCrLf &
                            "Vigencia: " & LicenciaManager.FechaVencimiento,
                            "Activación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            MessageBox.Show("No se pudo activar:" & vbCrLf & errorMsg, "Error de Activación", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class