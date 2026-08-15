Imports System.Data.SQLite
Imports System.Drawing

Public Class FormEntrada

    ' Ruta de la base de datos (compartida con Form1)
    Dim cadenaConexion As String = "Data Source=BaseDatosADN.db;Version=3;"

    ' =========================================================
    ' 1. INICIALIZACIÓN
    ' =========================================================
    Private Sub FormEntrada_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AplicarEstiloFluent()

        ' Estado inicial de campos bloqueados
        txtRFC.ReadOnly = True
        txtDireccion.ReadOnly = True

        ActualizarProveedores()
        txtCodigo.Focus()
    End Sub

    Private Sub ActualizarProveedores()
        cmbProveedor.Items.Clear()
        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT Proveedor FROM Proveedores ORDER BY Proveedor"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        cmbProveedor.Items.Add(lector("Proveedor").ToString())
                    End While
                End Using
            End Using
        End Using
    End Sub

    ' =========================================================
    ' 2. LÓGICA DE BÚSQUEDA Y AUTORELLENO
    ' =========================================================

    ' Al escribir o seleccionar proveedor
    Private Sub cmbProveedor_TextChanged(sender As Object, e As EventArgs) Handles cmbProveedor.TextChanged
        If cmbProveedor.Text = "" Then Return

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT RFC, Direccion FROM Proveedores WHERE Proveedor = @prov"
            Using comando As New SQLiteCommand(consulta, conexion)
                comando.Parameters.AddWithValue("@prov", cmbProveedor.Text.ToUpper())
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    If lector.Read() Then
                        txtRFC.Text = lector("RFC").ToString()
                        txtDireccion.Text = lector("Direccion").ToString()
                        txtRFC.ReadOnly = True
                        txtDireccion.ReadOnly = True
                    Else
                        ' Si no existe, desbloqueamos para nuevo registro
                        txtRFC.ReadOnly = False
                        txtDireccion.ReadOnly = False
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' Buscar Medicamento (al salir del campo código)
    Private Sub txtCodigo_Leave(sender As Object, e As EventArgs) Handles txtCodigo.Leave
        If txtCodigo.Text = "" Then Return

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Inventario WHERE Codigo = @codigo"
            Using comando As New SQLiteCommand(consulta, conexion)
                comando.Parameters.AddWithValue("@codigo", txtCodigo.Text)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    If lector.Read() Then
                        txtGenerico.Text = lector("Generico").ToString()
                        txtDistintivo.Text = lector("Distintivo").ToString()
                        txtPresentacion.Text = lector("Presentacion").ToString()
                        txtAware.Text = lector("AWARE").ToString()
                        txtExistencia.Text = lector("ExistenciaActual").ToString()
                    Else
                        txtExistencia.Text = "0"
                        txtGenerico.Focus() ' Si es nuevo, pedimos que llene datos
                    End If
                End Using
            End Using
        End Using
    End Sub

    ' =========================================================
    ' 3. GUARDADO (Transacción SQL segura)
    ' =========================================================
    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        If txtCodigo.Text = "" Or cmbProveedor.Text = "" Then
            MessageBox.Show("Código y Proveedor son obligatorios.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        Dim stockAnt As Double = Val(txtExistencia.Text)
        Dim cantSur As Double = Val(txtSurtido.Text)
        Dim saldoFinal As Double = stockAnt + cantSur

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Using transaccion As SQLiteTransaction = conexion.BeginTransaction()
                Try
                    ' 1. Guardar/Actualizar Proveedor
                    Dim cmdProv As New SQLiteCommand("INSERT OR IGNORE INTO Proveedores (Proveedor, RFC, Direccion) VALUES (@prov, @rfc, @dir)", conexion, transaccion)
                    cmdProv.Parameters.AddWithValue("@prov", cmbProveedor.Text.ToUpper())
                    cmdProv.Parameters.AddWithValue("@rfc", txtRFC.Text.ToUpper())
                    cmdProv.Parameters.AddWithValue("@dir", txtDireccion.Text.ToUpper())
                    cmdProv.ExecuteNonQuery()

                    ' 2. Actualizar Inventario (Upsert)
                    Dim cmdInv As New SQLiteCommand("INSERT INTO Inventario (Codigo, Generico, Distintivo, Presentacion, AWARE, ExistenciaActual) VALUES (@codigo, @gen, @dis, @pres, @aware, @saldo) ON CONFLICT(Codigo) DO UPDATE SET ExistenciaActual = @saldo", conexion, transaccion)
                    cmdInv.Parameters.AddWithValue("@codigo", txtCodigo.Text)
                    cmdInv.Parameters.AddWithValue("@gen", txtGenerico.Text)
                    cmdInv.Parameters.AddWithValue("@dis", txtDistintivo.Text)
                    cmdInv.Parameters.AddWithValue("@pres", txtPresentacion.Text)
                    cmdInv.Parameters.AddWithValue("@aware", txtAware.Text)
                    cmdInv.Parameters.AddWithValue("@saldo", saldoFinal)
                    cmdInv.ExecuteNonQuery()

                    ' 3. Bitácora de Entrada
                    Dim cmdEnt As New SQLiteCommand("INSERT INTO Entradas (Fecha, Codigo, Generico, Distintivo, Presentacion, AWARE, Lote, Caducidad, Existencia, Surtido, Saldo, Factura, Proveedor, RFC, Direccion) VALUES (@fecha, @codigo, @gen, @dis, @pres, @aware, @lote, @caducidad, @existencia, @surtido, @saldo, @factura, @prov, @rfc, @dir)", conexion, transaccion)
                    cmdEnt.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    cmdEnt.Parameters.AddWithValue("@codigo", txtCodigo.Text)
                    cmdEnt.Parameters.AddWithValue("@gen", txtGenerico.Text)
                    cmdEnt.Parameters.AddWithValue("@dis", txtDistintivo.Text)
                    cmdEnt.Parameters.AddWithValue("@pres", txtPresentacion.Text)
                    cmdEnt.Parameters.AddWithValue("@aware", txtAware.Text)
                    cmdEnt.Parameters.AddWithValue("@lote", txtLote.Text)
                    cmdEnt.Parameters.AddWithValue("@caducidad", txtCaducidad.Text)
                    cmdEnt.Parameters.AddWithValue("@existencia", stockAnt)
                    cmdEnt.Parameters.AddWithValue("@surtido", cantSur)
                    cmdEnt.Parameters.AddWithValue("@saldo", saldoFinal)
                    cmdEnt.Parameters.AddWithValue("@factura", txtFactura.Text)
                    cmdEnt.Parameters.AddWithValue("@prov", cmbProveedor.Text.ToUpper())
                    cmdEnt.Parameters.AddWithValue("@rfc", txtRFC.Text.ToUpper())
                    cmdEnt.Parameters.AddWithValue("@dir", txtDireccion.Text.ToUpper())
                    cmdEnt.ExecuteNonQuery()

                    transaccion.Commit()
                    MessageBox.Show("Registro exitoso.", "Completado", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Me.Close()
                Catch ex As Exception
                    transaccion.Rollback()
                    MessageBox.Show("Error al guardar: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End Using
        End Using
    End Sub

    ' =========================================================
    ' 4. ESTILO FLUENT
    ' =========================================================
    Private Sub AplicarEstiloFluent()
        Me.BackColor = Drawing.Color.White
        Me.StartPosition = FormStartPosition.CenterScreen

        For Each control As Control In Me.Controls
            If TypeOf control Is Button Then
                Dim btn As Button = CType(control, Button)
                btn.FlatStyle = FlatStyle.Flat
                btn.FlatAppearance.BorderSize = 0
                btn.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
                btn.ForeColor = Drawing.Color.White
                btn.BackColor = Drawing.Color.FromArgb(0, 102, 204)
                btn.Cursor = Cursors.Hand
            End If
            If TypeOf control Is TextBox Or TypeOf control Is ComboBox Then
                control.Font = New Drawing.Font("Segoe UI", 10.0F)
            End If
        Next
    End Sub

    Private Sub btnNuevoProv_Click(sender As Object, e As EventArgs) Handles btnNuevoProv.Click
        cmbProveedor.Text = ""
        txtRFC.Text = ""
        txtDireccion.Text = ""
        txtRFC.ReadOnly = False
        txtDireccion.ReadOnly = False
    End Sub

End Class