Imports System.Data.SQLite

Public Class FormLogin
    Private cadenaConexion As String = "Data Source=BaseDatosADN.db;Version=3;"

    Private txtUsuario As New TextBox()
    Private txtPassword As New TextBox()
    Private WithEvents btnIngresar As New Button()
    Private WithEvents btnSalir As New Button()
    Private lblError As New Label()

    Private Sub FormLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Acceso al Sistema"
        Me.Size = New Size(420, 480)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Drawing.Color.White

        Dim pnlCard As New Panel With {
            .Size = New Size(350, 390),
            .Location = New Point(28, 25),
            .BackColor = Drawing.Color.FromArgb(248, 250, 252)
        }
        Me.Controls.Add(pnlCard)

        Dim lblIcono As New Label With {
            .Text = "🔐",
            .Font = New Font("Segoe UI", 32.0F),
            .Size = New Size(350, 55),
            .Location = New Point(0, 15),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        Dim lblTitulo As New Label With {
            .Text = "Iniciar Sesión",
            .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(15, 23, 42),
            .Size = New Size(350, 30),
            .Location = New Point(0, 75),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        Dim lblSub As New Label With {
            .Text = "Ingresa tus credenciales para continuar",
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Regular),
            .ForeColor = Drawing.Color.FromArgb(100, 116, 139),
            .Size = New Size(350, 20),
            .Location = New Point(0, 105),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        Dim lblUser As New Label With {.Text = "Usuario:", .Location = New Point(30, 135), .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .AutoSize = True}
        txtUsuario.Location = New Point(30, 158)
        txtUsuario.Size = New Size(290, 30)
        txtUsuario.Font = New Font("Segoe UI", 11.0F)

        Dim lblPass As New Label With {.Text = "Contraseña:", .Location = New Point(30, 200), .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .AutoSize = True}
        txtPassword.Location = New Point(30, 223)
        txtPassword.Size = New Size(290, 30)
        txtPassword.Font = New Font("Segoe UI", 11.0F)
        txtPassword.UseSystemPasswordChar = True

        lblError.Location = New Point(30, 258)
        lblError.Size = New Size(290, 20)
        lblError.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        lblError.ForeColor = Drawing.Color.FromArgb(220, 38, 38)
        lblError.TextAlign = ContentAlignment.MiddleCenter
        lblError.Visible = False

        btnIngresar.Text = "Entrar al Sistema"
        btnIngresar.Location = New Point(30, 285)
        btnIngresar.Size = New Size(290, 42)
        btnIngresar.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnIngresar.ForeColor = Drawing.Color.White
        btnIngresar.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        btnIngresar.FlatStyle = FlatStyle.Flat
        btnIngresar.FlatAppearance.BorderSize = 0
        btnIngresar.Cursor = Cursors.Hand

        btnSalir.Text = "Cancelar"
        btnSalir.Location = New Point(30, 335)
        btnSalir.Size = New Size(290, 32)
        btnSalir.BackColor = Drawing.Color.Transparent
        btnSalir.ForeColor = Drawing.Color.FromArgb(100, 116, 139)
        btnSalir.Font = New Font("Segoe UI", 9.5F)
        btnSalir.FlatStyle = FlatStyle.Flat
        btnSalir.FlatAppearance.BorderSize = 0
        btnSalir.Cursor = Cursors.Hand

        pnlCard.Controls.Add(lblIcono)
        pnlCard.Controls.Add(lblTitulo)
        pnlCard.Controls.Add(lblSub)
        pnlCard.Controls.Add(lblUser)
        pnlCard.Controls.Add(txtUsuario)
        pnlCard.Controls.Add(lblPass)
        pnlCard.Controls.Add(txtPassword)
        pnlCard.Controls.Add(lblError)
        pnlCard.Controls.Add(btnIngresar)
        pnlCard.Controls.Add(btnSalir)

        Me.AcceptButton = btnIngresar
    End Sub

    Private Sub btnIngresar_Click(sender As Object, e As EventArgs) Handles btnIngresar.Click
        Dim user As String = txtUsuario.Text.Trim()
        Dim pass As String = txtPassword.Text.Trim()

        If user = "" OrElse pass = "" Then
            lblError.Text = "Ingresa usuario y contraseña"
            lblError.Visible = True
            Return
        End If

        Using con As New SQLiteConnection(cadenaConexion)
            con.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Usuarios WHERE UPPER(Usuario) = @u AND Password = @p", con)
            cmd.Parameters.AddWithValue("@u", user.ToUpper())
            cmd.Parameters.AddWithValue("@p", pass)

            Using lector As SQLiteDataReader = cmd.ExecuteReader()
                If lector.Read() Then
                    SesionActual.IdUsuario = Convert.ToInt32(lector("Id"))
                    SesionActual.Usuario = lector("Usuario").ToString()
                    SesionActual.NombreCompleto = lector("Nombre").ToString()
                    SesionActual.Rol = lector("Rol").ToString().ToUpper()

                    Me.DialogResult = DialogResult.OK
                    Me.Close()
                Else
                    lblError.Text = "Credenciales incorrectas"
                    lblError.Visible = True
                    txtPassword.Clear()
                    txtPassword.Focus()
                End If
            End Using
        End Using
    End Sub

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub
End Class