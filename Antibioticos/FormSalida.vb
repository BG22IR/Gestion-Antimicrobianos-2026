Imports System.Data.SQLite

Public Class FormSalida
    Dim cadenaConexion As String = "Data Source=BaseDatosADN.db;Version=3;"
    Private bloqueandoEventos As Boolean = False

    ' ==========================================
    ' 1. INICIALIZACIÓN
    ' ==========================================
    Private Sub FormSalida_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AplicarEstiloFluent()

        ' Configurar Movimientos
        cmbMovimiento.Items.Clear()
        cmbMovimiento.Items.AddRange(New String() {"Salida con Receta Retenida", "Salida con Receta NO retenida", "Destruccion", "Devolucion a Proveedor"})
        cmbMovimiento.SelectedIndex = 0

        ' Cargar catálogo de médicos
        CargarCatalogosMedicos()
        AlternarCamposMedico(False)
        txtCodigo.Focus()
    End Sub

    Private Sub CargarCatalogosMedicos()
        cmbCedula.Items.Clear()
        cmbNombreMed.Items.Clear()
        Using conexion As New SQLiteConnection(cadenaConexion)
            Try
                conexion.Open()
                Dim cmd As New SQLiteCommand("SELECT Cedula, NombreMed FROM Medicos", conexion)
                Using lector As SQLiteDataReader = cmd.ExecuteReader()
                    While lector.Read()
                        If Not IsDBNull(lector("Cedula")) Then cmbCedula.Items.Add(lector("Cedula").ToString())
                        If Not IsDBNull(lector("NombreMed")) Then cmbNombreMed.Items.Add(lector("NombreMed").ToString())
                    End While
                End Using
            Catch ex As Exception
            End Try
        End Using
    End Sub

    ' ==========================================
    ' 2. LÓGICA DE INTERFAZ
    ' ==========================================
    Private Sub cmbMovimiento_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbMovimiento.SelectedIndexChanged
        AlternarCamposMedico(cmbMovimiento.Text.Contains("Receta"))
    End Sub

    Sub AlternarCamposMedico(estado As Boolean)
        cmbCedula.Visible = estado
        cmbNombreMed.Visible = estado
        txtCalle.Visible = estado
        txtNoInt.Visible = estado
        txtNoExt.Visible = estado
        txtColonia.Visible = estado
        txtCiudad.Visible = estado
        txtEstado.Visible = estado
        txtCP.Visible = estado
        txtPais.Visible = estado
        txtTel.Visible = estado
    End Sub

    ' ==========================================
    ' 3. BÚSQUEDA DE PRODUCTO (CON VALIDACIÓN)
    ' ==========================================
    Private Sub txtCodigo_Leave(sender As Object, e As EventArgs) Handles txtCodigo.Leave
        If String.IsNullOrWhiteSpace(txtCodigo.Text) Then Return

        Using conexion As New SQLiteConnection(cadenaConexion)
            Try
                conexion.Open()
                Dim cmd As New SQLiteCommand("SELECT * FROM Inventario WHERE Codigo = @cod", conexion)
                cmd.Parameters.AddWithValue("@cod", txtCodigo.Text)
                Using lector As SQLiteDataReader = cmd.ExecuteReader()
                    If lector.Read() Then
                        txtGenerico.Text = lector("Generico").ToString()
                        txtDistintivo.Text = lector("Distintivo").ToString()
                        txtPresentacion.Text = lector("Presentacion").ToString()
                        txtAware.Text = lector("AWARE").ToString()
                        txtExistencia.Text = lector("ExistenciaActual").ToString()
                    Else
                        MessageBox.Show("Producto no encontrado.")
                        txtCodigo.Focus()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Error al buscar producto: " & ex.Message)
            End Try
        End Using
    End Sub

    ' ==========================================
    ' 4. AUTORELLENADO DE MÉDICO
    ' ==========================================
    Private Sub cmbCedula_TextChanged(sender As Object, e As EventArgs) Handles cmbCedula.TextChanged
        If bloqueandoEventos Then Return
        If String.IsNullOrWhiteSpace(cmbCedula.Text) Then Return

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Medicos WHERE Cedula = @ced", conexion)
            cmd.Parameters.AddWithValue("@ced", cmbCedula.Text)
            Using lector As SQLiteDataReader = cmd.ExecuteReader()
                If lector.Read() Then
                    bloqueandoEventos = True
                    cmbNombreMed.Text = lector("NombreMed").ToString()
                    RellenarDatosMedico(lector)
                    bloqueandoEventos = False
                End If
            End Using
        End Using
    End Sub

    Sub RellenarDatosMedico(l As SQLiteDataReader)
        txtCalle.Text = l("Calle").ToString()
        txtNoInt.Text = l("NoInt").ToString()
        txtNoExt.Text = l("NoExt").ToString()
        txtColonia.Text = l("Colonia").ToString()
        txtCiudad.Text = l("Ciudad").ToString()
        txtEstado.Text = l("Estado").ToString()
        txtCP.Text = l("CP").ToString()
        txtPais.Text = l("Pais").ToString()
        txtTel.Text = l("Tel").ToString()
    End Sub

    ' ==========================================
    ' 5. GUARDAR SALIDA (TRANSACCIÓN)
    ' ==========================================
    Private Sub btnGuardarSalida_Click(sender As Object, e As EventArgs) Handles btnGuardarSalida.Click
        ' Validación de seguridad
        Dim cant As Double = Val(txtSurtido.Text)
        Dim exist As Double = Val(txtExistencia.Text)

        If txtCodigo.Text = "" Or cant <= 0 Then
            MessageBox.Show("Datos incompletos o cantidad inválida.")
            Return
        End If

        If cant > exist Then
            MessageBox.Show("Stock insuficiente.")
            Return
        End If

        Using conn As New SQLiteConnection(cadenaConexion)
            conn.Open()
            Using trans As SQLiteTransaction = conn.BeginTransaction()
                Try
                    ' 1. Registro Médico (si aplica)
                    If cmbCedula.Visible And Not String.IsNullOrWhiteSpace(cmbCedula.Text) Then
                        Dim cmdM As New SQLiteCommand("INSERT OR IGNORE INTO Medicos (Cedula, NombreMed, Calle, NoInt, NoExt, Colonia, Ciudad, Estado, CP, Pais, Tel) VALUES (@ced, @nom, @c, @ni, @ne, @col, @ciu, @est, @cp, @pai, @tel)", conn, trans)
                        cmdM.Parameters.AddWithValue("@ced", cmbCedula.Text)
                        cmdM.Parameters.AddWithValue("@nom", cmbNombreMed.Text.ToUpper())
                        cmdM.Parameters.AddWithValue("@c", txtCalle.Text.ToUpper())
                        cmdM.Parameters.AddWithValue("@ni", txtNoInt.Text)
                        cmdM.Parameters.AddWithValue("@ne", txtNoExt.Text)
                        cmdM.Parameters.AddWithValue("@col", txtColonia.Text.ToUpper())
                        cmdM.Parameters.AddWithValue("@ciu", txtCiudad.Text.ToUpper())
                        cmdM.Parameters.AddWithValue("@est", txtEstado.Text.ToUpper())
                        cmdM.Parameters.AddWithValue("@cp", txtCP.Text)
                        cmdM.Parameters.AddWithValue("@pai", txtPais.Text.ToUpper())
                        cmdM.Parameters.AddWithValue("@tel", txtTel.Text)
                        cmdM.ExecuteNonQuery()
                    End If

                    ' 2. Registrar Salida
                    Dim cmdSal As New SQLiteCommand("INSERT INTO Salidas (Fecha, Codigo, Generico, Distintivo, Presentacion, AWARE, Lote, Caducidad, Existencia, Surtido, Saldo, Movimiento, Folio, Cedula, Nombre, Direccion, Telefono) VALUES (@f, @cod, @gen, @dis, @pres, @aw, @lot, @cad, @ext, @sur, @sal, @mov, @fol, @ced, @nom, @dir, @tel)", conn, trans)
                    cmdSal.Parameters.AddWithValue("@f", Now.ToString("dd/MM/yyyy HH:mm"))
                    cmdSal.Parameters.AddWithValue("@cod", txtCodigo.Text)
                    cmdSal.Parameters.AddWithValue("@gen", txtGenerico.Text)
                    cmdSal.Parameters.AddWithValue("@dis", txtDistintivo.Text)
                    cmdSal.Parameters.AddWithValue("@pres", txtPresentacion.Text)
                    cmdSal.Parameters.AddWithValue("@aw", txtAware.Text)
                    cmdSal.Parameters.AddWithValue("@lot", txtLote.Text)
                    cmdSal.Parameters.AddWithValue("@cad", txtCaducidad.Text)
                    cmdSal.Parameters.AddWithValue("@ext", exist)
                    cmdSal.Parameters.AddWithValue("@sur", cant)
                    cmdSal.Parameters.AddWithValue("@sal", (exist - cant))
                    cmdSal.Parameters.AddWithValue("@mov", cmbMovimiento.Text)
                    cmdSal.Parameters.AddWithValue("@fol", txtFolio.Text)
                    cmdSal.Parameters.AddWithValue("@ced", If(cmbCedula.Visible, cmbCedula.Text, ""))
                    cmdSal.Parameters.AddWithValue("@nom", If(cmbNombreMed.Visible, cmbNombreMed.Text, ""))
                    cmdSal.Parameters.AddWithValue("@dir", txtCalle.Text & " " & txtColonia.Text)
                    cmdSal.Parameters.AddWithValue("@tel", txtTel.Text)
                    cmdSal.ExecuteNonQuery()

                    ' 3. Actualizar Inventario
                    Dim cmdInv As New SQLiteCommand("UPDATE Inventario SET ExistenciaActual = ExistenciaActual - @cant WHERE Codigo = @cod", conn, trans)
                    cmdInv.Parameters.AddWithValue("@cant", cant)
                    cmdInv.Parameters.AddWithValue("@cod", txtCodigo.Text)
                    cmdInv.ExecuteNonQuery()

                    trans.Commit()
                    MessageBox.Show("Salida registrada.")
                    Me.Close()
                Catch ex As Exception
                    trans.Rollback()
                    MessageBox.Show("Error en transacción: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' 6. ESTILO VISUAL
    Private Sub AplicarEstiloFluent()
        Me.BackColor = Color.White
        For Each c As Control In Me.Controls
            If TypeOf c Is Button Then
                Dim b As Button = CType(c, Button)
                b.FlatStyle = FlatStyle.Flat
                b.FlatAppearance.BorderSize = 0
                b.BackColor = Color.FromArgb(0, 102, 204)
                b.ForeColor = Color.White
                b.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            End If
        Next
    End Sub
End Class