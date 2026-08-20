Imports System.Data.SQLite
Imports System.Drawing.Drawing2D

Public Class FormUsuarios
    Private cadenaConexion As String = "Data Source=BaseDatosADN.db;Version=3;"

    Private dgvUsuarios As New DataGridView()
    Private txtUsuario As New TextBox()
    Private txtPassword As New TextBox()
    Private txtNombre As New TextBox()
    Private cmbRol As New ComboBox()

    Private WithEvents btnGuardar As New Button()
    Private WithEvents btnNuevo As New Button()
    Private WithEvents btnEliminar As New Button()
    Private idUsuarioSeleccionado As Integer = 0

    Private Sub FormUsuarios_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Control de Usuarios y Permisos - Farmacias ADN"
        Me.Size = New Size(780, 520)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Drawing.Color.White

        ConfigurarInterfaz()
        CargarListaUsuarios()
    End Sub

    Private Sub ConfigurarInterfaz()
        Dim lblTitulo As New Label With {
            .Text = "👥 Administración de Usuarios y Roles",
            .Font = New Font("Segoe UI", 14.0F, FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(15, 23, 42),
            .Location = New Point(25, 15),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitulo)

        ' Panel de captura (Izquierda)
        Dim pnlForm As New Panel With {
            .Location = New Point(25, 55),
            .Size = New Size(280, 400),
            .BackColor = Drawing.Color.FromArgb(248, 250, 252)
        }
        RedondearPanelBorde(pnlForm, 10, Drawing.Color.FromArgb(203, 213, 225), 1.2F)
        Me.Controls.Add(pnlForm)

        Dim lblNom As New Label With {.Text = "Nombre Completo:", .Location = New Point(15, 15), .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold), .AutoSize = True}
        txtNombre.Location = New Point(15, 35)
        txtNombre.Size = New Size(250, 26)
        txtNombre.Font = New Font("Segoe UI", 10.0F)

        Dim lblUser As New Label With {.Text = "Nombre de Usuario:", .Location = New Point(15, 75), .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold), .AutoSize = True}
        txtUsuario.Location = New Point(15, 95)
        txtUsuario.Size = New Size(250, 26)
        txtUsuario.Font = New Font("Segoe UI", 10.0F)

        Dim lblPass As New Label With {.Text = "Contraseña:", .Location = New Point(15, 135), .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold), .AutoSize = True}
        txtPassword.Location = New Point(15, 155)
        txtPassword.Size = New Size(250, 26)
        txtPassword.Font = New Font("Segoe UI", 10.0F)

        Dim lblRol As New Label With {.Text = "Rol / Nivel de Acceso:", .Location = New Point(15, 195), .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold), .AutoSize = True}
        cmbRol.Location = New Point(15, 215)
        cmbRol.Size = New Size(250, 26)
        cmbRol.Font = New Font("Segoe UI", 10.0F)
        cmbRol.DropDownStyle = ComboBoxStyle.DropDownList
        cmbRol.Items.AddRange(New String() {"ADMIN", "USUARIO"})
        cmbRol.SelectedIndex = 1

        btnGuardar.Text = "💾 Guardar Usuario"
        btnGuardar.Location = New Point(15, 270)
        btnGuardar.Size = New Size(250, 36)
        btnGuardar.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnGuardar.ForeColor = Drawing.Color.White
        btnGuardar.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        btnGuardar.FlatStyle = FlatStyle.Flat
        btnGuardar.FlatAppearance.BorderSize = 0
        btnGuardar.Cursor = Cursors.Hand

        btnNuevo.Text = "➕ Nuevo / Limpiar"
        btnNuevo.Location = New Point(15, 312)
        btnNuevo.Size = New Size(120, 32)
        btnNuevo.BackColor = Drawing.Color.FromArgb(226, 232, 240)
        btnNuevo.ForeColor = Drawing.Color.FromArgb(15, 23, 42)
        btnNuevo.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        btnNuevo.FlatStyle = FlatStyle.Flat
        btnNuevo.FlatAppearance.BorderSize = 0
        btnNuevo.Cursor = Cursors.Hand

        btnEliminar.Text = "🗑 Eliminar"
        btnEliminar.Location = New Point(145, 312)
        btnEliminar.Size = New Size(120, 32)
        btnEliminar.BackColor = Drawing.Color.FromArgb(254, 226, 226)
        btnEliminar.ForeColor = Drawing.Color.FromArgb(185, 28, 28)
        btnEliminar.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        btnEliminar.FlatStyle = FlatStyle.Flat
        btnEliminar.FlatAppearance.BorderSize = 0
        btnEliminar.Cursor = Cursors.Hand

        pnlForm.Controls.Add(lblNom)
        pnlForm.Controls.Add(txtNombre)
        pnlForm.Controls.Add(lblUser)
        pnlForm.Controls.Add(txtUsuario)
        pnlForm.Controls.Add(lblPass)
        pnlForm.Controls.Add(txtPassword)
        pnlForm.Controls.Add(lblRol)
        pnlForm.Controls.Add(cmbRol)
        pnlForm.Controls.Add(btnGuardar)
        pnlForm.Controls.Add(btnNuevo)
        pnlForm.Controls.Add(btnEliminar)

        ' Tabla de Usuarios (Derecha)
        dgvUsuarios.Location = New Point(320, 55)
        dgvUsuarios.Size = New Size(420, 400)
        dgvUsuarios.BackgroundColor = Drawing.Color.White
        dgvUsuarios.BorderStyle = BorderStyle.None
        dgvUsuarios.RowHeadersVisible = False
        dgvUsuarios.AllowUserToAddRows = False
        dgvUsuarios.AllowUserToDeleteRows = False
        dgvUsuarios.ReadOnly = True
        dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvUsuarios.EnableHeadersVisualStyles = False
        dgvUsuarios.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = Drawing.Color.FromArgb(15, 23, 42)
        dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
        dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        dgvUsuarios.ColumnHeadersHeight = 40
        dgvUsuarios.DefaultCellStyle.Font = New Font("Segoe UI", 9.5F)
        dgvUsuarios.RowTemplate.Height = 36
        dgvUsuarios.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        dgvUsuarios.GridColor = Drawing.Color.FromArgb(203, 213, 225)
        dgvUsuarios.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(241, 245, 249)
        AddHandler dgvUsuarios.CellClick, AddressOf DgvUsuarios_CellClick
        Me.Controls.Add(dgvUsuarios)
    End Sub

    Private Sub CargarListaUsuarios()
        Dim dt As New DataTable()
        Using con As New SQLiteConnection(cadenaConexion)
            con.Open()
            Dim cmd As New SQLiteCommand("SELECT Id, Nombre, Usuario, Password, Rol FROM Usuarios ORDER BY Id ASC", con)
            Using da As New SQLiteDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using

        dgvUsuarios.DataSource = dt
        dgvUsuarios.Columns("Id").Visible = False
        dgvUsuarios.Columns("Password").Visible = False
        dgvUsuarios.Columns("Nombre").FillWeight = 40
        dgvUsuarios.Columns("Usuario").FillWeight = 30
        dgvUsuarios.Columns("Rol").FillWeight = 30
    End Sub

    Private Sub DgvUsuarios_CellClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex >= 0 Then
            Dim row As DataGridViewRow = dgvUsuarios.Rows(e.RowIndex)
            idUsuarioSeleccionado = Convert.ToInt32(row.Cells("Id").Value)
            txtNombre.Text = row.Cells("Nombre").Value.ToString()
            txtUsuario.Text = row.Cells("Usuario").Value.ToString()
            txtPassword.Text = row.Cells("Password").Value.ToString()
            cmbRol.SelectedItem = row.Cells("Rol").Value.ToString().ToUpper()
            btnGuardar.Text = "🔄 Actualizar Usuario"
        End If
    End Sub

    Private Sub btnNuevo_Click(sender As Object, e As EventArgs) Handles btnNuevo.Click
        LimpiarCampos()
    End Sub

    Private Sub LimpiarCampos()
        idUsuarioSeleccionado = 0
        txtNombre.Clear()
        txtUsuario.Clear()
        txtPassword.Clear()
        cmbRol.SelectedIndex = 1
        btnGuardar.Text = "💾 Guardar Usuario"
        txtNombre.Focus()
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        Dim nom As String = txtNombre.Text.Trim()
        Dim usr As String = txtUsuario.Text.Trim()
        Dim pass As String = txtPassword.Text.Trim()
        Dim rol As String = cmbRol.SelectedItem.ToString()

        If nom = "" OrElse usr = "" OrElse pass = "" Then
            MessageBox.Show("Por favor completa todos los campos.", "Campos vacíos", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using con As New SQLiteConnection(cadenaConexion)
            con.Open()

            If idUsuarioSeleccionado = 0 Then
                ' Crear nuevo usuario
                Dim cmdCheck As New SQLiteCommand("SELECT COUNT(*) FROM Usuarios WHERE UPPER(Usuario) = @u", con)
                cmdCheck.Parameters.AddWithValue("@u", usr.ToUpper())
                If Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0 Then
                    MessageBox.Show("El nombre de usuario ya está registrado.", "Usuario Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                Dim cmdIns As New SQLiteCommand("INSERT INTO Usuarios (Nombre, Usuario, Password, Rol) VALUES (@n, @u, @p, @r)", con)
                cmdIns.Parameters.AddWithValue("@n", nom)
                cmdIns.Parameters.AddWithValue("@u", usr)
                cmdIns.Parameters.AddWithValue("@p", pass)
                cmdIns.Parameters.AddWithValue("@r", rol)
                cmdIns.ExecuteNonQuery()
                MessageBox.Show("Usuario registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                ' Actualizar existente
                Dim cmdUpd As New SQLiteCommand("UPDATE Usuarios SET Nombre = @n, Usuario = @u, Password = @p, Rol = @r WHERE Id = @id", con)
                cmdUpd.Parameters.AddWithValue("@n", nom)
                cmdUpd.Parameters.AddWithValue("@u", usr)
                cmdUpd.Parameters.AddWithValue("@p", pass)
                cmdUpd.Parameters.AddWithValue("@r", rol)
                cmdUpd.Parameters.AddWithValue("@id", idUsuarioSeleccionado)
                cmdUpd.ExecuteNonQuery()
                MessageBox.Show("Usuario actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Using

        CargarListaUsuarios()
        LimpiarCampos()
    End Sub

    Private Sub btnEliminar_Click(sender As Object, e As EventArgs) Handles btnEliminar.Click
        If idUsuarioSeleccionado = 0 Then
            MessageBox.Show("Selecciona un usuario de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If idUsuarioSeleccionado = 1 Then
            MessageBox.Show("No es posible eliminar al Administrador Principal del sistema.", "Acción Denegada", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return
        End If

        If idUsuarioSeleccionado = SesionActual.IdUsuario Then
            MessageBox.Show("No puedes eliminar la cuenta con la que tienes sesión activa.", "Acción Denegada", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return
        End If

        If MessageBox.Show("¿Seguro que deseas eliminar a este usuario?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Using con As New SQLiteConnection(cadenaConexion)
                con.Open()
                Dim cmd As New SQLiteCommand("DELETE FROM Usuarios WHERE Id = @id", con)
                cmd.Parameters.AddWithValue("@id", idUsuarioSeleccionado)
                cmd.ExecuteNonQuery()
            End Using

            MessageBox.Show("Usuario eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CargarListaUsuarios()
            LimpiarCampos()
        End If
    End Sub

    Private Sub RedondearPanelBorde(pnl As Panel, radio As Integer, colorBorde As Drawing.Color, grosor As Single)
        AddHandler pnl.Paint, Sub(s, e)
                                  Dim g As Graphics = e.Graphics
                                  g.SmoothingMode = SmoothingMode.AntiAlias
                                  Using path As New GraphicsPath()
                                      Dim d As Integer = radio * 2
                                      Dim r As New Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1)
                                      path.AddArc(r.X, r.Y, d, d, 180, 90)
                                      path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
                                      path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
                                      path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
                                      path.CloseFigure()
                                      Using pen As New Pen(colorBorde, grosor)
                                          g.DrawPath(pen, path)
                                      End Using
                                  End Using
                              End Sub
    End Sub
End Class