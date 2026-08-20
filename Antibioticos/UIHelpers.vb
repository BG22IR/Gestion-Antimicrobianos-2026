Module UIHelpers
    Public Sub AplicarEstiloFluentAjustado(frm As Form)
        ' Aplica estilos de tipo Fluent de forma segura comprobando existencia de controles por nombre
        Dim panel1 As Panel = TryCast(frm.Controls("Panel1"), Panel)
        If panel1 IsNot Nothing Then
            panel1.Dock = DockStyle.Left
            panel1.BackColor = Drawing.Color.FromArgb(248, 250, 252)
            panel1.Padding = New Padding(12, 12, 8, 12)

            ' Ajusta todos los botones dentro del panel si existen
            For Each control As Control In panel1.Controls
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

            ' Intenta traer al frente el contenedor principal si existe
            Dim pnlContenedor As Panel = TryCast(frm.Controls("pnlContenedorVistas"), Panel)
            If pnlContenedor IsNot Nothing Then
                panel1.SendToBack()
                pnlContenedor.BringToFront()
            End If
        End If

        ' Color de fondo genérico para el formulario
        frm.BackColor = Drawing.Color.White
    End Sub
End Module
