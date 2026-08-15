<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormSalida
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Label11 = New Label()
        txtCaducidad = New TextBox()
        Label10 = New Label()
        Label9 = New Label()
        txtAware = New TextBox()
        Label8 = New Label()
        txtLote = New TextBox()
        txtPresentacion = New TextBox()
        Label7 = New Label()
        txtSurtido = New TextBox()
        txtExistencia = New TextBox()
        txtDistintivo = New TextBox()
        txtGenerico = New TextBox()
        txtCodigo = New TextBox()
        Label6 = New Label()
        Label5 = New Label()
        Label4 = New Label()
        Label3 = New Label()
        Label2 = New Label()
        Label1 = New Label()
        Label12 = New Label()
        cmbMovimiento = New ComboBox()
        Label13 = New Label()
        Label14 = New Label()
        cmbCedula = New ComboBox()
        cmbNombreMed = New ComboBox()
        Label15 = New Label()
        txtCalle = New TextBox()
        Label16 = New Label()
        Label17 = New Label()
        txtNoInt = New TextBox()
        txtNoExt = New TextBox()
        Label18 = New Label()
        txtColonia = New TextBox()
        Label19 = New Label()
        Label20 = New Label()
        txtCiudad = New TextBox()
        Label21 = New Label()
        Label22 = New Label()
        txtEstado = New TextBox()
        txtPais = New TextBox()
        btnGuardarSalida = New Button()
        Label23 = New Label()
        txtFolio = New TextBox()
        txtTel = New TextBox()
        Label24 = New Label()
        txtCP = New TextBox()
        SuspendLayout()
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.Location = New Point(30, 258)
        Label11.Name = "Label11"
        Label11.Size = New Size(129, 15)
        Label11.TabIndex = 50
        Label11.Text = "III. DATOS DEL MEDICO"
        ' 
        ' txtCaducidad
        ' 
        txtCaducidad.Location = New Point(428, 164)
        txtCaducidad.Name = "txtCaducidad"
        txtCaducidad.Size = New Size(177, 23)
        txtCaducidad.TabIndex = 49
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Location = New Point(326, 167)
        Label10.Name = "Label10"
        Label10.Size = New Size(64, 15)
        Label10.TabIndex = 48
        Label10.Text = "Caducidad"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Location = New Point(326, 136)
        Label9.Name = "Label9"
        Label9.Size = New Size(30, 15)
        Label9.TabIndex = 47
        Label9.Text = "Lote"
        ' 
        ' txtAware
        ' 
        txtAware.Location = New Point(565, 75)
        txtAware.Name = "txtAware"
        txtAware.Size = New Size(177, 23)
        txtAware.TabIndex = 46
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(417, 78)
        Label8.Name = "Label8"
        Label8.Size = New Size(132, 15)
        Label8.TabIndex = 45
        Label8.Text = "Clasificación de AWARE"
        ' 
        ' txtLote
        ' 
        txtLote.Location = New Point(428, 133)
        txtLote.Name = "txtLote"
        txtLote.Size = New Size(177, 23)
        txtLote.TabIndex = 44
        ' 
        ' txtPresentacion
        ' 
        txtPresentacion.Location = New Point(565, 44)
        txtPresentacion.Name = "txtPresentacion"
        txtPresentacion.Size = New Size(177, 23)
        txtPresentacion.TabIndex = 43
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(417, 47)
        Label7.Name = "Label7"
        Label7.Size = New Size(75, 15)
        Label7.TabIndex = 42
        Label7.Text = "Presentación"
        ' 
        ' txtSurtido
        ' 
        txtSurtido.Location = New Point(187, 164)
        txtSurtido.Name = "txtSurtido"
        txtSurtido.Size = New Size(92, 23)
        txtSurtido.TabIndex = 41
        ' 
        ' txtExistencia
        ' 
        txtExistencia.Location = New Point(187, 133)
        txtExistencia.Name = "txtExistencia"
        txtExistencia.Size = New Size(92, 23)
        txtExistencia.TabIndex = 40
        ' 
        ' txtDistintivo
        ' 
        txtDistintivo.Location = New Point(187, 104)
        txtDistintivo.Name = "txtDistintivo"
        txtDistintivo.Size = New Size(177, 23)
        txtDistintivo.TabIndex = 39
        ' 
        ' txtGenerico
        ' 
        txtGenerico.Location = New Point(187, 75)
        txtGenerico.Name = "txtGenerico"
        txtGenerico.Size = New Size(177, 23)
        txtGenerico.TabIndex = 38
        ' 
        ' txtCodigo
        ' 
        txtCodigo.Location = New Point(187, 44)
        txtCodigo.Name = "txtCodigo"
        txtCodigo.Size = New Size(177, 23)
        txtCodigo.TabIndex = 37
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(30, 167)
        Label6.Name = "Label6"
        Label6.Size = New Size(84, 15)
        Label6.TabIndex = 36
        Label6.Text = "Piezas Surtidas"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(30, 136)
        Label5.Name = "Label5"
        Label5.Size = New Size(58, 15)
        Label5.TabIndex = 35
        Label5.Text = "Existencia"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(30, 107)
        Label4.Name = "Label4"
        Label4.Size = New Size(137, 15)
        Label4.TabIndex = 34
        Label4.Text = "Denominación Distintiva"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(30, 78)
        Label3.Name = "Label3"
        Label3.Size = New Size(134, 15)
        Label3.TabIndex = 33
        Label3.Text = "Denominación Genérica"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(30, 47)
        Label2.Name = "Label2"
        Label2.Size = New Size(46, 15)
        Label2.TabIndex = 32
        Label2.Text = "Código"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(30, 16)
        Label1.Name = "Label1"
        Label1.Size = New Size(163, 15)
        Label1.TabIndex = 31
        Label1.Text = "I. DATOS DEL MEDICAMENTO"
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.Location = New Point(30, 213)
        Label12.Name = "Label12"
        Label12.Size = New Size(145, 15)
        Label12.TabIndex = 51
        Label12.Text = "II. TIPOS DE MOVIMEINTO"
        ' 
        ' cmbMovimiento
        ' 
        cmbMovimiento.FormattingEnabled = True
        cmbMovimiento.Location = New Point(187, 210)
        cmbMovimiento.Name = "cmbMovimiento"
        cmbMovimiento.Size = New Size(177, 23)
        cmbMovimiento.TabIndex = 52
        ' 
        ' Label13
        ' 
        Label13.AutoSize = True
        Label13.Location = New Point(30, 300)
        Label13.Name = "Label13"
        Label13.Size = New Size(44, 15)
        Label13.TabIndex = 53
        Label13.Text = "Cédula"
        ' 
        ' Label14
        ' 
        Label14.AutoSize = True
        Label14.Location = New Point(30, 329)
        Label14.Name = "Label14"
        Label14.Size = New Size(51, 15)
        Label14.TabIndex = 55
        Label14.Text = "Nombre"
        ' 
        ' cmbCedula
        ' 
        cmbCedula.FormattingEnabled = True
        cmbCedula.Location = New Point(187, 297)
        cmbCedula.Name = "cmbCedula"
        cmbCedula.Size = New Size(282, 23)
        cmbCedula.TabIndex = 56
        ' 
        ' cmbNombreMed
        ' 
        cmbNombreMed.FormattingEnabled = True
        cmbNombreMed.Location = New Point(187, 326)
        cmbNombreMed.Name = "cmbNombreMed"
        cmbNombreMed.Size = New Size(282, 23)
        cmbNombreMed.TabIndex = 57
        ' 
        ' Label15
        ' 
        Label15.AutoSize = True
        Label15.Location = New Point(30, 358)
        Label15.Name = "Label15"
        Label15.Size = New Size(33, 15)
        Label15.TabIndex = 59
        Label15.Text = "Calle"
        ' 
        ' txtCalle
        ' 
        txtCalle.Location = New Point(187, 358)
        txtCalle.Name = "txtCalle"
        txtCalle.Size = New Size(282, 23)
        txtCalle.TabIndex = 60
        ' 
        ' Label16
        ' 
        Label16.AutoSize = True
        Label16.Location = New Point(30, 389)
        Label16.Name = "Label16"
        Label16.Size = New Size(53, 15)
        Label16.TabIndex = 61
        Label16.Text = "Teléfono"
        ' 
        ' Label17
        ' 
        Label17.AutoSize = True
        Label17.Location = New Point(532, 305)
        Label17.Name = "Label17"
        Label17.Size = New Size(67, 15)
        Label17.TabIndex = 63
        Label17.Text = "No. Interior"
        ' 
        ' txtNoInt
        ' 
        txtNoInt.Location = New Point(623, 302)
        txtNoInt.Name = "txtNoInt"
        txtNoInt.Size = New Size(192, 23)
        txtNoInt.TabIndex = 64
        ' 
        ' txtNoExt
        ' 
        txtNoExt.Location = New Point(623, 331)
        txtNoExt.Name = "txtNoExt"
        txtNoExt.Size = New Size(192, 23)
        txtNoExt.TabIndex = 65
        ' 
        ' Label18
        ' 
        Label18.AutoSize = True
        Label18.Location = New Point(532, 334)
        Label18.Name = "Label18"
        Label18.Size = New Size(68, 15)
        Label18.TabIndex = 66
        Label18.Text = "No. Exterior"
        ' 
        ' txtColonia
        ' 
        txtColonia.Location = New Point(187, 418)
        txtColonia.Name = "txtColonia"
        txtColonia.Size = New Size(177, 23)
        txtColonia.TabIndex = 67
        ' 
        ' Label19
        ' 
        Label19.AutoSize = True
        Label19.Location = New Point(30, 421)
        Label19.Name = "Label19"
        Label19.Size = New Size(48, 15)
        Label19.TabIndex = 68
        Label19.Text = "Colonia"
        ' 
        ' Label20
        ' 
        Label20.AutoSize = True
        Label20.Location = New Point(30, 450)
        Label20.Name = "Label20"
        Label20.Size = New Size(45, 15)
        Label20.TabIndex = 69
        Label20.Text = "Ciudad"
        ' 
        ' txtCiudad
        ' 
        txtCiudad.Location = New Point(187, 447)
        txtCiudad.Name = "txtCiudad"
        txtCiudad.Size = New Size(177, 23)
        txtCiudad.TabIndex = 70
        ' 
        ' Label21
        ' 
        Label21.AutoSize = True
        Label21.Location = New Point(532, 421)
        Label21.Name = "Label21"
        Label21.Size = New Size(42, 15)
        Label21.TabIndex = 71
        Label21.Text = "Estado"
        ' 
        ' Label22
        ' 
        Label22.AutoSize = True
        Label22.Location = New Point(532, 450)
        Label22.Name = "Label22"
        Label22.Size = New Size(28, 15)
        Label22.TabIndex = 72
        Label22.Text = "Pais"
        ' 
        ' txtEstado
        ' 
        txtEstado.Location = New Point(623, 418)
        txtEstado.Name = "txtEstado"
        txtEstado.Size = New Size(192, 23)
        txtEstado.TabIndex = 73
        ' 
        ' txtPais
        ' 
        txtPais.Location = New Point(623, 447)
        txtPais.Name = "txtPais"
        txtPais.Size = New Size(192, 23)
        txtPais.TabIndex = 74
        ' 
        ' btnGuardarSalida
        ' 
        btnGuardarSalida.Location = New Point(766, 509)
        btnGuardarSalida.Name = "btnGuardarSalida"
        btnGuardarSalida.Size = New Size(75, 23)
        btnGuardarSalida.TabIndex = 75
        btnGuardarSalida.Text = "Guardar"
        btnGuardarSalida.UseVisualStyleBackColor = True
        ' 
        ' Label23
        ' 
        Label23.AutoSize = True
        Label23.Location = New Point(462, 218)
        Label23.Name = "Label23"
        Label23.Size = New Size(71, 15)
        Label23.TabIndex = 76
        Label23.Text = "Folio Receta"
        ' 
        ' txtFolio
        ' 
        txtFolio.Location = New Point(565, 215)
        txtFolio.Name = "txtFolio"
        txtFolio.Size = New Size(177, 23)
        txtFolio.TabIndex = 77
        ' 
        ' txtTel
        ' 
        txtTel.Location = New Point(187, 389)
        txtTel.Name = "txtTel"
        txtTel.Size = New Size(177, 23)
        txtTel.TabIndex = 62
        ' 
        ' Label24
        ' 
        Label24.AutoSize = True
        Label24.Location = New Point(532, 389)
        Label24.Name = "Label24"
        Label24.Size = New Size(81, 15)
        Label24.TabIndex = 78
        Label24.Text = "Codigo Postal"
        ' 
        ' txtCP
        ' 
        txtCP.Location = New Point(623, 386)
        txtCP.Name = "txtCP"
        txtCP.Size = New Size(192, 23)
        txtCP.TabIndex = 79
        ' 
        ' FormSalida
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(896, 555)
        Controls.Add(txtCP)
        Controls.Add(Label24)
        Controls.Add(txtFolio)
        Controls.Add(Label23)
        Controls.Add(btnGuardarSalida)
        Controls.Add(txtPais)
        Controls.Add(txtEstado)
        Controls.Add(Label22)
        Controls.Add(Label21)
        Controls.Add(txtCiudad)
        Controls.Add(Label20)
        Controls.Add(Label19)
        Controls.Add(txtColonia)
        Controls.Add(Label18)
        Controls.Add(txtNoExt)
        Controls.Add(txtNoInt)
        Controls.Add(Label17)
        Controls.Add(txtTel)
        Controls.Add(Label16)
        Controls.Add(txtCalle)
        Controls.Add(Label15)
        Controls.Add(cmbNombreMed)
        Controls.Add(cmbCedula)
        Controls.Add(Label14)
        Controls.Add(Label13)
        Controls.Add(cmbMovimiento)
        Controls.Add(Label12)
        Controls.Add(Label11)
        Controls.Add(txtCaducidad)
        Controls.Add(Label10)
        Controls.Add(Label9)
        Controls.Add(txtAware)
        Controls.Add(Label8)
        Controls.Add(txtLote)
        Controls.Add(txtPresentacion)
        Controls.Add(Label7)
        Controls.Add(txtSurtido)
        Controls.Add(txtExistencia)
        Controls.Add(txtDistintivo)
        Controls.Add(txtGenerico)
        Controls.Add(txtCodigo)
        Controls.Add(Label6)
        Controls.Add(Label5)
        Controls.Add(Label4)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Name = "FormSalida"
        Text = "Form2"
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents Label11 As Label
    Friend WithEvents txtCaducidad As TextBox
    Friend WithEvents Label10 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents txtAware As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents txtLote As TextBox
    Friend WithEvents txtPresentacion As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents txtSurtido As TextBox
    Friend WithEvents txtExistencia As TextBox
    Friend WithEvents txtDistintivo As TextBox
    Friend WithEvents txtGenerico As TextBox
    Friend WithEvents txtCodigo As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents cmbMovimiento As ComboBox
    Friend WithEvents Label13 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents cmbCedula As ComboBox
    Friend WithEvents cmbNombreMed As ComboBox
    Friend WithEvents Label15 As Label
    Friend WithEvents txtCalle As TextBox
    Friend WithEvents Label16 As Label
    Friend WithEvents Label17 As Label
    Friend WithEvents txtNoInt As TextBox
    Friend WithEvents txtNoExt As TextBox
    Friend WithEvents Label18 As Label
    Friend WithEvents txtColonia As TextBox
    Friend WithEvents Label19 As Label
    Friend WithEvents Label20 As Label
    Friend WithEvents txtCiudad As TextBox
    Friend WithEvents Label21 As Label
    Friend WithEvents Label22 As Label
    Friend WithEvents txtEstado As TextBox
    Friend WithEvents txtPais As TextBox
    Friend WithEvents btnGuardarSalida As Button
    Friend WithEvents Label23 As Label
    Friend WithEvents txtFolio As TextBox
    Friend WithEvents txtTel As TextBox
    Friend WithEvents Label24 As Label
    Friend WithEvents txtCP As TextBox
End Class
