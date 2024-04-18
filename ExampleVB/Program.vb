Imports System
Imports System.Globalization
Imports TOCSharp
Imports TOCSharp.Commands
Imports TOCSharp.Models

Module Program
    Dim WithEvents Client As TOCClient = New TOCClient("SCREENNAME", "PASSWORD")

    Sub Main()
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
        StartBot().Wait()
    End Sub

    Private Async Function StartBot As Task
        Dim commandSys As CommandsSystem = Client.UseCommands()
        commandSys.RegisterCommands(new TestCommandModule())

        Await Client.ConnectAsync()
        Await Task.Delay(- 1) ' Wait forever
    End Function

    Private Async Function SignOnDone(sender As Object, args As EventArgs) As Task Handles Client.SignOnDone
        Await client.JoinChatAsync("ChatRoom")
    End Function

    Private Async Function ChatMessageReceived(Sender As Object, args as ChatMessage) As Task Handles Client.ChatMessageReceived
        Dim MSG$ = Utils.StripHTML(args.Message) ' We use this so we don't have <HTML> tags.

        if (MSG$.Contains("NINA")) Then
            Await Client.SendChatMessageAsync(args.Room, "NINA is the best!")
        End If
    End Function
End Module
