<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Button1 = New Button()
        Button2 = New Button()
        Button3 = New Button()
        Button4 = New Button()
        Button5 = New Button()
        DataGridView1 = New DataGridView()
        Panel1 = New Panel()
        Button9 = New Button()
        Button8 = New Button()
        Button7 = New Button()
        Button6 = New Button()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        Panel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(7, 5)
        Button1.Name = "Button1"
        Button1.Size = New Size(124, 49)
        Button1.TabIndex = 0
        Button1.Text = "Inicio"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(7, 60)
        Button2.Name = "Button2"
        Button2.Size = New Size(124, 49)
        Button2.TabIndex = 1
        Button2.Text = "Entradas"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(7, 115)
        Button3.Name = "Button3"
        Button3.Size = New Size(124, 49)
        Button3.TabIndex = 2
        Button3.Text = "Salidas"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' Button4
        ' 
        Button4.Location = New Point(7, 170)
        Button4.Name = "Button4"
        Button4.Size = New Size(124, 49)
        Button4.TabIndex = 3
        Button4.Text = "Medicos"
        Button4.UseVisualStyleBackColor = True
        ' 
        ' Button5
        ' 
        Button5.Location = New Point(7, 225)
        Button5.Name = "Button5"
        Button5.Size = New Size(124, 49)
        Button5.TabIndex = 4
        Button5.Text = "Proveedores"
        Button5.UseVisualStyleBackColor = True
        ' 
        ' DataGridView1
        ' 
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Location = New Point(183, 44)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(1264, 838)
        DataGridView1.TabIndex = 5
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(Button9)
        Panel1.Controls.Add(Button8)
        Panel1.Controls.Add(Button7)
        Panel1.Controls.Add(Button6)
        Panel1.Controls.Add(Button5)
        Panel1.Controls.Add(Button4)
        Panel1.Controls.Add(Button3)
        Panel1.Controls.Add(Button2)
        Panel1.Controls.Add(Button1)
        Panel1.Location = New Point(5, 44)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(180, 827)
        Panel1.TabIndex = 6
        ' 
        ' Button9
        ' 
        Button9.Location = New Point(7, 445)
        Button9.Name = "Button9"
        Button9.Size = New Size(124, 49)
        Button9.TabIndex = 8
        Button9.Text = "AWARE: Estadisticas"
        Button9.UseVisualStyleBackColor = True
        ' 
        ' Button8
        ' 
        Button8.Location = New Point(7, 390)
        Button8.Name = "Button8"
        Button8.Size = New Size(124, 49)
        Button8.TabIndex = 7
        Button8.Text = "Impresion"
        Button8.UseVisualStyleBackColor = True
        ' 
        ' Button7
        ' 
        Button7.Location = New Point(7, 335)
        Button7.Name = "Button7"
        Button7.Size = New Size(124, 49)
        Button7.TabIndex = 6
        Button7.Text = "Configuración"
        Button7.UseVisualStyleBackColor = True
        ' 
        ' Button6
        ' 
        Button6.Location = New Point(7, 280)
        Button6.Name = "Button6"
        Button6.Size = New Size(124, 49)
        Button6.TabIndex = 5
        Button6.Text = "Inventario"
        Button6.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1459, 894)
        Controls.Add(Panel1)
        Controls.Add(DataGridView1)
        Name = "Form1"
        Text = "Form1"
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        Panel1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Button6 As Button
    Friend WithEvents Button7 As Button
    Friend WithEvents Button8 As Button
    Friend WithEvents Button9 As Button
End Class
