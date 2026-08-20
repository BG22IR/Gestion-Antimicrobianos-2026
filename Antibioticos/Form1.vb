Imports System.Data.SQLite
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports System.Drawing.Text
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.FileIO

Public Class Form1

    ' =========================================================
    ' 0. VARIABLES GLOBALES Y CONTROLES
    ' =========================================================
    ' Al hacerla PUBLIC SHARED, todas tus demás ventanas podrán leer esta misma ruta
    Public Shared cadenaConexion As String = ""

    ' Paleta de colores suaves modernos
    Private ReadOnly ColorPrimario As Drawing.Color = Drawing.Color.FromArgb(37, 99, 235)
    Private ReadOnly ColorPrimarioHover As Drawing.Color = Drawing.Color.FromArgb(59, 130, 246)
    Private ReadOnly ColorExito As Drawing.Color = Drawing.Color.FromArgb(16, 149, 106)
    Private ReadOnly ColorExitoHover As Drawing.Color = Drawing.Color.FromArgb(20, 168, 120)
    Private ReadOnly ColorAlerta As Drawing.Color = Drawing.Color.FromArgb(217, 119, 6)
    Private ReadOnly ColorAlertaHover As Drawing.Color = Drawing.Color.FromArgb(245, 158, 11)
    Private ReadOnly ColorPeligro As Drawing.Color = Drawing.Color.FromArgb(194, 65, 12)
    Private ReadOnly ColorPeligroHover As Drawing.Color = Drawing.Color.FromArgb(234, 88, 12)
    Private ReadOnly ColorOscuro As Drawing.Color = Drawing.Color.FromArgb(30, 41, 59)
    Private ReadOnly ColorOscuroHover As Drawing.Color = Drawing.Color.FromArgb(51, 65, 85)

    ' Contenedor maestro de vistas (Área delimitada a la derecha)
    Private pnlContenedorVistas As New Panel()

    ' Paneles principales de contenido
    Private panelInicio As New Panel()
    Private panelConfig As New Panel()
    Private panelReportes As New Panel()
    Private panelAware As New Panel()
    Private pnlModuloTablas As New Panel()

    ' Barra superior de búsqueda, títulos, exportación e importación para las tablas
    Private pnlHeaderTabla As New Panel()
    Private lblTituloModuloTabla As New Label()
    Private lblContadorRegistros As New Label()
    Private pnlBuscadorBox As New Panel()
    Private txtBuscadorTabla As New TextBox()
    Private WithEvents btnExportarModuloCSV As New Button()
    Private WithEvents btnImportarModuloCSV As New Button()

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
    Private WithEvents btnGestionUsuarios As New Button()
    Private WithEvents btnRespaldarBD As New Button()
    Private WithEvents btnRestaurarBD As New Button()

    ' Controles de Reportes Oficiales (Entradas / Salidas / Kardex Combinado)
    Private cmbModuloRep As New ComboBox()
    Private cmbMesRep As New ComboBox()
    Private txtAnioRep As New TextBox()
    Private WithEvents btnGenerarRep As New Button()
    Private WithEvents docImprimir As New PrintDocument()
    Private dtImprimir As New DataTable()
    Private indiceImpresion As Integer = 0
    Private numPaginaReporte As Integer = 0
    Private codigoActualGrupo As String = ""

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
    Private pnlTablaAwareContainer As New Panel()
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

        ' --- NUEVA LÓGICA DE RUTA (C:\Gestion de Antimicrobianos) ---
        Dim carpetaApp As String = "C:\Gestion de Antimicrobianos"

        Try
            If Not IO.Directory.Exists(carpetaApp) Then
                IO.Directory.CreateDirectory(carpetaApp)
            End If
        Catch ex As Exception
            MessageBox.Show("No se pudo acceder o crear la carpeta en C:\Gestion de Antimicrobianos." & vbCrLf & "Por favor ejecuta el programa como Administrador.", "Error de Permisos", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Dim rutaBaseDatos As String = IO.Path.Combine(carpetaApp, "BaseDatosADN.db")
        cadenaConexion = "Data Source=" & rutaBaseDatos & ";Version=3;"
        ' ---------------------------------------------------

        Dim msgErrorLic As String = ""
        If Not LicenciaManager.ValidarLicenciaActual(msgErrorLic) Then
            Dim ventanaActivacion As New FormActivacion()
            ventanaActivacion.StartPosition = FormStartPosition.CenterScreen
            If ventanaActivacion.ShowDialog(Me) <> DialogResult.OK Then
                Application.Exit()
                Return
            End If
        End If

        CrearBaseDeDatosSiNoExiste()

        Dim ventanaLogin As New FormLogin()
        ventanaLogin.StartPosition = FormStartPosition.CenterScreen
        If ventanaLogin.ShowDialog(Me) <> DialogResult.OK Then
            Application.Exit()
            Return
        End If

        HabilitarDobleBuffer(Me)

        pnlContenedorVistas.Dock = DockStyle.Fill
        pnlContenedorVistas.BackColor = Drawing.Color.White
        Me.Controls.Add(pnlContenedorVistas)
        HabilitarDobleBuffer(pnlContenedorVistas)

        Panel1.SendToBack()
        pnlContenedorVistas.BringToFront()

        picFadeOverlay.Dock = DockStyle.Fill
        picFadeOverlay.Visible = False
        pnlContenedorVistas.Controls.Add(picFadeOverlay)

        tmrAnimIndicador.Interval = 10
        AddHandler tmrAnimIndicador.Tick, AddressOf AnimarIndicadorMenu_Tick

        tmrFade.Interval = 15
        AddHandler tmrFade.Tick, AddressOf AnimarFade_Tick

        AplicarEstiloFluent()
        ConfigurarIndicadorMenu()

        ConfigurarPantallaInicio()
        ConfigurarPantallaAjustes()
        ConfigurarPantallaReportes()
        ConfigurarPantallaAware()
        ConfigurarContenedorTablasConBuscador()
        CargarConfiguracionActual()

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

            comando.CommandText = "INSERT OR IGNORE INTO Configuracion (Id, NombreFarmacia, Direccion, Responsable, RutaLogo) VALUES (1, '', '', '', '')"
            comando.ExecuteNonQuery()

            comando.CommandText = "CREATE TABLE IF NOT EXISTS Usuarios (Id INTEGER PRIMARY KEY AUTOINCREMENT, Usuario TEXT UNIQUE, Password TEXT, Nombre TEXT, Rol TEXT)"
            comando.ExecuteNonQuery()

            comando.CommandText = "INSERT OR IGNORE INTO Usuarios (Id, Usuario, Password, Nombre, Rol) VALUES (1, 'admin', 'admin', 'Administrador General', 'ADMIN')"
            comando.ExecuteNonQuery()
        End Using
    End Sub


    ' =========================================================
    ' 2. FUNCIONES DE DIBUJO SUAVE Y ANTI-ALIASING
    ' =========================================================
    Private Sub EstilizarBotonSuave(btn As Button, radio As Integer, colorFondo As Drawing.Color, colorHover As Drawing.Color, colorTexto As Drawing.Color, Optional colorBorde As Drawing.Color = Nothing, Optional grosorBorde As Single = 1.0F)
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.ForeColor = colorTexto
        btn.Cursor = Cursors.Hand
        btn.BackColor = Drawing.Color.Transparent

        Dim isHovered As Boolean = False
        Dim isPressed As Boolean = False

        AddHandler btn.MouseEnter, Sub(s, e)
                                       isHovered = True
                                       btn.Invalidate()
                                   End Sub
        AddHandler btn.MouseLeave, Sub(s, e)
                                       isHovered = False
                                       isPressed = False
                                       btn.Invalidate()
                                   End Sub
        AddHandler btn.MouseDown, Sub(s, e)
                                      isPressed = True
                                      btn.Invalidate()
                                  End Sub
        AddHandler btn.MouseUp, Sub(s, e)
                                    isPressed = False
                                    btn.Invalidate()
                                End Sub

        AddHandler btn.Paint, Sub(s, e)
                                  Dim g As Graphics = e.Graphics
                                  g.SmoothingMode = SmoothingMode.AntiAlias
                                  g.PixelOffsetMode = PixelOffsetMode.HighQuality
                                  g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

                                  Dim parentBg As Drawing.Color = If(btn.Parent IsNot Nothing, btn.Parent.BackColor, Drawing.Color.White)
                                  Using brushParent As New SolidBrush(parentBg)
                                      g.FillRectangle(brushParent, btn.ClientRectangle)
                                  End Using

                                  Dim currentColor As Drawing.Color = colorFondo
                                  If isPressed Then
                                      currentColor = OscurecerColor(colorHover, 0.08F)
                                  ElseIf isHovered Then
                                      currentColor = colorHover
                                  End If

                                  Dim rect As New Rectangle(0, 0, btn.Width - 1, btn.Height - 1)
                                  Using path As GraphicsPath = CrearRutaRedondeada(rect, radio)
                                      Using brushBtn As New SolidBrush(currentColor)
                                          g.FillPath(brushBtn, path)
                                      End Using

                                      If colorBorde <> Drawing.Color.Empty AndAlso colorBorde <> Drawing.Color.Transparent Then
                                          Using penBrd As New Pen(colorBorde, grosorBorde)
                                              g.DrawPath(penBrd, path)
                                          End Using
                                      End If
                                  End Using

                                  Dim sf As New StringFormat With {
                                      .Alignment = If(btn.TextAlign = ContentAlignment.MiddleLeft, StringAlignment.Near, StringAlignment.Center),
                                      .LineAlignment = StringAlignment.Center
                                  }

                                  Dim textRect As Rectangle = btn.ClientRectangle
                                  If btn.TextAlign = ContentAlignment.MiddleLeft Then
                                      textRect.X += btn.Padding.Left
                                      textRect.Width -= btn.Padding.Left
                                  End If

                                  Using brushTxt As New SolidBrush(colorTexto)
                                      g.DrawString(btn.Text, btn.Font, brushTxt, textRect, sf)
                                  End Using
                              End Sub
    End Sub

    Private Function OscurecerColor(c As Drawing.Color, factor As Single) As Drawing.Color
        Return Drawing.Color.FromArgb(c.A, Math.Max(0, CInt(c.R * (1.0F - factor))), Math.Max(0, CInt(c.G * (1.0F - factor))), Math.Max(0, CInt(c.B * (1.0F - factor))))
    End Function

    Private Function CrearRutaRedondeada(r As Rectangle, radio As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d As Integer = radio * 2
        If d > r.Height Then d = r.Height
        If d > r.Width Then d = r.Width
        If d <= 0 Then d = 1

        path.StartFigure()
        path.AddArc(r.X, r.Y, d, d, 180, 90)
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90)
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90)
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function

    Private Sub RedondearPanelBorde(pnl As Panel, Optional radio As Integer = 12, Optional colorBorde As Drawing.Color = Nothing, Optional grosor As Single = 1.0F)
        If colorBorde = Drawing.Color.Empty Then colorBorde = Drawing.Color.FromArgb(203, 213, 225)
        pnl.BorderStyle = BorderStyle.None

        AddHandler pnl.Paint, Sub(s, e)
                                  Dim g As Graphics = e.Graphics
                                  g.SmoothingMode = SmoothingMode.AntiAlias
                                  g.PixelOffsetMode = PixelOffsetMode.HighQuality
                                  Using path As GraphicsPath = CrearRutaRedondeada(New Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1), radio)
                                      Using penBrd As New Pen(colorBorde, grosor)
                                          g.DrawPath(penBrd, path)
                                      End Using
                                  End Using
                              End Sub
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
        pnlIndicadorMenu.Size = New Size(4, 26)
        pnlIndicadorMenu.BackColor = ColorPrimario
        pnlIndicadorMenu.Location = New Point(2, 10)
        pnlIndicadorMenu.Visible = True
        Panel1.Controls.Add(pnlIndicadorMenu)
        pnlIndicadorMenu.BringToFront()
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
        pnlModuloTablas.Visible = False
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
                    b.BackColor = Drawing.Color.FromArgb(239, 246, 255)
                    b.ForeColor = ColorPrimario
                    b.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
                Else
                    b.BackColor = Drawing.Color.FromArgb(248, 250, 252)
                    b.ForeColor = Drawing.Color.FromArgb(71, 85, 105)
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
        SeleccionarMenu(Button2, pnlModuloTablas)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ConfigurarTablaSalidas()
        SeleccionarMenu(Button3, pnlModuloTablas)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ConfigurarTablaMedicos()
        SeleccionarMenu(Button4, pnlModuloTablas)
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ConfigurarTablaProveedores()
        SeleccionarMenu(Button5, pnlModuloTablas)
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        ConfigurarTablaInventario()
        SeleccionarMenu(Button6, pnlModuloTablas)
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

        picLogoInicio.Size = New Size(420, 240)
        picLogoInicio.Location = New Point((panelCentroInicio.Width - picLogoInicio.Width) \ 2, 10)
        picLogoInicio.SizeMode = PictureBoxSizeMode.Zoom
        panelCentroInicio.Controls.Add(picLogoInicio)

        lblNomInicio.Font = New Drawing.Font("Segoe UI", 18.0F, Drawing.FontStyle.Bold)
        lblNomInicio.ForeColor = ColorPrimario
        lblNomInicio.TextAlign = ContentAlignment.MiddleCenter
        lblNomInicio.AutoSize = False
        lblNomInicio.Size = New Size(650, 36)
        lblNomInicio.Location = New Point(0, 255)

        lblDirInicio.Font = New Drawing.Font("Segoe UI", 10.5F, Drawing.FontStyle.Regular)
        lblDirInicio.ForeColor = Drawing.Color.FromArgb(70, 70, 70)
        lblDirInicio.TextAlign = ContentAlignment.MiddleCenter
        lblDirInicio.AutoSize = False
        lblDirInicio.Size = New Size(650, 25)
        lblDirInicio.Location = New Point(0, 293)

        lblRespInicio.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Italic)
        lblRespInicio.ForeColor = Drawing.Color.FromArgb(100, 100, 100)
        lblRespInicio.TextAlign = ContentAlignment.MiddleCenter
        lblRespInicio.AutoSize = False
        lblRespInicio.Size = New Size(650, 25)
        lblRespInicio.Location = New Point(0, 320)

        panelCentroInicio.Controls.Add(lblNomInicio)
        panelCentroInicio.Controls.Add(lblDirInicio)
        panelCentroInicio.Controls.Add(lblRespInicio)

        btnNuevaEntrada.Text = "+ Registrar Entrada"
        btnNuevaEntrada.Size = New Size(290, 65)
        btnNuevaEntrada.Location = New Point(25, 365)
        btnNuevaEntrada.Font = New Drawing.Font("Segoe UI", 11.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnNuevaEntrada, 16, ColorExito, ColorExitoHover, Drawing.Color.White)

        btnNuevaSalida.Text = "+ Registrar Salida (Receta)"
        btnNuevaSalida.Size = New Size(290, 65)
        btnNuevaSalida.Location = New Point(335, 365)
        btnNuevaSalida.Font = New Drawing.Font("Segoe UI", 11.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnNuevaSalida, 16, ColorPrimario, ColorPrimarioHover, Drawing.Color.White)

        btnImportarCSV.Text = "📁 Importación Inteligente desde CSV"
        btnImportarCSV.Size = New Size(600, 55)
        btnImportarCSV.Location = New Point(25, 442)
        btnImportarCSV.Font = New Drawing.Font("Segoe UI", 11.0F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnImportarCSV, 16, ColorAlerta, ColorAlertaHover, Drawing.Color.White)

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

        Dim lblTitulo As New Label With {.Text = "Configuración del Sistema", .Location = New Point(35, 20), .Font = New Drawing.Font("Segoe UI", 16.0F, Drawing.FontStyle.Bold), .AutoSize = True}

        Dim lblNom As New Label With {.Text = "Nombre de la Farmacia:", .Location = New Point(35, 65), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F)}
        txtNomFarmacia.Location = New Point(35, 88)
        txtNomFarmacia.Size = New Size(400, 28)
        txtNomFarmacia.Font = New Drawing.Font("Segoe UI", 11.0F)

        Dim lblDir As New Label With {.Text = "Dirección Completa:", .Location = New Point(35, 125), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F)}
        txtDireccion.Location = New Point(35, 148)
        txtDireccion.Size = New Size(400, 28)
        txtDireccion.Font = New Drawing.Font("Segoe UI", 11.0F)

        Dim lblResp As New Label With {.Text = "Nombre del Responsable Sanitario:", .Location = New Point(35, 185), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 10.0F)}
        txtResponsable.Location = New Point(35, 208)
        txtResponsable.Size = New Size(400, 28)
        txtResponsable.Font = New Drawing.Font("Segoe UI", 11.0F)

        picLogoConfig.Location = New Point(460, 88)
        picLogoConfig.Size = New Size(150, 148)
        picLogoConfig.SizeMode = PictureBoxSizeMode.Zoom
        picLogoConfig.BorderStyle = BorderStyle.FixedSingle

        btnSubirLogo.Text = "Cargar Logo"
        btnSubirLogo.Location = New Point(460, 245)
        btnSubirLogo.Size = New Size(150, 35)
        btnSubirLogo.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
        EstilizarBotonSuave(btnSubirLogo, 10, Drawing.Color.FromArgb(241, 245, 249), Drawing.Color.FromArgb(226, 232, 240), Drawing.Color.FromArgb(51, 65, 85), Drawing.Color.FromArgb(203, 213, 225), 1.0F)

        btnGuardarConfig.Text = "💾 Guardar Datos de la Farmacia"
        btnGuardarConfig.Location = New Point(35, 255)
        btnGuardarConfig.Size = New Size(400, 42)
        btnGuardarConfig.Font = New Drawing.Font("Segoe UI", 10.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnGuardarConfig, 12, ColorPrimario, ColorPrimarioHover, Drawing.Color.White)

        btnGestionUsuarios.Text = "👥 Administrar Usuarios y Permisos"
        btnGestionUsuarios.Location = New Point(35, 305)
        btnGestionUsuarios.Size = New Size(400, 42)
        btnGestionUsuarios.Font = New Drawing.Font("Segoe UI", 10.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnGestionUsuarios, 12, ColorOscuro, ColorOscuroHover, Drawing.Color.White)

        btnRespaldarBD.Text = "📦 Crear Respaldo de BD"
        btnRespaldarBD.Location = New Point(35, 355)
        btnRespaldarBD.Size = New Size(195, 42)
        btnRespaldarBD.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnRespaldarBD, 12, ColorExito, ColorExitoHover, Drawing.Color.White)

        btnRestaurarBD.Text = "♻ Restaurar Respaldo"
        btnRestaurarBD.Location = New Point(240, 355)
        btnRestaurarBD.Size = New Size(195, 42)
        btnRestaurarBD.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnRestaurarBD, 12, ColorPeligro, ColorPeligroHover, Drawing.Color.White)

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
        panelConfig.Controls.Add(btnGestionUsuarios)
        panelConfig.Controls.Add(btnRespaldarBD)
        panelConfig.Controls.Add(btnRestaurarBD)
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

                    lblNomInicio.Text = If(nom.Trim() <> "", nom.ToUpper(), "MI FARMACIA (CONFIGURAR EN AJUSTES)")
                    lblDirInicio.Text = If(dir.Trim() <> "", dir, "Dirección no registrada")
                    lblRespInicio.Text = If(resp.Trim() <> "", "Responsable Sanitario: " & resp, "Responsable Sanitario: Pendiente de registrar")

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
        If dialog.ShowDialog(Me) = DialogResult.OK Then
            Try
                ' --- RUTA EN C:\ ---
                Dim carpetaDestino As String = Path.Combine("C:\Gestion de Antimicrobianos", "Recursos")

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
                MessageBox.Show("Error al cargar imagen. Revisa los permisos de administrador: " & ex.Message)
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

        lblNomInicio.Text = If(txtNomFarmacia.Text.Trim() <> "", txtNomFarmacia.Text.Trim().ToUpper(), "MI FARMACIA (CONFIGURAR EN AJUSTES)")
        lblDirInicio.Text = If(txtDireccion.Text.Trim() <> "", txtDireccion.Text.Trim(), "Dirección no registrada")
        lblRespInicio.Text = If(txtResponsable.Text.Trim() <> "", "Responsable Sanitario: " & txtResponsable.Text.Trim(), "Responsable Sanitario: Pendiente de registrar")

        MessageBox.Show("Configuración guardada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnGestionUsuarios_Click(sender As Object, e As EventArgs) Handles btnGestionUsuarios.Click
        If Not SesionActual.EsAdmin() Then
            MessageBox.Show("Acceso Restringido: Solo los administradores pueden gestionar usuarios y permisos.", "Sin Permiso", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return
        End If

        Dim ventanaUsers As New FormUsuarios()
        ventanaUsers.ShowDialog(Me)
    End Sub

    Private Sub btnRespaldarBD_Click(sender As Object, e As EventArgs) Handles btnRespaldarBD.Click
        If Not SesionActual.EsAdmin() Then
            MessageBox.Show("Acceso Restringido: Solo los administradores pueden generar respaldos de seguridad.", "Sin Permiso", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return
        End If

        Dim sfd As New SaveFileDialog()
        sfd.Title = "Guardar Respaldo de Base de Datos"
        sfd.Filter = "Base de Datos SQLite (*.db)|*.db|Archivo de Respaldo (*.bak)|*.bak"
        sfd.FileName = "Respaldo_FarmaciaADN_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".db"

        If sfd.ShowDialog(Me) = DialogResult.OK Then
            Try
                Using conOrigen As New SQLiteConnection(cadenaConexion)
                    Using conDestino As New SQLiteConnection("Data Source=" & sfd.FileName & ";Version=3;")
                        conOrigen.Open()
                        conDestino.Open()
                        conOrigen.BackupDatabase(conDestino, "main", "main", -1, Nothing, 0)
                    End Using
                End Using

                MessageBox.Show("¡Respaldo de seguridad creado exitosamente!" & vbCrLf & vbCrLf & "Archivo guardado en:" & vbCrLf & sfd.FileName,
                                "Respaldo Completado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Error al generar respaldo: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnRestaurarBD_Click(sender As Object, e As EventArgs) Handles btnRestaurarBD.Click
        If Not SesionActual.EsAdmin() Then
            MessageBox.Show("Acceso Restringido: Solo los administradores pueden restaurar respaldos del sistema.", "Sin Permiso", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return
        End If

        Dim ofd As New OpenFileDialog()
        ofd.Title = "Seleccionar Archivo de Respaldo para Restaurar"
        ofd.Filter = "Archivos de Base de Datos (*.db;*.bak)|*.db;*.bak|Todos los archivos (*.*)|*.*"

        If ofd.ShowDialog(Me) = DialogResult.OK Then
            Dim confirmacion As DialogResult = MessageBox.Show(
                "ADVERTENCIA DE SEGURIDAD:" & vbCrLf & vbCrLf &
                "Al restaurar este respaldo, la información actual de inventarios, entradas, salidas y recetas será reemplazada por los datos del archivo seleccionado." & vbCrLf & vbCrLf &
                "¿Deseas continuar con la restauración?",
                "Confirmar Recuperación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If confirmacion = DialogResult.Yes Then
                Try
                    Using conOrigen As New SQLiteConnection("Data Source=" & ofd.FileName & ";Version=3;")
                        Using conDestino As New SQLiteConnection(cadenaConexion)
                            conOrigen.Open()
                            conDestino.Open()
                            conOrigen.BackupDatabase(conDestino, "main", "main", -1, Nothing, 0)
                        End Using
                    End Using

                    CargarConfiguracionActual()
                    MessageBox.Show("¡Base de datos restaurada correctamente en un solo paso!", "Restauración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    If pnlModuloTablas.Visible Then
                        If lblTituloModuloTabla.Text.Contains("Entradas") Then
                            ConfigurarTablaEntradas()
                        ElseIf lblTituloModuloTabla.Text.Contains("Salidas") Then
                            ConfigurarTablaSalidas()
                        ElseIf lblTituloModuloTabla.Text.Contains("Inventario") Then
                            ConfigurarTablaInventario()
                        ElseIf lblTituloModuloTabla.Text.Contains("Proveedores") Then
                            ConfigurarTablaProveedores()
                        ElseIf lblTituloModuloTabla.Text.Contains("Médicos") Then
                            ConfigurarTablaMedicos()
                        End If
                    ElseIf panelAware.Visible Then
                        CargarReporteAware()
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error al restaurar base de datos: " & ex.Message, "Error de Restauración", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End If
    End Sub


    ' =========================================================
    ' 8. CONTENEDOR MODERNO DE TABLAS CON BUSCADOR, EXPORTACIÓN E IMPORTACIÓN
    ' =========================================================
    Private Sub ConfigurarContenedorTablasConBuscador()
        pnlModuloTablas.Dock = DockStyle.Fill
        pnlModuloTablas.BackColor = Drawing.Color.White
        pnlContenedorVistas.Controls.Add(pnlModuloTablas)
        HabilitarDobleBuffer(pnlModuloTablas)

        pnlHeaderTabla.Dock = DockStyle.Top
        pnlHeaderTabla.Height = 70
        pnlHeaderTabla.BackColor = Drawing.Color.FromArgb(248, 250, 252)
        pnlHeaderTabla.Padding = New Padding(20, 10, 20, 10)
        pnlModuloTablas.Controls.Add(pnlHeaderTabla)

        AddHandler pnlHeaderTabla.Paint, Sub(s, e)
                                             Using penLinea As New Pen(Drawing.Color.FromArgb(226, 232, 240), 1.5F)
                                                 e.Graphics.DrawLine(penLinea, 0, pnlHeaderTabla.Height - 1, pnlHeaderTabla.Width, pnlHeaderTabla.Height - 1)
                                             End Using
                                         End Sub

        lblTituloModuloTabla.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        lblTituloModuloTabla.ForeColor = Drawing.Color.FromArgb(15, 23, 42)
        lblTituloModuloTabla.Location = New Point(20, 12)
        lblTituloModuloTabla.AutoSize = True

        lblContadorRegistros.Font = New Font("Segoe UI", 9.5F, FontStyle.Regular)
        lblContadorRegistros.ForeColor = Drawing.Color.FromArgb(100, 116, 139)
        lblContadorRegistros.Location = New Point(22, 38)
        lblContadorRegistros.AutoSize = True

        btnImportarModuloCSV.Text = "📥 Importar CSV"
        btnImportarModuloCSV.Size = New Size(140, 38)
        btnImportarModuloCSV.Location = New Point(pnlModuloTablas.ClientSize.Width - 615, 15)
        btnImportarModuloCSV.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnImportarModuloCSV.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        EstilizarBotonSuave(btnImportarModuloCSV, 10, ColorPrimario, ColorPrimarioHover, Drawing.Color.White)

        btnExportarModuloCSV.Text = "📤 Exportar CSV"
        btnExportarModuloCSV.Size = New Size(140, 38)
        btnExportarModuloCSV.Location = New Point(pnlModuloTablas.ClientSize.Width - 465, 15)
        btnExportarModuloCSV.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnExportarModuloCSV.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        EstilizarBotonSuave(btnExportarModuloCSV, 10, ColorExito, ColorExitoHover, Drawing.Color.White)

        pnlBuscadorBox.Size = New Size(300, 38)
        pnlBuscadorBox.Location = New Point(pnlModuloTablas.ClientSize.Width - 315, 15)
        pnlBuscadorBox.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        pnlBuscadorBox.BackColor = Drawing.Color.White
        RedondearPanelBorde(pnlBuscadorBox, 10, Drawing.Color.FromArgb(148, 163, 184), 1.2F)

        Dim lblIconoBuscar As New Label With {
            .Text = "🔍",
            .Font = New Font("Segoe UI", 10.5F),
            .Size = New Size(26, 24),
            .Location = New Point(10, 8),
            .ForeColor = Drawing.Color.FromArgb(100, 116, 139)
        }

        txtBuscadorTabla.BorderStyle = BorderStyle.None
        txtBuscadorTabla.Font = New Font("Segoe UI", 10.5F)
        txtBuscadorTabla.Location = New Point(38, 9)
        txtBuscadorTabla.Size = New Size(250, 22)
        txtBuscadorTabla.BackColor = Drawing.Color.White
        AddHandler txtBuscadorTabla.TextChanged, AddressOf TxtBuscadorTabla_TextChanged

        pnlBuscadorBox.Controls.Add(lblIconoBuscar)
        pnlBuscadorBox.Controls.Add(txtBuscadorTabla)

        pnlHeaderTabla.Controls.Add(lblTituloModuloTabla)
        pnlHeaderTabla.Controls.Add(lblContadorRegistros)
        pnlHeaderTabla.Controls.Add(btnImportarModuloCSV)
        pnlHeaderTabla.Controls.Add(btnExportarModuloCSV)
        pnlHeaderTabla.Controls.Add(pnlBuscadorBox)

        pnlModuloTablas.Controls.Add(DataGridView1)
        DataGridView1.Dock = DockStyle.Fill
        DataGridView1.BringToFront()
        HabilitarDobleBuffer(DataGridView1)
    End Sub

    Private Sub btnExportarModuloCSV_Click(sender As Object, e As EventArgs) Handles btnExportarModuloCSV.Click
        ExportarTablaActualACSV()
    End Sub

    Private Sub btnImportarModuloCSV_Click(sender As Object, e As EventArgs) Handles btnImportarModuloCSV.Click
        Dim moduloSugerido As String = ""
        If lblTituloModuloTabla.Text.Contains("Entradas") Then
            moduloSugerido = "Entradas"
        ElseIf lblTituloModuloTabla.Text.Contains("Salidas") Then
            moduloSugerido = "Salidas"
        ElseIf lblTituloModuloTabla.Text.Contains("Inventario") Then
            moduloSugerido = "Inventario"
        ElseIf lblTituloModuloTabla.Text.Contains("Proveedores") Then
            moduloSugerido = "Proveedores"
        ElseIf lblTituloModuloTabla.Text.Contains("Médicos") Then
            moduloSugerido = "Medicos"
        End If

        EjecutarImportadorCSVGeneral(moduloSugerido)
    End Sub

    Private Sub ExportarTablaActualACSV()
        If DataGridView1.Rows.Count = 0 Then
            MessageBox.Show("No hay registros en esta área para exportar.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim nombreModulo As String = "Datos"
        If lblTituloModuloTabla.Text.Contains("Entradas") Then
            nombreModulo = "Entradas"
        ElseIf lblTituloModuloTabla.Text.Contains("Salidas") Then
            nombreModulo = "Salidas"
        ElseIf lblTituloModuloTabla.Text.Contains("Inventario") Then
            nombreModulo = "Inventario"
        ElseIf lblTituloModuloTabla.Text.Contains("Proveedores") Then
            nombreModulo = "Proveedores"
        ElseIf lblTituloModuloTabla.Text.Contains("Médicos") Then
            nombreModulo = "Medicos"
        End If

        Dim sfd As New SaveFileDialog()
        sfd.Title = "Exportar Catálogo a CSV (Excel)"
        sfd.Filter = "Archivo CSV (*.csv)|*.csv"
        sfd.FileName = nombreModulo & "_Exportado_" & DateTime.Now.ToString("yyyyMMdd_HHmm") & ".csv"

        If sfd.ShowDialog(Me) = DialogResult.OK Then
            Try
                Dim sb As New StringBuilder()

                Dim columnasExportables As New List(Of DataGridViewColumn)()
                For Each col As DataGridViewColumn In DataGridView1.Columns
                    If col.Visible AndAlso col.Name <> "AccionRevertir" Then
                        columnasExportables.Add(col)
                    End If
                Next

                Dim lineaHeader As New List(Of String)()
                For Each col As DataGridViewColumn In columnasExportables
                    lineaHeader.Add("""" & col.HeaderText.Replace("""", """""") & """")
                Next
                sb.AppendLine(String.Join(",", lineaHeader))

                For Each row As DataGridViewRow In DataGridView1.Rows
                    If row.IsNewRow OrElse Not row.Visible Then Continue For

                    Dim lineaFila As New List(Of String)()
                    For Each col As DataGridViewColumn In columnasExportables
                        Dim val As String = If(row.Cells(col.Index).Value IsNot Nothing, row.Cells(col.Index).Value.ToString(), "")
                        lineaFila.Add("""" & val.Replace("""", """""") & """")
                    Next
                    sb.AppendLine(String.Join(",", lineaFila))
                Next

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8)
                MessageBox.Show("¡Área de " & nombreModulo & " exportada exitosamente!" & vbCrLf & vbCrLf & "Archivo generado:" & vbCrLf & sfd.FileName,
                                "Exportación Completa", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Error al exportar archivo CSV: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub TxtBuscadorTabla_TextChanged(sender As Object, e As EventArgs)
        Dim filtro As String = txtBuscadorTabla.Text.Trim().ToLower()
        Dim visibles As Integer = 0

        If DataGridView1.DataSource IsNot Nothing Then
            Me.BindingContext(DataGridView1.DataSource).SuspendBinding()
        End If

        For Each row As DataGridViewRow In DataGridView1.Rows
            If row.IsNewRow Then Continue For

            If filtro = "" Then
                row.Visible = True
                visibles += 1
            Else
                Dim coincide As Boolean = False
                For Each cell As DataGridViewCell In row.Cells
                    If cell.Value IsNot Nothing AndAlso cell.Value.ToString().ToLower().Contains(filtro) Then
                        coincide = True
                        Exit For
                    End If
                Next
                row.Visible = coincide
                If coincide Then visibles += 1
            End If
        Next

        If DataGridView1.DataSource IsNot Nothing Then
            Me.BindingContext(DataGridView1.DataSource).ResumeBinding()
        End If
        lblContadorRegistros.Text = "Mostrando " & visibles.ToString("N0") & " registro(s)"
    End Sub

    Private Sub ActualizarHeaderModulo(titulo As String)
        txtBuscadorTabla.Clear()
        lblTituloModuloTabla.Text = titulo
        lblContadorRegistros.Text = "Total de registros: " & DataGridView1.Rows.Count.ToString("N0")
    End Sub


    ' =========================================================
    ' 9. ESTILIZACIÓN DE TABLA Y RENDERIZADO VISUAL GDI+ SUAVE
    ' =========================================================
    Private Sub AplicarEstiloTabla()
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.BackgroundColor = Drawing.Color.White
        DataGridView1.BorderStyle = BorderStyle.None

        DataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        DataGridView1.GridColor = Drawing.Color.FromArgb(226, 232, 240)
        DataGridView1.RowHeadersVisible = False

        DataGridView1.EnableHeadersVisualStyles = False
        DataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        DataGridView1.ColumnHeadersDefaultCellStyle.BackColor = ColorOscuro
        DataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
        DataGridView1.ColumnHeadersDefaultCellStyle.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
        DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True
        DataGridView1.ColumnHeadersDefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
        DataGridView1.ColumnHeadersHeight = 46

        DataGridView1.DefaultCellStyle.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Regular)
        DataGridView1.DefaultCellStyle.ForeColor = Drawing.Color.FromArgb(30, 41, 59)
        DataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True
        DataGridView1.DefaultCellStyle.Padding = New Padding(6, 4, 6, 4)
        DataGridView1.DefaultCellStyle.SelectionBackColor = Drawing.Color.FromArgb(239, 246, 255)
        DataGridView1.DefaultCellStyle.SelectionForeColor = ColorPrimario
        DataGridView1.RowTemplate.Height = 44

        DataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(248, 250, 252)
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
    End Sub

    Private Sub DataGridView1_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles DataGridView1.CellPainting
        If e.RowIndex < 0 Then Return

        ' 1. Badges nítidos y vectoriales para AWaRe
        If DataGridView1.Columns(e.ColumnIndex).Name = "AWARE" AndAlso e.Value IsNot Nothing Then
            e.PaintBackground(e.CellBounds, True)

            Dim valor As String = e.Value.ToString().Trim().ToUpper()
            Dim colorFondo As Drawing.Color = Drawing.Color.FromArgb(241, 245, 249)
            Dim colorTexto As Drawing.Color = Drawing.Color.FromArgb(71, 85, 105)
            Dim colorBorde As Drawing.Color = Drawing.Color.FromArgb(203, 213, 225)

            If valor.Contains("ACCES") Then
                colorFondo = Drawing.Color.FromArgb(220, 252, 231)
                colorTexto = Drawing.Color.FromArgb(21, 128, 61)
                colorBorde = Drawing.Color.FromArgb(134, 239, 172)
            ElseIf valor.Contains("VIGILAN") OrElse valor.Contains("WATCH") Then
                colorFondo = Drawing.Color.FromArgb(254, 243, 199)
                colorTexto = Drawing.Color.FromArgb(180, 83, 9)
                colorBorde = Drawing.Color.FromArgb(252, 211, 77)
            ElseIf valor.Contains("RESERV") Then
                colorFondo = Drawing.Color.FromArgb(254, 226, 226)
                colorTexto = Drawing.Color.FromArgb(185, 28, 28)
                colorBorde = Drawing.Color.FromArgb(252, 165, 165)
            End If

            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

            Dim rectBadge As New Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 9, e.CellBounds.Width - 17, 24)
            If rectBadge.Width > 120 Then rectBadge.Width = 120
            rectBadge.X = e.CellBounds.X + ((e.CellBounds.Width - rectBadge.Width) \ 2)

            Using path As GraphicsPath = CrearRutaRedondeada(rectBadge, 6)
                Using brushFondo As New SolidBrush(colorFondo)
                    g.FillPath(brushFondo, path)
                End Using
                Using penBadge As New Pen(colorBorde, 1.0F)
                    g.DrawPath(penBadge, path)
                End Using
            End Using

            Dim fuenteBadge As New Font("Segoe UI", 8.5F, FontStyle.Bold)
            Dim sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
            Using brushTxt As New SolidBrush(colorTexto)
                g.DrawString(valor, fuenteBadge, brushTxt, rectBadge, sf)
            End Using

            e.Handled = True
            Return
        End If

        ' 2. Botón de Revertir / Eliminar limpio con renderizado vectorial
        If DataGridView1.Columns(e.ColumnIndex).Name = "AccionRevertir" Then
            e.PaintBackground(e.CellBounds, True)

            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

            Dim rectBtn As New Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 9, e.CellBounds.Width - 17, 25)
            If rectBtn.Width > 115 Then rectBtn.Width = 115
            rectBtn.X = e.CellBounds.X + ((e.CellBounds.Width - rectBtn.Width) \ 2)

            Using path As GraphicsPath = CrearRutaRedondeada(rectBtn, 6)
                Using brushFondo As New SolidBrush(Drawing.Color.FromArgb(254, 242, 242))
                    g.FillPath(brushFondo, path)
                End Using
                Using penBtn As New Pen(Drawing.Color.FromArgb(254, 202, 202), 1.0F)
                    g.DrawPath(penBtn, path)
                End Using
            End Using

            Dim textoBtn As String = If(DataGridView1.Columns.Contains("Surtido"), "✖ Revertir", "✖ Eliminar")
            Dim fBtn As New Font("Segoe UI", 8.5F, FontStyle.Bold)
            Dim sfBtn As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
            Using brushTexto As New SolidBrush(Drawing.Color.FromArgb(220, 38, 38))
                g.DrawString(textoBtn, fBtn, brushTexto, rectBtn, sfBtn)
            End Using

            e.Handled = True
            Return
        End If
    End Sub


    ' =========================================================
    ' 10. PANTALLA: MÓDULO AWARE (BOTÓN 9)
    ' =========================================================
    Private Sub ConfigurarPantallaAware()
        panelAware.Dock = DockStyle.Fill
        panelAware.BackColor = Drawing.Color.FromArgb(248, 250, 252)
        panelAware.AutoScroll = True
        pnlContenedorVistas.Controls.Add(panelAware)
        HabilitarDobleBuffer(panelAware)

        Dim lblTitulo As New Label With {
            .Text = "📊 Monitoreo y Análisis AWaRe (Uso Racional de Antimicrobianos)",
            .Location = New Point(25, 20),
            .Font = New Drawing.Font("Segoe UI", 15.0F, Drawing.FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(30, 41, 59),
            .AutoSize = True
        }

        Dim lblSubtitulo As New Label With {
            .Text = "Clasificación de consumo según directrices de la OMS y COFEPRIS: Acceso, Vigilancia y Reserva.",
            .Location = New Point(27, 50),
            .Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Regular),
            .ForeColor = Drawing.Color.FromArgb(100, 116, 139),
            .AutoSize = True
        }

        Dim pnlFiltros As New Panel With {
            .Location = New Point(25, 80),
            .Size = New Size(panelAware.ClientSize.Width - 50, 55),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Drawing.Color.White
        }
        RedondearPanelBorde(pnlFiltros, 12, Drawing.Color.FromArgb(226, 232, 240), 1.0F)

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
        btnFiltrarAware.Size = New Size(170, 35)
        btnFiltrarAware.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnFiltrarAware, 10, ColorPrimario, ColorPrimarioHover, Drawing.Color.White)

        btnImprimirAware.Text = "🖨 Imprimir Informe AWaRe"
        btnImprimirAware.Location = New Point(510, 10)
        btnImprimirAware.Size = New Size(220, 35)
        btnImprimirAware.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnImprimirAware, 10, ColorExito, ColorExitoHover, Drawing.Color.White)

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

        cardAcceso = CrearTarjetaKpi("ACCESO (Access)", ColorExito, lblKpiAccesoNum, lblKpiAccesoPct)
        cardVigi = CrearTarjetaKpi("VIGILANCIA (Watch)", ColorAlerta, lblKpiVigiNum, lblKpiVigiPct)
        cardRes = CrearTarjetaKpi("RESERVA (Reserve)", Drawing.Color.FromArgb(220, 38, 38), lblKpiResNum, lblKpiResPct)
        cardTot = CrearTarjetaKpi("TOTAL DISPENSADO", ColorPrimario, lblKpiTotalNum, lblKpiCumplimiento)

        pnlKpisContainer.Controls.Add(cardAcceso)
        pnlKpisContainer.Controls.Add(cardVigi)
        pnlKpisContainer.Controls.Add(cardRes)
        pnlKpisContainer.Controls.Add(cardTot)
        AjustarTarjetasKpi()

        picGraficoAware.Location = New Point(25, 260)
        picGraficoAware.Size = New Size(panelAware.ClientSize.Width - 50, 110)
        picGraficoAware.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        picGraficoAware.BackColor = Drawing.Color.White
        picGraficoAware.BorderStyle = BorderStyle.None

        Dim lblTitTabla As New Label With {
            .Text = "Detalle de Salidas por Medicamento y Clasificación:",
            .Location = New Point(25, 380),
            .Font = New Drawing.Font("Segoe UI", 11.0F, Drawing.FontStyle.Bold),
            .ForeColor = Drawing.Color.FromArgb(30, 41, 59),
            .AutoSize = True
        }

        pnlTablaAwareContainer.Location = New Point(25, 410)
        pnlTablaAwareContainer.Size = New Size(panelAware.ClientSize.Width - 50, 240)
        pnlTablaAwareContainer.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pnlTablaAwareContainer.BackColor = Drawing.Color.White
        RedondearPanelBorde(pnlTablaAwareContainer, 12, Drawing.Color.FromArgb(226, 232, 240), 1.0F)

        dgvDetalleAware.Dock = DockStyle.Fill
        dgvDetalleAware.BackgroundColor = Drawing.Color.White
        dgvDetalleAware.BorderStyle = BorderStyle.None
        dgvDetalleAware.RowHeadersVisible = False
        dgvDetalleAware.AllowUserToAddRows = False
        dgvDetalleAware.AllowUserToDeleteRows = False
        dgvDetalleAware.ReadOnly = True
        dgvDetalleAware.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvDetalleAware.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvDetalleAware.EnableHeadersVisualStyles = False
        dgvDetalleAware.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.BackColor = ColorOscuro
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.ForeColor = Drawing.Color.White
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Bold)
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True
        dgvDetalleAware.ColumnHeadersDefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
        dgvDetalleAware.ColumnHeadersHeight = 46
        dgvDetalleAware.DefaultCellStyle.Font = New Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Regular)
        dgvDetalleAware.DefaultCellStyle.ForeColor = Drawing.Color.FromArgb(30, 41, 59)
        dgvDetalleAware.DefaultCellStyle.WrapMode = DataGridViewTriState.True
        dgvDetalleAware.DefaultCellStyle.Padding = New Padding(6, 4, 6, 4)
        dgvDetalleAware.RowTemplate.Height = 44
        dgvDetalleAware.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        dgvDetalleAware.GridColor = Drawing.Color.FromArgb(226, 232, 240)
        dgvDetalleAware.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.FromArgb(248, 250, 252)
        pnlTablaAwareContainer.Controls.Add(dgvDetalleAware)

        panelAware.Controls.Add(lblTitulo)
        panelAware.Controls.Add(lblSubtitulo)
        panelAware.Controls.Add(pnlFiltros)
        panelAware.Controls.Add(pnlKpisContainer)
        panelAware.Controls.Add(picGraficoAware)
        panelAware.Controls.Add(lblTitTabla)
        panelAware.Controls.Add(pnlTablaAwareContainer)
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
            .BackColor = Drawing.Color.White
        }
        RedondearPanelBorde(pnl, 12, Drawing.Color.FromArgb(226, 232, 240), 1.2F)

        Dim header As New Label With {
            .Text = titulo,
            .Dock = DockStyle.Top,
            .Height = 28,
            .BackColor = colorCabecera,
            .ForeColor = Drawing.Color.White,
            .Font = New Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter
        }

        lblNum.Text = "0 cajas"
        lblNum.Font = New Drawing.Font("Segoe UI", 16.0F, Drawing.FontStyle.Bold)
        lblNum.ForeColor = Drawing.Color.FromArgb(30, 41, 59)
        lblNum.TextAlign = ContentAlignment.MiddleCenter
        lblNum.Dock = DockStyle.Fill

        lblSub.Text = "0.0% del total"
        lblSub.Font = New Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Bold)
        lblSub.ForeColor = Drawing.Color.FromArgb(100, 116, 139)
        lblSub.TextAlign = ContentAlignment.MiddleCenter
        lblSub.Dock = DockStyle.Bottom
        lblSub.Height = 26

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
            lblKpiCumplimiento.ForeColor = ColorExito
        Else
            lblKpiCumplimiento.Text = "⚠ Meta OMS: < 60% Acceso"
            lblKpiCumplimiento.ForeColor = Drawing.Color.FromArgb(220, 38, 38)
        End If

        picGraficoAware.Invalidate()
    End Sub

    Private Sub picGraficoAware_Paint(sender As Object, e As PaintEventArgs) Handles picGraficoAware.Paint
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

        Using pathBox As GraphicsPath = CrearRutaRedondeada(New Rectangle(0, 0, picGraficoAware.Width - 1, picGraficoAware.Height - 1), 12)
            Using brushFondo As New SolidBrush(Drawing.Color.White)
                g.FillPath(brushFondo, pathBox)
            End Using
            Using penBox As New Pen(Drawing.Color.FromArgb(226, 232, 240), 1.0F)
                g.DrawPath(penBox, pathBox)
            End Using
        End Using

        Dim anchoGrafico As Integer = picGraficoAware.Width - 60
        Dim altoBarra As Integer = 32
        Dim xInicio As Integer = 30
        Dim yBarra As Integer = 40

        Dim fuenteTit As New Font("Segoe UI", 10.0F, FontStyle.Bold)
        Dim fuenteTexto As New Font("Segoe UI", 8.5F, FontStyle.Bold)
        Dim fuenteLeyenda As New Font("Segoe UI", 8.5F, FontStyle.Regular)

        g.DrawString("Distribución Porcentual del Consumo de Antibióticos:", fuenteTit, Brushes.Black, xInicio, 12)

        If totalAware = 0 Then
            g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(241, 245, 249)), xInicio, yBarra, anchoGrafico, altoBarra)
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

        Dim brushAcc As New SolidBrush(ColorExito)
        Dim brushVig As New SolidBrush(ColorAlerta)
        Dim brushRes As New SolidBrush(Drawing.Color.FromArgb(220, 38, 38))

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
        Using penMeta As New Pen(Drawing.Color.FromArgb(37, 99, 235), 2) With {.DashStyle = DashStyle.Dash}
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
    ' 11. MOTOR DE IMPRESIÓN DEL INFORME AWARE (OFICIAL Y NÍTIDO)
    ' =========================================================
    Private Sub btnImprimirAware_Click(sender As Object, e As EventArgs) Handles btnImprimirAware.Click
        If totalAware = 0 Then
            MessageBox.Show("No hay datos de dispensación para el periodo seleccionado.", "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim vistaPrevia As New PrintPreviewDialog()
        vistaPrevia.StartPosition = FormStartPosition.CenterScreen
        docImprimirAware.DefaultPageSettings.Landscape = False
        vistaPrevia.Document = docImprimirAware
        vistaPrevia.WindowState = FormWindowState.Maximized
        vistaPrevia.ShowDialog(Me)
    End Sub

    ' EVENTO PARA RESETEAR PÁGINAS AWARE
    Private Sub docImprimirAware_BeginPrint(sender As Object, e As PrintEventArgs) Handles docImprimirAware.BeginPrint
        numPaginaReporte = 0
    End Sub

    Private Sub docImprimirAware_PrintPage(sender As Object, e As PrintPageEventArgs) Handles docImprimirAware.PrintPage
        numPaginaReporte += 1
        Dim g As Graphics = e.Graphics

        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.InterpolationMode = InterpolationMode.HighQualityBicubic

        Dim fTitulo As New Font("Segoe UI", 13.5F, FontStyle.Bold)
        Dim fSub As New Font("Segoe UI", 9.0F, FontStyle.Regular)
        Dim fSubBold As New Font("Segoe UI", 9.0F, FontStyle.Bold)
        Dim fKpiTit As New Font("Segoe UI", 8.5F, FontStyle.Bold)
        Dim fKpiNum As New Font("Segoe UI", 12.0F, FontStyle.Bold)
        Dim fTablaHeader As New Font("Segoe UI", 8.5F, FontStyle.Bold)
        Dim fTabla As New Font("Segoe UI", 8.5F, FontStyle.Regular)
        Dim brochaNegra As New SolidBrush(Drawing.Color.FromArgb(15, 23, 42))

        Dim margenIzq As Integer = 50
        Dim margenDer As Integer = e.PageBounds.Width - 50
        Dim anchoDisp As Integer = margenDer - margenIzq
        Dim Y As Integer = 45

        Dim xHeaderAware As Integer = margenIzq
        If picLogoConfig.Image IsNot Nothing Then
            Dim rectLogoAware As Rectangle = CalcularRectanguloProporcional(picLogoConfig.Image, margenIzq, Y, 105, 75)
            g.DrawImage(picLogoConfig.Image, rectLogoAware)
            xHeaderAware = margenIzq + rectLogoAware.Width + 15
        End If

        Dim nomFarmaciaReporte As String = If(txtNomFarmacia.Text.Trim() <> "", txtNomFarmacia.Text.Trim().ToUpper(), "NOMBRE DE LA FARMACIA")
        Dim dirFarmaciaReporte As String = If(txtDireccion.Text.Trim() <> "", txtDireccion.Text.Trim(), "Dirección no registrada")
        Dim respFarmaciaReporte As String = If(txtResponsable.Text.Trim() <> "", txtResponsable.Text.Trim(), "No asignado")

        g.DrawString(nomFarmaciaReporte, fTitulo, brochaNegra, xHeaderAware, Y)
        g.DrawString(dirFarmaciaReporte, fSub, brochaNegra, xHeaderAware, Y + 22)
        g.DrawString("Responsable Sanitario: " & respFarmaciaReporte, fSubBold, brochaNegra, xHeaderAware, Y + 38)
        g.DrawString("INFORME DE USO RACIONAL Y CLASIFICACIÓN AWaRe (OMS)", fSubBold, New SolidBrush(ColorPrimario), xHeaderAware, Y + 56)

        Dim periodoStr As String = If(cmbMesAware.Text = "TODOS", "Todo el Año " & txtAnioAware.Text, "Mes: " & cmbMesAware.Text & " / " & txtAnioAware.Text)
        g.DrawString("Periodo Evaluado: " & periodoStr & " | Fecha de Emisión: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fSub, brochaNegra, xHeaderAware, Y + 72)

        Y += 95
        g.DrawLine(New Pen(Drawing.Color.FromArgb(148, 163, 184), 1.2F), margenIzq, Y, margenDer, Y)
        Y += 15

        Dim wCard As Integer = (anchoDisp - 30) \ 4

        Dim pctAcceso As Double = If(totalAware > 0, (cantAcceso / totalAware) * 100.0, 0.0)
        Dim pctVigi As Double = If(totalAware > 0, (cantVigilancia / totalAware) * 100.0, 0.0)
        Dim pctRes As Double = If(totalAware > 0, (cantReserva / totalAware) * 100.0, 0.0)

        DibujarKpiImpresion(g, margenIzq, Y, wCard, 55, "ACCESO", cantAcceso, pctAcceso, ColorExito, fKpiTit, fKpiNum, fSub)
        DibujarKpiImpresion(g, margenIzq + wCard + 10, Y, wCard, 55, "VIGILANCIA", cantVigilancia, pctVigi, ColorAlerta, fKpiTit, fKpiNum, fSub)
        DibujarKpiImpresion(g, margenIzq + (wCard * 2) + 20, Y, wCard, 55, "RESERVA", cantReserva, pctRes, Drawing.Color.FromArgb(220, 38, 38), fKpiTit, fKpiNum, fSub)
        DibujarKpiImpresion(g, margenIzq + (wCard * 3) + 30, Y, wCard, 55, "TOTAL DISPENSADO", totalAware, 100.0, ColorPrimario, fKpiTit, fKpiNum, fSub)

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
            g.FillRectangle(New SolidBrush(ColorExito), margenIzq, Y, wAcc, 24)
        End If
        If wVig > 0 Then
            g.FillRectangle(New SolidBrush(ColorAlerta), margenIzq + wAcc, Y, wVig, 24)
        End If
        If wRes > 0 Then
            g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(220, 38, 38)), margenIzq + wAcc + wVig, Y, wRes, 24)
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
        g.DrawLine(New Pen(Drawing.Color.FromArgb(148, 163, 184), 1.2F), margenIzq, Y, margenDer, Y)
        Y += 15

        g.DrawString("DETALLE DE SALIDAS REGISTRADAS POR CATEGORÍA:", fSubBold, brochaNegra, margenIzq, Y)
        Y += 20

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

        g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(241, 245, 249)), margenIzq, Y, anchoDisp, 26)
        g.DrawRectangle(New Pen(Drawing.Color.FromArgb(203, 213, 225), 1.2F), margenIzq, Y, anchoDisp, 26)

        Dim sfHeaderAw As New StringFormat With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}
        g.DrawString("CATEGORÍA", fTablaHeader, brochaNegra, New RectangleF(xCatAw + 4, Y, wCatAw - 8, 26), sfHeaderAw)
        g.DrawString("GENÉRICO", fTablaHeader, brochaNegra, New RectangleF(xGenAw + 4, Y, wGenAw - 8, 26), sfHeaderAw)
        g.DrawString("DISTINTIVO", fTablaHeader, brochaNegra, New RectangleF(xDisAw + 4, Y, wDisAw - 8, 26), sfHeaderAw)
        g.DrawString("PRESENTACIÓN", fTablaHeader, brochaNegra, New RectangleF(xPreAw + 4, Y, wPreAw - 8, 26), sfHeaderAw)
        g.DrawString("SURTIDO", fTablaHeader, brochaNegra, New RectangleF(xSurAw + 4, Y, wSurAw - 8, 26), sfHeaderAw)
        g.DrawString("% GRUPO", fTablaHeader, brochaNegra, New RectangleF(xPctAw + 4, Y, wPctAw - 8, 26), sfHeaderAw)

        Y += 26

        Dim filasImpresas As Integer = 0
        For Each r As DataRow In dtDetalleAwareSource.Rows
            Dim cat As String = r("Categoría AWaRe").ToString()
            Dim gen As String = r("Genérico").ToString().Trim()
            Dim dis As String = r("Distintivo").ToString().Trim()
            Dim pre As String = r("Presentación").ToString().Trim()
            Dim surt As String = Convert.ToDouble(r("Cajas Surtidas")).ToString("N0")
            Dim pctG As String = r("% de su Grupo").ToString()

            Dim sfGen As SizeF = g.MeasureString(gen, fTabla, wGenAw - 8)
            Dim sfDis As SizeF = g.MeasureString(dis, fTabla, wDisAw - 8)
            Dim sfPre As SizeF = g.MeasureString(pre, fTabla, wPreAw - 8)
            Dim altoFilaAw As Single = Math.Max(24.0F, Math.Max(sfGen.Height, Math.Max(sfDis.Height, sfPre.Height)) + 6.0F)

            If Y + altoFilaAw > e.PageBounds.Height - 110 Then
                Exit For
            End If

            g.DrawString(cat, fTabla, brochaNegra, New RectangleF(xCatAw + 4, Y + 3, wCatAw - 8, altoFilaAw))
            g.DrawString(gen, fTabla, brochaNegra, New RectangleF(xGenAw + 4, Y + 3, wGenAw - 8, altoFilaAw))
            g.DrawString(dis, fTabla, brochaNegra, New RectangleF(xDisAw + 4, Y + 3, wDisAw - 8, altoFilaAw))
            g.DrawString(pre, fTabla, brochaNegra, New RectangleF(xPreAw + 4, Y + 3, wPreAw - 8, altoFilaAw))
            g.DrawString(surt, fTabla, brochaNegra, New RectangleF(xSurAw + 4, Y + 3, wSurAw - 8, altoFilaAw))
            g.DrawString(pctG, fTabla, brochaNegra, New RectangleF(xPctAw + 4, Y + 3, wPctAw - 8, altoFilaAw))

            Y += CInt(altoFilaAw)
            g.DrawLine(New Pen(Drawing.Color.FromArgb(226, 232, 240), 1.0F), margenIzq, Y, margenDer, Y)
            filasImpresas += 1
        Next

        Y = e.PageBounds.Height - 110
        g.DrawLine(Pens.Black, margenIzq + 180, Y, margenDer - 180, Y)
        Y += 8

        Dim sfCentro As New StringFormat With {.Alignment = StringAlignment.Center}
        g.DrawString(If(txtResponsable.Text.Trim() <> "", txtResponsable.Text.Trim().ToUpper(), "RESPONSABLE SANITARIO"), fSubBold, brochaNegra, e.PageBounds.Width \ 2, Y, sfCentro)
        Y += 16
        g.DrawString("Responsable Sanitario", fSub, brochaNegra, e.PageBounds.Width \ 2, Y, sfCentro)

        e.HasMorePages = False
    End Sub

    Private Sub DibujarKpiImpresion(g As Graphics, x As Integer, y As Integer, w As Integer, h As Integer, titulo As String, total As Double, pct As Double, colorBorde As Drawing.Color, fTit As Font, fNum As Font, fSub As Font)
        g.FillRectangle(Brushes.White, x, y, w, h)
        Using penKpi As New Pen(colorBorde, 1.5F)
            g.DrawRectangle(penKpi, x, y, w, h)
        End Using
        Using brushKpi As New SolidBrush(colorBorde)
            g.FillRectangle(brushKpi, x, y, w, 20)
        End Using

        Dim sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
        Using brushBlanco As New SolidBrush(Drawing.Color.White)
            g.DrawString(titulo, fTit, brushBlanco, New RectangleF(x, y, w, 20), sf)
        End Using
        Using brushNegro As New SolidBrush(Drawing.Color.FromArgb(15, 23, 42))
            g.DrawString(total.ToString("N0") & " cajas", fNum, brushNegro, New RectangleF(x, y + 21, w, 20), sf)
        End Using
        Using brushGris As New SolidBrush(Drawing.Color.DimGray)
            g.DrawString(pct.ToString("0.0") & "% del total", fSub, brushGris, New RectangleF(x, y + 38, w, 16), sf)
        End Using
    End Sub


    ' =========================================================
    ' 12. PANTALLA DE REPORTES REGULARES Y KARDEX
    ' =========================================================
    Private Sub ConfigurarPantallaReportes()
        panelReportes.Dock = DockStyle.Fill
        panelReportes.BackColor = Drawing.Color.White
        panelReportes.AutoScroll = True
        pnlContenedorVistas.Controls.Add(panelReportes)
        HabilitarDobleBuffer(panelReportes)

        Dim lblTitulo As New Label With {.Text = "Generador de Reportes (Bitácora Oficial)", .Location = New Point(35, 25), .Font = New Drawing.Font("Segoe UI", 16.0F, Drawing.FontStyle.Bold), .AutoSize = True}

        Dim lblMod As New Label With {.Text = "Módulo a imprimir:", .Location = New Point(35, 80), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 11.0F)}
        cmbModuloRep.Items.Clear()
        cmbModuloRep.Items.AddRange(New String() {"Entradas", "Salidas", "Kardex Combinado (Entradas y Salidas)"})
        cmbModuloRep.SelectedIndex = 2
        cmbModuloRep.Location = New Point(200, 78)
        cmbModuloRep.Size = New Size(280, 30)
        cmbModuloRep.Font = New Drawing.Font("Segoe UI", 10.5F)
        cmbModuloRep.DropDownStyle = ComboBoxStyle.DropDownList

        Dim lblMes As New Label With {.Text = "Mes (MM):", .Location = New Point(35, 130), .AutoSize = True, .Font = New Drawing.Font("Segoe UI", 11.0F)}
        cmbMesRep.Items.Clear()
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
        btnGenerarRep.Size = New Size(445, 48)
        btnGenerarRep.Font = New Drawing.Font("Segoe UI", 11.5F, Drawing.FontStyle.Bold)
        EstilizarBotonSuave(btnGenerarRep, 14, ColorPrimario, ColorPrimarioHover, Drawing.Color.White)

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

            If cmbModuloRep.Text.Contains("Kardex") Then
                consulta = "SELECT 'ENTRADA' AS TipoMov, E.Id, E.Fecha, E.Codigo, E.Generico, E.Distintivo, E.Presentacion, E.AWARE, E.Lote, E.Caducidad, E.Existencia, E.Surtido, E.Saldo, E.Factura AS RefDoc, E.Proveedor AS NomTercero, P.RFC AS RFCTercero, P.Direccion AS DirTercero " &
                           "FROM Entradas E LEFT JOIN Proveedores P ON TRIM(E.Proveedor) = TRIM(P.Proveedor) WHERE E.Fecha LIKE @filtro " &
                           "UNION ALL " &
                           "SELECT 'SALIDA' AS TipoMov, S.Id, S.Fecha, S.Codigo, S.Generico, S.Distintivo, S.Presentacion, S.AWARE, S.Lote, S.Caducidad, S.Existencia, S.Surtido, S.Saldo, (S.Movimiento || ' Fol:' || S.Folio) AS RefDoc, M.NombreMed AS NomTercero, M.Cedula AS RFCTercero, (M.Calle || ' ' || M.NoExt || ', ' || M.Colonia || ', ' || M.Ciudad) AS DirTercero " &
                           "FROM Salidas S LEFT JOIN Medicos M ON TRIM(S.Cedula) = TRIM(M.Cedula) WHERE S.Fecha LIKE @filtro " &
                           "ORDER BY Codigo ASC, Fecha ASC, TipoMov ASC"
            ElseIf cmbModuloRep.Text = "Salidas" Then
                consulta = "SELECT S.*, M.NombreMed, M.Calle, M.NoInt, M.NoExt, M.Colonia, M.Ciudad, M.Estado, M.CP, M.Pais, M.Tel AS TelMed " &
                           "FROM Salidas S LEFT JOIN Medicos M ON TRIM(S.Cedula) = TRIM(M.Cedula) " &
                           "WHERE S.Fecha LIKE @filtro ORDER BY Codigo ASC, Fecha ASC"
            Else
                consulta = "SELECT E.*, P.RFC AS RFCProv, P.Direccion AS DirProv " &
                           "FROM Entradas E LEFT JOIN Proveedores P ON TRIM(E.Proveedor) = TRIM(P.Proveedor) " &
                           "WHERE E.Fecha LIKE @filtro ORDER BY Codigo ASC, Fecha ASC"
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
        vistaPrevia.StartPosition = FormStartPosition.CenterScreen
        docImprimir.DefaultPageSettings.Landscape = True
        vistaPrevia.Document = docImprimir
        vistaPrevia.WindowState = FormWindowState.Maximized
        vistaPrevia.ShowDialog(Me)
    End Sub

    ' =========================================================================
    ' EVENTO: REINICIO DE CONTADORES ANTES DE GENERAR REPORTE O VISTA PREVIA
    ' =========================================================================
    Private Sub docImprimir_BeginPrint(sender As Object, e As Printing.PrintEventArgs) Handles docImprimir.BeginPrint
        numPaginaReporte = 0
        indiceImpresion = 0
        codigoActualGrupo = ""
    End Sub

    ' =========================================================================
    ' MOTOR DE IMPRESIÓN OFICIAL: AGRUPADO, ULTRA COMPACTO, AWARE Y NÍTIDO
    ' =========================================================================
    Private Sub docImprimir_PrintPage(sender As Object, e As PrintPageEventArgs) Handles docImprimir.PrintPage
        numPaginaReporte += 1
        Dim g As Graphics = e.Graphics

        ' Alta calidad de renderizado vectorial
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
        g.InterpolationMode = InterpolationMode.HighQualityBicubic

        ' Fuentes para cabeceras y pie
        Dim fuenteTitulo As New Font("Segoe UI", 12.0F, FontStyle.Bold)
        Dim fuenteSub As New Font("Segoe UI", 8.5F, FontStyle.Regular)
        Dim fuenteSubBold As New Font("Segoe UI", 8.5F, FontStyle.Bold)

        ' FUENTES DE TABLA ULTRA REDUCIDAS (Para máxima densidad de información)
        Dim fuenteTablaHeader As New Font("Segoe UI", 7.0F, FontStyle.Bold)
        Dim fuenteTabla As New Font("Segoe UI", 6.5F, FontStyle.Regular)
        Dim fuenteGrupo As New Font("Segoe UI", 7.0F, FontStyle.Bold)
        Dim fuenteAware As New Font("Segoe UI", 6.5F, FontStyle.Bold)

        Dim brochaNegra As New SolidBrush(Drawing.Color.FromArgb(15, 23, 42))
        Dim brochaAzul As New SolidBrush(ColorPrimario)

        Dim margenIzq As Integer = 35
        Dim margenDer As Integer = e.PageBounds.Width - 35
        Dim anchoTotal As Integer = margenDer - margenIzq
        Dim Y As Integer = 35

        ' --- CABECERA DEL REPORTE ---
        Dim xHeader As Integer = margenIzq
        If picLogoConfig.Image IsNot Nothing Then
            Dim rectLogo As Rectangle = CalcularRectanguloProporcional(picLogoConfig.Image, margenIzq, Y, 110, 75)
            g.DrawImage(picLogoConfig.Image, rectLogo)
            xHeader = margenIzq + rectLogo.Width + 15
        End If

        Dim nomFarmaciaReporte As String = If(txtNomFarmacia.Text.Trim() <> "", txtNomFarmacia.Text.Trim().ToUpper(), "NOMBRE DE LA FARMACIA")
        Dim dirFarmaciaReporte As String = If(txtDireccion.Text.Trim() <> "", txtDireccion.Text.Trim(), "Dirección no registrada")
        Dim respFarmaciaReporte As String = If(txtResponsable.Text.Trim() <> "", txtResponsable.Text.Trim(), "No asignado")

        g.DrawString(nomFarmaciaReporte, fuenteTitulo, brochaAzul, xHeader, Y)
        g.DrawString(dirFarmaciaReporte, fuenteSub, brochaNegra, xHeader, Y + 18)
        g.DrawString("Responsable Sanitario: " & respFarmaciaReporte, fuenteSubBold, brochaNegra, xHeader, Y + 32)

        Dim tituloBitacora As String = "BITÁCORA OFICIAL DE CONTROL DE GRUPO IV - ANTIMICROBIANOS"
        g.DrawString(tituloBitacora, fuenteSubBold, brochaNegra, xHeader, Y + 48)

        ' Regresa el Subtítulo a Mes/Año
        Dim strPeriodo As String = "Periodo: Mes " & cmbMesRep.Text & " del Año " & txtAnioRep.Text & "  |  Emisión: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm") & "  |  Pág. " & numPaginaReporte.ToString()
        g.DrawString(strPeriodo, fuenteSub, Brushes.DimGray, xHeader, Y + 62)

        Y += 80
        g.DrawLine(New Pen(ColorPrimario, 1.5F), margenIzq, Y, margenDer, Y)
        Y += 4

        ' --- CÁLCULO DE COLUMNAS ULTRA OPTIMIZADO ---
        Dim wFecha As Integer = CInt(anchoTotal * 0.1)
        Dim wLoteCad As Integer = CInt(anchoTotal * 0.16)
        Dim wStock As Integer = CInt(anchoTotal * 0.16)
        Dim wMovFac As Integer = CInt(anchoTotal * 0.2)
        Dim wTercero As Integer = anchoTotal - (wFecha + wLoteCad + wStock + wMovFac)

        Dim xFecha As Integer = margenIzq
        Dim xLote As Integer = xFecha + wFecha
        Dim xStock As Integer = xLote + wLoteCad
        Dim xMovFac As Integer = xStock + wStock
        Dim xTercero As Integer = xMovFac + wMovFac

        ' --- ENCABEZADOS DE TABLA ---
        Dim altoHeader As Integer = 18
        g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(241, 245, 249)), margenIzq, Y, anchoTotal, altoHeader)
        g.DrawRectangle(New Pen(Drawing.Color.FromArgb(203, 213, 225), 1.2F), margenIzq, Y, anchoTotal, altoHeader)

        Dim sfHeader As New StringFormat With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}

        g.DrawString("FECHA / HORA", fuenteTablaHeader, brochaNegra, New RectangleF(xFecha + 2, Y, wFecha - 4, altoHeader), sfHeader)
        g.DrawString("LOTE Y CADUCIDAD", fuenteTablaHeader, brochaNegra, New RectangleF(xLote + 2, Y, wLoteCad - 4, altoHeader), sfHeader)
        g.DrawString("EXIST. / MOV. / SALDO", fuenteTablaHeader, brochaNegra, New RectangleF(xStock + 2, Y, wStock - 4, altoHeader), sfHeader)

        If cmbModuloRep.Text.Contains("Kardex") Then
            g.DrawString("MOVIMIENTO / FOLIO", fuenteTablaHeader, brochaNegra, New RectangleF(xMovFac + 2, Y, wMovFac - 4, altoHeader), sfHeader)
            g.DrawString("PROVEEDOR / MÉDICO PRESCRIPTOR", fuenteTablaHeader, brochaNegra, New RectangleF(xTercero + 2, Y, wTercero - 4, altoHeader), sfHeader)
        ElseIf cmbModuloRep.Text = "Entradas" Then
            g.DrawString("MOVIMIENTO / FACTURA", fuenteTablaHeader, brochaNegra, New RectangleF(xMovFac + 2, Y, wMovFac - 4, altoHeader), sfHeader)
            g.DrawString("PROVEEDOR (RAZÓN SOCIAL, RFC Y DIR.)", fuenteTablaHeader, brochaNegra, New RectangleF(xTercero + 2, Y, wTercero - 4, altoHeader), sfHeader)
        Else
            g.DrawString("MOVIMIENTO / FOLIO", fuenteTablaHeader, brochaNegra, New RectangleF(xMovFac + 2, Y, wMovFac - 4, altoHeader), sfHeader)
            g.DrawString("MÉDICO PRESCRIPTOR (DATOS COMPLETOS)", fuenteTablaHeader, brochaNegra, New RectangleF(xTercero + 2, Y, wTercero - 4, altoHeader), sfHeader)
        End If

        Dim penGrid As New Pen(Drawing.Color.FromArgb(203, 213, 225), 1.0F)
        Y += altoHeader

        Dim esPrimerRegistroPagina As Boolean = True

        ' --- DIBUJO DE FILAS ---
        While indiceImpresion < dtImprimir.Rows.Count
            Dim fila As DataRow = dtImprimir.Rows(indiceImpresion)
            Dim codigoStr As String = If(fila.Table.Columns.Contains("Codigo"), fila("Codigo").ToString(), "")

            ' =========================================================
            ' LÓGICA DE AGRUPADOR AZUL CON BADGE AWARE
            ' =========================================================
            If codigoStr <> codigoActualGrupo OrElse esPrimerRegistroPagina Then
                Dim genStrG As String = fila("Generico").ToString().Trim()
                Dim distStrG As String = If(fila.Table.Columns.Contains("Distintivo"), fila("Distintivo").ToString().Trim(), "")
                Dim presStrG As String = If(fila.Table.Columns.Contains("Presentacion"), fila("Presentacion").ToString().Trim(), "")
                Dim awareStrG As String = If(fila.Table.Columns.Contains("AWARE"), fila("AWARE").ToString().Trim().ToUpper(), "")

                Dim tituloGrupo As String = "▶ CÓD. " & codigoStr & " | Denominación Distintiva: " & If(distStrG = "", "N/A", distStrG) & " | Denominación Genérica: " & genStrG & " | Presentación: " & presStrG
                If codigoStr = codigoActualGrupo AndAlso esPrimerRegistroPagina Then
                    tituloGrupo &= " (Continuación)"
                End If

                Dim espacioBlanco As Integer = If(esPrimerRegistroPagina, 0, 6)
                Dim altoGrupo As Integer = 16
                Dim anchoBadge As Integer = 70

                If Y + espacioBlanco + altoGrupo + 18 > e.PageBounds.Height - 75 Then
                    e.HasMorePages = True
                    Exit While
                End If

                Y += espacioBlanco

                ' Fondo de la banda azul
                Dim rectGrupo As New RectangleF(margenIzq, Y, anchoTotal, altoGrupo)
                g.FillRectangle(New SolidBrush(Drawing.Color.FromArgb(239, 246, 255)), rectGrupo)
                g.DrawRectangle(New Pen(Drawing.Color.FromArgb(186, 230, 253), 1.0F), margenIzq, Y, anchoTotal, altoGrupo)

                Dim sfGrupo As New StringFormat With {.Alignment = StringAlignment.Near, .LineAlignment = StringAlignment.Center}
                g.DrawString(tituloGrupo, fuenteGrupo, brochaNegra, New RectangleF(margenIzq + 4, Y, anchoTotal - anchoBadge - 10, altoGrupo), sfGrupo)

                ' Dibujo del Badge AWaRe
                If awareStrG <> "" AndAlso awareStrG <> "NO ASIGNADO" Then
                    Dim colorFondoAw As Drawing.Color = Drawing.Color.FromArgb(241, 245, 249)
                    Dim colorTextoAw As Drawing.Color = Drawing.Color.FromArgb(71, 85, 105)
                    Dim colorBordeAw As Drawing.Color = Drawing.Color.FromArgb(203, 213, 225)

                    If awareStrG.Contains("ACCES") Then
                        colorFondoAw = Drawing.Color.FromArgb(220, 252, 231)
                        colorTextoAw = Drawing.Color.FromArgb(21, 128, 61)
                        colorBordeAw = Drawing.Color.FromArgb(134, 239, 172)
                    ElseIf awareStrG.Contains("VIGILAN") OrElse awareStrG.Contains("WATCH") Then
                        colorFondoAw = Drawing.Color.FromArgb(254, 243, 199)
                        colorTextoAw = Drawing.Color.FromArgb(180, 83, 9)
                        colorBordeAw = Drawing.Color.FromArgb(252, 211, 77)
                    ElseIf awareStrG.Contains("RESERV") Then
                        colorFondoAw = Drawing.Color.FromArgb(254, 226, 226)
                        colorTextoAw = Drawing.Color.FromArgb(185, 28, 28)
                        colorBordeAw = Drawing.Color.FromArgb(252, 165, 165)
                    End If

                    Dim rectBadge As New Rectangle(margenDer - anchoBadge - 2, Y + 2, anchoBadge, altoGrupo - 4)
                    Using pathBadge As Drawing2D.GraphicsPath = CrearRutaRedondeada(rectBadge, 4)
                        Using bFondo As New SolidBrush(colorFondoAw)
                            g.FillPath(bFondo, pathBadge)
                        End Using
                        Using pBorde As New Pen(colorBordeAw, 1.0F)
                            g.DrawPath(pBorde, pathBadge)
                        End Using
                    End Using

                    Dim sfAware As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                    Using bTexto As New SolidBrush(colorTextoAw)
                        g.DrawString(awareStrG, fuenteAware, bTexto, rectBadge, sfAware)
                    End Using
                End If

                Y += altoGrupo
                codigoActualGrupo = codigoStr
                esPrimerRegistroPagina = False
            End If

            ' --- EXTRACCIÓN DE DATOS DE LA FILA ---
            Dim fechaRaw As String = fila("Fecha").ToString()
            Dim partesF As String() = fechaRaw.Split(" "c)
            Dim fechaSolo As String = partesF(0)
            Dim horaSolo As String = If(partesF.Length > 1, partesF(1) & If(partesF.Length > 2, " " & partesF(2), ""), "")
            Dim fechaCelda As String = fechaSolo & If(horaSolo <> "", vbCrLf & horaSolo, "")

            Dim loteVal As String = fila("Lote").ToString().Trim()
            Dim cadVal As String = fila("Caducidad").ToString().Trim()
            Dim loteCadStr As String = "Lote: " & loteVal & vbCrLf & "Cad: " & cadVal

            Dim exisVal As Double = If(fila.Table.Columns.Contains("Existencia") AndAlso Not IsDBNull(fila("Existencia")), Convert.ToDouble(fila("Existencia")), 0)
            Dim surtVal As Double = If(fila.Table.Columns.Contains("Surtido") AndAlso Not IsDBNull(fila("Surtido")), Convert.ToDouble(fila("Surtido")), 0)
            Dim saldoVal As Double = If(fila.Table.Columns.Contains("Saldo") AndAlso Not IsDBNull(fila("Saldo")), Convert.ToDouble(fila("Saldo")), 0)

            Dim esSalida As Boolean = False
            If fila.Table.Columns.Contains("TipoMov") Then
                esSalida = (fila("TipoMov").ToString() = "SALIDA")
            Else
                esSalida = (cmbModuloRep.Text = "Salidas")
            End If

            Dim stockStr As String = "Existencia: " & exisVal.ToString("N0") & vbCrLf &
                                     If(esSalida, "Surtido: -", "Entrada: +") & surtVal.ToString("N0") & vbCrLf &
                                     "Saldo: " & saldoVal.ToString("N0")

            Dim movFacStr As String = ""
            Dim terceroStr As String = ""

            If cmbModuloRep.Text.Contains("Kardex") Then
                Dim tipo As String = fila("TipoMov").ToString()
                Dim ref As String = fila("RefDoc").ToString().Trim()
                movFacStr = tipo & vbCrLf & If(esSalida, "Folio: ", "Factura: ") & ref

                Dim nom As String = fila("NomTercero").ToString().Trim()
                Dim rfcCed As String = fila("RFCTercero").ToString().Trim()
                Dim dirT As String = fila("DirTercero").ToString().Trim()
                terceroStr = nom & If(rfcCed <> "", " (" & rfcCed & ")", "") & vbCrLf & If(dirT <> "", dirT, "S/D")
            ElseIf cmbModuloRep.Text = "Entradas" Then
                movFacStr = "ENTRADA" & vbCrLf & "Factura: " & fila("Factura").ToString().Trim()

                Dim prov As String = fila("Proveedor").ToString().Trim()
                Dim rfc As String = If(fila.Table.Columns.Contains("RFCProv") AndAlso fila("RFCProv").ToString().Trim() <> "", fila("RFCProv").ToString().Trim(), If(fila.Table.Columns.Contains("RFC"), fila("RFC").ToString().Trim(), ""))
                Dim dirProv As String = If(fila.Table.Columns.Contains("DirProv") AndAlso fila("DirProv").ToString().Trim() <> "", fila("DirProv").ToString().Trim(), If(fila.Table.Columns.Contains("Direccion"), fila("Direccion").ToString().Trim(), ""))
                terceroStr = prov & If(rfc <> "", " (RFC: " & rfc & ")", "") & vbCrLf & dirProv
            Else
                Dim mov As String = If(fila.Table.Columns.Contains("Movimiento"), fila("Movimiento").ToString().Trim(), "SALIDA")
                Dim fol As String = If(fila.Table.Columns.Contains("Folio"), fila("Folio").ToString().Trim(), "")
                movFacStr = mov & vbCrLf & "Folio: " & fol

                Dim nomMed As String = If(fila.Table.Columns.Contains("NombreMed") AndAlso fila("NombreMed").ToString().Trim() <> "", fila("NombreMed").ToString().Trim(), If(fila.Table.Columns.Contains("Nombre"), fila("Nombre").ToString().Trim(), ""))
                Dim cedMed As String = If(fila.Table.Columns.Contains("Cedula"), fila("Cedula").ToString().Trim(), "")
                terceroStr = nomMed & If(cedMed <> "", " (Cédula: " & cedMed & ")", "")
            End If

            ' Medir altura dinámica
            Dim sfFecha As SizeF = g.MeasureString(fechaCelda, fuenteTabla, wFecha - 4)
            Dim sfLote As SizeF = g.MeasureString(loteCadStr, fuenteTabla, wLoteCad - 4)
            Dim sfStock As SizeF = g.MeasureString(stockStr, fuenteTabla, wStock - 4)
            Dim sfMov As SizeF = g.MeasureString(movFacStr, fuenteTabla, wMovFac - 4)
            Dim sfTercero As SizeF = g.MeasureString(terceroStr, fuenteTabla, wTercero - 4)

            Dim maxAlturaContenido As Single = Math.Max(sfFecha.Height, Math.Max(sfLote.Height, Math.Max(sfStock.Height, Math.Max(sfMov.Height, sfTercero.Height))))

            Dim altoFila As Single = Math.Max(15.0F, maxAlturaContenido + 3.0F)

            If Y + altoFila > e.PageBounds.Height - 65 AndAlso indiceImpresion < dtImprimir.Rows.Count Then
                e.HasMorePages = True
                Exit While
            End If

            ' Dibujar textos ajustados
            g.DrawString(fechaCelda, fuenteTabla, brochaNegra, New RectangleF(xFecha + 2, Y + 1, wFecha - 4, altoFila))
            g.DrawString(loteCadStr, fuenteTabla, brochaNegra, New RectangleF(xLote + 2, Y + 1, wLoteCad - 4, altoFila))
            g.DrawString(stockStr, fuenteTabla, brochaNegra, New RectangleF(xStock + 2, Y + 1, wStock - 4, altoFila))
            g.DrawString(movFacStr, fuenteTabla, brochaNegra, New RectangleF(xMovFac + 2, Y + 1, wMovFac - 4, altoFila))
            g.DrawString(terceroStr, fuenteTabla, brochaNegra, New RectangleF(xTercero + 2, Y + 1, wTercero - 4, altoFila))

            ' Dibujar bordes separadores
            g.DrawLine(penGrid, xLote, Y, xLote, Y + altoFila)
            g.DrawLine(penGrid, xStock, Y, xStock, Y + altoFila)
            g.DrawLine(penGrid, xMovFac, Y, xMovFac, Y + altoFila)
            g.DrawLine(penGrid, xTercero, Y, xTercero, Y + altoFila)

            Y += CInt(altoFila)
            g.DrawLine(New Pen(Drawing.Color.FromArgb(226, 232, 240), 1.0F), margenIzq, Y, margenDer, Y)
            indiceImpresion += 1
        End While

        ' --- PIE DE PÁGINA (FIRMA OBLIGATORIA) ---
        Dim yFirma As Integer = e.PageBounds.Height - 55
        Dim firmaTexto As String = "________________________________________________"
        Dim respTexto As String = "Responsable Sanitario: " & If(txtResponsable.Text.Trim() <> "", txtResponsable.Text.Trim(), "No asignado")

        Dim anchoFirma As Integer = CInt(g.MeasureString(firmaTexto, fuenteSub).Width)
        Dim anchoResp As Integer = CInt(g.MeasureString(respTexto, fuenteSubBold).Width)
        Dim centroX As Integer = e.PageBounds.Width \ 2

        g.DrawString(firmaTexto, fuenteSub, brochaNegra, centroX - (anchoFirma \ 2), yFirma)
        g.DrawString(respTexto, fuenteSubBold, brochaNegra, centroX - (anchoResp \ 2), yFirma + 16)

        If Not e.HasMorePages Then
            indiceImpresion = 0
        End If
    End Sub

    ' =========================================================
    ' 13. MOTOR GENERAL DE IMPORTACIÓN CSV POR MENÚS
    ' =========================================================
    Private Sub btnImportarCSV_Click(sender As Object, e As EventArgs) Handles btnImportarCSV.Click
        EjecutarImportadorCSVGeneral("")
    End Sub

    Private Sub EjecutarImportadorCSVGeneral(moduloSugerido As String)
        Dim dialog As New OpenFileDialog()
        dialog.Filter = "Archivos CSV de Excel (*.csv)|*.csv"
        dialog.Title = If(moduloSugerido <> "", "Selecciona tu archivo CSV para: " & moduloSugerido, "Selecciona tu archivo guardado como CSV")

        If dialog.ShowDialog(Me) = DialogResult.OK Then
            Try
                Using parser As New TextFieldParser(dialog.FileName, Encoding.Default)
                    parser.TextFieldType = FieldType.Delimited
                    parser.SetDelimiters(",")

                    If parser.EndOfData Then
                        Return
                    End If

                    Dim encabezados As String() = parser.ReadFields()
                    Dim encabStr As String = String.Join("", encabezados).ToUpper().Replace(" ", "").Replace("_", "").Replace(".", "")

                    Dim tablaDestino As String = ""

                    If encabStr.Contains("FACTURA") OrElse (encabStr.Contains("SURTIDO") AndAlso encabStr.Contains("PROVEEDOR")) Then
                        tablaDestino = "Entradas"
                    ElseIf encabStr.Contains("RECETA") OrElse encabStr.Contains("FOLIO") OrElse (encabStr.Contains("SURTIDO") AndAlso encabStr.Contains("CEDULA")) Then
                        tablaDestino = "Salidas"
                    ElseIf encabStr.Contains("EXISTENCIA") OrElse encabStr.Contains("AWARE") Then
                        tablaDestino = "Inventario"
                    ElseIf encabStr.Contains("RFC") AndAlso encabStr.Contains("PROVEEDOR") Then
                        tablaDestino = "Proveedores"
                    ElseIf encabStr.Contains("CEDULA") AndAlso encabStr.Contains("NOMBREMED") Then
                        tablaDestino = "Medicos"
                    ElseIf moduloSugerido <> "" Then
                        tablaDestino = moduloSugerido
                    Else
                        MessageBox.Show("No se reconoció automáticamente el formato del archivo CSV." & vbCrLf & "Verifica las columnas correspondientes.", "Formato Inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If

                    If MessageBox.Show("Se procesará la importación hacia el área de: " & tablaDestino.ToUpper() & "." & vbCrLf & "¿Deseas continuar?", "Confirmar Importación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        Dim registros As Integer = 0

                        Using conexion As New SQLiteConnection(cadenaConexion)
                            conexion.Open()
                            Using transaccion As SQLiteTransaction = conexion.BeginTransaction()

                                While Not parser.EndOfData
                                    Dim datos As String() = parser.ReadFields()
                                    If datos Is Nothing OrElse datos.Length = 0 Then Continue While

                                    Select Case tablaDestino
                                        Case "Entradas"
                                            Dim fFecha As String = If(datos.Length > 0, datos(0), DateTime.Now.ToString("dd/MM/yyyy"))
                                            Dim fCod As String = If(datos.Length > 1, datos(1).Trim(), "")
                                            Dim fGen As String = If(datos.Length > 2, datos(2).Trim(), "")
                                            Dim fDis As String = If(datos.Length > 3, datos(3).Trim(), "")
                                            Dim fPre As String = If(datos.Length > 4, datos(4).Trim(), "")
                                            Dim fAw As String = If(datos.Length > 5, datos(5).Trim().ToUpper(), "")
                                            Dim fLot As String = If(datos.Length > 6, datos(6).Trim(), "")
                                            Dim fCad As String = If(datos.Length > 7, datos(7).Trim(), "")
                                            Dim fCant As Double = If(datos.Length > 8, Val(datos(8)), 0)
                                            Dim fFac As String = If(datos.Length > 9, datos(9).Trim(), "")
                                            Dim fProv As String = If(datos.Length > 10, datos(10).Trim(), "")
                                            Dim fRfc As String = If(datos.Length > 11, datos(11).Trim(), "")
                                            Dim fDir As String = If(datos.Length > 12, datos(12).Trim(), "")

                                            If fCod = "" Then Continue While

                                            Dim exisActual As Double = 0
                                            Dim cmdEx As New SQLiteCommand("SELECT ExistenciaActual FROM Inventario WHERE Codigo = @cod", conexion, transaccion)
                                            cmdEx.Parameters.AddWithValue("@cod", fCod)
                                            Dim objEx = cmdEx.ExecuteScalar()
                                            If objEx IsNot Nothing AndAlso Not IsDBNull(objEx) Then
                                                exisActual = Convert.ToDouble(objEx)
                                            Else
                                                Dim cmdNuevoMed As New SQLiteCommand("INSERT INTO Inventario (Codigo, Generico, Distintivo, Presentacion, AWARE, ExistenciaActual) VALUES (@c, @g, @d, @p, @a, 0)", conexion, transaccion)
                                                cmdNuevoMed.Parameters.AddWithValue("@c", fCod)
                                                cmdNuevoMed.Parameters.AddWithValue("@g", fGen)
                                                cmdNuevoMed.Parameters.AddWithValue("@d", fDis)
                                                cmdNuevoMed.Parameters.AddWithValue("@p", fPre)
                                                cmdNuevoMed.Parameters.AddWithValue("@a", fAw)
                                                cmdNuevoMed.ExecuteNonQuery()
                                            End If

                                            Dim nuevoSaldo As Double = exisActual + fCant

                                            Dim cmdEnt As New SQLiteCommand("INSERT INTO Entradas (Fecha, Codigo, Generico, Distintivo, Presentacion, AWARE, Lote, Caducidad, Existencia, Surtido, Saldo, Factura, Proveedor, RFC, Direccion) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14)", conexion, transaccion)
                                            cmdEnt.Parameters.AddWithValue("@p0", fFecha)
                                            cmdEnt.Parameters.AddWithValue("@p1", fCod)
                                            cmdEnt.Parameters.AddWithValue("@p2", fGen)
                                            cmdEnt.Parameters.AddWithValue("@p3", fDis)
                                            cmdEnt.Parameters.AddWithValue("@p4", fPre)
                                            cmdEnt.Parameters.AddWithValue("@p5", fAw)
                                            cmdEnt.Parameters.AddWithValue("@p6", fLot)
                                            cmdEnt.Parameters.AddWithValue("@p7", fCad)
                                            cmdEnt.Parameters.AddWithValue("@p8", exisActual)
                                            cmdEnt.Parameters.AddWithValue("@p9", fCant)
                                            cmdEnt.Parameters.AddWithValue("@p10", nuevoSaldo)
                                            cmdEnt.Parameters.AddWithValue("@p11", fFac)
                                            cmdEnt.Parameters.AddWithValue("@p12", fProv)
                                            cmdEnt.Parameters.AddWithValue("@p13", fRfc)
                                            cmdEnt.Parameters.AddWithValue("@p14", fDir)
                                            cmdEnt.ExecuteNonQuery()

                                            Dim cmdUpdStock As New SQLiteCommand("UPDATE Inventario SET ExistenciaActual = @saldo WHERE Codigo = @cod", conexion, transaccion)
                                            cmdUpdStock.Parameters.AddWithValue("@saldo", nuevoSaldo)
                                            cmdUpdStock.Parameters.AddWithValue("@cod", fCod)
                                            cmdUpdStock.ExecuteNonQuery()

                                        Case "Salidas"
                                            Dim fFecha As String = If(datos.Length > 0, datos(0), DateTime.Now.ToString("dd/MM/yyyy"))
                                            Dim fCod As String = If(datos.Length > 1, datos(1).Trim(), "")
                                            Dim fGen As String = If(datos.Length > 2, datos(2).Trim(), "")
                                            Dim fDis As String = If(datos.Length > 3, datos(3).Trim(), "")
                                            Dim fPre As String = If(datos.Length > 4, datos(4).Trim(), "")
                                            Dim fAw As String = If(datos.Length > 5, datos(5).Trim().ToUpper(), "")
                                            Dim fLot As String = If(datos.Length > 6, datos(6).Trim(), "")
                                            Dim fCad As String = If(datos.Length > 7, datos(7).Trim(), "")
                                            Dim fCant As Double = If(datos.Length > 8, Val(datos(8)), 0)
                                            Dim fMov As String = If(datos.Length > 9, datos(9).Trim(), "RECETA")
                                            Dim fFol As String = If(datos.Length > 10, datos(10).Trim(), "")
                                            Dim fCed As String = If(datos.Length > 11, datos(11).Trim(), "")
                                            Dim fNom As String = If(datos.Length > 12, datos(12).Trim(), "")
                                            Dim fDir As String = If(datos.Length > 13, datos(13).Trim(), "")
                                            Dim fTel As String = If(datos.Length > 14, datos(14).Trim(), "")

                                            If fCod = "" Then Continue While

                                            Dim exisActual As Double = 0
                                            Dim cmdEx As New SQLiteCommand("SELECT ExistenciaActual FROM Inventario WHERE Codigo = @cod", conexion, transaccion)
                                            cmdEx.Parameters.AddWithValue("@cod", fCod)
                                            Dim objEx = cmdEx.ExecuteScalar()
                                            If objEx IsNot Nothing AndAlso Not IsDBNull(objEx) Then
                                                exisActual = Convert.ToDouble(objEx)
                                            End If

                                            Dim nuevoSaldo As Double = exisActual - fCant

                                            Dim cmdSal As New SQLiteCommand("INSERT INTO Salidas (Fecha, Codigo, Generico, Distintivo, Presentacion, AWARE, Lote, Caducidad, Existencia, Surtido, Saldo, Movimiento, Folio, Cedula, Nombre, Direccion, Telefono) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16)", conexion, transaccion)
                                            cmdSal.Parameters.AddWithValue("@p0", fFecha)
                                            cmdSal.Parameters.AddWithValue("@p1", fCod)
                                            cmdSal.Parameters.AddWithValue("@p2", fGen)
                                            cmdSal.Parameters.AddWithValue("@p3", fDis)
                                            cmdSal.Parameters.AddWithValue("@p4", fPre)
                                            cmdSal.Parameters.AddWithValue("@p5", fAw)
                                            cmdSal.Parameters.AddWithValue("@p6", fLot)
                                            cmdSal.Parameters.AddWithValue("@p7", fCad)
                                            cmdSal.Parameters.AddWithValue("@p8", exisActual)
                                            cmdSal.Parameters.AddWithValue("@p9", fCant)
                                            cmdSal.Parameters.AddWithValue("@p10", nuevoSaldo)
                                            cmdSal.Parameters.AddWithValue("@p11", fMov)
                                            cmdSal.Parameters.AddWithValue("@p12", fFol)
                                            cmdSal.Parameters.AddWithValue("@p13", fCed)
                                            cmdSal.Parameters.AddWithValue("@p14", fNom)
                                            cmdSal.Parameters.AddWithValue("@p15", fDir)
                                            cmdSal.Parameters.AddWithValue("@p16", fTel)
                                            cmdSal.ExecuteNonQuery()

                                            Dim cmdUpdStock As New SQLiteCommand("UPDATE Inventario SET ExistenciaActual = @saldo WHERE Codigo = @cod", conexion, transaccion)
                                            cmdUpdStock.Parameters.AddWithValue("@saldo", nuevoSaldo)
                                            cmdUpdStock.Parameters.AddWithValue("@cod", fCod)
                                            cmdUpdStock.ExecuteNonQuery()

                                        Case "Inventario"
                                            Dim insertInv As String = "INSERT INTO Inventario (Codigo, Generico, Distintivo, Presentacion, AWARE, ExistenciaActual) VALUES (@p0, @p1, @p2, @p3, @p4, @p5) ON CONFLICT(Codigo) DO UPDATE SET Generico=@p1, Distintivo=@p2, Presentacion=@p3, AWARE=@p4, ExistenciaActual=@p5"
                                            Dim cmdInv As New SQLiteCommand(insertInv, conexion, transaccion)
                                            cmdInv.Parameters.AddWithValue("@p0", If(datos.Length > 0, datos(0).Trim(), ""))
                                            cmdInv.Parameters.AddWithValue("@p1", If(datos.Length > 1, datos(1).Trim(), ""))
                                            cmdInv.Parameters.AddWithValue("@p2", If(datos.Length > 2, datos(2).Trim(), ""))
                                            cmdInv.Parameters.AddWithValue("@p3", If(datos.Length > 3, datos(3).Trim(), ""))
                                            cmdInv.Parameters.AddWithValue("@p4", If(datos.Length > 4, datos(4).Trim().ToUpper(), ""))
                                            cmdInv.Parameters.AddWithValue("@p5", If(datos.Length > 5, Val(datos(5)), 0))
                                            cmdInv.ExecuteNonQuery()

                                        Case "Proveedores"
                                            Dim insertProv As String = "INSERT INTO Proveedores (Proveedor, RFC, Direccion) VALUES (@p0, @p1, @p2) ON CONFLICT(Proveedor) DO UPDATE SET RFC=@p1, Direccion=@p2"
                                            Dim cmdProv As New SQLiteCommand(insertProv, conexion, transaccion)
                                            cmdProv.Parameters.AddWithValue("@p0", If(datos.Length > 0, datos(0).Trim(), ""))
                                            cmdProv.Parameters.AddWithValue("@p1", If(datos.Length > 1, datos(1).Trim(), ""))
                                            cmdProv.Parameters.AddWithValue("@p2", If(datos.Length > 2, datos(2).Trim(), ""))
                                            cmdProv.ExecuteNonQuery()

                                        Case "Medicos"
                                            Dim insertMed As String = "INSERT INTO Medicos (Cedula, NombreMed, Calle, NoInt, NoExt, Colonia, Ciudad, Estado, CP, Pais, Tel) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10) ON CONFLICT(Cedula) DO UPDATE SET NombreMed=@p1, Tel=@p10"
                                            Dim cmdMed As New SQLiteCommand(insertMed, conexion, transaccion)
                                            For j As Integer = 0 To 10
                                                cmdMed.Parameters.AddWithValue("@p" & j, If(datos.Length > j, datos(j).Trim(), ""))
                                            Next
                                            cmdMed.ExecuteNonQuery()
                                    End Select

                                    registros += 1
                                End While

                                transaccion.Commit()
                            End Using
                        End Using

                        MessageBox.Show("¡Éxito! Se importaron " & registros & " registros en " & tablaDestino & ".", "Finalizado", MessageBoxButtons.OK, MessageBoxIcon.Information)

                        If pnlModuloTablas.Visible Then
                            If tablaDestino = "Entradas" Then
                                ConfigurarTablaEntradas()
                            ElseIf tablaDestino = "Salidas" Then
                                ConfigurarTablaSalidas()
                            ElseIf tablaDestino = "Inventario" Then
                                ConfigurarTablaInventario()
                            ElseIf tablaDestino = "Proveedores" Then
                                ConfigurarTablaProveedores()
                            ElseIf tablaDestino = "Medicos" Then
                                ConfigurarTablaMedicos()
                            End If
                        End If
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show("Error al leer el archivo CSV: " & ex.Message, "Error de Importación", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnNuevaEntrada_Click(sender As Object, e As EventArgs) Handles btnNuevaEntrada.Click
        Dim ventanaCaptura As New FormEntrada()
        ventanaCaptura.StartPosition = FormStartPosition.CenterScreen
        ventanaCaptura.ShowDialog(Me)
        If pnlModuloTablas.Visible AndAlso Not panelInicio.Visible Then
            ConfigurarTablaEntradas()
        End If
    End Sub

    Private Sub btnNuevaSalida_Click(sender As Object, e As EventArgs) Handles btnNuevaSalida.Click
        Dim ventanaSalida As New FormSalida()
        ventanaSalida.StartPosition = FormStartPosition.CenterScreen
        ventanaSalida.ShowDialog(Me)
        If pnlModuloTablas.Visible AndAlso Not panelInicio.Visible Then
            ConfigurarTablaSalidas()
        End If
    End Sub
    ' =========================================================
    ' 14. ESTILO VISUAL DE LA BARRA LATERAL (FLUENT DESIGN)
    ' =========================================================
    Private Sub AplicarEstiloFluent()
        Panel1.Dock = DockStyle.Left
        Panel1.BackColor = Drawing.Color.FromArgb(248, 250, 252)
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
                btn.BackColor = Drawing.Color.FromArgb(248, 250, 252)
                btn.ForeColor = Drawing.Color.FromArgb(71, 85, 105)
                btn.Font = New Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Regular)
                btn.TextAlign = Drawing.ContentAlignment.MiddleLeft
                btn.Padding = New Padding(14, 0, 0, 0)
                btn.Height = 42
                btn.Dock = DockStyle.Top
                btn.Margin = New Padding(0, 2, 0, 2)
                btn.FlatAppearance.MouseOverBackColor = Drawing.Color.FromArgb(241, 245, 249)
                btn.FlatAppearance.MouseDownBackColor = Drawing.Color.FromArgb(226, 232, 240)
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

    ' =========================================================
    ' 15. CONFIGURACIÓN DE TABLAS Y LECTURA SQLITE
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
        DataGridView1.Columns.Add("Direccion", "Dirección")

        Dim btnRevertir As New DataGridViewButtonColumn()
        btnRevertir.Name = "AccionRevertir"
        btnRevertir.HeaderText = "Acción"
        btnRevertir.Text = "✖ Revertir"
        btnRevertir.UseColumnTextForButtonValue = True
        btnRevertir.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnRevertir)

        AplicarEstiloTabla()
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Entradas ORDER BY Id DESC"
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

        ActualizarHeaderModulo("📥 Registro de Entradas (Facturas de Proveedor)")
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
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        DataGridView1.Columns("Codigo").FillWeight = 13
        DataGridView1.Columns("Generico").FillWeight = 27
        DataGridView1.Columns("Distintivo").FillWeight = 18
        DataGridView1.Columns("Presentacion").FillWeight = 18
        DataGridView1.Columns("AWARE").FillWeight = 12
        DataGridView1.Columns("ExistenciaActual").FillWeight = 12
        DataGridView1.Columns("AccionRevertir").FillWeight = 10
        DataGridView1.Columns("AccionRevertir").MinimumWidth = 95

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Inventario ORDER BY Generico ASC"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Codigo"), lector("Generico"), lector("Distintivo"),
                                                lector("Presentacion"), lector("AWARE"), lector("ExistenciaActual"))
                    End While
                End Using
            End Using
        End Using

        ActualizarHeaderModulo("📦 Catálogo e Inventario de Medicamentos")
    End Sub

    Private Sub ConfigurarTablaProveedores()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Proveedor", "Proveedor / Razón Social")
        DataGridView1.Columns.Add("RFC", "RFC")
        DataGridView1.Columns.Add("Direccion", "Dirección Fiscal")

        Dim btnBorrar As New DataGridViewButtonColumn()
        btnBorrar.Name = "AccionRevertir"
        btnBorrar.HeaderText = "Acción"
        btnBorrar.Text = "✖ Eliminar"
        btnBorrar.UseColumnTextForButtonValue = True
        btnBorrar.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnBorrar)

        AplicarEstiloTabla()
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        DataGridView1.Columns("Proveedor").FillWeight = 24
        DataGridView1.Columns("RFC").FillWeight = 16
        DataGridView1.Columns("Direccion").FillWeight = 50
        DataGridView1.Columns("AccionRevertir").FillWeight = 10
        DataGridView1.Columns("AccionRevertir").MinimumWidth = 100

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Proveedores ORDER BY Proveedor ASC"
            Using comando As New SQLiteCommand(consulta, conexion)
                Using lector As SQLiteDataReader = comando.ExecuteReader()
                    While lector.Read()
                        DataGridView1.Rows.Add(lector("Proveedor"), lector("RFC"), lector("Direccion"))
                    End While
                End Using
            End Using
        End Using

        ActualizarHeaderModulo("🚚 Directorio de Proveedores y Distribuidores")
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
        DataGridView1.Columns.Add("Cedula", "Cédula")
        DataGridView1.Columns.Add("Nombre", "Nombre")
        DataGridView1.Columns.Add("Direccion", "Dirección")
        DataGridView1.Columns.Add("Telefono", "Teléfono")

        Dim btnRevertir As New DataGridViewButtonColumn()
        btnRevertir.Name = "AccionRevertir"
        btnRevertir.HeaderText = "Acción"
        btnRevertir.Text = "✖ Revertir"
        btnRevertir.UseColumnTextForButtonValue = True
        btnRevertir.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnRevertir)

        AplicarEstiloTabla()
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Salidas ORDER BY Id DESC"
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

        ActualizarHeaderModulo("📤 Registro de Salidas y Recetas Dispensadas")
    End Sub

    Private Sub ConfigurarTablaMedicos()
        DataGridView1.Columns.Clear()
        DataGridView1.Rows.Clear()

        DataGridView1.Columns.Add("Cedula", "Cédula Profesional")
        DataGridView1.Columns.Add("NombreMed", "Nombre del Médico")
        DataGridView1.Columns.Add("Calle", "Calle")
        DataGridView1.Columns.Add("NoInt", "No. Int.")
        DataGridView1.Columns.Add("NoExt", "No. Ext.")
        DataGridView1.Columns.Add("Colonia", "Colonia")
        DataGridView1.Columns.Add("Ciudad", "Ciudad")
        DataGridView1.Columns.Add("Estado", "Estado")
        DataGridView1.Columns.Add("CP", "C.P.")
        DataGridView1.Columns.Add("Pais", "País")
        DataGridView1.Columns.Add("Tel", "Teléfono")

        Dim btnBorrar As New DataGridViewButtonColumn()
        btnBorrar.Name = "AccionRevertir"
        btnBorrar.HeaderText = "Acción"
        btnBorrar.Text = "✖ Eliminar"
        btnBorrar.UseColumnTextForButtonValue = True
        btnBorrar.FlatStyle = FlatStyle.Flat
        DataGridView1.Columns.Add(btnBorrar)

        AplicarEstiloTabla()
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        DataGridView1.Columns("Cedula").FillWeight = 11
        DataGridView1.Columns("NombreMed").FillWeight = 18
        DataGridView1.Columns("Calle").FillWeight = 14
        DataGridView1.Columns("NoInt").FillWeight = 6
        DataGridView1.Columns("NoExt").FillWeight = 6
        DataGridView1.Columns("Colonia").FillWeight = 10
        DataGridView1.Columns("Ciudad").FillWeight = 9
        DataGridView1.Columns("Estado").FillWeight = 8
        DataGridView1.Columns("CP").FillWeight = 6
        DataGridView1.Columns("Pais").FillWeight = 5
        DataGridView1.Columns("Tel").FillWeight = 8
        DataGridView1.Columns("AccionRevertir").FillWeight = 8
        DataGridView1.Columns("AccionRevertir").MinimumWidth = 95

        Using conexion As New SQLiteConnection(cadenaConexion)
            conexion.Open()
            Dim consulta As String = "SELECT * FROM Medicos ORDER BY NombreMed ASC"
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

        ActualizarHeaderModulo("🩺 Padrón de Médicos Prescriptores")
    End Sub

    ' =========================================================
    ' 16. EVENTOS DE ELIMINACIÓN Y REVERSIÓN (CELL CLICK)
    ' =========================================================
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex >= 0 AndAlso DataGridView1.Columns(e.ColumnIndex).Name = "AccionRevertir" Then

            If Not SesionActual.EsAdmin() Then
                MessageBox.Show("Acceso Restringido:" & vbCrLf & "Solo los usuarios con rol de Administrador pueden eliminar registros o revertir movimientos.",
                                "Permisos Insuficientes", MessageBoxButtons.OK, MessageBoxIcon.Stop)
                Return
            End If

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
                    Dim proveedor As String = DataGridView1.Rows(e.RowIndex).Cells("Proveedor / Razón Social").Value.ToString()
                    If MessageBox.Show("¿Eliminar a " & proveedor & "?", "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                        Dim cmd As New SQLiteCommand("DELETE FROM Proveedores WHERE Proveedor = @prov", conexion)
                        cmd.Parameters.AddWithValue("@prov", proveedor)
                        cmd.ExecuteNonQuery()
                        ConfigurarTablaProveedores()
                    End If

                ElseIf DataGridView1.Columns.Contains("NombreMed") Then
                    Dim cedula As String = DataGridView1.Rows(e.RowIndex).Cells("Cédula Profesional").Value.ToString()
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
