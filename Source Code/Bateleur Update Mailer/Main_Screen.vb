Imports System.ComponentModel
Imports System.Threading
Imports System.IO
Imports System.Net
Imports System.Web


Public Class Main_Screen

    Dim precount_max As Integer
    Dim cancel_operation As Boolean
    Dim currentcount As Integer
    Dim percentComplete As Integer

    Dim address As ArrayList
    Dim currentaddress As String
    Dim previousaddress As String

    Private Sub Error_Handler(ByVal ex As Exception, Optional ByVal identifier_msg As String = "")
        Try
            If My.Computer.FileSystem.FileExists((Application.StartupPath & "\Sounds\UHOH.WAV").Replace("\\", "\")) = True Then
                My.Computer.Audio.Play((Application.StartupPath & "\Sounds\UHOH.WAV").Replace("\\", "\"), AudioPlayMode.Background)
            End If
            Dim Display_Message1 As New Display_Message()
            Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ": " & ex.Message.ToString
            Display_Message1.Timer1.Interval = 1000
            Display_Message1.ShowDialog()
            If My.Computer.FileSystem.DirectoryExists((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs") = False Then
                My.Computer.FileSystem.CreateDirectory((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs")
            End If
            Dim filewriter As System.IO.StreamWriter = New System.IO.StreamWriter((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs\" & Format(Now(), "yyyyMMdd") & "_Error_Log.txt", True)
            filewriter.WriteLine("#" & Format(Now(), "dd/MM/yyyy hh:mm:ss tt") & " - " & identifier_msg & ": " & ex.ToString)
            filewriter.Flush()
            filewriter.Close()
            filewriter = Nothing
        Catch exc As Exception
            MsgBox("An error occurred in the application's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub

    Private Sub startAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles startAsyncButton.Click
        Try
            cancelAsyncButton.Enabled = True
            startAsyncButton.Enabled = False
            CourseList.Enabled = False
            MenuStrip1.Enabled = False
            currentaddress = ""
            previousaddress = ""
            address.Clear()
            Label4.Text = CourseList.Text
            ProgressBar1.Value = 0
            BackgroundWorker1.RunWorkerAsync(CourseList.Text)

        Catch ex As Exception
            Error_Handler(ex, "startAsyncButton_Click")
        End Try
    End Sub


    Private Sub cancelAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancelAsyncButton.Click
        Try
            cancel_operation = True
            Me.BackgroundWorker1.CancelAsync()
            cancelAsyncButton.Enabled = False
            startAsyncButton.Enabled = True
            CourseList.Enabled = True
            MenuStrip1.Enabled = True
        Catch ex As Exception
            Error_Handler(ex, "cancelAsyncButton_Click")
        End Try
    End Sub

    Private Sub backgroundWorker1_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try
            Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)
            precount_max = 0
            currentcount = 0
            percentComplete = 0
            cancel_operation = False
            If worker.CancellationPending Then
                e.Cancel = True
                cancel_operation = True
            End If

            previousaddress = ""
            currentaddress = ""

            If address.Count > 0 Then
                currentaddress = address.Item(0)
                previousaddress = ""
            End If
            If precount_max > 0 Then
                percentComplete = CSng(currentcount) / CSng(precount_max) * 100
            Else
                percentComplete = 100
            End If
            worker.ReportProgress(percentComplete)

            Precount(e.Argument.ToString)
            Dim tempstring As String = ""
            For Each element As String In address
                Try

                
                    If worker.CancellationPending Then
                        e.Cancel = True
                        cancel_operation = True
                        Exit For
                    End If

                    If precount_max > 0 Then
                        percentComplete = CSng(currentcount) / CSng(precount_max) * 100
                    Else
                        percentComplete = 100
                    End If
                    worker.ReportProgress(percentComplete)

                    previousaddress = currentaddress
                    currentaddress = element



                    Dim obj As System.Web.Mail.SmtpMail
                    Dim Mailmsg As New System.Web.Mail.MailMessage
                    obj.SmtpServer = "obe1.com.uct.ac.za"
                    Mailmsg.From = "\Commerce Webmaster\ <unattended-mailbox@obe1.com.uct.ac.za>"
                    Mailmsg.BodyFormat = Web.Mail.MailFormat.Html   'Send the mail in HTML Format
                    Mailmsg.Headers.Add("Reply-To", "unattended-mailbox@obe1.com.uct.ac.za")
                    Mailmsg.Subject = e.Argument.ToString.Replace("_G", "") & " Website Updated"
                    Mailmsg.Body = "<html><body><p>This email serves as a notification that the <b>" & e.Argument.ToString.Replace("_G", "") & "</b> course website has been updated on the Commerce webserver (<a href=""http://www.commerce.uct.ac.za"">http://www.commerce.uct.ac.za</a>) on " & Format(Now(), "dd/MM/yyyy") & ".</p>"
                    Mailmsg.Body = Mailmsg.Body & vbCrLf & "<p><font size=1 color=#808080>This update notification was generated by BATELEUR UPDATE MAILER (build " & System.String.Format(Label3.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, My.Application.Info.Version.Revision) & ")<br>Please don't reply to this email as it is sent via an unattended email account.</font></p></body></html>"
                    Mailmsg.To = element

                    obj.Send(Mailmsg)

                    Mailmsg = Nothing
                    obj = Nothing



                    currentcount = currentcount + 1
                    If precount_max > 0 Then
                        percentComplete = CSng(currentcount) / CSng(precount_max) * 100
                    Else
                        percentComplete = 100
                    End If
                    worker.ReportProgress(percentComplete)
                    If worker.CancellationPending Then
                        e.Cancel = True
                        cancel_operation = True
                        Exit For
                    End If
                Catch ex As Exception
                    Error_Handler(ex, "Sending Mail")
                    currentcount = currentcount + 1
                End Try
            Next

            e.Result = ""
        Catch ex As Exception
            Error_Handler(ex, "backgroundWorker1_DoWork")
        End Try
    End Sub

    Private Sub backgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        Try
            If Not (e.Error Is Nothing) Then
                Error_Handler(e.Error, "backgroundWorker1_RunWorkerCompleted")
            ElseIf e.Cancelled Then
                Me.ProgressBar1.Value = 0
                If My.Computer.FileSystem.FileExists((Application.StartupPath & "\Sounds\HEEY.WAV").Replace("\\", "\")) = True Then
                    If CheckBox1.Checked = False Then
                        My.Computer.Audio.Play((Application.StartupPath & "\Sounds\HEEY.WAV").Replace("\\", "\"), AudioPlayMode.Background)
                    End If
                End If
            Else
                Me.ProgressBar1.Value = 100
                If My.Computer.FileSystem.FileExists((Application.StartupPath & "\Sounds\VICTORY.WAV").Replace("\\", "\")) = True Then
                    If CheckBox1.Checked = False Then
                        My.Computer.Audio.Play((Application.StartupPath & "\Sounds\VICTORY.WAV").Replace("\\", "\"), AudioPlayMode.Background)
                    End If
                End If
            End If
            CourseList.Enabled = True
            MenuStrip1.Enabled = True
                cancelAsyncButton.Enabled = False
                startAsyncButton.Enabled = True
        Catch ex As Exception
            Error_Handler(ex, "backgroundWorker1_RunWorkerCompleted")
        End Try
    End Sub

    Private Sub backgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Try
            Me.Label4.Text = currentcount & "/" & precount_max
            If e.ProgressPercentage < 100 Then
                Me.ProgressBar1.Value = e.ProgressPercentage
            Else
                Me.ProgressBar1.Value = 100
            End If
            Me.Label2.Text = "Current: " & currentaddress & "  (Previous: " & previousaddress & ")"
        Catch ex As Exception
            Error_Handler(ex, "backgroundWorker1_ProgressChanged")
        End Try
    End Sub

    Private Sub Precount(ByVal course As String)
        Try
            address.Clear()
            My.Computer.Network.DownloadFile("http://www.commerce.uct.ac.za/Services/LDAP_Login/groupmembers_service.php?group=" & course, (Application.StartupPath & "\members.tmp").Replace("\\", "\"), "", "", False, 100000, True)
            If My.Computer.FileSystem.FileExists((Application.StartupPath & "\members.tmp").Replace("\\", "\")) = True Then
                Dim reader As StreamReader = My.Computer.FileSystem.OpenTextFileReader((Application.StartupPath & "\members.tmp").Replace("\\", "\"))
                Dim tempstring As String

                While reader.Peek <> -1
                    tempstring = reader.ReadLine
                    If tempstring.ToLower.StartsWith("cn=") Then
                        If tempstring.Split(",").Length > 0 Then
                            tempstring = tempstring.Split(",")(0).ToLower.Replace("cn=", "").Trim.ToUpper & "@uct.ac.za"
                            address.Add(tempstring)
                        End If

                    End If
                End While
                reader.Close()
                reader.Dispose()
                reader = Nothing
                precount_max = address.Count
                My.Computer.FileSystem.DeleteFile((Application.StartupPath & "\members.tmp").Replace("\\", "\"), FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
            End If
        Catch ex As Exception
            Error_Handler(ex, "Precount")
        End Try
    End Sub

 

    Private Sub LoadSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")
            If My.Computer.FileSystem.FileExists(configfile) Then
                Dim reader As StreamReader = New StreamReader(configfile)
                Dim lineread As String
                Dim variablevalue As String
                While reader.Peek <> -1
                    lineread = reader.ReadLine
                    If lineread.IndexOf("=") <> -1 Then

                        variablevalue = lineread.Remove(0, lineread.IndexOf("=") + 1)

                        If lineread.StartsWith("CheckBox1=") Then
                            CheckBox1.Checked = variablevalue
                        End If



                    End If
                End While
                reader.Close()
                reader = Nothing
            End If
        Catch ex As Exception
            Error_Handler(ex, "Load Settings")
        End Try
    End Sub

    Private Sub SaveSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")

            Dim writer As StreamWriter = New StreamWriter(configfile, False)

            

            writer.WriteLine("CheckBox1=" & CheckBox1.Checked)

            writer.Flush()
            writer.Close()
            writer = Nothing

        Catch ex As Exception
            Error_Handler(ex, "Save Settings")
        End Try
    End Sub


    Private Sub Main_Screen_Closed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            address.Clear()
            address = Nothing
            SaveSettings()
        Catch ex As Exception
            Error_Handler(ex, "Closed")
        End Try
    End Sub



    Private Sub Main_Screen_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            loadsettings()
            Label3.Text = System.String.Format(Label3.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, My.Application.Info.Version.Revision)
            address = New ArrayList
            address.Clear()
            If My.Computer.Keyboard.ShiftKeyDown = False Then


                My.Computer.Network.DownloadFile("http://www.commerce.uct.ac.za/Services/LDAP_Login/availablegroups_service.php?container=ou=fsf-grp-comlab,ou=com,ou=main,o=uct&group=*", (Application.StartupPath & "\members.tmp").Replace("\\", "\"), "", "", False, 100000, True)
                If My.Computer.FileSystem.FileExists((Application.StartupPath & "\members.tmp").Replace("\\", "\")) = True Then
                    Dim reader As StreamReader = My.Computer.FileSystem.OpenTextFileReader((Application.StartupPath & "\members.tmp").Replace("\\", "\"))
                    Dim tempstring As String
                    Dim members As ArrayList = New ArrayList
                    While reader.Peek <> -1
                        If My.Computer.Keyboard.ShiftKeyDown = True Then
                            Exit While
                        End If
                        tempstring = reader.ReadLine
                        If tempstring.ToLower.StartsWith("cn=") Then
                            If tempstring.Split(",").Length > 0 Then
                                tempstring = tempstring.Split(",")(0).ToLower.Replace("cn=", "").ToUpper.Trim
                                members.Add(tempstring)
                            End If

                        End If
                    End While
                    reader.Close()
                    reader.Dispose()
                    reader = Nothing

                    CourseList.Items.Clear()
                    If My.Computer.Keyboard.ShiftKeyDown = False Then
                        CourseList.Items.AddRange(members.ToArray)
                    End If
                    members.Clear()
                    members = Nothing
                    My.Computer.FileSystem.DeleteFile((Application.StartupPath & "\members.tmp").Replace("\\", "\"), FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.DeletePermanently)
                End If
            End If
        Catch ex As Exception
            Error_Handler(ex, "Load")
        End Try
    End Sub

    Private Sub HelpToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpToolStripMenuItem.Click
        Try
            HelpBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display Help Screen")
        End Try
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Try
            AboutBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display About Screen")
        End Try
    End Sub
End Class
