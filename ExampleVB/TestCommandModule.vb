Imports TOCSharp.Commands
Imports TOCSharp.Commands.Attributes

Public Class TestCommandModule
    Implements ICommandModule

    <Command("scream")>
    Public Async Function ScreamCommand(ctx As CommandContext, <RemainingText> text As String) As Task
        await ctx.ReplyAsync("Someone told me to scream this: " & UCase$(text))
    End Function

    <Command("windows")>
    Public Async Function WindowsCommand(Ctx As CommandContext, <RemainingText> Target$) As Task
        Call Randomize

        Dim Versions(16) As String
        Versions(0) = "1.0"
        Versions(1) = "2.0"
        Versions(2) = "3.0"
        Versions(3) = "3.1"
        Versions(4) = "95"
        Versions(5) = "98"
        Versions(6) = "ME"
        Versions(7) = "NT 3.5"
        Versions(8) = "NT 4.0"
        Versions(9) = "2000"
        Versions(10) = "XP"
        Versions(11) = "Vista"
        Versions(12) = "7"
        Versions(13) = "8"
        Versions(14) = "8.1"
        Versions(15) = "10"
        Versions(16) = "11"

        Dim Version = Versions(Int(Rnd() * 16))

        Await Ctx.ReplyAsync("Hey " & Ctx.Sender & ", " & Target & "'s favorite Windows version is Windows " & Version & ".")
    End Function


End Class