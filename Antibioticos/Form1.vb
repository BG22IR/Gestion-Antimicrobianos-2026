Imports System.Data.SQLite
Imports System.IO
Imports Microsoft.VisualBasic.FileIO
Imports System.Drawing.Printing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Reflection

Public Class Form1

    ' =========================================================
    ' 0. VARIABLES GLOBALES Y CONTROLES
    ' =========================================================
    Dim cadenaConexion As String = "Data Source=BaseDatosADN.db;Version=3;"

    ' Contenedor maestro de vistas (Área delimitada a la derecha)
    Private pnlContenedorVistas As New Panel()

    ' Paneles principales de contenido
    Private panelInicio As New Panel()
    Private panelConfig As New Panel()
    Private panelReportes As New Panel()
    Private panelAware As New Panel()

    ' Controles de la Barra Indicadora de Menú (Animación lateral)
    Private pnlIndicadorMenu As New Panel()
    Private tmrAnimIndicador As New Timer()
    Private targetYIndicador As Integer = 0
    Private botonActivoActual As Button = Nothing

    ' Controles del Motor de Transición Fade (Fundido suave)
    Private tmrFade As New Timer()
    Private picFadeOverlay As New PictureBox()
    Private bmpVistaAnterior As Bitmap = Nothing
    Private alphaFade As Single = 1.0F
    Private controlActualVisible As Control = Nothing
    Private controlProximoVisible As Control = Nothing

    ' Controles de Inicio (Contenedor central, logo grande y membrete)
    Private panelCentroInicio As New Panel()
    Private picLogoInicio As New PictureBox()
    Private lblNomInicio As New Label()
    Private lblDirInicio As New Label()
    Private lblRespInicio As New Label()
    Private WithEvents btnNuevaEntrada As New Button()
    Private WithEvents btnNuevaSalida As New Button()
    Private WithEvents btnImportarCSV As New Button()

    ' Controles de Configuración
    Private txtNomFarmacia As New TextBox()
    Private txtDireccion As New TextBox()
    Private txtResponsable As New TextBox()
    Private picLogoConfig As New PictureBox()
    Private WithEvents btnSubirLogo As New Button()
    Private WithEvents btnGuardarConfig As New Button()

    ' Controles de Reportes Oficiales (Entradas / Salidas)
    Private cmbModuloRep As New ComboBox()
    Private cmbMesRep As New ComboBox()
    Private txtAnioRep As New TextBox()
    Private WithEvents btnGenerarRep As New Button()
    Private WithEvents docImprimir As New PrintDocument()
    Private dtImprimir As New DataTable()
    Private indiceImpresion As Integer = 0
    Private numPaginaReporte As Integer = 0

    ' Controles del Módulo AWaRe
    Private cmbMesAware As New ComboBox()
    Private txtAnioAware As New TextBox()
    Private WithEvents btnFiltrarAware As New Button()
    Private WithEvents btnImprimirAware As New Button()
    Private lblKpiAccesoNum As New Label()
    Private lblKpiAccesoPct As New Label()
    Private lblKpiVigiNum As New Label()
    Private lblKpiVigiPct As New Label()
    Private lblKpiResNum As New Label()
    Private lblKpiResPct As New Label()
    Private lblKpiTotalNum As New Label()
    Private lblKpiCumplimiento As New Label()
    Private WithEvents picGraficoAware As New PictureBox()
    Private dgvDetalleAware As New DataGridView()
    Private WithEvents docImprimirAware As New PrintDocument()

    ' Tarjetas KPI para autoajuste
    Private cardAcceso As Panel
    Private cardVigi As Panel
    Private cardRes As Panel
    Private cardTot As Panel
    Private pnlKpisContainer As New Panel()

    ' Datos acumulados para métricas AWaRe
    Private cantAcceso As Double = 0
    Private cantVigilancia As Double = 0
    Private cantReserva As Double = 0
    Private cantOtros As Double = 0
    Private totalAware As Double = 0
    Private dtDetalleAwareSource As New DataTable()


    ' =========================================================
    ' 1. AL CARGAR EL PROGRAMA
    ' =========================================================
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HabilitarDobleBuffer(Me)
        CrearBaseDeDatosSiNoExiste()

        ' =========================================================
        ' 1. VERIFICACIÓN DE LICENCIA LOCAL RSA
        ' =========================================================
        Dim msgErrorLic As String = ""
        If Not LicenciaManager.ValidarLicenciaActual(msgErrorLic) Then
            Dim ventanaActivacion As New FormActivacion()
            If ventanaActivacion.ShowDialog() <> DialogResult.OK Then
                ' Si el usuario cancela o no activa la licencia, se cierra el programa
                Application.Exit()
                Return
            End If
        End If

        ' =========================================================
        ' 2. CARGA NORMAL DE LA APLICACIÓN
        ' =========================================================
        HabilitarDobleBuffer(Me)
        CrearBaseDeDatosSiNoExiste()
        ' ... resto de tu código de Form1_Load ...

        ' 1. Configurar y delimitar contenedor maestro
        pnlContenedorVistas.Dock = DockStyle.Fill
        pnlContenedorVistas.BackColor = Drawing.Color.White
        Me.Controls.Add(pnlContenedorVistas)
        HabilitarDobleBuffer(pnlContenedorVistas)

        ' 2. Prioridad Z-Order: Menú a la izquierda y contenedor libre a la derecha
        Panel1.SendToBack()
        pnlContenedorVistas.BringToFront()

        ' 3. Overlay para transición Fade
        picFadeOverlay.Dock = DockStyle.Fill
        picFadeOverlay.Visible = False
        pnlContenedorVistas.Controls.Add(picFadeOverlay)

        ' 4. Timers de Animación
        tmrAnimIndicador.Interval = 10
        AddHandler tmrAnimIndicador.Tick, AddressOf AnimarIndicadorMenu_Tick

        tmrFade.Interval = 15
        AddHandler tmrFade.Tick, AddressOf AnimarFade_Tick

        ' 5. Inicializar diseño, pantallas y datos
        AplicarEstiloFluent()
        ConfigurarIndicadorMenu()

        ConfigurarPantallaInicio()
        ConfigurarPantallaAjustes()
        ConfigurarPantallaReportes()
        ConfigurarPantallaAware()
        ConfigurarContenedorDataGridView()
        CargarConfiguracionActual()

        ' Foco inicial en Pantalla de Inicio
        SeleccionarMenu(Button1, panelInicio, True)
    End Sub

    Private Sub HabilitarDobleBuffer(c As Control)
        Try
            Dim prop As PropertyInfo = GetType(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic Or BindingFlags.Instance)
            If prop IsNot Nothing Then
                prop.SetValue(c, True, Nothing)
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub CrearBaseDeDatosSiNoExiste()
        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim comando As New SQLiteCommand(conexion)

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Inventario (Codigo TEXT PRIMARY KEY, Generico TEXT, Distintivo TEXT, Presentacion TEXT, AWARE TEXT, ExistenciaActual REAL)"
            comando.ExecuteNonQuery()

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Proveedores (Proveedor TEXT PRIMARY KEY, RFC TEXT, Direccion TEXT)"
            comando.ExecuteNonQuery()

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Entradas (Id INTEGER PRIMARY KEY AUTOINCREMENT, Fecha TEXT, Codigo TEXT, Generico TEXT, Distintivo TEXT, Presentacion TEXT, AWARE TEXT, Lote TEXT, Caducidad TEXT, Existencia REAL, Surtido REAL, Saldo REAL, Factura TEXT, Proveedor TEXT, RFC TEXT, Direccion TEXT)"
            comando.ExecuteNonQuery()

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Medicos (Cedula TEXT PRIMARY KEY, NombreMed TEXT, Calle TEXT, NoInt TEXT, NoExt TEXT, Colonia TEXT, Ciudad TEXT, Estado TEXT, CP TEXT, Pais TEXT, Tel TEXT)"
            comando.ExecuteNonQuery()

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Salidas (Id INTEGER PRIMARY KEY AUTOINCREMENT, Fecha TEXT, Codigo TEXT, Generico TEXT, Distintivo TEXT, Presentacion TEXT, AWARE TEXT, Lote TEXT, Caducidad TEXT, Existencia REAL, Surtido REAL, Saldo REAL, Movimiento TEXT, Folio TEXT, Cedula TEXT, Nombre TEXT, Direccion TEXT, Telefono TEXT)"
            comando.ExecuteNonQuery()

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Configuracion (Id INTEGER PRIMARY KEY, NombreFarmacia TEXT, Direccion TEXT, Responsable TEXT, RutaLogo TEXT)"
            comando.ExecuteNonQuery()

            comando.CommandText = "INSERT OR IGNORE INTO Configuracion (Id, NombreFarmacia, Responsable) VALUES (1, 'FARMACIAS ADN', 'C. SILVIA CARBAJAL PERALES')"
            comando.ExecuteNonQuery()
        End Using
    End Sub


    ' =========================================================
    ' 2. FUNCIONES AUXILIARES DE DIBUJO Y BORDES
    ' =========================================================
    Private Sub RedondearBoton(btn As Button, Optional radio As Integer = 16)
        AddHandler btn.SizeChanged, Sub(s, e) ActualizarRegionControl(btn, radio)
        ActualizarRegionControl(btn, radio)
    End Sub

    Private Sub ActualizarRegionControl(ctrl As Control, radio As Integer)
        If ctrl.Width <= 0 OrElse ctrl.Height <= 0 Then
            Return
        End If
        Using path As New GraphicsPath()
            Dim r As New Rectangle(0, 0, ctrl.Width, ctrl.Height)
            Dim d As Integer = radio * 2
            If d > ctrl.Height Then d = ctrl.Height
            If d > ctrl.Width Then d = ctrl.Width

            path.StartFigure()
            path.AddArc(r.X, r.Y, d, d, 180, 90)
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
            path.CloseFigure()

            ctrl.Region = New Region(path)
        End Using
    End Sub

    Private Function CalcularRectanguloProporcional(img As Image, posX As Integer, posY As Integer, maxAncho As Integer, maxAlto As Integer) As Rectangle
        Dim escala As Single = Math.Min(CSng(maxAncho) / img.Width, CSng(maxAlto) / img.Height)
        Dim anchoFinal As Integer = CInt(img.Width * escala)
        Dim altoFinal As Integer = CInt(img.Height * escala)
        Dim yCentrado As Integer = posY + ((maxAlto - altoFinal) \ 2)
        Return New Rectangle(posX, yCentrado, anchoFinal, altoFinal)
    End Function


    ' =========================================================
    ' 3. INDICADOR DINÁMICO DE MENÚ LATERAL
    ' =========================================================
    Private Sub ConfigurarIndicadorMenu()
        pnlIndicadorMenu.Size = New Size(5, 24)
        pnlIndicadorMenu.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        pnlIndicadorMenu.Location = New Point(3, 10)
        pnlIndicadorMenu.Visible = True
        Panel1.Controls.Add(pnlIndicadorMenu)
        pnlIndicadorMenu.BringToFront()
        ActualizarRegionControl(pnlIndicadorMenu, 2)
    End Sub

    Private Sub AnimarIndicadorMenu_Tick(sender As Object, e As EventArgs)
        Dim diferencia As Integer = targetYIndicador - pnlIndicadorMenu.Top
        If Math.Abs(diferencia) <= 1 Then
            pnlIndicadorMenu.Top = targetYIndicador
            tmrAnimIndicador.Stop()
        Else
            pnlIndicadorMenu.Top += CInt(Math.Ceiling(diferencia * 0.35))
        End If
    End Sub


    ' =========================================================
    ' 4. MOTOR DE TRANSICIÓN FADE IN / FADE OUT
    ' =========================================================
    Private Sub CambiarVistaConFade(nuevoControl As Control, Optional saltarAnimacion As Boolean = False)
        If controlActualVisible Is nuevoControl AndAlso Not saltarAnimacion Then
            Return
        End If

        If saltarAnimacion OrElse controlActualVisible Is Nothing Then
            OcultarTodasLasVistas()
            nuevoControl.Visible = True
            nuevoControl.BringToFront()
            controlActualVisible = nuevoControl
            Return
        End If

        Try
            If bmpVistaAnterior IsNot Nothing Then
                bmpVistaAnterior.Dispose()
            End If
            bmpVistaAnterior = New Bitmap(pnlContenedorVistas.Width, pnlContenedorVistas.Height)
            pnlContenedorVistas.DrawToBitmap(bmpVistaAnterior, New Rectangle(0, 0, pnlContenedorVistas.Width, pnlContenedorVistas.Height))
        Catch ex As Exception
            bmpVistaAnterior = Nothing
        End Try

        controlProximoVisible = nuevoControl
        alphaFade = 1.0F

        picFadeOverlay.Image = bmpVistaAnterior
        picFadeOverlay.Visible = True
        picFadeOverlay.BringToFront()

        OcultarTodasLasVistas()
        controlProximoVisible.Visible = True
        controlProximoVisible.BringToFront()
        picFadeOverlay.BringToFront()

        tmrFade.Start()
    End Sub

    Private Sub AnimarFade_Tick(sender As Object, e As EventArgs)
        alphaFade -= 0.18F

        If alphaFade <= 0.05F OrElse bmpVistaAnterior Is Nothing Then
            tmrFade.Stop()
            picFadeOverlay.Visible = False
            controlActualVisible = controlProximoVisible
            If picFadeOverlay.Image IsNot Nothing Then
                picFadeOverlay.Image = Nothing
            End If
            If bmpVistaAnterior IsNot Nothing Then
                bmpVistaAnterior.Dispose()
                bmpVistaAnterior = Nothing
            End If
        Else
            Dim bmpTemp As New Bitmap(bmpVistaAnterior.Width, bmpVistaAnterior.Height)
            Using g As Graphics = Graphics.FromImage(bmpTemp)
                Dim matriz As New ColorMatrix() With {.Matrix33 = alphaFade}
                Dim attr As New ImageAttributes()
                attr.SetColorMatrix(matriz, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)

                g.DrawImage(bmpVistaAnterior, New Rectangle(0, 0, bmpTemp.Width, bmpTemp.Height),
                            0, 0, bmpVistaAnterior.Width, bmpVistaAnterior.Height, GraphicsUnit.Pixel, attr)
            End Using

            Dim imgVieja As Image = picFadeOverlay.Image
            picFadeOverlay.Image = bmpTemp
            If imgVieja IsNot Nothing AndAlso imgVieja IsNot bmpVistaAnterior Then
                imgVieja.Dispose()
            End If
        End If
    End Sub

    Private Sub OcultarTodasLasVistas()
        panelInicio.Visible = False
        panelConfig.Visible = False
        panelReportes.Visible = False
        panelAware.Visible = False
        DataGridView1.Visible = False
    End Sub

    Private Sub SeleccionarMenu(btn As Button, controlVista As Control, Optional forzarInmediato As Boolean = False)
        botonActivoActual = btn

        targetYIndicador = btn.Top + ((btn.Height - pnlIndicadorMenu.Height) \ 2)
        If forzarInmediato Then
            pnlIndicadorMenu.Top = targetYIndicador
        Else
            tmrAnimIndicador.Start()
        End If

        For Each ctrl As Control In Panel1.Controls
            If TypeOf ctrl Is Button Then
                Dim b As Button = CType(ctrl, Button)
                If b Is btn Then
                    b.BackColor = Drawing.Color.FromArgb(229, 238, 249)
                    b.ForeColor = Drawing.Color.FromArgb(0, 102, 204)
                    b.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
                Else
                    b.BackColor = Drawing.Color.FromArgb(243, 243, 243)
                    b.ForeColor = Drawing.Color.FromArgb(50, 50, 50)
                    b.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Regular)
                End If
            End If
        Next

        CambiarVistaConFade(controlVista, forzarInmediato)
    End Sub


    ' =========================================================
    ' 5. EVENTOS DE LOS BOTONES DEL MENÚ LATERAL
    ' =========================================================
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SeleccionarMenu(Button1, panelInicio)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ConfigurarTablaEntradas()
        SeleccionarMenu(Button2, DataGridView1)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ConfigurarTablaSalidas()
        SeleccionarMenu(Button3, DataGridView1)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ConfigurarTablaMedicos()
        SeleccionarMenu(Button4, DataGridView1)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ConfigurarTablaProveedores()
        SeleccionarMenu(Button5, DataGridView1)
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        ConfigurarTablaInventario()
        SeleccionarMenu(Button6, DataGridView1)
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        SeleccionarMenu(Button7, panelConfig)
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        SeleccionarMenu(Button8, panelReportes)
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        CargarReporteAware()
        SeleccionarMenu(Button9, panelAware)
    End Sub


    ' =========================================================
    ' 6. PANTALLA DE INICIO (MEMBRETE Y ACCIONES)
    ' =========================================================
    Private Sub ConfigurarPantallaInicio()
        panelInicio.Dock = DockStyle.Fill
        panelInicio.BackColor = Drawing.Color.White
        panelInicio.AutoScroll = True
        pnlContenedorVistas.Controls.Add(panelInicio)
        HabilitarDobleBuffer(panelInicio)

        panelCentroInicio.Size = New Size(650, 560)
        panelCentroInicio.BackColor = Drawing.Color.White
        panelInicio.Controls.Add(panelCentroInicio)
        HabilitarDobleBuffer(panelCentroInicio)

        ' 1. Logotipo grande y centrado
        picLogoInicio.Size = New Size(420, 240)
        picLogoInicio.Location = New Point((panelCentroInicio.Width - picLogoInicio.Width) \ 2, 10)
        picLogoInicio.SizeMode = PictureBoxSizeMode.Zoom
        panelCentroInicio.Controls.Add(picLogoInicio)

        ' 2. Nombre de la Farmacia (Centrado)
        lblNomInicio.Font = New Drawing.Font("Segoe UI", 18.0F, Drawing.FontStyle.Bold)
        lblNomInicio.ForeColor = Drawing.Color.FromArgb(0, 102, 204)
        lblNomInicio.TextAlign = ContentAlignment.MiddleCenter
        lblNomInicio.AutoSize = False
        lblNomInicio.Size = New Size(650, 36)
        lblNomInicio.Location = New Point(0, 255)

        ' 3. Dirección Completa (Centrada)
        lblDirInicio.Font = New Drawing.Font("Segoe UI", 10.5F, Drawing.FontStyle.Regular)
        lblDirInicio.ForeColor = Drawing.Color.FromArgb(70, 70, 70)
        lblDirInicio.TextAlign = ContentAlignment.MiddleCenter
        lblDirInicio.AutoSize = False
        lblDirInicio.Size = New Size(650, 25)
        lblDirInicio.Location = New Point(0, 293)

        ' 4. Responsable Sanitario (Centrado)
        lblRespInicio.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Italic)
        lblRespInicio.ForeColor = Drawing.Color.FromArgb(100, 100, 100)
        lblRespInicio.TextAlign = ContentAlignment.MiddleCenter
        lblRespInicio.AutoSize = False
        lblRespInicio.Size = New Size(650, 25)
        lblRespInicio.Location = New Point(0, 320)

        panelCentroInicio.Controls.Add(lblNomInicio)
        panelCentroInicio.Controls.Add(lblDirInicio)
        panelCentroInicio.Controls.Add(lblRespInicio)

        ' 5. Botones de Acción Rápida Redondeados
        btnNuevaEntrada.Text = "+ Registrar Entrada"
        btnNuevaEntrada.Size = New Size(290, 65)
        btnNuevaEntrada.Location = New Point(25, 365)
        btnNuevaEntrada.BackColor = Drawing.Color.FromArgb(0, 153, 76)
        btnNuevaEntrada.ForeColor = Drawing.Color.White
        btnNuevaEntrada.Font = New Drawing.Font("Segoe UI", 12.0F, Drawing.FontStyle.Bold)
        btnNuevaEntrada.FlatStyle = FlatStyle.Flat
        btnNuevaEntrada.FlatAppearance.BorderSize = 0
        btnNuevaEntrada.Cursor = Cursors.Hand
        RedondearBoton(btnNuevaEntrada, 18)

        btnNuevaSalida.Text = "+ Registrar Salida (Receta)"
        btnNuevaSalida.Size = New Size(290, 65)
        btnNuevaSalida.Location = New Point(335, 365)
        btnNuevaSalida.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnNuevaSalida.ForeColor = Drawing.Color.White
        btnNuevaSalida.Font = New Drawing.Font("Segoe UI", 12.0F, Drawing.FontStyle.Bold)
        btnNuevaSalida.FlatStyle = FlatStyle.Flat
        btnNuevaSalida.FlatAppearance.BorderSize = 0
        btnNuevaSalida.Cursor = Cursors.Hand
        RedondearBoton(btnNuevaSalida, 18)

        btnImportarCSV.Text = "📁 Importar Catálogos desde CSV"
        btnImportarCSV.Size = New Size(600, 55)
        btnImportarCSV.Location = New Point(25, 442)
        btnImportarCSV.BackColor = Drawing.Color.FromArgb(242, 101, 34)
        btnImportarCSV.ForeColor = Drawing.Color.White
        btnImportarCSV.Font = New Drawing.Font("Segoe UI", 11.5F, Drawing.FontStyle.Bold)
        btnImportarCSV.FlatStyle = FlatStyle.Flat
        btnImportarCSV.FlatAppearance.BorderSize = 0
        btnImportarCSV.Cursor = Cursors.Hand
        RedondearBoton(btnImportarCSV, 18)

        panelCentroInicio.Controls.Add(btnNuevaEntrada)
        panelCentroInicio.Controls.Add(btnNuevaSalida)
        panelCentroInicio.Controls.Add(btnImportarCSV)

        CentrarPanelInicio()
    End Sub

    Private Sub CentrarPanelInicio()
        If panelCentroInicio IsNot Nothing AndAlso panelInicio IsNot Nothing Then
            Dim posX As Integer = Math.Max(10, (panelInicio.ClientSize.Width - panelCentroInicio.Width) \ 2)
            Dim posY As Integer = Math.Max(10, (panelInicio.ClientSize.Height - panelCentroInicio.Height) \ 2)
            panelCentroInicio.Location = New Point(posX, posY)
        End If
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        CentrarPanelInicio()
        AjustarTarjetasKpi()
    End Sub


    ' =========================================================
    ' 7. PANTALLA: CONFIGURACIÓN
    ' =========================================================
    Private Sub ConfigurarPantallaAjustes()
        panelConfig.Dock = DockStyle.Fill
        panelConfig.BackColor = Drawing.Color.White
        panelConfig.AutoScroll = True
        pnlContenedorVistas.Controls.Add(panelConfig)
        HabilitarDobleBuffer(panelConfig)

        Dim lblTitulo As New Label With {.Text = "Configuración del Sistema", .Location = New Point(35, 25), .Font = New Drawing.Font("Segoe UI", 16.0F, Drawing.FontStyle.Bold), .AutoSize = True}

        Dim lblNom As New Label With {.Text = "Nombre de la Farmacia:", .Location = New Point(35, 80), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F)}
        txtNomFarmacia.Location = New Point(35, 105)
        txtNomFarmacia.Size = New Size(400, 30)
        txtNomFarmacia.Font = New Drawing.Font("Segoe UI", 11.0F)

        Dim lblDir As New Label With {.Text = "Dirección Completa:", .Location = New Point(35, 150), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F)}
        txtDireccion.Location = New Point(35, 175)
        txtDireccion.Size = New Size(400, 30)
        txtDireccion.Font = New Drawing.Font("Segoe UI", 11.0F)

        Dim lblResp As New Label With {.Text = "Nombre del Responsable Sanitario:", .Location = New Point(35, 220), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F)}
        txtResponsable.Location = New Point(35, 245)
        txtResponsable.Size = New Size(400, 30)
        txtResponsable.Font = New Drawing.Font("Segoe UI", 11.0F)

        picLogoConfig.Location = New Point(460, 105)
        picLogoConfig.Size = New Size(150, 150)
        picLogoConfig.SizeMode = PictureBoxSizeMode.Zoom
        picLogoConfig.BorderStyle = BorderStyle.FixedSingle

        btnSubirLogo.Text = "Cargar Logo"
        btnSubirLogo.Location = New Point(460, 265)
        btnSubirLogo.Size = New Size(150, 35)
        btnSubirLogo.BackColor = Drawing.Color.FromArgb(230, 230, 230)
        btnSubirLogo.FlatStyle = FlatStyle.Flat
        btnSubirLogo.FlatAppearance.BorderSize = 0
        btnSubirLogo.Cursor = Cursors.Hand
        RedondearBoton(btnSubirLogo, 12)

        btnGuardarConfig.Text = "Guardar Configuración"
        btnGuardarConfig.Location = New Point(35, 310)
        btnGuardarConfig.Size = New Size(400, 48)
        btnGuardarConfig.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnGuardarConfig.ForeColor = Drawing.Color.White
        btnGuardarConfig.Font = New Drawing.Font("Segoe UI", 12.0F, Drawing.FontStyle.Bold)
        btnGuardarConfig.FlatStyle = FlatStyle.Flat
        btnGuardarConfig.FlatAppearance.BorderSize = 0
        btnGuardarConfig.Cursor = Cursors.Hand
        RedondearBoton(btnGuardarConfig, 16)

        panelConfig.Controls.Add(lblTitulo)
        panelConfig.Controls.Add(lblNom)
        panelConfig.Controls.Add(txtNomFarmacia)
        panelConfig.Controls.Add(lblDir)
        panelConfig.Controls.Add(txtDireccion)
        panelConfig.Controls.Add(lblResp)
        panelConfig.Controls.Add(txtResponsable)
        panelConfig.Controls.Add(picLogoConfig)
        panelConfig.Controls.Add(btnSubirLogo)
        panelConfig.Controls.Add(btnGuardarConfig)
    End Sub

    Private Sub CargarConfiguracionActual()
        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Configuracion WHERE Id = 1", conexion)
            Using lector As SQLiteDataReader = cmd.ExecuteReader()
                If lector.Read() Then
                    Dim nom As String = lector("NombreFarmacia").ToString()
                    Dim dir As String = lector("Direccion").ToString()
                    Dim resp As String = lector("Responsable").ToString()

                    txtNomFarmacia.Text = nom
                    txtDireccion.Text = dir
                    txtResponsable.Text = resp

                    lblNomInicio.Text = nom.ToUpper()
                    lblDirInicio.Text = If(dir.Trim() <> "", dir, "Dirección no registrada")
                    lblRespInicio.Text = If(resp.Trim() <> "", "Responsable Sanitario: " & resp, "Responsable Sanitario: No registrado")

                    Dim rutaLogo As String = lector("RutaLogo").ToString()
                    If File.Exists(rutaLogo) Then
                        Dim img As Image = Image.FromFile(rutaLogo)
                        picLogoConfig.Image = New Bitmap(img)
                        picLogoInicio.Image = New Bitmap(img)
                        img.Dispose()
                    End If
                End If
            End Using
        End Using
    End Sub

    Private Sub btnSubirLogo_Click(sender As Object, e As EventArgs) Handles btnSubirLogo.Click
        Dim dialog As New OpenFileDialog()
        dialog.Filter = "Archivos de Imagen|*.jpg;*.jpeg;*.png;*.bmp"
        If dialog.ShowDialog() = DialogResult.OK Then
            Try
                Dim carpetaDestino As String = Path.Combine(Application.StartupPath, "Recursos")
                If Not Directory.Exists(carpetaDestino) Then
                    Directory.CreateDirectory(carpetaDestino)
                End If

                Dim rutaFinal As String = Path.Combine(carpetaDestino, "LogoFarmacia" & Path.GetExtension(dialog.FileName))
                File.Copy(dialog.FileName, rutaFinal, True)

                Dim img As Image = Image.FromFile(rutaFinal)
                picLogoConfig.Image = New Bitmap(img)
                picLogoInicio.Image = New Bitmap(img)
                img.Dispose()

                picLogoConfig.Tag = rutaFinal
            Catch ex As Exception
                MessageBox.Show("Error al cargar imagen: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub btnGuardarConfig_Click(sender As Object, e As EventArgs) Handles btnGuardarConfig.Click
        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim cmd As New SQLiteCommand("UPDATE Configuracion SET NombreFarmacia = @nom, Direccion = @dir, Responsable = @resp, RutaLogo = @ruta WHERE Id = 1", conexion)
            cmd.Parameters.AddWithValue("@nom", txtNomFarmacia.Text.Trim())
            cmd.Parameters.AddWithValue("@dir", txtDireccion.Text.Trim())
            cmd.Parameters.AddWithValue("@resp", txtResponsable.Text.Trim())
            cmd.Parameters.AddWithValue("@ruta", If(picLogoConfig.Tag IsNot Nothing, picLogoConfig.Tag.ToString(), ""))
            cmd.ExecuteNonQuery()
        End Using

        lblNomInicio.Text = txtNomFarmacia.Text.Trim().ToUpper()
        lblDirInicio.Text = If(txtDireccion.Text.Trim() <> "", txtDireccion.Text.Trim(), "Dirección no registrada")
        lblRespInicio.Text = If(txtResponsable.Text.Trim() <> "", "Responsable Sanitario: " & txtResponsable.Text.Trim(), "Responsable Sanitario: No registrado")

        MessageBox.Show("Configuración guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub


    ' =========================================================
    ' 8. PANTALLA: MÓDULO AWARE (BOTÓN 9) CON AJUSTE DINÁMICO
    ' =========================================================
    Private Sub ConfigurarPantallaAware()
        panelAware.Dock = DockStyle.Fill
        panelAware.BackColor = Drawing.Color.FromArgb(248, 249, 250)
        panelAware.AutoScroll = True
        pnlContenedorVistas.Controls.Add(panelAware)
        HabilitarDobleBuffer(panelAware)

        Dim lblTitulo As New Label With {
            .Text = "📊 Monitoreo y Análisis AWaRe (Uso Racional de Antimicrobianos)",
            .Location = New Point(25, 20),
            .Font = New Drawing.Font("Segoe UI", 15.0F, Drawing.FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(33, 37, 41),
            .AutoSize = True
        }

        Dim lblSubtitulo As New Label With {
            .Text = "Clasificación de consumo según directrices de la OMS y COFEPRIS: Acceso, Vigilancia y Reserva.",
            .Location = New Point(27, 50),
            .Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Regular),
            .ForeColor = Drawing.Color.FromArgb(108, 117, 125),
            .AutoSize = True
        }

        Dim pnlFiltros As New Panel With {
            .Location = New Point(25, 80),
            .Size = New Size(panelAware.ClientSize.Width - 50, 55),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Drawing.Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim lblMes As New Label With {.Text = "Mes:", .Location = New Point(15, 17), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)}
        cmbMesAware.Items.AddRange(New String() {"TODOS", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"})
        cmbMesAware.SelectedIndex = 0
        cmbMesAware.Location = New Point(60, 14)
        cmbMesAware.Size = New Size(90, 28)
        cmbMesAware.Font = New Drawing.Font("Segoe UI", 10.0F)
        cmbMesAware.DropDownStyle = ComboBoxStyle.DropDownList

        Dim lblAnio As New Label With {.Text = "Año:", .Location = New Point(170, 17), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)}
        txtAnioAware.Location = New Point(215, 14)
        txtAnioAware.Size = New Size(80, 28)
        txtAnioAware.Font = New Drawing.Font("Segoe UI", 10.0F)
        txtAnioAware.Text = DateTime.Now.Year.ToString()

        btnFiltrarAware.Text = "🔍 Actualizar Datos"
        btnFiltrarAware.Location = New Point(320, 10)
        btnFiltrarAware.Size = New Size(170, 34)
        btnFiltrarAware.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnFiltrarAware.ForeColor = Drawing.Color.White
        btnFiltrarAware.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
        btnFiltrarAware.FlatStyle = FlatStyle.Flat
        btnFiltrarAware.FlatAppearance.BorderSize = 0
        btnFiltrarAware.Cursor = Cursors.Hand
        RedondearBoton(btnFiltrarAware, 12)

        btnImprimirAware.Text = "🖨 Imprimir Informe AWaRe"
        btnImprimirAware.Location = New Point(510, 10)
        btnImprimirAware.Size = New Size(220, 34)
        btnImprimirAware.BackColor = Drawing.Color.FromArgb(40, 167, 69)
        btnImprimirAware.ForeColor = Drawing.Color.White
        btnImprimirAware.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
        btnImprimirAware.FlatStyle = FlatStyle.Flat
        btnImprimirAware.FlatAppearance.BorderSize = 0
        btnImprimirAware.Cursor = Cursors.Hand
        RedondearBoton(btnImprimirAware, 12)

        pnlFiltros.Controls.Add(lblMes)
        pnlFiltros.Controls.Add(cmbMesAware)
        pnlFiltros.Controls.Add(lblAnio)
        pnlFiltros.Controls.Add(txtAnioAware)
        pnlFiltros.Controls.Add(btnFiltrarAware)
        pnlFiltros.Controls.Add(btnImprimirAware)

        pnlKpisContainer.Location = New Point(25, 145)
        pnlKpisContainer.Size = New Size(panelAware.ClientSize.Width - 50, 105)
        pnlKpisContainer.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pnlKpisContainer.BackColor = Drawing.Color.Transparent

        cardAcceso = CrearTarjetaKpi("ACCESO (Access)", Drawing.Color.FromArgb(40, 167, 69), lblKpiAccesoNum, lblKpiAccesoPct)
        cardVigi = CrearTarjetaKpi("VIGILANCIA (Watch)", Drawing.Color.FromArgb(243, 156, 18), lblKpiVigiNum, lblKpiVigiPct)
        cardRes = CrearTarjetaKpi("RESERVA (Reserve)", Drawing.Color.FromArgb(220, 53, 69), lblKpiResNum, lblKpiResPct)
        cardTot = CrearTarjetaKpi("TOTAL DISPENSADO", Drawing.Color.FromArgb(0, 102, 204), lblKpiTotalNum, lblKpiCumplimiento)

        pnlKpisContainer.Controls.Add(cardAcceso)
        pnlKpisContainer.Controls.Add(cardVigi)
        pnlKpisContainer.Controls.Add(cardRes)
        pnlKpisContainer.Controls.Add(cardTot)
        AjustarTarjetasKpi()

        picGraficoAware.Location = New Point(25, 260)
        picGraficoAware.Size = New Size(panelAware.ClientSize.Width - 50, 110)
        picGraficoAware.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        picGraficoAware.BackColor = Drawing.Color.White
        picGraficoAware.BorderStyle = BorderStyle.FixedSingle

        Dim lblTitTabla As New Label With {
            .Text = "Detalle de Salidas por Medicamento y Clasificación:",
            .Location = New Point(25, 380),
            .Font = New Drawing.Font("Segoe UI", 11.0F, Drawing.FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(33, 37, 41),
            .AutoSize = True
        }

        dgvDetalleAware.Location = New Point(25, 410)
        dgvDetalleAware.Size = New Size(panelAware.ClientSize.Width - 50, 240)
        dgvDetalleAware.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        dgvDetalleAware.BackgroundColor = Drawing.Color.White
        dgvDetalleAware.BorderStyle = BorderStyle.FixedSingle
        dgvDetalleAware.RowHeadersVisible = False
        dgvDetalleAware.AllowUserToAddRows = False
        dgvDetalleAware.AllowUserToDeleteRows = False
        dgvDetalleAware.ReadOnly = True
        dgvDetalleAware.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvDetalleAware.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvDetalleAware.EnableHeadersVisualStyles = False
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.BackColor = Drawing.Color.FromArgb(52, 58, 64)
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        dgvDetalleAware.ColumnHeadersHeight = 32
        dgvDetalleAware.DefaultCellStyle.Font = New Drawing.Font("Segoe UI", 9.0F)
        dgvDetalleAware.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(245, 245, 245)

        panelAware.Controls.Add(lblTitulo)
        panelAware.Controls.Add(lblSubtitulo)
        panelAware.Controls.Add(pnlFiltros)
        panelAware.Controls.Add(pnlKpisContainer)
        panelAware.Controls.Add(picGraficoAware)
        panelAware.Controls.Add(lblTitTabla)
        panelAware.Controls.Add(dgvDetalleAware)
    End Sub

    Private Sub AjustarTarjetasKpi()
        If pnlKpisContainer IsNot Nothing AndAlso cardAcceso IsNot Nothing Then
            Dim totalW As Integer = pnlKpisContainer.ClientSize.Width
            Dim wCard As Integer = Math.Max(150, (totalW - 36) \ 4)
            cardAcceso.Size = New Size(wCard, 100)
            cardAcceso.Location = New Point(0, 0)

            cardVigi.Size = New Size(wCard, 100)
            cardVigi.Location = New Point(wCard + 12, 0)

            cardRes.Size = New Size(wCard, 100)
            cardRes.Location = New Point((wCard * 2) + 24, 0)

            cardTot.Size = New Size(wCard, 100)
            cardTot.Location = New Point((wCard * 3) + 36, 0)
        End If
    End Sub

    Private Function CrearTarjetaKpi(titulo As String, colorCabecera As Drawing.Color, ByRef lblNum As Label, ByRef lblSub As Label) As Panel
        Dim pnl As New Panel With {
            .BackColor = Drawing.Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }

        Dim header As New Label With {
            .Text = titulo,
            .Dock = DockStyle.Top,
            .Height = 26,
            .BackColor = colorCabecera,
            .ForeColor = Drawing.Color.White,
            .Font = New Drawing.Font("Segoe UI", 8.5F, Drawing.FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        lblNum.Text = "0 cajas"
        lblNum.Font = New Drawing.Font("Segoe UI", 15.0F, Drawing.FontStyle.Bold)
        lblNum.ForeColor = Drawing.Color.FromArgb(33, 37, 41)
        lblNum.TextAlign = ContentAlignment.MiddleCenter
        lblNum.Dock = DockStyle.Fill

        lblSub.Text = "0.0% del total"
        lblSub.Font = New Drawing.Font("Segoe UI", 8.5F, Drawing.FontStyle.Regular)
        lblSub.ForeColor = Drawing.Color.FromArgb(108, 117, 125)
        lblSub.TextAlign = ContentAlignment.MiddleCenter
        lblSub.Dock = DockStyle.Bottom
        lblSub.Height = 24

        pnl.Controls.Add(lblNum)
        pnl.Controls.Add(lblSub)
        pnl.Controls.Add(header)
        Return pnl
    End Function

    Private Sub btnFiltrarAware_Click(sender As Object, e As EventArgs) Handles btnFiltrarAware.Click
        CargarReporteAware()
    End Sub

    Private Sub CargarReporteAware()
        Dim anio As String = txtAnioAware.Text.Trim()
        If anio = "" Then
            anio = DateTime.Now.Year.ToString()
        End If

        Dim filtroFecha As String = ""
        If cmbMesAware.Text = "TODOS" OrElse cmbMesAware.Text = "" Then
            filtroFecha = "%/" & anio & "%"
        Else
            filtroFecha = "%/" & cmbMesAware.Text & "/" & anio & "%"
        End If

        cantAcceso = 0
        cantVigilancia = 0
        cantReserva = 0
        cantOtros = 0
        totalAware = 0

        dtDetalleAwareSource = New DataTable()
        dtDetalleAwareSource.Columns.Add("Categoría AWaRe")
        dtDetalleAwareSource.Columns.Add("Genérico")
        dtDetalleAwareSource.Columns.Add("Distintivo")
        dtDetalleAwareSource.Columns.Add("Presentación")
        dtDetalleAwareSource.Columns.Add("Cajas Surtidas", GetType(Double))
        dtDetalleAwareSource.Columns.Add("% de su Grupo")

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()

            Dim query As String = "SELECT UPPER(TRIM(IFNULL(AWARE, 'NO ASIGNADO'))) AS CatAware, Generico, Distintivo, Presentacion, SUM(Surtido) AS Cantidad " &
                                  "FROM Salidas WHERE Fecha LIKE @filtro GROUP BY CatAware, Generico, Distintivo, Presentacion ORDER BY CatAware, Cantidad DESC"

            Using cmd As New SQLiteCommand(query, conexion)
                cmd.Parameters.AddWithValue("@filtro", filtroFecha)
                Using lector As SQLiteDataReader = cmd.ExecuteReader()
                    While lector.Read()
                        Dim cat As String = lector("CatAware").ToString().ToUpper()
                        Dim cant As Double = Convert.ToDouble(lector("Cantidad"))
                        Dim gen As String = lector("Generico").ToString()
                        Dim dis As String = lector("Distintivo").ToString()
                        Dim pre As String = lector("Presentacion").ToString()

                        If cat.Contains("ACCES") Then
                            cantAcceso += cant
                            cat = "ACCESO"
                        ElseIf cat.Contains("VIGILAN") OrElse cat.Contains("WATCH") OrElse cat.Contains("PRECAUC") Then
                            cantVigilancia += cant
                            cat = "VIGILANCIA"
                        ElseIf cat.Contains("RESERV") Then
                            cantReserva += cant
                            cat = "RESERVA"
                        Else
                            cantOtros += cant
                            cat = "NO ASIGNADO"
                        End If

                        dtDetalleAwareSource.Rows.Add(cat, gen, dis, pre, cant, "")
                    End While
                End Using
            End Using
        End Using

        totalAware = cantAcceso + cantVigilancia + cantReserva + cantOtros

        For Each row As DataRow In dtDetalleAwareSource.Rows
            Dim cat As String = row("Categoría AWaRe").ToString()
            Dim cant As Double = Convert.ToDouble(row("Cajas Surtidas"))
            Dim subtotalCat As Double = 1

            If cat = "ACCESO" Then
                subtotalCat = If(cantAcceso > 0, cantAcceso, 1)
            End If
            If cat = "VIGILANCIA" Then
                subtotalCat = If(cantVigilancia > 0, cantVigilancia, 1)
            End If
            If cat = "RESERVA" Then
                subtotalCat = If(cantReserva > 0, cantReserva, 1)
            End If

            Dim pct As Double = (cant / subtotalCat) * 100.0
            row("% de su Grupo") = pct.ToString("0.0") & " %"
        Next

        dgvDetalleAware.DataSource = dtDetalleAwareSource

        Dim pctAcc As Double = If(totalAware > 0, (cantAcceso / totalAware) * 100.0, 0)
        Dim pctVig As Double = If(totalAware > 0, (cantVigilancia / totalAware) * 100.0, 0)
        Dim pctRes As Double = If(totalAware > 0, (cantReserva / totalAware) * 100.0, 0)

        lblKpiAccesoNum.Text = cantAcceso.ToString("N0") & " cajas"
        lblKpiAccesoPct.Text = pctAcc.ToString("0.0") & "% del total"

        lblKpiVigiNum.Text = cantVigilancia.ToString("N0") & " cajas"
        lblKpiVigiPct.Text = pctVig.ToString("0.0") & "% del total"

        lblKpiResNum.Text = cantReserva.ToString("N0") & " cajas"
        lblKpiResPct.Text = pctRes.ToString("0.0") & "% del total"

        lblKpiTotalNum.Text = totalAware.ToString("N0") & " cajas"
        If pctAcc >= 60.0 Then
            lblKpiCumplimiento.Text = "✔ Meta OMS Cumplida (≥60%)"
            lblKpiCumplimiento.ForeColor = Drawing.Color.FromArgb(40, 167, 69)
        Else
            lblKpiCumplimiento.Text = "⚠ Meta OMS: < 60% Acceso"
            lblKpiCumplimiento.ForeColor = Drawing.Color.FromArgb(220, 53, 69)
        End If

        picGraficoAware.Invalidate()
    End Sub

    Private Sub picGraficoAware_Paint(sender As Object, e As PaintEventArgs) Handles picGraficoAware.Paint
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim anchoGrafico As Integer = picGraficoAware.Width - 60
        Dim altoBarra As Integer = 32
        Dim xInicio As Integer = 30
        Dim yBarra As Integer = 40

        Dim fuenteTit As New Font("Segoe UI", 10.0F, FontStyle.Bold)
        Dim fuenteTexto As New Font("Segoe UI", 8.5F, FontStyle.Bold)
        Dim fuenteLeyenda As New Font("Segoe UI", 8.5F, FontStyle.Regular)

        g.DrawString("Distribución Porcentual del Consumo de Antibióticos:", fuenteTit, Brushes.Black, xInicio, 12)

        If totalAware = 0 Then
            g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(230, 230, 230)), xInicio, yBarra, anchoGrafico, altoBarra)
            g.DrawString("Sin registros de salidas para el periodo seleccionado.", fuenteLeyenda, Brushes.Gray, xInicio + 15, yBarra + 8)
            Return
        End If

        Dim pctAcc As Double = (cantAcceso / totalAware)
        Dim pctVig As Double = (cantVigilancia / totalAware)
        Dim pctRes As Double = (cantReserva / totalAware)

        Dim wAcc As Integer = CInt(anchoGrafico * pctAcc)
        Dim wVig As Integer = CInt(anchoGrafico * pctVig)
        Dim wRes As Integer = anchoGrafico - wAcc - wVig
        If wRes < 0 Then
            wRes = 0
        End If

        Dim rectAcc As New Rectangle(xInicio, yBarra, wAcc, altoBarra)
        Dim rectVig As New Rectangle(xInicio + wAcc, yBarra, wVig, altoBarra)
        Dim rectRes As New Rectangle(xInicio + wAcc + wVig, yBarra, wRes, altoBarra)

        Dim brushAcc As New SolidBrush(Drawing.Color.FromArgb(40, 167, 69))
        Dim brushVig As New SolidBrush(Drawing.Color.FromArgb(243, 156, 18))
        Dim brushRes As New SolidBrush(Drawing.Color.FromArgb(220, 53, 69))

        If wAcc > 0 Then
            g.FillRectangle(brushAcc, rectAcc)
        End If
        If wVig > 0 Then
            g.FillRectangle(brushVig, rectVig)
        End If
        If wRes > 0 Then
            g.FillRectangle(brushRes, rectRes)
        End If

        Dim formatCentro As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
        If wAcc > 40 Then
            g.DrawString((pctAcc * 100).ToString("0") & "%", fuenteTexto, Brushes.White, rectAcc, formatCentro)
        End If
        If wVig > 40 Then
            g.DrawString((pctVig * 100).ToString("0") & "%", fuenteTexto, Brushes.White, rectVig, formatCentro)
        End If
        If wRes > 40 Then
            g.DrawString((pctRes * 100).ToString("0") & "%", fuenteTexto, Brushes.White, rectRes, formatCentro)
        End If

        Dim xMeta60 As Integer = xInicio + CInt(anchoGrafico * 0.6)
        Using penMeta As New Pen(Drawing.Color.FromArgb(0, 80, 160), 2) With {.DashStyle = DashStyle.Dash}
            g.DrawLine(penMeta, xMeta60, yBarra - 6, xMeta60, yBarra + altoBarra + 6)
        End Using
        g.DrawString("Meta OMS (≥60%)", fuenteTexto, Brushes.DarkBlue, xMeta60 - 45, yBarra - 18)

        Dim yLeyenda As Integer = yBarra + altoBarra + 12
        g.FillRectangle(brushAcc, xInicio, yLeyenda + 2, 12, 12)
        g.DrawString("Acceso (" & (pctAcc * 100).ToString("0.0") & "%)", fuenteLeyenda, Brushes.Black, xInicio + 18, yLeyenda)

        g.FillRectangle(brushVig, xInicio + 150, yLeyenda + 2, 12, 12)
        g.DrawString("Vigilancia (" & (pctVig * 100).ToString("0.0") & "%)", fuenteLeyenda, Brushes.Black, xInicio + 168, yLeyenda)

        g.FillRectangle(brushRes, xInicio + 310, yLeyenda + 2, 12, 12)
        g.DrawString("Reserva (" & (pctRes * 100).ToString("0.0") & "%)", fuenteLeyenda, Brushes.Black, xInicio + 328, yLeyenda)
    End Sub


    ' =========================================================
    ' 9. MOTOR DE IMPRESIÓN DEL INFORME AWARE (OFICIAL)
    ' =========================================================
    Private Sub btnImprimirAware_Click(sender As Object, e As EventArgs) Handles btnImprimirAware.Click
        If totalAware = 0 Then
            MessageBox.Show("No hay datos de dispensación para el periodo seleccionado.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim vistaPrevia As New PrintPreviewDialog()
        docImprimirAware.DefaultPageSettings.Landscape = False
        vistaPrevia.Document = docImprimirAware
        vistaPrevia.WindowState = FormWindowState.Maximized
        vistaPrevia.ShowDialog()
    End Sub

    Private Sub docImprimirAware_PrintPage(sender As Object, e As PrintPageEventArgs) Handles docImprimirAware.PrintPage
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim fTitulo As New Font("Arial", 14, FontStyle.Bold)
        Dim fSub As New Font("Arial", 9, FontStyle.Regular)
        Dim fSubBold As New Font("Arial", 9, FontStyle.Bold)
        Dim fKpiTit As New Font("Arial", 8, FontStyle.Bold)
        Dim fKpiNum As New Font("Arial", 12, FontStyle.Bold)
        Dim fTablaHeader As New Font("Arial", 8.5F, FontStyle.Bold)
        Dim fTabla As New Font("Arial", 8.0F, FontStyle.Regular)
        Dim brochaNegra As New SolidBrush(Drawing.Color.Black)

        Dim margenIzq As Integer = 50
        Dim margenDer As Integer = e.PageBounds.Width - 50
        Dim anchoDisp As Integer = margenDer - margenIzq
        Dim Y As Integer = 45

        ' 1. Logotipo proporcional (cuadrado o rectangular)
        Dim xHeaderAware As Integer = margenIzq
        If picLogoConfig.Image IsNot Nothing Then
            Dim rectLogoAware As Rectangle = CalcularRectanguloProporcional(picLogoConfig.Image, margenIzq, Y, 100, 75)
            g.DrawImage(picLogoConfig.Image, rectLogoAware)
            xHeaderAware = margenIzq + rectLogoAware.Width + 15
        End If

        g.DrawString(txtNomFarmacia.Text.ToUpper(), fTitulo, brochaNegra, xHeaderAware, Y)
        g.DrawString(txtDireccion.Text, fSub, brochaNegra, xHeaderAware, Y + 22)
        g.DrawString("Responsable Sanitario: " & txtResponsable.Text, fSubBold, brochaNegra, xHeaderAware, Y + 38)
        g.DrawString("INFORME DE USO RACIONAL Y CLASIFICACIÓN AWaRe (OMS)", fSubBold, New SolidBrush(Drawing.Color.FromArgb(0, 102, 204)), xHeaderAware, Y + 56)

        Dim periodoStr As String = If(cmbMesAware.Text = "TODOS", "Todo el Año " & txtAnioAware.Text, "Mes: " & cmbMesAware.Text & " / " & txtAnioAware.Text)
        g.DrawString("Periodo Evaluado: " & periodoStr & " | Fecha de Emisión: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fSub, brochaNegra, xHeaderAware, Y + 72)

        Y += 95
        g.DrawLine(Pens.DarkGray, margenIzq, Y, margenDer, Y)
        Y += 15

        Dim wCard As Integer = (anchoDisp - 30) \ 4
        DibujarKpiImpresion(g, margenIzq, Y, wCard, 55, "ACCESO", cantAcceso, (cantAcceso / totalAware) * 100, Drawing.Color.FromArgb(40, 167, 69), fKpiTit, fKpiNum, fSub)
        DibujarKpiImpresion(g, margenIzq + wCard + 10, Y, wCard, 55, "VIGILANCIA", cantVigilancia, (cantVigilancia / totalAware) * 100, Drawing.Color.FromArgb(243, 156, 18), fKpiTit, fKpiNum, fSub)
        DibujarKpiImpresion(g, margenIzq + (wCard * 2) + 20, Y, wCard, 55, "RESERVA", cantReserva, (cantReserva / totalAware) * 100, Drawing.Color.FromArgb(220, 53, 69), fKpiTit, fKpiNum, fSub)
        DibujarKpiImpresion(g, margenIzq + (wCard * 3) + 30, Y, wCard, 55, "TOTAL DISPENSADO", totalAware, 100, Drawing.Color.FromArgb(0, 102, 204), fKpiTit, fKpiNum, fSub)

        Y += 70

        g.DrawString("Gráfico Proporcional de Consumo:", fSubBold, brochaNegra, margenIzq, Y)
        Y += 18

        Dim pctAcc As Double = (cantAcceso / totalAware)
        Dim pctVig As Double = (cantVigilancia / totalAware)
        Dim wAcc As Integer = CInt(anchoDisp * pctAcc)
        Dim wVig As Integer = CInt(anchoDisp * pctVig)
        Dim wRes As Integer = anchoDisp - wAcc - wVig
        If wRes < 0 Then
            wRes = 0
        End If

        If wAcc > 0 Then
            g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(40, 167, 69)), margenIzq, Y, wAcc, 24)
        End If
        If wVig > 0 Then
            g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(243, 156, 18)), margenIzq + wAcc, Y, wVig, 24)
        End If
        If wRes > 0 Then
            g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(220, 53, 69)), margenIzq + wAcc + wVig, Y, wRes, 24)
        End If

        Dim xMeta60 As Integer = margenIzq + CInt(anchoDisp * 0.6)
        Using penMeta As New Pen(Drawing.Color.DarkBlue, 2) With {.DashStyle = DashStyle.Dash}
            g.DrawLine(penMeta, xMeta60, Y - 4, xMeta60, Y + 28)
        End Using
        g.DrawString("Meta OMS (≥60%)", fSubBold, Brushes.DarkBlue, xMeta60 - 40, Y + 28)

        Y += 48
        Dim textoCumpl As String = If(pctAcc >= 0.6, "✔ CUMPLIMIENTO: La proporción del grupo Acceso cumple con la meta recomendada por la OMS (≥60%).", "⚠ OBSERVACIÓN: El consumo del grupo Acceso se encuentra por debajo del 60% recomendado.")
        g.DrawString(textoCumpl, fSubBold, If(pctAcc >= 0.6, Brushes.DarkGreen, Brushes.DarkRed), margenIzq, Y)

        Y += 25
        g.DrawLine(Pens.DarkGray, margenIzq, Y, margenDer, Y)
        Y += 15

        g.DrawString("DETALLE DE SALIDAS REGISTRADAS POR CATEGORÍA:", fSubBold, brochaNegra, margenIzq, Y)
        Y += 20

        ' Dimensiones proporcionales de la tabla AWaRe
        Dim wCatAw As Integer = CInt(anchoDisp * 0.14)
        Dim wGenAw As Integer = CInt(anchoDisp * 0.28)
        Dim wDisAw As Integer = CInt(anchoDisp * 0.18)
        Dim wPreAw As Integer = CInt(anchoDisp * 0.2)
        Dim wSurAw As Integer = CInt(anchoDisp * 0.1)
        Dim wPctAw As Integer = anchoDisp - (wCatAw + wGenAw + wDisAw + wPreAw + wSurAw)

        Dim xCatAw As Integer = margenIzq
        Dim xGenAw As Integer = xCatAw + wCatAw
        Dim xDisAw As Integer = xGenAw + wGenAw
        Dim xPreAw As Integer = xDisAw + wDisAw
        Dim xSurAw As Integer = xPreAw + wPreAw
        Dim xPctAw As Integer = xSurAw + wSurAw

        g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(235, 235, 235)), margenIzq, Y, anchoDisp, 24)
        g.DrawRectangle(Pens.Gray, margenIzq, Y, anchoDisp, 24)

        Dim sfHeaderAw As New StringFormat With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}
        g.DrawString("CATEGORÍA", fTablaHeader, brochaNegra, New RectangleF(xCatAw + 3, Y, wCatAw - 6, 24), sfHeaderAw)
        g.DrawString("GENÉRICO", fTablaHeader, brochaNegra, New RectangleF(xGenAw + 3, Y, wGenAw - 6, 24), sfHeaderAw)
        g.DrawString("DISTINTIVO", fTablaHeader, brochaNegra, New RectangleF(xDisAw + 3, Y, wDisAw - 6, 24), sfHeaderAw)
        g.DrawString("PRESENTACIÓN", fTablaHeader, brochaNegra, New RectangleF(xPreAw + 3, Y, wPreAw - 6, 24), sfHeaderAw)
        g.DrawString("SURTIDO", fTablaHeader, brochaNegra, New RectangleF(xSurAw + 3, Y, wSurAw - 6, sfHeaderAw.Alignment = StringAlignment.Far), sfHeaderAw)
        g.DrawString("% GRUPO", fTablaHeader, brochaNegra, New RectangleF(xPctAw + 3, Y, wPctAw - 6, 24), sfHeaderAw)

        Y += 24

        Dim filasImpresas As Integer = 0
        For Each r As DataRow In dtDetalleAwareSource.Rows
            Dim cat As String = r("Categoría AWaRe").ToString()
            Dim gen As String = r("Genérico").ToString().Trim()
            Dim dis As String = r("Distintivo").ToString().Trim()
            Dim pre As String = r("Presentación").ToString().Trim()
            Dim surt As String = Convert.ToDouble(r("Cajas Surtidas")).ToString("N0")
            Dim pctG As String = r("% de su Grupo").ToString()

            ' Autoajuste de altura completa sin cortar texto
            Dim sfGen As SizeF = g.MeasureString(gen, fTabla, wGenAw - 6)
            Dim sfDis As SizeF = g.MeasureString(dis, fTabla, wDisAw - 6)
            Dim sfPre As SizeF = g.MeasureString(pre, fTabla, wPreAw - 6)
            Dim altoFilaAw As Single = Math.Max(22.0F, Math.Max(sfGen.Height, Math.Max(sfDis.Height, sfPre.Height)) + 4.0F)

            If Y + altoFilaAw > e.PageBounds.Height - 110 Then
                Exit For
            End If

            g.DrawString(cat, fTabla, brochaNegra, New RectangleF(xCatAw + 3, Y + 2, wCatAw - 6, altoFilaAw))
            g.DrawString(gen, fTabla, brochaNegra, New RectangleF(xGenAw + 3, Y + 2, wGenAw - 6, altoFilaAw))
            g.DrawString(dis, fTabla, brochaNegra, New RectangleF(xDisAw + 3, Y + 2, wDisAw - 6, altoFilaAw))
            g.DrawString(pre, fTabla, brochaNegra, New RectangleF(xPreAw + 3, Y + 2, wPreAw - 6, altoFilaAw))
            g.DrawString(surt, fTabla, brochaNegra, New RectangleF(xSurAw + 3, Y + 2, wSurAw - 6, altoFilaAw))
            g.DrawString(pctG, fTabla, brochaNegra, New RectangleF(xPctAw + 3, Y + 2, wPctAw - 6, altoFilaAw))

            Y += CInt(altoFilaAw)
            g.DrawLine(Pens.Gainsboro, margenIzq, Y, margenDer, Y)
            filasImpresas += 1
        Next

        Y = e.PageBounds.Height - 110
        g.DrawLine(Pens.Black, margenIzq + 180, Y, margenDer - 180, Y)
        Y += 8

        Dim sfCentro As New StringFormat With {.Alignment = StringAlignment.Center}
        g.DrawString(txtResponsable.Text.ToUpper(), fSubBold, brochaNegra, e.PageBounds.Width \ 2, Y, sfCentro)
        Y += 16
        g.DrawString("Responsable Sanitario", fSub, brochaNegra, e.PageBounds.Width \ 2, Y, sfCentro)

        e.HasMorePages = False
    End Sub

    Private Sub DibujarKpiImpresion(g As Graphics, x As Integer, y As Integer, w As Integer, h As Integer, titulo As String, total As Double, pct As Double, colorBorde As Drawing.Color, fTit As Font, fNum As Font, fSub As Font)
        g.FillRectangle(Brushes.White, x, y, w, h)
        g.DrawRectangle(New Pen(colorBorde, 1.5F), x, y, w, h)
        g.FillRectangle(New SolidBrush(colorBorde), x, y, w, 18)

        Dim sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
        g.DrawString(titulo, fTit, Brushes.White, New RectangleF(x, y, w, 18), sf)
        g.DrawString(total.ToString("N0") & " cajas", fNum, Brushes.Black, New RectangleF(x, y + 19, w, 20), sf)
        g.DrawString(pct.ToString("0.0") & "% del total", fSub, Brushes.DimGray, New RectangleF(x, y + 37, w, 16), sf)
    End Sub


    ' =========================================================
    ' 10. PANTALLA DE REPORTES REGULARES
    ' =========================================================
    Private Sub ConfigurarPantallaReportes()
        panelReportes.Dock = DockStyle.Fill
        panelReportes.BackColor = Drawing.Color.White
        panelReportes.AutoScroll = True
        pnlContenedorVistas.Controls.Add(panelReportes)
        HabilitarDobleBuffer(panelReportes)

        Dim lblTitulo As New Label With {.Text = "Generador de Reportes (Bitácora Oficial)", .Location = New Point(35, 25), .Font = New Drawing.Font("Segoe UI", 16.0F, Drawing.FontStyle.Bold), .AutoSize = True}

        Dim lblMod As New Label With {.Text = "Módulo a imprimir:", .Location = New Point(35, 80), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 11.0F)}
        cmbModuloRep.Items.AddRange(New String() {"Entradas", "Salidas"})
        cmbModuloRep.Location = New Point(200, 78)
        cmbModuloRep.Size = New Size(200, 30)
        cmbModuloRep.Font = New Drawing.Font("Segoe UI", 11.0F)
        cmbModuloRep.DropDownStyle = ComboBoxStyle.DropDownList

        Dim lblMes As New Label With {.Text = "Mes (MM):", .Location = New Point(35, 130), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 11.0F)}
        cmbMesRep.Items.AddRange(New String() {"01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"})
        cmbMesRep.Location = New Point(200, 128)
        cmbMesRep.Size = New Size(100, 30)
        cmbMesRep.Font = New Drawing.Font("Segoe UI", 11.0F)
        cmbMesRep.DropDownStyle = ComboBoxStyle.DropDownList

        Dim lblAnio As New Label With {.Text = "Año (AAAA):", .Location = New Point(35, 180), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 11.0F)}
        txtAnioRep.Location = New Point(200, 178)
        txtAnioRep.Size = New Size(100, 30)
        txtAnioRep.Font = New Drawing.Font("Segoe UI", 11.0F)
        txtAnioRep.Text = DateTime.Now.Year.ToString()

        btnGenerarRep.Text = "🖨 Vista Previa e Imprimir"
        btnGenerarRep.Location = New Point(35, 240)
        btnGenerarRep.Size = New Size(365, 48)
        btnGenerarRep.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        btnGenerarRep.ForeColor = Drawing.Color.White
        btnGenerarRep.Font = New Drawing.Font("Segoe UI", 12.0F, Drawing.FontStyle.Bold)
        btnGenerarRep.FlatStyle = FlatStyle.Flat
        btnGenerarRep.FlatAppearance.BorderSize = 0
        btnGenerarRep.Cursor = Cursors.Hand
        RedondearBoton(btnGenerarRep, 16)

        panelReportes.Controls.Add(lblTitulo)
        panelReportes.Controls.Add(lblMod)
        panelReportes.Controls.Add(cmbModuloRep)
        panelReportes.Controls.Add(lblMes)
        panelReportes.Controls.Add(cmbMesRep)
        panelReportes.Controls.Add(lblAnio)
        panelReportes.Controls.Add(txtAnioRep)
        panelReportes.Controls.Add(btnGenerarRep)
    End Sub

    Private Sub btnGenerarRep_Click(sender As Object, e As EventArgs) Handles btnGenerarRep.Click
        If cmbModuloRep.Text = "" Or cmbMesRep.Text = "" Or txtAnioRep.Text = "" Then
            MessageBox.Show("Por favor completa todos los campos del filtro.", "Faltan datos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return
        End If

        Dim filtroFecha As String = "%/" & cmbMesRep.Text & "/" & txtAnioRep.Text & "%"
        dtImprimir.Clear()

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = ""

            If cmbModuloRep.Text = "Salidas" Then
                consulta = "SELECT S.*, M.NombreMed, M.Calle, M.NoInt, M.NoExt, M.Colonia, M.Ciudad, M.Estado, M.CP, M.Pais, M.Tel AS TelMed " &
                           "FROM Salidas S LEFT JOIN Medicos M ON TRIM(S.Cedula) = TRIM(M.Cedula) " &
                           "WHERE S.Fecha LIKE @filtro"
            Else
                consulta = "SELECT E.*, P.RFC AS RFCProv, P.Direccion AS DirProv " &
                           "FROM Entradas E LEFT JOIN Proveedores P ON TRIM(E.Proveedor) = TRIM(P.Proveedor) " &
                           "WHERE E.Fecha LIKE @filtro"
            End If

            Using comando As New SQLiteCommand(consulta, conexion)
                comando.Parameters.AddWithValue("@filtro", filtroFecha)
                Using adaptador As New SQLiteDataAdapter(comando)
                    adaptador.Fill(dtImprimir)
                End Using
            End Using
        End Using

        If dtImprimir.Rows.Count = 0 Then
            MessageBox.Show("No se encontraron registros para este mes.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim vistaPrevia As New PrintPreviewDialog()
        docImprimir.DefaultPageSettings.Landscape = True
        vistaPrevia.Document = docImprimir
        vistaPrevia.WindowState = FormWindowState.Maximized
        vistaPrevia.ShowDialog()
    End Sub

    Private Sub docImprimir_BeginPrint(sender As Object, e As PrintEventArgs) Handles docImprimir.BeginPrint
        indiceImpresion = 0
        numPaginaReporte = 0
    End Sub

    ' =========================================================================
    ' MOTOR DE IMPRESIÓN OFICIAL: AUTOAJUSTE DE LOTES LARGOS Y TODA LA INFORMACIÓN
    ' =========================================================================
    Private Sub docImprimir_PrintPage(sender As Object, e As PrintPageEventArgs) Handles docImprimir.PrintPage
        numPaginaReporte += 1
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim fuenteTitulo As New Font("Segoe UI", 13, FontStyle.Bold)
        Dim fuenteSub As New Font("Segoe UI", 8.5F, FontStyle.Regular)
        Dim fuenteSubBold As New Font("Segoe UI", 8.5F, FontStyle.Bold)
        Dim fuenteTablaHeader As New Font("Segoe UI", 7.5F, FontStyle.Bold)
        Dim fuenteTabla As New Font("Segoe UI", 7.5F, FontStyle.Regular)
        Dim brochaNegra As New SolidBrush(Drawing.Color.Black)
        Dim brochaAzul As New SolidBrush(Drawing.Color.FromArgb(0, 102, 204))

        Dim margenIzq As Integer = 35
        Dim margenDer As Integer = e.PageBounds.Width - 35
        Dim anchoTotal As Integer = margenDer - margenIzq
        Dim Y As Integer = 35

        ' 1. Logotipo proporcional (cuadrado o rectangular)
        Dim xHeader As Integer = margenIzq
        If picLogoConfig.Image IsNot Nothing Then
            Dim rectLogo As Rectangle = CalcularRectanguloProporcional(picLogoConfig.Image, margenIzq, Y, 110, 75)
            g.DrawImage(picLogoConfig.Image, rectLogo)
            xHeader = margenIzq + rectLogo.Width + 15
        End If

        g.DrawString(txtNomFarmacia.Text.Trim().ToUpper(), fuenteTitulo, brochaAzul, xHeader, Y)
        g.DrawString(txtDireccion.Text.Trim(), fuenteSub, brochaNegra, xHeader, Y + 22)
        g.DrawString("Responsable Sanitario: " & txtResponsable.Text.Trim(), fuenteSubBold, brochaNegra, xHeader, Y + 37)
        g.DrawString("BITÁCORA OFICIAL DE CONTROL DE GRUPO IV - " & cmbModuloRep.Text.ToUpper(), fuenteSubBold, brochaNegra, xHeader, Y + 52)

        Dim strPeriodo As String = "Periodo: " & cmbMesRep.Text & "/" & txtAnioRep.Text & " | Emisión: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm") & " | Pág. " & numPaginaReporte.ToString()
        g.DrawString(strPeriodo, fuenteSub, Brushes.DimGray, xHeader, Y + 67)

        Y += 85
        g.DrawLine(New Pen(Drawing.Color.FromArgb(0, 102, 204), 1.5F), margenIzq, Y, margenDer, Y)
        Y += 8

        ' 2. Dimensiones proporcionales calculadas con mayor espacio para lotes largos
        Dim wFecha As Integer = CInt(anchoTotal * 0.065)
        Dim wCodigo As Integer = CInt(anchoTotal * 0.06)
        Dim wMedicamento As Integer = CInt(anchoTotal * (If(cmbModuloRep.Text = "Entradas", 0.23, 0.21)))
        Dim wLoteCad As Integer = CInt(anchoTotal * 0.11) ' Ampliado a 11% para lotes largos sin apretar
        Dim wStock As Integer = CInt(anchoTotal * 0.105)
        Dim wMovFac As Integer = CInt(anchoTotal * (If(cmbModuloRep.Text = "Entradas", 0.105, 0.095)))
        Dim wTercero As Integer = anchoTotal - (wFecha + wCodigo + wMedicamento + wLoteCad + wStock + wMovFac)

        Dim xFecha As Integer = margenIzq
        Dim xCodigo As Integer = xFecha + wFecha
        Dim xMed As Integer = xCodigo + wCodigo
        Dim xLote As Integer = xMed + wMedicamento
        Dim xStock As Integer = xLote + wLoteCad
        Dim xMovFac As Integer = xStock + wStock
        Dim xTercero As Integer = xMovFac + wMovFac

        Dim altoHeader As Integer = 28
        g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(235, 240, 248)), margenIzq, Y, anchoTotal, altoHeader)
        g.DrawRectangle(Pens.LightGray, margenIzq, Y, anchoTotal, altoHeader)

        Dim sfHeader As New StringFormat With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}

        ' Dibujar títulos con cajas delimitadoras RectangleF
        g.DrawString("FECHA", fuenteTablaHeader, brochaNegra, New RectangleF(xFecha + 3, Y, wFecha - 6, altoHeader), sfHeader)
        g.DrawString("CÓDIGO", fuenteTablaHeader, brochaNegra, New RectangleF(xCodigo + 3, Y, wCodigo - 6, altoHeader), sfHeader)
        g.DrawString("MEDICAMENTO (GENÉRICO / DIST. / AWARE)", fuenteTablaHeader, brochaNegra, New RectangleF(xMed + 3, Y, wMedicamento - 6, altoHeader), sfHeader)
        g.DrawString("LOTE / CADUCIDAD", fuenteTablaHeader, brochaNegra, New RectangleF(xLote + 3, Y, wLoteCad - 6, altoHeader), sfHeader)
        g.DrawString("EXIST / CANT / SALDO", fuenteTablaHeader, brochaNegra, New RectangleF(xStock + 3, Y, wStock - 6, altoHeader), sfHeader)

        If cmbModuloRep.Text = "Entradas" Then
            g.DrawString("FACTURA", fuenteTablaHeader, brochaNegra, New RectangleF(xMovFac + 3, Y, wMovFac - 6, altoHeader), sfHeader)
            g.DrawString("PROVEEDOR (RAZÓN SOCIAL, RFC, DIRECCIÓN)", fuenteTablaHeader, brochaNegra, New RectangleF(xTercero + 3, Y, wTercero - 6, altoHeader), sfHeader)
        Else
            g.DrawString("MOV. / FOLIO", fuenteTablaHeader, brochaNegra, New RectangleF(xMovFac + 3, Y, wMovFac - 6, altoHeader), sfHeader)
            g.DrawString("MÉDICO PRESCRIPTOR (DATOS COMPLETOS)", fuenteTablaHeader, brochaNegra, New RectangleF(xTercero + 3, Y, wTercero - 6, altoHeader), sfHeader)
        End If

        ' Líneas divisorias de columnas en encabezados
        g.DrawLine(Pens.LightGray, xCodigo, Y, xCodigo, Y + altoHeader)
        g.DrawLine(Pens.LightGray, xMed, Y, xMed, Y + altoHeader)
        g.DrawLine(Pens.LightGray, xLote, Y, xLote, Y + altoHeader)
        g.DrawLine(Pens.LightGray, xStock, Y, xStock, Y + altoHeader)
        g.DrawLine(Pens.LightGray, xMovFac, Y, xMovFac, Y + altoHeader)
        g.DrawLine(Pens.LightGray, xTercero, Y, xTercero, Y + altoHeader)

        Y += altoHeader

        ' 3. Filas de Datos con autoajuste dinámico de altura considerando TODAS las columnas
        While indiceImpresion < dtImprimir.Rows.Count
            Dim fila As DataRow = dtImprimir.Rows(indiceImpresion)

            Dim fechaStr As String = fila("Fecha").ToString().Split(" ")(0)
            Dim codigoStr As String = If(fila.Table.Columns.Contains("Codigo"), fila("Codigo").ToString(), "")

            Dim genStr As String = fila("Generico").ToString().Trim()
            Dim distStr As String = If(fila.Table.Columns.Contains("Distintivo"), fila("Distintivo").ToString().Trim(), "")
            Dim presStr As String = If(fila.Table.Columns.Contains("Presentacion"), fila("Presentacion").ToString().Trim(), "")
            Dim awareStr As String = If(fila.Table.Columns.Contains("AWARE"), fila("AWARE").ToString().Trim().ToUpper(), "")

            Dim medCompleto As String = genStr
            If distStr <> "" OrElse presStr <> "" Then
                medCompleto &= vbCrLf & distStr & If(distStr <> "" AndAlso presStr <> "", " - ", "") & presStr
            End If
            If awareStr <> "" Then
                medCompleto &= vbCrLf & "[" & awareStr & "]"
            End If

            ' Formateo de lote y caducidad
            Dim loteVal As String = fila("Lote").ToString().Trim()
            Dim cadVal As String = fila("Caducidad").ToString().Trim()
            Dim loteCadStr As String = "Lot: " & loteVal & vbCrLf & "Cad: " & cadVal

            Dim exisVal As Double = If(fila.Table.Columns.Contains("Existencia") AndAlso Not IsDBNull(fila("Existencia")), Convert.ToDouble(fila("Existencia")), 0)
            Dim surtVal As Double = If(fila.Table.Columns.Contains("Surtido") AndAlso Not IsDBNull(fila("Surtido")), Convert.ToDouble(fila("Surtido")), 0)
            Dim saldoVal As Double = If(fila.Table.Columns.Contains("Saldo") AndAlso Not IsDBNull(fila("Saldo")), Convert.ToDouble(fila("Saldo")), 0)
            Dim stockStr As String = "Ant: " & exisVal.ToString("N0") & vbCrLf &
                                     If(cmbModuloRep.Text = "Entradas", "Ent: ", "Surt: ") & surtVal.ToString("N0") & vbCrLf &
                                     "Saldo: " & saldoVal.ToString("N0")

            Dim movFacStr As String = ""
            Dim terceroStr As String = ""

            If cmbModuloRep.Text = "Entradas" Then
                movFacStr = "Factura:" & vbCrLf & fila("Factura").ToString().Trim()
                Dim prov As String = fila("Proveedor").ToString().Trim()
                Dim rfc As String = If(fila.Table.Columns.Contains("RFCProv") AndAlso fila("RFCProv").ToString().Trim() <> "",
                                       fila("RFCProv").ToString().Trim(),
                                       If(fila.Table.Columns.Contains("RFC"), fila("RFC").ToString().Trim(), ""))
                Dim dirProv As String = If(fila.Table.Columns.Contains("DirProv") AndAlso fila("DirProv").ToString().Trim() <> "",
                                           fila("DirProv").ToString().Trim(),
                                           If(fila.Table.Columns.Contains("Direccion"), fila("Direccion").ToString().Trim(), ""))
                terceroStr = prov & If(rfc <> "", " (RFC: " & rfc & ")", "") & vbCrLf & dirProv
            Else
                ' MODULO SALIDAS: Ficha completa del Médico
                Dim mov As String = If(fila.Table.Columns.Contains("Movimiento"), fila("Movimiento").ToString().Trim(), "")
                Dim fol As String = If(fila.Table.Columns.Contains("Folio"), fila("Folio").ToString().Trim(), "")
                movFacStr = mov & vbCrLf & "Fol: " & fol

                Dim nomMed As String = If(fila.Table.Columns.Contains("NombreMed") AndAlso fila("NombreMed").ToString().Trim() <> "",
                                          fila("NombreMed").ToString().Trim(),
                                          If(fila.Table.Columns.Contains("Nombre"), fila("Nombre").ToString().Trim(), ""))
                Dim cedMed As String = If(fila.Table.Columns.Contains("Cedula"), fila("Cedula").ToString().Trim(), "")
                Dim telMed As String = If(fila.Table.Columns.Contains("TelMed") AndAlso fila("TelMed").ToString().Trim() <> "",
                                          fila("TelMed").ToString().Trim(),
                                          If(fila.Table.Columns.Contains("Telefono"), fila("Telefono").ToString().Trim(), ""))

                Dim calle As String = If(fila.Table.Columns.Contains("Calle"), fila("Calle").ToString().Trim(), "")
                Dim noExt As String = If(fila.Table.Columns.Contains("NoExt"), fila("NoExt").ToString().Trim(), "")
                Dim noInt As String = If(fila.Table.Columns.Contains("NoInt"), fila("NoInt").ToString().Trim(), "")
                Dim col As String = If(fila.Table.Columns.Contains("Colonia"), fila("Colonia").ToString().Trim(), "")
                Dim ciudad As String = If(fila.Table.Columns.Contains("Ciudad"), fila("Ciudad").ToString().Trim(), "")
                Dim estado As String = If(fila.Table.Columns.Contains("Estado"), fila("Estado").ToString().Trim(), "")
                Dim cp As String = If(fila.Table.Columns.Contains("CP"), fila("CP").ToString().Trim(), "")

                Dim partesDir As New List(Of String)()
                Dim lineaCalle As String = calle
                If noExt <> "" Then lineaCalle &= If(lineaCalle <> "", " #", "#") & noExt
                If noInt <> "" Then lineaCalle &= If(lineaCalle <> "", " Int. ", "Int. ") & noInt
                If lineaCalle <> "" Then partesDir.Add(lineaCalle)

                If col <> "" Then partesDir.Add("Col. " & col)
                If ciudad <> "" OrElse estado <> "" Then
                    partesDir.Add(ciudad & If(ciudad <> "" AndAlso estado <> "", ", ", "") & estado)
                End If
                If cp <> "" Then partesDir.Add("C.P. " & cp)

                Dim dirMed As String = ""
                If partesDir.Count > 0 Then
                    dirMed = String.Join(", ", partesDir)
                Else
                    dirMed = If(fila.Table.Columns.Contains("Direccion"), fila("Direccion").ToString().Trim(), "")
                End If

                terceroStr = nomMed & If(cedMed <> "", " (Céd: " & cedMed & ")", "") & vbCrLf &
                             If(dirMed <> "", "Dir: " & dirMed, "Dir: S/D") &
                             If(telMed <> "", " | Tel: " & telMed, "")
            End If

            ' Medir altura requerida de TODAS las columnas (incluyendo LOTE, MEDICAMENTO, MÉDICO/PROVEEDOR, STOCK)
            Dim sfFecha As SizeF = g.MeasureString(fechaStr, fuenteTabla, wFecha - 6)
            Dim sfCodigo As SizeF = g.MeasureString(codigoStr, fuenteTabla, wCodigo - 6)
            Dim sfMed As SizeF = g.MeasureString(medCompleto, fuenteTabla, wMedicamento - 6)
            Dim sfLote As SizeF = g.MeasureString(loteCadStr, fuenteTabla, wLoteCad - 6)
            Dim sfStock As SizeF = g.MeasureString(stockStr, fuenteTabla, wStock - 6)
            Dim sfMov As SizeF = g.MeasureString(movFacStr, fuenteTabla, wMovFac - 6)
            Dim sfTercero As SizeF = g.MeasureString(terceroStr, fuenteTabla, wTercero - 6)

            Dim maxAlturaContenido As Single = Math.Max(sfFecha.Height,
                                               Math.Max(sfCodigo.Height,
                                               Math.Max(sfMed.Height,
                                               Math.Max(sfLote.Height,
                                               Math.Max(sfStock.Height,
                                               Math.Max(sfMov.Height, sfTercero.Height))))))

            Dim altoFila As Single = Math.Max(32.0F, maxAlturaContenido + 8.0F)

            ' Salto de página
            If Y + altoFila > e.PageBounds.Height - 110 AndAlso indiceImpresion < dtImprimir.Rows.Count Then
                e.HasMorePages = True
                Return
            End If

            ' Dibujar datos en sus celdas delimitadas (ajuste de texto automático si el lote es largo)
            g.DrawString(fechaStr, fuenteTabla, brochaNegra, New RectangleF(xFecha + 3, Y + 3, wFecha - 6, altoFila))
            g.DrawString(codigoStr, fuenteTabla, brochaNegra, New RectangleF(xCodigo + 3, Y + 3, wCodigo - 6, altoFila))
            g.DrawString(medCompleto, fuenteTabla, brochaNegra, New RectangleF(xMed + 3, Y + 3, wMedicamento - 6, altoFila))
            g.DrawString(loteCadStr, fuenteTabla, brochaNegra, New RectangleF(xLote + 3, Y + 3, wLoteCad - 6, altoFila))
            g.DrawString(stockStr, fuenteTabla, brochaNegra, New RectangleF(xStock + 3, Y + 3, wStock - 6, altoFila))
            g.DrawString(movFacStr, fuenteTabla, brochaNegra, New RectangleF(xMovFac + 3, Y + 3, wMovFac - 6, altoFila))
            g.DrawString(terceroStr, fuenteTabla, brochaNegra, New RectangleF(xTercero + 3, Y + 3, wTercero - 6, altoFila))

            ' Líneas divisorias de columnas en datos
            g.DrawLine(Pens.Gainsboro, xCodigo, Y, xCodigo, Y + altoFila)
            g.DrawLine(Pens.Gainsboro, xMed, Y, xMed, Y + altoFila)
            g.DrawLine(Pens.Gainsboro, xLote, Y, xLote, Y + altoFila)
            g.DrawLine(Pens.Gainsboro, xStock, Y, xStock, Y + altoFila)
            g.DrawLine(Pens.Gainsboro, xMovFac, Y, xMovFac, Y + altoFila)
            g.DrawLine(Pens.Gainsboro, xTercero, Y, xTercero, Y + altoFila)

            Y += CInt(altoFila)
            g.DrawLine(Pens.Gainsboro, margenIzq, Y, margenDer, Y)
            indiceImpresion += 1
        End While

        ' 4. Firma del Responsable Sanitario en la última hoja
        Y = Math.Max(Y + 20, e.PageBounds.Height - 90)
        Dim firmaTexto As String = "________________________________________________"
        Dim respTexto As String = "Responsable Sanitario: " & txtResponsable.Text.Trim()

        Dim anchoFirma As Integer = CInt(g.MeasureString(firmaTexto, fuenteSub).Width)
        Dim anchoResp As Integer = CInt(g.MeasureString(respTexto, fuenteSubBold).Width)
        Dim centroX As Integer = e.PageBounds.Width \ 2

        g.DrawString(firmaTexto, fuenteSub, brochaNegra, centroX - (anchoFirma \ 2), Y)
        Y += 16
        g.DrawString(respTexto, fuenteSubBold, brochaNegra, centroX - (anchoResp \ 2), Y)

        e.HasMorePages = False
        indiceImpresion = 0
    End Sub


    ' =========================================================
    ' 11. IMPORTADOR INTELIGENTE DE CSV
    ' =========================================================
    Private Sub btnImportarCSV_Click(sender As Object, e As EventArgs) Handles btnImportarCSV.Click
        Dim dialog As New OpenFileDialog()
        dialog.Filter = "Archivos CSV de Excel (*.csv)|*.csv"
        dialog.Title = "Selecciona tu archivo guardado como CSV"

        If dialog.ShowDialog() = DialogResult.OK Then
            Try
                Using parser As New TextFieldParser(dialog.FileName, System.Text.Encoding.Default)
                    parser.TextFieldType = FieldType.Delimited
                    parser.SetDelimiters(",")

                    If parser.EndOfData Then
                        Return
                    End If

                    Dim encabezados As String() = parser.ReadFields()
                    Dim encabStr As String = String.Join("", encabezados).ToUpper().Replace(" ", "")

                    Dim tablaDestino As String = ""
                    Dim insertSQL As String = ""

                    If encabStr.Contains("EXISTENCIA") OrElse encabStr.Contains("AWARE") Then
                        tablaDestino = "Inventario"
                        insertSQL = "INSERT INTO Inventario (Codigo, Generico, Distintivo, Presentacion, AWARE, ExistenciaActual) VALUES (@p0, @p1, @p2, @p3, @p4, @p5) ON CONFLICT(Codigo) DO UPDATE SET Generico=@p1, Distintivo=@p2, Presentacion=@p3, ExistenciaActual=@p5"
                    ElseIf encabStr.Contains("RFC") AndAlso encabStr.Contains("PROVEEDOR") Then
                        tablaDestino = "Proveedores"
                        insertSQL = "INSERT INTO Proveedores (Proveedor, RFC, Direccion) VALUES (@p0, @p1, @p2) ON CONFLICT(Proveedor) DO UPDATE SET RFC=@p1, Direccion=@p2"
                    ElseIf encabStr.Contains("CEDULA") AndAlso encabStr.Contains("NOMBREMED") Then
                        tablaDestino = "Medicos"
                        insertSQL = "INSERT INTO Medicos (Cedula, NombreMed, Calle, NoInt, NoExt, Colonia, Ciudad, Estado, CP, Pais, Tel) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10) ON CONFLICT(Cedula) DO UPDATE SET NombreMed=@p1, Tel=@p10"
                    Else
                        MessageBox.Show("No reconocí el formato del archivo CSV.", "Formato Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If

                    If MessageBox.Show("Se detectaron datos para: " & tablaDestino & "." & vbCrLf & "¿Deseas importar?", "Confirmar Importación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        Dim registros As Integer = 0
                        Using conexion As New SQLiteConnection(cadenaConexion)
                            conexion.Open()
                            Using transaccion As SQLiteTransaction = conexion.BeginTransaction()
                                Using comando As New SQLiteCommand(insertSQL, conexion, transaccion)
                                    While Not parser.EndOfData
                                        Dim datos As String() = parser.ReadFields()
                                        If datos Is Nothing Then
                                            Continue While
                                        End If

                                        comando.Parameters.Clear()

                                        If tablaDestino = "Inventario" Then
                                            comando.Parameters.AddWithValue("@p0", If(datos.Length > 0, datos(0), ""))
                                            comando.Parameters.AddWithValue("@p1", If(datos.Length > 1, datos(1), ""))
                                            comando.Parameters.AddWithValue("@p2", If(datos.Length > 2, datos(2), ""))
                                            comando.Parameters.AddWithValue("@p3", If(datos.Length > 3, datos(3), ""))
                                            comando.Parameters.AddWithValue("@p4", If(datos.Length > 4, datos(4), ""))
                                            comando.Parameters.AddWithValue("@p5", If(datos.Length > 5, Val(datos(5)), 0))
                                        ElseIf tablaDestino = "Proveedores" Then
                                            comando.Parameters.AddWithValue("@p0", If(datos.Length > 0, datos(0), ""))
                                            comando.Parameters.AddWithValue("@p1", If(datos.Length > 1, datos(1), ""))
                                            comando.Parameters.AddWithValue("@p2", If(datos.Length > 2, datos(2), ""))
                                        ElseIf tablaDestino = "Medicos" Then
                                            For j As Integer = 0 To 10
                                                comando.Parameters.AddWithValue("@p" & j, If(datos.Length > j, datos(j), ""))
                                            Next
                                        End If

                                        comando.ExecuteNonQuery()
                                        registros += 1
                                    End While
                                End Using
                                transaccion.Commit()
                            End Using
                        End Using
                        MessageBox.Show("¡Éxito! Se actualizaron " & registros & " registros en " & tablaDestino & ".", "Finalizado", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Error al leer tu archivo: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnNuevaEntrada_Click(sender As Object, e As EventArgs) Handles btnNuevaEntrada.Click
        Dim ventanaCaptura As New FormEntrada()
        ventanaCaptura.ShowDialog()
        If DataGridView1.Visible AndAlso Not panelInicio.Visible Then
            ConfigurarTablaEntradas()
        End If
    End Sub

    Private Sub btnNuevaSalida_Click(sender As Object, e As EventArgs) Handles btnNuevaSalida.Click
        Dim ventanaSalida As New FormSalida()
        ventanaSalida.ShowDialog()
        If DataGridView1.Visible AndAlso Not panelInicio.Visible Then
            ConfigurarTablaSalidas()
        End If
    End Sub


    ' =========================================================
    ' 12. ESTILO VISUAL DE LA BARRA LATERAL (FLUENT DESIGN)
    ' =========================================================
    Private Sub AplicarEstiloFluent()
        Panel1.Dock = DockStyle.Left
        Panel1.BackColor = Drawing.Color.FromArgb(243, 243, 243)
        Panel1.Padding = New Padding(12, 12, 8, 12)

        Button1.Text = "🏠 Inicio"
        Button2.Text = "📥 Entradas"
        Button3.Text = "📤 Salidas"
        Button4.Text = "🩺 Médicos"
        Button5.Text = "🚚 Proveedores"
        Button6.Text = "📦 Inventario"
        Button7.Text = "⚙ Configuración"
        Button8.Text = "🖨 Reportes"
        Button9.Text = "📊 Módulo AWaRe"

        For Each control As Control In Panel1.Controls
            If TypeOf control Is Button Then
                Dim btn As Button = CType(control, Button)
                btn.FlatStyle = FlatStyle.Flat
                btn.FlatAppearance.BorderSize = 0
                btn.BackColor = Drawing.Color.FromArgb(243, 243, 243)
                btn.ForeColor = Drawing.Color.FromArgb(40, 40, 40)
                btn.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Regular)
                btn.TextAlign = Drawing.ContentAlignment.MiddleLeft
                btn.Padding = New Padding(14, 0, 0, 0)
                btn.Height = 42
                btn.Dock = DockStyle.Top
                btn.Margin = New Padding(0, 2, 0, 2)
                btn.FlatAppearance.MouseOverBackColor = Drawing.Color.FromArgb(232, 232, 232)
                btn.FlatAppearance.MouseDownBackColor = Drawing.Color.FromArgb(215, 215, 215)
                RedondearBoton(btn, 10)
            End If
        Next

        Button1.BringToFront()
        Button2.BringToFront()
        Button3.BringToFront()
        Button4.BringToFront()
        Button5.BringToFront()
        Button6.BringToFront()
        Button7.BringToFront()
        Button8.BringToFront()
        Button9.BringToFront()

        Panel1.SendToBack()
        pnlContenedorVistas.BringToFront()

        Me.BackColor = Drawing.Color.White
    End Sub

    Private Sub ConfigurarContenedorDataGridView()
        pnlContenedorVistas.Controls.Add(DataGridView1)
        DataGridView1.Dock = DockStyle.Fill
        HabilitarDobleBuffer(DataGridView1)
    End Sub

    Private Sub AplicarEstiloTabla()
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.BackgroundColor = Drawing.Color.White
        DataGridView1.BorderStyle = BorderStyle.None
        DataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        DataGridView1.GridColor = Drawing.Color.FromArgb(230, 230, 230)
        DataGridView1.RowHeadersVisible = False

        DataGridView1.EnableHeadersVisualStyles = False
        DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Drawing.Color.FromArgb(0, 102, 204)
        DataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
        DataGridView1.ColumnHeadersDefaultCellStyle.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
        DataGridView1.ColumnHeadersHeight = 40

        DataGridView1.DefaultCellStyle.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Regular)
        DataGridView1.DefaultCellStyle.SelectionBackColor = Drawing.Color.FromArgb(204, 232, 255)
        DataGridView1.DefaultCellStyle.SelectionForeColor = Drawing.Color.Black
        DataGridView1.RowTemplate.Height = 35
        DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(249, 249, 249)
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
    End Sub


    ' =========================================================
    ' 13. CONFIGURACIÓN DE TABLAS Y LECTURA SQLITE
    ' =========================================================
    Private Sub ConfigurarTablaEntradas()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Id", "Id")
        DataGridView1.Columns("Id").Visible = False
        DataGridView1.Columns.Add("Fecha", "Fecha")
        DataGridView1.Columns.Add("Codigo", "Código")
        DataGridView1.Columns.Add("Generico", "Genérico")
        DataGridView1.Columns.Add("Distintivo", "Distintivo")
        DataGridView1.Columns.Add("Presentacion", "Presentación")
        DataGridView1.Columns.Add("AWARE", "AWARE")
        DataGridView1.Columns.Add("Lote", "Lote")
        DataGridView1.Columns.Add("Caducidad", "Caducidad")
        DataGridView1.Columns.Add("Existencia", "Existencia")
        DataGridView1.Columns.Add("Surtido", "Surtido")
        DataGridView1.Columns.Add("Saldo", "Saldo")
        DataGridView1.Columns.Add("Factura", "Factura")
        DataGridView1.Columns.Add("Proveedor", "Proveedor")
        DataGridView1.Columns.Add("RFC", "RFC")
        DataGridView1.Columns.Add("Direccion", "Direccion")

        Dim btnRevertir As New DataGridViewButtonColumn()
        btnRevertir.Name = "AccionRevertir"
        btnRevertir.HeaderText = "Acción"
        btnRevertir.Text = "✖ Revertir"
        btnRevertir.UseColumnTextForButtonValue = True
        btnRevertir.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnRevertir)

        AplicarEstiloTabla()

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Entradas"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Id"), lector("Fecha"), lector("Codigo"), lector("Generico"), lector("Distintivo"),
                                               lector("Presentacion"), lector("AWARE"), lector("Lote"), lector("Caducidad"),
                                               lector("Existencia"), lector("Surtido"), lector("Saldo"), lector("Factura"),
                                               lector("Proveedor"), lector("RFC"), lector("Direccion"))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub ConfigurarTablaInventario()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Codigo", "Código")
        DataGridView1.Columns.Add("Generico", "Genérico")
        DataGridView1.Columns.Add("Distintivo", "Distintivo")
        DataGridView1.Columns.Add("Presentacion", "Presentación")
        DataGridView1.Columns.Add("AWARE", "AWARE")
        DataGridView1.Columns.Add("ExistenciaActual", "Existencia Actual")

        Dim btnBorrar As New DataGridViewButtonColumn()
        btnBorrar.Name = "AccionRevertir"
        btnBorrar.HeaderText = "Acción"
        btnBorrar.Text = "✖ Eliminar"
        btnBorrar.UseColumnTextForButtonValue = True
        btnBorrar.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnBorrar)

        AplicarEstiloTabla()

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Inventario"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Codigo"), lector("Generico"), lector("Distintivo"),
                                               lector("Presentacion"), lector("AWARE"), lector("ExistenciaActual"))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub ConfigurarTablaProveedores()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Proveedor", "Proveedor")
        DataGridView1.Columns.Add("RFC", "RFC")
        DataGridView1.Columns.Add("Direccion", "Direccion")

        Dim btnBorrar As New DataGridViewButtonColumn()
        btnBorrar.Name = "AccionRevertir"
        btnBorrar.HeaderText = "Acción"
        btnBorrar.Text = "✖ Eliminar"
        btnBorrar.UseColumnTextForButtonValue = True
        btnBorrar.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnBorrar)

        AplicarEstiloTabla()

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Proveedores"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Proveedor"), lector("RFC"), lector("Direccion"))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub ConfigurarTablaSalidas()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Id", "Id")
        DataGridView1.Columns("Id").Visible = False
        DataGridView1.Columns.Add("Fecha", "Fecha")
        DataGridView1.Columns.Add("Codigo", "Código")
        DataGridView1.Columns.Add("Generico", "Genérico")
        DataGridView1.Columns.Add("Distintivo", "Distintivo")
        DataGridView1.Columns.Add("Presentacion", "Presentación")
        DataGridView1.Columns.Add("AWARE", "AWARE")
        DataGridView1.Columns.Add("Lote", "Lote")
        DataGridView1.Columns.Add("Caducidad", "Caducidad")
        DataGridView1.Columns.Add("Existencia", "Existencia")
        DataGridView1.Columns.Add("Surtido", "Surtido")
        DataGridView1.Columns.Add("Saldo", "Saldo")
        DataGridView1.Columns.Add("Movimiento", "Movimiento")
        DataGridView1.Columns.Add("Folio", "Folio")
        DataGridView1.Columns.Add("Cedula", "Cedula")
        DataGridView1.Columns.Add("Nombre", "Nombre")
        DataGridView1.Columns.Add("Direccion", "Direccion")
        DataGridView1.Columns.Add("Telefono", "Telefono")

        Dim btnRevertir As New DataGridViewButtonColumn()
        btnRevertir.Name = "AccionRevertir"
        btnRevertir.HeaderText = "Acción"
        btnRevertir.Text = "✖ Revertir"
        btnRevertir.UseColumnTextForButtonValue = True
        btnRevertir.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnRevertir)

        AplicarEstiloTabla()

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Salidas"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Id"), lector("Fecha"), lector("Codigo"), lector("Generico"), lector("Distintivo"),
                                               lector("Presentacion"), lector("AWARE"), lector("Lote"), lector("Caducidad"),
                                               lector("Existencia"), lector("Surtido"), lector("Saldo"), lector("Movimiento"),
                                               lector("Folio"), lector("Cedula"), lector("Nombre"), lector("Direccion"), lector("Telefono"))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub ConfigurarTablaMedicos()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Cedula", "Cedula")
        DataGridView1.Columns.Add("NombreMed", "NombreMed")
        DataGridView1.Columns.Add("Calle", "Calle")
        DataGridView1.Columns.Add("NoInt", "NoInt")
        DataGridView1.Columns.Add("NoExt", "NoExt")
        DataGridView1.Columns.Add("Colonia", "Colonia")
        DataGridView1.Columns.Add("Ciudad", "Ciudad")
        DataGridView1.Columns.Add("Estado", "Estado")
        DataGridView1.Columns.Add("CP", "CP")
        DataGridView1.Columns.Add("Pais", "Pais")
        DataGridView1.Columns.Add("Tel", "Tel")

        Dim btnBorrar As New DataGridViewButtonColumn()
        btnBorrar.Name = "AccionRevertir"
        btnBorrar.HeaderText = "Acción"
        btnBorrar.Text = "✖ Eliminar"
        btnBorrar.UseColumnTextForButtonValue = True
        btnBorrar.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnBorrar)

        AplicarEstiloTabla()

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Medicos"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Cedula"), lector("NombreMed"), lector("Calle"), lector("NoInt"),
                                               lector("NoExt"), lector("Colonia"), lector("Ciudad"), lector("Estado"),
                                               lector("CP"), lector("Pais"), lector("Tel"))
                    End While
                End Using
            End Using
        End Using
    End Sub


    ' =========================================================
    ' 14. EVENTOS DE ELIMINACIÓN Y REVERSIÓN
    ' =========================================================
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex >= 0 AndAlso DataGridView1.Columns(e.ColumnIndex).Name = "AccionRevertir" Then

            Using conexion As New SQLiteConnection(cadenaConexion)
                conexion.Open()

                If DataGridView1.Columns.Contains("Surtido") AndAlso DataGridView1.Columns.Contains("Factura") Then
                    Dim idEntrada As Integer = Convert.ToInt32(DataGridView1.Rows(e.RowIndex).Cells("Id").Value)
                    Dim codigoMed As String = DataGridView1.Rows(e.RowIndex).Cells("Codigo").Value.ToString()
                    Dim cantSurtida As Double = Convert.ToDouble(DataGridView1.Rows(e.RowIndex).Cells("Surtido").Value)

                    If MessageBox.Show("¿Seguro que deseas revertir esta entrada? Se restarán " & cantSurtida & " cajas.", "Revertir", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                        Using transaccion As SQLiteTransaction = conexion.BeginTransaction()
                            Try
                                Dim cmdInv As New SQLiteCommand("UPDATE Inventario SET ExistenciaActual = ExistenciaActual - @cant WHERE Codigo = @codigo", conexion, transaccion)
                                cmdInv.Parameters.AddWithValue("@cant", cantSurtida)
                                cmdInv.Parameters.AddWithValue("@codigo", codigoMed)
                                cmdInv.ExecuteNonQuery()

                                Dim cmdDel As New SQLiteCommand("DELETE FROM Entradas WHERE Id = @id", conexion, transaccion)
                                cmdDel.Parameters.AddWithValue("@id", idEntrada)
                                cmdDel.ExecuteNonQuery()

                                transaccion.Commit()
                                ConfigurarTablaEntradas()
                            Catch ex As Exception
                                transaccion.Rollback()
                                MessageBox.Show("Error: " & ex.Message)
                            End Try
                        End Using
                    End If

                ElseIf DataGridView1.Columns.Contains("Surtido") AndAlso DataGridView1.Columns.Contains("Folio") Then
                    Dim idSalida As Integer = Convert.ToInt32(DataGridView1.Rows(e.RowIndex).Cells("Id").Value)
                    Dim codigoMed As String = DataGridView1.Rows(e.RowIndex).Cells("Codigo").Value.ToString()
                    Dim cantSurtida As Double = Convert.ToDouble(DataGridView1.Rows(e.RowIndex).Cells("Surtido").Value)

                    If MessageBox.Show("¿Seguro que deseas revertir esta salida? Se sumarán " & cantSurtida & " cajas al inventario.", "Revertir", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                        Using transaccion As SQLiteTransaction = conexion.BeginTransaction()
                            Try
                                Dim cmdInv As New SQLiteCommand("UPDATE Inventario SET ExistenciaActual = ExistenciaActual + @cant WHERE Codigo = @codigo", conexion, transaccion)
                                cmdInv.Parameters.AddWithValue("@cant", cantSurtida)
                                cmdInv.Parameters.AddWithValue("@codigo", codigoMed)
                                cmdInv.ExecuteNonQuery()

                                Dim cmdDel As New SQLiteCommand("DELETE FROM Salidas WHERE Id = @id", conexion, transaccion)
                                cmdDel.Parameters.AddWithValue("@id", idSalida)
                                cmdDel.ExecuteNonQuery()

                                transaccion.Commit()
                                ConfigurarTablaSalidas()
                            Catch ex As Exception
                                transaccion.Rollback()
                                MessageBox.Show("Error: " & ex.Message)
                            End Try
                        End Using
                    End If

                ElseIf DataGridView1.Columns.Contains("ExistenciaActual") Then
                    Dim codigo As String = DataGridView1.Rows(e.RowIndex).Cells("Codigo").Value.ToString()
                    If MessageBox.Show("¿Eliminar este medicamento del catálogo?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                        Dim cmd As New SQLiteCommand("DELETE FROM Inventario WHERE Codigo = @codigo", conexion)
                        cmd.Parameters.AddWithValue("@codigo", codigo)
                        cmd.ExecuteNonQuery()
                        ConfigurarTablaInventario()
                    End If

                ElseIf DataGridView1.Columns.Contains("RFC") AndAlso Not DataGridView1.Columns.Contains("Surtido") Then
                    Dim proveedor As String = DataGridView1.Rows(e.RowIndex).Cells("Proveedor").Value.ToString()
                    If MessageBox.Show("¿Eliminar a " & proveedor & "?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                        Dim cmd As New SQLiteCommand("DELETE FROM Proveedores WHERE Proveedor = @prov", conexion)
                        cmd.Parameters.AddWithValue("@prov", proveedor)
                        cmd.ExecuteNonQuery()
                        ConfigurarTablaProveedores()
                    End If

                ElseIf DataGridView1.Columns.Contains("Cedula") AndAlso Not DataGridView1.Columns.Contains("Folio") Then
                    Dim cedula As String = DataGridView1.Rows(e.RowIndex).Cells("Cedula").Value.ToString()
                    If MessageBox.Show("¿Eliminar a este médico?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                        Dim cmd As New SQLiteCommand("DELETE FROM Medicos WHERE Cedula = @ced", conexion)
                        cmd.Parameters.AddWithValue("@ced", cedula)
                        cmd.ExecuteNonQuery()
                        ConfigurarTablaMedicos()
                    End If
                End If

            End Using
        End If
    End Sub

End Class