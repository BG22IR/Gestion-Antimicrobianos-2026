<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormEntrada
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
        Label1 = New Label()
        Label2 = New Label()
        Label3 = New Label()
        Label4 = New Label()
        Label5 = New Label()
        Label6 = New Label()
        txtCodigo = New TextBox()
        txtGenerico = New TextBox()
        txtDistintivo = New TextBox()
        txtExistencia = New TextBox()
        txtSurtido = New TextBox()
        Label7 = New Label()
        txtPresentacion = New TextBox()
        txtLote = New TextBox()
        Label8 = New Label()
        txtAware = New TextBox()
        Label9 = New Label()
        Label10 = New Label()
        txtCaducidad = New TextBox()
        Label11 = New Label()
        Label12 = New Label()
        Label13 = New Label()
        Label14 = New Label()
        txtRFC = New TextBox()
        btnNuevoProv = New Button()
        txtDireccion = New TextBox()
        cmbProveedor = New ComboBox()
        btnGuardar = New Button()
        Label15 = New Label()
        txtFactura = New TextBox()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(40, 63)
        Label1.Name = "Label1"
        Label1.Size = New Size(163, 15)
        Label1.TabIndex = 0
        Label1.Text = "I. DATOS DEL MEDICAMENTO"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(40, 94)
        Label2.Name = "Label2"
        Label2.Size = New Size(46, 15)
        Label2.TabIndex = 1
        Label2.Text = "Código"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(40, 125)
        Label3.Name = "Label3"
        Label3.Size = New Size(134, 15)
        Label3.TabIndex = 2
        Label3.Text = "Denominación Genérica"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(40, 154)
        Label4.Name = "Label4"
        Label4.Size = New Size(137, 15)
        Label4.TabIndex = 3
        Label4.Text = "Denominación Distintiva"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(40, 183)
        Label5.Name = "Label5"
        Label5.Size = New Size(58, 15)
        Label5.TabIndex = 4
        Label5.Text = "Existencia"
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Location = New Point(40, 214)
        Label6.Name = "Label6"
        Label6.Size = New Size(84, 15)
        Label6.TabIndex = 5
        Label6.Text = "Piezas Surtidas"
        ' 
        ' txtCodigo
        ' 
        txtCodigo.Location = New Point(197, 91)
        txtCodigo.Name = "txtCodigo"
        txtCodigo.Size = New Size(177, 23)
        txtCodigo.TabIndex = 6
        ' 
        ' txtGenerico
        ' 
        txtGenerico.Location = New Point(197, 122)
        txtGenerico.Name = "txtGenerico"
        txtGenerico.Size = New Size(177, 23)
        txtGenerico.TabIndex = 7
        ' 
        ' txtDistintivo
        ' 
        txtDistintivo.Location = New Point(197, 151)
        txtDistintivo.Name = "txtDistintivo"
        txtDistintivo.Size = New Size(177, 23)
        txtDistintivo.TabIndex = 8
        ' 
        ' txtExistencia
        ' 
        txtExistencia.Location = New Point(197, 180)
        txtExistencia.Name = "txtExistencia"
        txtExistencia.Size = New Size(92, 23)
        txtExistencia.TabIndex = 9
        ' 
        ' txtSurtido
        ' 
        txtSurtido.Location = New Point(197, 211)
        txtSurtido.Name = "txtSurtido"
        txtSurtido.Size = New Size(92, 23)
        txtSurtido.TabIndex = 10
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Location = New Point(404, 94)
        Label7.Name = "Label7"
        Label7.Size = New Size(75, 15)
        Label7.TabIndex = 11
        Label7.Text = "Presentación"
        ' 
        ' txtPresentacion
        ' 
        txtPresentacion.Location = New Point(542, 91)
        txtPresentacion.Name = "txtPresentacion"
        txtPresentacion.Size = New Size(177, 23)
        txtPresentacion.TabIndex = 12
        ' 
        ' txtLote
        ' 
        txtLote.Location = New Point(438, 180)
        txtLote.Name = "txtLote"
        txtLote.Size = New Size(177, 23)
        txtLote.TabIndex = 13
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(404, 125)
        Label8.Name = "Label8"
        Label8.Size = New Size(132, 15)
        Label8.TabIndex = 14
        Label8.Text = "Clasificación de AWARE"
        ' 
        ' txtAware
        ' 
        txtAware.Location = New Point(542, 122)
        txtAware.Name = "txtAware"
        txtAware.Size = New Size(177, 23)
        txtAware.TabIndex = 15
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Location = New Point(336, 183)
        Label9.Name = "Label9"
        Label9.Size = New Size(30, 15)
        Label9.TabIndex = 16
        Label9.Text = "Lote"
        ' 
        ' Label10
        ' 
        Label10.AutoSize = True
        Label10.Location = New Point(336, 214)
        Label10.Name = "Label10"
        Label10.Size = New Size(64, 15)
        Label10.TabIndex = 17
        Label10.Text = "Caducidad"
        ' 
        ' txtCaducidad
        ' 
        txtCaducidad.Location = New Point(438, 211)
        txtCaducidad.Name = "txtCaducidad"
        txtCaducidad.Size = New Size(177, 23)
        txtCaducidad.TabIndex = 18
        ' 
        ' Label11
        ' 
        Label11.AutoSize = True
        Label11.Location = New Point(40, 272)
        Label11.Name = "Label11"
        Label11.Size = New Size(147, 15)
        Label11.TabIndex = 19
        Label11.Text = "II. DATOS DEL PROVEEDOR"
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.Location = New Point(40, 301)
        Label12.Name = "Label12"
        Label12.Size = New Size(127, 15)
        Label12.TabIndex = 20
        Label12.Text = "Nombre del Proveedor"
        ' 
        ' Label13
        ' 
        Label13.AutoSize = True
        Label13.Location = New Point(404, 301)
        Label13.Name = "Label13"
        Label13.Size = New Size(28, 15)
        Label13.TabIndex = 21
        Label13.Text = "RFC"
        ' 
        ' Label14
        ' 
        Label14.AutoSize = True
        Label14.Location = New Point(40, 415)
        Label14.Name = "Label14"
        Label14.Size = New Size(60, 15)
        Label14.TabIndex = 22
        Label14.Text = "Dirección:"
        ' 
        ' txtRFC
        ' 
        txtRFC.Location = New Point(542, 301)
        txtRFC.Name = "txtRFC"
        txtRFC.Size = New Size(177, 23)
        txtRFC.TabIndex = 24
        ' 
        ' btnNuevoProv
        ' 
        btnNuevoProv.Location = New Point(220, 327)
        btnNuevoProv.Name = "btnNuevoProv"
        btnNuevoProv.Size = New Size(132, 25)
        btnNuevoProv.TabIndex = 25
        btnNuevoProv.Text = "Nuevo Proveedor"
        btnNuevoProv.UseVisualStyleBackColor = True
        ' 
        ' txtDireccion
        ' 
        txtDireccion.Location = New Point(197, 412)
        txtDireccion.Name = "txtDireccion"
        txtDireccion.Size = New Size(522, 23)
        txtDireccion.TabIndex = 26
        ' 
        ' cmbProveedor
        ' 
        cmbProveedor.FormattingEnabled = True
        cmbProveedor.Location = New Point(197, 298)
        cmbProveedor.Name = "cmbProveedor"
        cmbProveedor.Size = New Size(177, 23)
        cmbProveedor.TabIndex = 27
        ' 
        ' btnGuardar
        ' 
        btnGuardar.Location = New Point(706, 459)
        btnGuardar.Name = "btnGuardar"
        btnGuardar.Size = New Size(75, 23)
        btnGuardar.TabIndex = 28
        btnGuardar.Text = "Guardar"
        btnGuardar.UseVisualStyleBackColor = True
        ' 
        ' Label15
        ' 
        Label15.AutoSize = True
        Label15.Location = New Point(40, 369)
        Label15.Name = "Label15"
        Label15.Size = New Size(46, 15)
        Label15.TabIndex = 29
        Label15.Text = "Factura"
        ' 
        ' txtFactura
        ' 
        txtFactura.Location = New Point(197, 366)
        txtFactura.Name = "txtFactura"
        txtFactura.Size = New Size(177, 23)
        txtFactura.TabIndex = 30
        ' 
        ' FormEntrada
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(813, 504)
        Controls.Add(txtFactura)
        Controls.Add(Label15)
        Controls.Add(btnGuardar)
        Controls.Add(cmbProveedor)
        Controls.Add(txtDireccion)
        Controls.Add(btnNuevoProv)
        Controls.Add(txtRFC)
        Controls.Add(Label14)
        Controls.Add(Label13)
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
        Name = "FormEntrada"
        Text = "Form2"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents txtCodigo As TextBox
    Friend WithEvents txtGenerico As TextBox
    Friend WithEvents txtDistintivo As TextBox
    Friend WithEvents txtExistencia As TextBox
    Friend WithEvents txtSurtido As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents txtPresentacion As TextBox
    Friend WithEvents txtLote As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents txtAware As TextBox
    Friend WithEvents Label9 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents txtCaducidad As TextBox
    Friend WithEvents Label11 As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents Label13 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents txtRFC As TextBox
    Friend WithEvents btnNuevoProv As Button
    Friend WithEvents txtDireccion As TextBox
    Friend WithEvents cmbProveedor As ComboBox
    Friend WithEvents btnGuardar As Button
    Friend WithEvents Label15 As Label
    Friend WithEvents txtFactura As TextBox
End Class
